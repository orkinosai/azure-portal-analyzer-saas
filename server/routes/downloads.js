const express = require('express');
const path = require('path');
const fs = require('fs');
const { v4: uuidv4 } = require('uuid');
const authMiddleware = require('../middleware/auth');
const db = require('../models/database');

const router = express.Router();

// Generate secure download token
router.post('/token/:documentId', authMiddleware, (req, res) => {
  const documentId = req.params.documentId;
  const userId = req.user.userId;

  // Verify user owns the document
  db.get(
    'SELECT id, original_name FROM documents WHERE id = ? AND user_id = ?',
    [documentId, userId],
    (err, document) => {
      if (err) {
        console.error(err);
        return res.status(500).json({ error: 'Database error' });
      }

      if (!document) {
        return res.status(404).json({ error: 'Document not found' });
      }

      // Generate secure token
      const token = uuidv4();
      const expiresAt = new Date(Date.now() + 15 * 60 * 1000); // 15 minutes from now

      // Save token to database
      db.run(
        'INSERT INTO download_tokens (token, document_id, user_id, expires_at) VALUES (?, ?, ?, ?)',
        [token, documentId, userId, expiresAt.toISOString()],
        function(err) {
          if (err) {
            console.error(err);
            return res.status(500).json({ error: 'Failed to generate download token' });
          }

          res.json({
            token,
            expiresAt: expiresAt.toISOString(),
            downloadUrl: `/api/downloads/file/${token}`,
            filename: document.original_name
          });
        }
      );
    }
  );
});

// Download file using secure token
router.get('/file/:token', (req, res) => {
  const token = req.params.token;

  // Get token info and verify it's valid
  db.get(`
    SELECT dt.*, d.filename, d.original_name, d.file_type 
    FROM download_tokens dt 
    JOIN documents d ON dt.document_id = d.id 
    WHERE dt.token = ? AND dt.used = FALSE AND dt.expires_at > datetime('now')
  `, [token], (err, tokenData) => {
    if (err) {
      console.error(err);
      return res.status(500).json({ error: 'Database error' });
    }

    if (!tokenData) {
      return res.status(404).json({ error: 'Invalid or expired download token' });
    }

    const filePath = path.join(__dirname, '../../uploads', tokenData.filename);

    // Check if file exists
    if (!fs.existsSync(filePath)) {
      return res.status(404).json({ error: 'File not found' });
    }

    // Mark token as used (single-use tokens)
    db.run('UPDATE download_tokens SET used = TRUE WHERE token = ?', [token], (err) => {
      if (err) {
        console.error('Failed to mark token as used:', err);
      }
    });

    // Set appropriate headers for download
    res.setHeader('Content-Disposition', `attachment; filename="${tokenData.original_name}"`);
    res.setHeader('Content-Type', getContentType(tokenData.file_type));

    // Stream the file
    const fileStream = fs.createReadStream(filePath);
    fileStream.pipe(res);

    fileStream.on('error', (error) => {
      console.error('File stream error:', error);
      if (!res.headersSent) {
        res.status(500).json({ error: 'Failed to download file' });
      }
    });
  });
});

// Get download history for user
router.get('/history', authMiddleware, (req, res) => {
  const userId = req.user.userId;

  db.all(`
    SELECT dt.token, dt.expires_at, dt.used, dt.created_at, d.original_name, d.file_type
    FROM download_tokens dt 
    JOIN documents d ON dt.document_id = d.id 
    WHERE dt.user_id = ? 
    ORDER BY dt.created_at DESC 
    LIMIT 50
  `, [userId], (err, history) => {
    if (err) {
      console.error(err);
      return res.status(500).json({ error: 'Failed to fetch download history' });
    }

    res.json({ history });
  });
});

// Clean up expired tokens (utility endpoint)
router.post('/cleanup', authMiddleware, (req, res) => {
  db.run('DELETE FROM download_tokens WHERE expires_at < datetime("now")', (err) => {
    if (err) {
      console.error(err);
      return res.status(500).json({ error: 'Cleanup failed' });
    }

    res.json({ message: 'Expired tokens cleaned up successfully' });
  });
});

// Helper function to determine content type
function getContentType(fileType) {
  switch (fileType.toLowerCase()) {
    case '.pdf':
      return 'application/pdf';
    case '.md':
      return 'text/markdown';
    case '.txt':
      return 'text/plain';
    default:
      return 'application/octet-stream';
  }
}

module.exports = router;