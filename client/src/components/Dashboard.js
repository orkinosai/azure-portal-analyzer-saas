import React, { useState, useEffect } from 'react';
import axios from 'axios';

function Dashboard({ user, onLogout }) {
  const [documents, setDocuments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  // Document generator state
  const [generatorData, setGeneratorData] = useState({
    title: '',
    content: '',
    format: 'pdf'
  });

  // File upload state
  const [uploadFile, setUploadFile] = useState(null);

  useEffect(() => {
    fetchDocuments();
  }, []);

  const fetchDocuments = async () => {
    try {
      const response = await axios.get('/api/documents');
      setDocuments(response.data.documents);
    } catch (error) {
      setError('Failed to fetch documents');
    } finally {
      setLoading(false);
    }
  };

  const generateDocument = async (e) => {
    e.preventDefault();
    if (!generatorData.title || !generatorData.content) {
      setError('Title and content are required');
      return;
    }

    try {
      setError('');
      const response = await axios.post('/api/documents/generate', generatorData);
      setSuccess('Document generated successfully!');
      setGeneratorData({ title: '', content: '', format: 'pdf' });
      fetchDocuments();
    } catch (error) {
      setError(error.response?.data?.error || 'Failed to generate document');
    }
  };

  const uploadDocument = async (e) => {
    e.preventDefault();
    if (!uploadFile) {
      setError('Please select a file to upload');
      return;
    }

    const formData = new FormData();
    formData.append('document', uploadFile);

    try {
      setError('');
      const response = await axios.post('/api/documents/upload', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      setSuccess('Document uploaded successfully!');
      setUploadFile(null);
      fetchDocuments();
    } catch (error) {
      setError(error.response?.data?.error || 'Failed to upload document');
    }
  };

  const generateDownloadToken = async (documentId, filename) => {
    try {
      const response = await axios.post(`/api/downloads/token/${documentId}`);
      const { token, downloadUrl } = response.data;
      
      // Create download link
      const fullUrl = `${window.location.origin}${downloadUrl}`;
      
      // Open download in new tab
      window.open(fullUrl, '_blank');
      
      setSuccess(`Download link generated for ${filename}`);
    } catch (error) {
      setError(error.response?.data?.error || 'Failed to generate download link');
    }
  };

  const deleteDocument = async (documentId) => {
    if (!window.confirm('Are you sure you want to delete this document?')) {
      return;
    }

    try {
      await axios.delete(`/api/documents/${documentId}`);
      setSuccess('Document deleted successfully');
      fetchDocuments();
    } catch (error) {
      setError(error.response?.data?.error || 'Failed to delete document');
    }
  };

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  if (loading) {
    return <div className="loading">Loading dashboard...</div>;
  }

  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1 className="dashboard-title">Azure Portal Analyzer</h1>
        <div className="user-info">
          <span>Welcome, {user.email}</span>
          <button onClick={onLogout} className="btn btn-secondary">
            Logout
          </button>
        </div>
      </div>

      {error && <div className="error" style={{ marginBottom: '1rem' }}>{error}</div>}
      {success && <div className="success" style={{ marginBottom: '1rem' }}>{success}</div>}

      <div className="sections">
        {/* Document Generator */}
        <section className="section">
          <h3>Generate Documentation</h3>
          <form onSubmit={generateDocument} className="generator-form">
            <div className="form-group">
              <label htmlFor="title">Document Title</label>
              <input
                type="text"
                id="title"
                value={generatorData.title}
                onChange={(e) => setGeneratorData({ ...generatorData, title: e.target.value })}
                placeholder="Enter document title..."
                required
              />
            </div>
            <div className="form-group">
              <label htmlFor="content">Content</label>
              <textarea
                id="content"
                value={generatorData.content}
                onChange={(e) => setGeneratorData({ ...generatorData, content: e.target.value })}
                placeholder="Enter your documentation content..."
                required
              />
            </div>
            <div className="form-group">
              <label>Format</label>
              <div className="format-selector">
                <label>
                  <input
                    type="radio"
                    name="format"
                    value="pdf"
                    checked={generatorData.format === 'pdf'}
                    onChange={(e) => setGeneratorData({ ...generatorData, format: e.target.value })}
                  />
                  PDF
                </label>
                <label>
                  <input
                    type="radio"
                    name="format"
                    value="markdown"
                    checked={generatorData.format === 'markdown'}
                    onChange={(e) => setGeneratorData({ ...generatorData, format: e.target.value })}
                  />
                  Markdown
                </label>
              </div>
            </div>
            <button type="submit" className="btn btn-success">
              Generate Document
            </button>
          </form>
        </section>

        {/* File Upload */}
        <section className="section">
          <h3>Upload Document</h3>
          <form onSubmit={uploadDocument}>
            <div className="upload-area">
              <p>Upload your existing documentation (PDF, Markdown, or Text files)</p>
              <input
                type="file"
                accept=".pdf,.md,.txt"
                onChange={(e) => setUploadFile(e.target.files[0])}
              />
            </div>
            <button type="submit" className="btn btn-primary" disabled={!uploadFile}>
              Upload Document
            </button>
          </form>
        </section>

        {/* Document List */}
        <section className="section">
          <h3>Your Documents</h3>
          {documents.length === 0 ? (
            <p>No documents found. Generate or upload your first document!</p>
          ) : (
            <div className="document-list">
              {documents.map((doc) => (
                <div key={doc.id} className="document-item">
                  <div className="document-info">
                    <div className="document-name">{doc.original_name}</div>
                    <div className="document-meta">
                      {doc.file_type.toUpperCase()} • {formatFileSize(doc.file_size)} • 
                      Created: {new Date(doc.created_at).toLocaleDateString()}
                    </div>
                  </div>
                  <div className="document-actions">
                    <button
                      onClick={() => generateDownloadToken(doc.id, doc.original_name)}
                      className="btn btn-primary btn-sm"
                    >
                      Download
                    </button>
                    <button
                      onClick={() => deleteDocument(doc.id)}
                      className="btn btn-danger btn-sm"
                    >
                      Delete
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </section>
      </div>
    </div>
  );
}

export default Dashboard;