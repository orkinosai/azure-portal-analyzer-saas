# Azure Portal Analyzer SaaS

A comprehensive SaaS solution for analyzing Azure Portal configurations using AI-powered insights and recommendations.

## Overview

Azure Portal Analyzer is a modern .NET 8 application that leverages Azure AI services to analyze Azure Portal configurations, providing intelligent recommendations and insights to optimize your Azure resources.

## Architecture

```
├── src/
│   ├── Api/                    # ASP.NET Core 8 Web API
│   ├── Web/                    # Blazor Server frontend
│   ├── AI/                     # Azure AI/OpenAI integration layer
│   └── Shared/                 # Shared DTOs, models, and utilities
├── infrastructure/
│   ├── provisioning/           # Bicep templates for Azure resources
│   └── pipelines/              # CI/CD pipeline configurations
├── tests/
│   ├── Unit/                   # Unit tests
│   ├── Integration/            # Integration tests
│   └── E2E/                    # End-to-end tests (to be implemented)
├── docs/                       # Documentation
└── .github/                    # GitHub workflows and issue templates
```

## Features

- **AI-Powered Analysis**: Leverages Azure OpenAI to analyze configurations
- **Blob Storage Integration**: Secure storage for configuration data
- **Modern Frontend**: Blazor Server application for interactive UI
- **RESTful API**: ASP.NET Core 8 Web API with OpenAPI/Swagger
- **Infrastructure as Code**: Bicep templates for Azure deployment
- **CI/CD Ready**: GitHub Actions workflows for automated deployment
- **Containerized**: Docker support for easy deployment

## Prerequisites

- .NET 8.0 SDK
- Azure Subscription
- Docker (optional, for containerized deployment)
- Azure CLI (for infrastructure deployment)

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/orkinosai/azure-portal-analyzer-saas.git
cd azure-portal-analyzer-saas
```

### 2. Build the Solution

```bash
dotnet restore
dotnet build
```

### 3. Run Tests

```bash
dotnet test
```

### 4. Run the Application Locally

```bash
# Start the API
cd src/Api/AzurePortalAnalyzer.Api
dotnet run

# Start the Web application (in another terminal)
cd src/Web/AzurePortalAnalyzer.Web
dotnet run
```

## Configuration

The application uses the following configuration settings:

### API Configuration (appsettings.json)

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-openai-endpoint.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-35-turbo"
  },
  "AzureStorage": {
    "ConnectionString": "your-storage-connection-string"
  },
  "ApplicationInsights": {
    "ConnectionString": "your-app-insights-connection-string"
  }
}
```

## Deployment

### Azure Infrastructure

Deploy the required Azure resources using Bicep templates:

```bash
# Login to Azure
az login

# Create resource group
az group create --name "azure-portal-analyzer-rg" --location "East US"

# Deploy infrastructure
az deployment group create \
  --resource-group "azure-portal-analyzer-rg" \
  --template-file infrastructure/provisioning/main.bicep \
  --parameters infrastructure/provisioning/main.parameters.json
```

### Application Deployment

The application can be deployed using:

1. **Azure Container Apps** (recommended)
2. **Azure App Service**
3. **Azure Kubernetes Service (AKS)**

See the [deployment documentation](docs/deployment.md) for detailed instructions.

## Development

### Project Structure

- **AzurePortalAnalyzer.Api**: RESTful API backend
- **AzurePortalAnalyzer.Web**: Blazor Server frontend
- **AzurePortalAnalyzer.AI**: Azure AI integration services
- **AzurePortalAnalyzer.Shared**: Shared models and DTOs

### Development Workflow

1. Create a feature branch
2. Make changes and add tests
3. Run `dotnet test` to ensure all tests pass
4. Create a pull request
5. GitHub Actions will validate the build and run tests
6. After approval, merge to main for automatic deployment

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Documentation

- [Architecture Overview](docs/architecture.md)
- [Setup Guide](docs/setup.md)
- [Deployment Guide](docs/deployment.md)
- [API Documentation](docs/api.md)
- [Contributing Guidelines](docs/contributing.md)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support, please create an issue in the GitHub repository or contact the development team.

## Azure Services Used

- **Azure OpenAI Service**: AI-powered configuration analysis
- **Azure Blob Storage**: Configuration data storage
- **Azure Container Registry**: Container image storage
- **Azure Container Apps**: Application hosting
- **Azure Application Insights**: Monitoring and logging
- **Azure Key Vault**: Secret management
- **Azure Log Analytics**: Centralized logging