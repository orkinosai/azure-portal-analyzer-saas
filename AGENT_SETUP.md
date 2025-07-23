# Azure Portal Analyzer SaaS - Agent Setup Guide

This document provides setup instructions for AI coding agents working on the Azure Portal Analyzer SaaS project.

## Project Overview

Azure Portal Analyzer is a .NET 8 SaaS application that uses Azure AI services to analyze Azure Portal configurations and provide intelligent recommendations.

## Repository Structure

```
azure-portal-analyzer-saas/
├── src/
│   ├── Api/AzurePortalAnalyzer.Api/           # ASP.NET Core 8 Web API
│   ├── Web/AzurePortalAnalyzer.Web/           # Blazor Server frontend
│   ├── AI/AzurePortalAnalyzer.AI/             # Azure AI integration layer
│   └── Shared/AzurePortalAnalyzer.Shared/     # Shared models and DTOs
├── tests/
│   ├── Unit/AzurePortalAnalyzer.Tests.Unit/
│   └── Integration/AzurePortalAnalyzer.Tests.Integration/
├── infrastructure/
│   ├── provisioning/                          # Bicep templates
│   └── pipelines/                             # CI/CD configurations
├── docs/                                      # Documentation
├── .github/
│   ├── workflows/                             # GitHub Actions
│   └── ISSUE_TEMPLATE/                        # Issue templates
├── AzurePortalAnalyzer.sln                    # Solution file
├── .editorconfig                              # Code style configuration
├── .gitignore                                 # Git ignore rules
└── README.md                                  # Project documentation
```

## Development Environment Setup

### Prerequisites

1. **.NET 8.0 SDK** - Required for building and running the application
2. **Azure CLI** - For infrastructure deployment and Azure resource management
3. **Docker** - Optional, for containerized development and deployment
4. **Visual Studio 2022** or **VS Code** - Recommended IDEs with C# extensions

### Quick Setup Commands

```bash
# Verify .NET installation
dotnet --version  # Should return 8.0.x

# Restore NuGet packages
dotnet restore

# Build the entire solution
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release

# Run specific project
cd src/Api/AzurePortalAnalyzer.Api
dotnet run

# Run with hot reload (development)
dotnet watch run
```

## Key Technologies and Frameworks

### Backend Technologies
- **.NET 8.0**: Latest LTS version of .NET
- **ASP.NET Core 8**: Web API framework
- **Entity Framework Core** (future): ORM for database operations
- **Azure SDK**: Integration with Azure services

### Frontend Technologies
- **Blazor Server**: Server-side rendered UI framework
- **Bootstrap**: CSS framework for responsive design
- **SignalR**: Real-time communication (built into Blazor Server)

### Azure Services
- **Azure OpenAI Service**: AI-powered configuration analysis
- **Azure Blob Storage**: Configuration data storage
- **Azure Container Apps**: Application hosting
- **Azure Application Insights**: Monitoring and telemetry
- **Azure Key Vault**: Secret management

### Testing Framework
- **xUnit**: Primary testing framework
- **ASP.NET Core Testing**: Integration testing utilities
- **Moq** (future): Mocking framework for unit tests

## Code Style and Conventions

The project uses the `.editorconfig` file for consistent code formatting:

- **Indentation**: 4 spaces
- **Line endings**: CRLF
- **C# naming conventions**: PascalCase for public members, camelCase for private fields
- **Braces**: Always use braces for control structures
- **Using directives**: Outside namespace, sorted system directives first

### Code Formatting

```bash
# Check code formatting
dotnet format --verify-no-changes

# Auto-format code
dotnet format
```

## Testing Guidelines

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Unit/AzurePortalAnalyzer.Tests.Unit/

# Run tests in watch mode
dotnet watch test
```

### Test Organization

- **Unit Tests**: Test individual components in isolation
- **Integration Tests**: Test component interactions and API endpoints
- **E2E Tests**: Test complete user workflows (to be implemented)

## Configuration Management

### Development Configuration

Create `appsettings.Development.json` files in API and Web projects:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-dev-openai.openai.azure.com/",
    "ApiKey": "your-dev-api-key",
    "DeploymentName": "gpt-35-turbo"
  },
  "AzureStorage": {
    "ConnectionString": "your-dev-storage-connection"
  }
}
```

### Environment Variables

For sensitive configuration, use environment variables:

- `AZURE_OPENAI_ENDPOINT`
- `AZURE_OPENAI_API_KEY`
- `AZURE_STORAGE_CONNECTION_STRING`
- `APPLICATIONINSIGHTS_CONNECTION_STRING`

## Build and Deployment

### Local Development

```bash
# Start API (runs on https://localhost:7001)
cd src/Api/AzurePortalAnalyzer.Api
dotnet run

# Start Web (runs on https://localhost:7002)
cd src/Web/AzurePortalAnalyzer.Web
dotnet run
```

### Docker Development

```bash
# Build Docker images
docker build -f src/Api/Dockerfile -t azure-portal-analyzer-api .
docker build -f src/Web/Dockerfile -t azure-portal-analyzer-web .

# Run with Docker Compose (future)
docker-compose up -d
```

### Azure Deployment

```bash
# Deploy infrastructure
az deployment group create \
  --resource-group azure-portal-analyzer-rg \
  --template-file infrastructure/provisioning/main.bicep \
  --parameters infrastructure/provisioning/main.parameters.json

# Deploy application (via GitHub Actions)
git push origin main
```

## CI/CD Pipeline

### GitHub Actions Workflows

1. **PR Validation** (`.github/workflows/pr-validation.yml`):
   - Build verification
   - Test execution
   - Code formatting check
   - Security scanning

2. **CI/CD Pipeline** (`.github/workflows/ci-cd.yml`):
   - Build and test
   - Docker image creation
   - Azure deployment
   - Multi-environment promotion

### Required Secrets

Configure these secrets in GitHub repository settings:

- `AZURE_CREDENTIALS`: Azure service principal for deployment
- `ACR_LOGIN_SERVER`: Azure Container Registry URL
- `ACR_USERNAME`: ACR username
- `ACR_PASSWORD`: ACR password
- `AZURE_RESOURCE_GROUP`: Target resource group
- `CODECOV_TOKEN`: Code coverage reporting token

## Common Development Tasks

### Adding a New API Endpoint

1. Create a new controller in `src/Api/Controllers/`
2. Add required DTOs in `src/Shared/DTOs/`
3. Implement business logic in appropriate service classes
4. Add unit tests for the controller and services
5. Add integration tests for the API endpoint

### Adding a New Blazor Page

1. Create a new Razor page in `src/Web/Components/Pages/`
2. Add required components in `src/Web/Components/`
3. Update navigation in `src/Web/Components/Layout/NavMenu.razor`
4. Add any required services to dependency injection

### Working with Azure Services

1. Add required NuGet packages to the appropriate project
2. Configure services in `Program.cs` or through dependency injection
3. Use Azure SDK clients through dependency injection
4. Store configuration in `appsettings.json` or environment variables
5. Add health checks for external dependencies

## Troubleshooting

### Common Issues

1. **Build Errors**: Ensure .NET 8.0 SDK is installed and NuGet packages are restored
2. **Test Failures**: Check for missing configuration or dependency injection setup
3. **Azure Connection Issues**: Verify credentials and network connectivity
4. **Docker Issues**: Ensure Docker is running and images can be built

### Debug Commands

```bash
# Check .NET version
dotnet --info

# List installed packages
dotnet list package

# Check for security vulnerabilities
dotnet list package --vulnerable

# Clean and rebuild
dotnet clean && dotnet build

# Restore NuGet packages
dotnet restore --force
```

## Resources

- [.NET 8 Documentation](https://docs.microsoft.com/en-us/dotnet/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Azure SDK for .NET](https://docs.microsoft.com/en-us/dotnet/azure/)
- [Azure OpenAI Service](https://docs.microsoft.com/en-us/azure/cognitive-services/openai/)

## Contact

For questions or issues, please:
1. Check existing GitHub issues
2. Create a new issue with detailed description
3. Tag appropriate team members for review