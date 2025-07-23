# Azure Portal Analyzer SaaS

A SaaS application for Azure Portal analysis with secure document download functionality.

## Features

- **Secure Authentication**: JWT-based user authentication and authorization
- **Document Generation**: Generate requirements documentation in PDF and Markdown formats
- **File Upload**: Upload existing documentation files (PDF, Markdown, Text)
- **Secure Downloads**: Time-limited, single-use download tokens for secure file access
- **User Dashboard**: Clean, intuitive web interface for document management
- **Security**: Rate limiting, CORS protection, helmet security headers

## Architecture

### Backend (Node.js/Express)
- RESTful API with secure endpoints
- SQLite database for user and document management
- JWT authentication middleware
- Multer for file upload handling
- jsPDF for PDF generation

### Frontend (React)
- Modern, responsive dashboard interface
- Axios for API communication
- File upload and download management
- User authentication flow

## Security Features

- **Expiring Download Links**: Download tokens expire after 15 minutes
- **Single-Use Tokens**: Tokens are invalidated after first use
- **User Authorization**: Users can only access their own documents
- **Rate Limiting**: API endpoints protected against abuse
- **Security Headers**: Helmet.js provides security headers
- **File Type Validation**: Only allowed file types can be uploaded

## Setup Instructions

### Prerequisites
- Node.js (v14 or higher)
- npm

### Installation

1. Clone the repository
2. Install dependencies:
   ```bash
   npm run install:all
   ```

3. Copy environment configuration:
   ```bash
   cp .env.example .env
   ```

4. Start the development servers:
   ```bash
   # Terminal 1: Start backend server
   npm run dev
   
   # Terminal 2: Start frontend development server
   cd client && npm start
   ```

### Production Build

```bash
# Build the React client
npm run build

# Start production server
NODE_ENV=production npm start
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user

### Documents
- `GET /api/documents` - List user's documents
- `POST /api/documents/generate` - Generate new document
- `POST /api/documents/upload` - Upload document file
- `DELETE /api/documents/:id` - Delete document

### Secure Downloads
- `POST /api/downloads/token/:documentId` - Generate secure download token
- `GET /api/downloads/file/:token` - Download file with token
- `GET /api/downloads/history` - Get download history

## Testing

Run the test suite:
```bash
npm test
```

## Usage

1. **Register/Login**: Create an account or login to existing account
2. **Generate Documents**: Use the document generator to create PDF or Markdown files
3. **Upload Files**: Upload existing documentation files
4. **Secure Download**: Click download to generate a secure, time-limited download link
5. **Manage Documents**: View, download, or delete your documents

## Technology Stack

- **Backend**: Node.js, Express.js, SQLite, JWT, Multer, jsPDF
- **Frontend**: React, Axios
- **Security**: Helmet, CORS, Rate Limiting, bcryptjs
- **Testing**: Jest, Supertest

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## License

MIT License