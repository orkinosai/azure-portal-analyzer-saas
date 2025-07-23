# Azure Portal Analyzer SaaS

A comprehensive SaaS application for analyzing Azure portals and generating requirements documentation using the Microsoft stack.

## Features

- **Portal Analysis**: Create and manage detailed portal component analyses
- **Requirements Documentation**: Generate professional requirements documents in PDF, Markdown, and HTML formats
- **Azure Integration**: Full Azure ecosystem integration with Blob Storage for document management
- **Authentication**: Secure authentication using ASP.NET Core Identity
- **Modern UI**: Blazor Server-based frontend for rich user experience

## Technology Stack

### Backend
- **ASP.NET Core 8 Web API**: RESTful API backend
- **Entity Framework Core**: Database ORM with SQL Server
- **ASP.NET Core Identity**: User authentication and authorization
- **Azure Blob Storage**: Cloud storage for generated documents

### Document Generation
- **QuestPDF**: Professional PDF document generation
- **Markdig**: Markdown processing and HTML generation

### Frontend
- **Blazor Server**: Interactive web UI
- **ASP.NET Core MVC**: Web framework

### Infrastructure
- **Azure**: Cloud-native deployment
- **GitHub Actions**: CI/CD pipeline
- **SQL Server**: Database backend

## Project Structure

```
├── AzurePortalAnalyzer.Api/          # Web API project
│   ├── Controllers/                  # API controllers
│   ├── Data/                        # Entity Framework context
│   ├── Models/                      # Data models and DTOs
│   └── Services/                    # Business logic services
├── AzurePortalAnalyzer.Web/         # Blazor Server project
├── AzurePortalAnalyzer.Tests/       # Unit tests
└── .github/workflows/               # CI/CD workflows
```

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (LocalDB for development)
- Azure Storage Account (or Azure Storage Emulator)

### Configuration

1. Update `appsettings.json` in the API project:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=AzurePortalAnalyzerDb;Trusted_Connection=true;MultipleActiveResultSets=true",
    "AzureStorage": "DefaultEndpointsProtocol=https;AccountName=<your_account>;AccountKey=<your_key>;EndpointSuffix=core.windows.net"
  }
}
```

2. For development, you can use:
```json
{
  "ConnectionStrings": {
    "AzureStorage": "UseDevelopmentStorage=true"
  }
}
```

### Running the Application

1. **Build the solution:**
```bash
dotnet build
```

2. **Run tests:**
```bash
dotnet test
```

3. **Start the API:**
```bash
cd AzurePortalAnalyzer.Api
dotnet run
```

4. **Start the Web UI (in a separate terminal):**
```bash
cd AzurePortalAnalyzer.Web
dotnet run
```

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - User login

### Portal Analysis
- `GET /api/portalanalysis` - Get user's analyses
- `POST /api/portalanalysis` - Create new analysis
- `GET /api/portalanalysis/{id}` - Get specific analysis
- `PUT /api/portalanalysis/{id}` - Update analysis
- `DELETE /api/portalanalysis/{id}` - Delete analysis

### Documents
- `POST /api/documents/generate` - Generate requirements document
- `GET /api/documents/{id}` - Get document details
- `GET /api/documents/{id}/download` - Download document
- `GET /api/documents/{id}/download-url` - Get download URL

## Usage Example

### Creating a Portal Analysis

```json
{
  "name": "E-commerce Portal Analysis",
  "description": "Analysis of the main e-commerce portal",
  "components": [
    {
      "name": "User Login",
      "type": "Form",
      "description": "User authentication form",
      "properties": {
        "fields": ["username", "password"],
        "validation": "required"
      },
      "requirements": [
        "Username field must be required",
        "Password field must be masked",
        "Login button must be prominent"
      ]
    }
  ]
}
```

### Generating Documentation

```json
{
  "portalAnalysisId": 1,
  "title": "E-commerce Portal Requirements",
  "description": "Comprehensive requirements documentation",
  "format": "PDF"
}
```

## Deployment

The application includes GitHub Actions workflow for automatic deployment to Azure:

1. **Azure Web Apps**: Separate deployments for API and Web
2. **Azure SQL Database**: Production database
3. **Azure Blob Storage**: Document storage
4. **Azure App Service**: Hosting

### Required Secrets
- `AZURE_WEBAPP_PUBLISH_PROFILE_API`
- `AZURE_WEBAPP_PUBLISH_PROFILE_WEB`

## Development

### Adding New Features
1. Create feature branch
2. Implement changes with tests
3. Update documentation
4. Create pull request

### Testing
- Unit tests cover core business logic
- Integration tests for API endpoints
- End-to-end tests for critical workflows

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.