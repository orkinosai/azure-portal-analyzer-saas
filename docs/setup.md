# Setup Guide

This guide will help you set up the Azure Portal Analyzer SaaS application for development and production environments.

## Prerequisites

### Required Software
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Azure CLI** - [Installation guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- **Git** - Version control system
- **Docker** (optional) - For containerized development

### Azure Subscription
- Active Azure subscription with sufficient permissions to create resources
- Azure OpenAI service access (may require special approval)

### Development Tools (Recommended)
- **Visual Studio 2022** with ASP.NET Core workload
- **Visual Studio Code** with C# extension
- **Azure Storage Explorer** - For managing blob storage
- **Postman** or similar tool for API testing

## Local Development Setup

### 1. Clone the Repository

```bash
git clone https://github.com/orkinosai/azure-portal-analyzer-saas.git
cd azure-portal-analyzer-saas
```

### 2. Verify Prerequisites

```bash
# Check .NET version
dotnet --version  # Should return 8.0.x

# Check Azure CLI
az --version

# Login to Azure
az login
```

### 3. Restore Dependencies

```bash
# Restore NuGet packages for the entire solution
dotnet restore

# Verify the build
dotnet build --configuration Release
```

### 4. Run Tests

```bash
# Run all tests to ensure everything is working
dotnet test --configuration Release --verbosity normal
```

## Azure Resource Setup

### 1. Create Resource Group

```bash
# Create a resource group for all resources
az group create \
  --name "azure-portal-analyzer-dev-rg" \
  --location "East US"
```

### 2. Deploy Infrastructure

```bash
# Deploy using Bicep template
az deployment group create \
  --resource-group "azure-portal-analyzer-dev-rg" \
  --template-file infrastructure/provisioning/main.bicep \
  --parameters applicationName=azure-portal-analyzer environment=dev location="East US"
```

### 3. Retrieve Configuration Values

After deployment, get the required configuration values:

```bash
# Get storage account connection string
az storage account show-connection-string \
  --name "azureportalanalyzerdevstorage" \
  --resource-group "azure-portal-analyzer-dev-rg" \
  --query connectionString -o tsv

# Get OpenAI endpoint and key
az cognitiveservices account show \
  --name "azure-portal-analyzer-dev-openai" \
  --resource-group "azure-portal-analyzer-dev-rg" \
  --query properties.endpoint -o tsv

az cognitiveservices account keys list \
  --name "azure-portal-analyzer-dev-openai" \
  --resource-group "azure-portal-analyzer-dev-rg" \
  --query key1 -o tsv

# Get Application Insights connection string
az monitor app-insights component show \
  --app "azure-portal-analyzer-dev-ai" \
  --resource-group "azure-portal-analyzer-dev-rg" \
  --query connectionString -o tsv
```

## Configuration

### 1. API Configuration

Create `src/Api/AzurePortalAnalyzer.Api/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AzureOpenAI": {
    "Endpoint": "https://azure-portal-analyzer-dev-openai.openai.azure.com/",
    "ApiKey": "your-openai-api-key-here",
    "DeploymentName": "gpt-35-turbo"
  },
  "AzureStorage": {
    "ConnectionString": "your-storage-connection-string-here"
  },
  "ApplicationInsights": {
    "ConnectionString": "your-app-insights-connection-string-here"
  },
  "AllowedHosts": "*"
}
```

### 2. Web Configuration

Create `src/Web/AzurePortalAnalyzer.Web/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7001"
  },
  "ApplicationInsights": {
    "ConnectionString": "your-app-insights-connection-string-here"
  },
  "AllowedHosts": "*"
}
```

### 3. Environment Variables (Alternative)

Instead of appsettings files, you can use environment variables:

```bash
# Windows
set AZURE_OPENAI_ENDPOINT=https://your-endpoint.openai.azure.com/
set AZURE_OPENAI_API_KEY=your-api-key
set AZURE_STORAGE_CONNECTION_STRING=your-connection-string
set APPLICATIONINSIGHTS_CONNECTION_STRING=your-app-insights-connection

# Linux/macOS
export AZURE_OPENAI_ENDPOINT=https://your-endpoint.openai.azure.com/
export AZURE_OPENAI_API_KEY=your-api-key
export AZURE_STORAGE_CONNECTION_STRING=your-connection-string
export APPLICATIONINSIGHTS_CONNECTION_STRING=your-app-insights-connection
```

## Running the Application

### 1. Start the API

```bash
cd src/Api/AzurePortalAnalyzer.Api
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7001`
- HTTP: `http://localhost:5001`
- Swagger UI: `https://localhost:7001/swagger`

### 2. Start the Web Application

In a new terminal:

```bash
cd src/Web/AzurePortalAnalyzer.Web
dotnet run
```

The web application will be available at:
- HTTPS: `https://localhost:7002`
- HTTP: `http://localhost:5002`

### 3. Development with Hot Reload

For active development with automatic reloading:

```bash
# API with hot reload
cd src/Api/AzurePortalAnalyzer.Api
dotnet watch run

# Web with hot reload
cd src/Web/AzurePortalAnalyzer.Web
dotnet watch run
```

## Docker Setup (Optional)

### 1. Build Docker Images

```bash
# Build API image
docker build -f src/Api/Dockerfile -t azure-portal-analyzer-api:dev .

# Build Web image
docker build -f src/Web/Dockerfile -t azure-portal-analyzer-web:dev .
```

### 2. Run with Docker Compose

Create `docker-compose.yml` in the root directory:

```yaml
version: '3.8'
services:
  api:
    build:
      context: .
      dockerfile: src/Api/Dockerfile
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - AZURE_OPENAI_ENDPOINT=${AZURE_OPENAI_ENDPOINT}
      - AZURE_OPENAI_API_KEY=${AZURE_OPENAI_API_KEY}
      - AZURE_STORAGE_CONNECTION_STRING=${AZURE_STORAGE_CONNECTION_STRING}
  
  web:
    build:
      context: .
      dockerfile: src/Web/Dockerfile
    ports:
      - "5002:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ApiSettings__BaseUrl=http://api:80
    depends_on:
      - api
```

Run with:
```bash
docker-compose up -d
```

## Testing Setup

### 1. Unit Tests

```bash
# Run unit tests
dotnet test tests/Unit/AzurePortalAnalyzer.Tests.Unit/ --configuration Release

# Run with coverage
dotnet test tests/Unit/AzurePortalAnalyzer.Tests.Unit/ --collect:"XPlat Code Coverage"
```

### 2. Integration Tests

```bash
# Run integration tests
dotnet test tests/Integration/AzurePortalAnalyzer.Tests.Integration/ --configuration Release
```

### 3. Test Configuration

For integration tests that require Azure services, create test-specific configuration or use Azure Storage Emulator/Azurite for local development.

## Troubleshooting

### Common Issues

#### 1. SSL Certificate Issues
If you encounter SSL certificate errors:

```bash
# Trust the development certificate
dotnet dev-certs https --trust
```

#### 2. Port Conflicts
If ports 7001/7002 are in use, modify `launchSettings.json` in each project:

```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:7003;http://localhost:5003"
    }
  }
}
```

#### 3. Azure Connection Issues
- Verify your Azure credentials: `az account show`
- Check resource group and resource names
- Ensure proper permissions on Azure resources

#### 4. NuGet Package Issues
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore --force
```

### Debugging Tips

1. **Enable detailed logging** in `appsettings.Development.json`:
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Debug",
         "System": "Information",
         "Microsoft": "Information"
       }
     }
   }
   ```

2. **Use Azure Storage Explorer** to verify blob storage operations

3. **Check Application Insights** for telemetry and errors

4. **Use browser developer tools** for frontend debugging

## Next Steps

1. **Explore the API** using Swagger UI at `https://localhost:7001/swagger`
2. **Review the architecture** in [architecture.md](architecture.md)
3. **Check deployment options** in [deployment.md](deployment.md)
4. **Set up CI/CD** following the GitHub Actions workflows

## Getting Help

- Check the [troubleshooting section](#troubleshooting) above
- Review existing [GitHub issues](https://github.com/orkinosai/azure-portal-analyzer-saas/issues)
- Create a new issue with detailed error information
- Join the discussion in GitHub Discussions