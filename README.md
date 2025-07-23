# Azure Portal Analyzer SaaS

A comprehensive solution for generating secure analysis reports for Azure Portal configurations using the Microsoft stack.

## Features

- **ASP.NET Core 8 Web API** - Modern, high-performance backend
- **Azure Blob Storage Integration** - Secure document storage with SAS tokens
- **ASP.NET Core Identity** - Built-in authentication and authorization
- **Document Generation** - PDF reports using QuestPDF and Markdown using Markdig
- **Blazor WebAssembly Frontend** - Modern, responsive user interface
- **Secure Downloads** - Time-limited access to generated documents

## Architecture

The solution follows clean architecture principles with three main projects:

- **AzurePortalAnalyzer.Core** - Domain models and service interfaces
- **AzurePortalAnalyzer.Infrastructure** - Data access, Azure integrations, and service implementations
- **AzurePortalAnalyzer.Web** - ASP.NET Core Web API with authentication
- **AzurePortalAnalyzer.Client** - Blazor WebAssembly frontend

## Technologies Used

### Backend
- .NET 8
- ASP.NET Core 8 Web API
- Entity Framework Core 8
- ASP.NET Core Identity
- Azure Blob Storage SDK
- QuestPDF for PDF generation
- Markdig for Markdown processing

### Frontend
- Blazor WebAssembly
- Bootstrap 5
- Bootstrap Icons

### Database
- SQLite (development)
- SQL Server (production ready)

## Getting Started

### Prerequisites
- .NET 8 SDK
- Azure Storage Account (or Azure Storage Emulator for development)

### Running the Application

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd azure-portal-analyzer-saas
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the Web API**
   ```bash
   cd src/AzurePortalAnalyzer.Web
   dotnet run
   ```
   The API will be available at `https://localhost:7066` and `http://localhost:5066`

4. **Run the Blazor Client** (in a separate terminal)
   ```bash
   cd src/AzurePortalAnalyzer.Client
   dotnet run
   ```
   The client will be available at `https://localhost:7014` and `http://localhost:5014`

### Configuration

Update `appsettings.json` in the Web project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=azureportalanalyzer.db",
    "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=<account>;AccountKey=<key>;EndpointSuffix=core.windows.net"
  }
}
```

For development, you can use the Azure Storage Emulator:
```json
{
  "ConnectionStrings": {
    "AzureStorage": "UseDevelopmentStorage=true"
  }
}
```

## API Endpoints

### Documents
- `GET /api/documents` - Get user's documents
- `POST /api/documents` - Create a new document
- `GET /api/documents/{id}` - Get document details
- `GET /api/documents/{id}/download` - Get secure download URL
- `DELETE /api/documents/{id}` - Delete a document

### Authentication
- `POST /register` - Register new user
- `POST /login` - User login
- `POST /logout` - User logout

## Security Features

### Document Storage
- Documents are stored in Azure Blob Storage with unique blob names
- Access is controlled through time-limited SAS tokens
- SAS tokens expire after 1 hour by default
- Users can only access their own documents

### Authentication
- ASP.NET Core Identity for user management
- JWT Bearer token authentication for API access
- Password requirements enforced
- Secure user registration and login

### Authorization
- User-specific document access control
- All API endpoints require authentication
- Documents are filtered by user ID

## Document Types

1. **Portal Analysis Report** - Comprehensive analysis of Azure Portal configuration
2. **Compliance Report** - Compliance assessment and recommendations
3. **Security Audit** - Security analysis with findings and remediation
4. **Custom Report** - Flexible report format for specific needs

## Deployment

### Azure App Service
1. Create an Azure App Service
2. Configure connection strings in Application Settings
3. Deploy using Visual Studio, Azure CLI, or GitHub Actions

### Azure Resources Required
- Azure App Service (for the Web API)
- Azure Storage Account (for document storage)
- Azure SQL Database (for production) or use SQLite for development

### Environment Variables
- `ConnectionStrings__DefaultConnection` - Database connection string
- `ConnectionStrings__AzureStorage` - Azure Storage connection string

## Development

### Project Structure
```
src/
├── AzurePortalAnalyzer.Core/          # Domain models and interfaces
│   ├── Models/                        # Entity models
│   └── Services/                      # Service interfaces
├── AzurePortalAnalyzer.Infrastructure/ # Infrastructure layer
│   ├── Data/                          # EF Core DbContext
│   ├── Services/                      # Service implementations
│   └── Storage/                       # Azure Blob Storage service
├── AzurePortalAnalyzer.Web/           # Web API project
│   └── Controllers/                   # API controllers
└── AzurePortalAnalyzer.Client/        # Blazor WebAssembly client
    ├── Pages/                         # Razor pages
    └── Layout/                        # Layout components
```

### Adding New Document Types
1. Add the new type to the `DocumentType` enum in `Core/Models/Enums.cs`
2. Update the document generation service to handle the new type
3. Update the frontend UI to support the new type

### Customizing Document Templates
Modify the `DocumentGenerationService` to customize PDF and Markdown templates:
- Update PDF templates in the `GeneratePdfReportAsync` method
- Update Markdown templates in the `GenerateMarkdownReportAsync` method

## Testing

Run the test suite:
```bash
dotnet test
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.