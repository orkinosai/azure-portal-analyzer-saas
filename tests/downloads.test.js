const request = require('supertest');
const app = require('../server/index');
const db = require('../server/models/database');
const fs = require('fs');
const path = require('path');

describe('Secure Download Functionality', () => {
  let userToken;
  let userId;
  let documentId;

  beforeAll(async () => {
    // Clean up test data
    await new Promise((resolve) => {
      db.run('DELETE FROM download_tokens WHERE 1=1', resolve);
    });
    await new Promise((resolve) => {
      db.run('DELETE FROM documents WHERE 1=1', resolve);
    });
    await new Promise((resolve) => {
      db.run('DELETE FROM users WHERE email LIKE "%test%"', resolve);
    });
  });

  afterAll(async () => {
    // Clean up test files
    const uploadsDir = path.join(__dirname, '../uploads');
    if (fs.existsSync(uploadsDir)) {
      const files = fs.readdirSync(uploadsDir);
      files.forEach(file => {
        const filePath = path.join(uploadsDir, file);
        if (fs.existsSync(filePath)) {
          fs.unlinkSync(filePath);
        }
      });
    }
  });

  test('Should register a new user', async () => {
    const response = await request(app)
      .post('/api/auth/register')
      .send({
        email: 'test@example.com',
        password: 'testpassword123'
      });

    expect(response.status).toBe(201);
    expect(response.body).toHaveProperty('token');
    expect(response.body.user).toHaveProperty('email', 'test@example.com');
    
    userToken = response.body.token;
    userId = response.body.user.id;
  });

  test('Should generate a document', async () => {
    const response = await request(app)
      .post('/api/documents/generate')
      .set('Authorization', `Bearer ${userToken}`)
      .send({
        title: 'Test Azure Requirements Document',
        content: 'This is a test document for Azure Portal analysis requirements.',
        format: 'pdf'
      });

    expect(response.status).toBe(201);
    expect(response.body.message).toBe('Document generated successfully');
    expect(response.body.document).toHaveProperty('id');
    
    documentId = response.body.document.id;
  });

  test('Should create a secure download token', async () => {
    const response = await request(app)
      .post(`/api/downloads/token/${documentId}`)
      .set('Authorization', `Bearer ${userToken}`);

    expect(response.status).toBe(200);
    expect(response.body).toHaveProperty('token');
    expect(response.body).toHaveProperty('expiresAt');
    expect(response.body).toHaveProperty('downloadUrl');
    expect(response.body.downloadUrl).toMatch(/^\/api\/downloads\/file\/.+$/);
  });

  test('Should download file with valid token', async () => {
    // First get a download token
    const tokenResponse = await request(app)
      .post(`/api/downloads/token/${documentId}`)
      .set('Authorization', `Bearer ${userToken}`);

    const { token } = tokenResponse.body;

    // Then download the file
    const downloadResponse = await request(app)
      .get(`/api/downloads/file/${token}`);

    expect(downloadResponse.status).toBe(200);
    expect(downloadResponse.headers['content-disposition']).toMatch(/attachment/);
    expect(downloadResponse.headers['content-type']).toBe('application/pdf');
  });

  test('Should reject download with expired/used token', async () => {
    // First get a download token
    const tokenResponse = await request(app)
      .post(`/api/downloads/token/${documentId}`)
      .set('Authorization', `Bearer ${userToken}`);

    const { token } = tokenResponse.body;

    // Download once to mark token as used
    await request(app).get(`/api/downloads/file/${token}`);

    // Try to download again with same token
    const secondDownloadResponse = await request(app)
      .get(`/api/downloads/file/${token}`);

    expect(secondDownloadResponse.status).toBe(404);
    expect(secondDownloadResponse.body.error).toBe('Invalid or expired download token');
  });

  test('Should require authentication for token generation', async () => {
    const response = await request(app)
      .post(`/api/downloads/token/${documentId}`);

    expect(response.status).toBe(401);
    expect(response.body.error).toBe('No token, authorization denied');
  });

  test('Should not allow access to other users documents', async () => {
    // Register another user
    const user2Response = await request(app)
      .post('/api/auth/register')
      .send({
        email: 'test2@example.com',
        password: 'testpassword123'
      });

    const user2Token = user2Response.body.token;

    // Try to access first user's document
    const response = await request(app)
      .post(`/api/downloads/token/${documentId}`)
      .set('Authorization', `Bearer ${user2Token}`);

    expect(response.status).toBe(404);
    expect(response.body.error).toBe('Document not found');
  });

  test('Should list user documents', async () => {
    const response = await request(app)
      .get('/api/documents')
      .set('Authorization', `Bearer ${userToken}`);

    expect(response.status).toBe(200);
    expect(response.body.documents).toHaveLength(1);
    expect(response.body.documents[0]).toHaveProperty('original_name');
    expect(response.body.documents[0]).toHaveProperty('file_type');
  });
});