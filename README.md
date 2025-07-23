# Azure Portal Analyzer SaaS

A comprehensive .NET 8 SaaS application for analyzing Azure Portal infrastructure, generating detailed reports, and delivering them via email. Built with ASP.NET Core Web API, Blazor, Azure services, and following Microsoft best practices.

## Features

- **Automated Azure Resource Analysis**: Discover and analyze Azure resources across subscriptions and resource groups
- **Professional PDF Reports**: Generate comprehensive PDF documents using QuestPDF
- **Email Delivery**: Send analysis results directly to users via Azure Communication Services
- **Real-time Status Tracking**: Monitor analysis progress with live updates
- **Secure Authentication**: Built-in ASP.NET Core Identity integration
- **Azure-Native Architecture**: Leverages Azure Blob Storage, SQL Database, and Communication Services
- **Modern UI**: Responsive Blazor web interface with Bootstrap styling

## Architecture

### Technology Stack
- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: Blazor Server (.NET 8)
- **Database**: Azure SQL Database with Entity Framework Core
- **Storage**: Azure Blob Storage for document storage
- **Email**: Azure Communication Services for email delivery
- **Authentication**: ASP.NET Core Identity
- **Documentation**: QuestPDF for PDF generation, Markdig for Markdown processing
- **Deployment**: Azure App Service with CI/CD via GitHub Actions

### Project Structure
```
├── AzurePortalAnalyzer.Core/           # Core business logic and services
│   ├── Models/                         # Domain models
│   ├── Services/                       # Business services interfaces
│   ├── Services/Implementations/       # Service implementations
│   └── Data/                          # Entity Framework DbContext
├── AzurePortalAnalyzer.API/           # Web API project
│   ├── Controllers/                   # API controllers
│   └── DTOs/                         # Data transfer objects
├── AzurePortalAnalyzer.Web/           # Blazor web application
│   ├── Components/Pages/              # Blazor pages
│   ├── Services/                      # HTTP client services
│   └── Models/                       # View models
├── deployment/                        # Azure ARM templates
└── .github/workflows/                # GitHub Actions CI/CD
```

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Azure subscription
- Azure CLI (for deployment)
- Git

### Local Development Setup

1. **Clone the repository**:
   ```bash
   git clone https://github.com/orkinosai/azure-portal-analyzer-saas.git
   cd azure-portal-analyzer-saas
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Update configuration**:
   - Update `AzurePortalAnalyzer.API/appsettings.json` with your Azure service connection strings
   - Update `AzurePortalAnalyzer.Web/appsettings.json` with the API base URL

4. **Run the applications**:
   ```bash
   # Terminal 1 - API
   cd AzurePortalAnalyzer.API
   dotnet run
   
   # Terminal 2 - Web
   cd AzurePortalAnalyzer.Web
   dotnet run
   ```

5. **Access the application**:
   - Web UI: https://localhost:7002
   - API: https://localhost:7001
   - Swagger: https://localhost:7001/swagger

### Azure Deployment

1. **Deploy Azure Infrastructure**:
   ```bash
   # Login to Azure
   az login
   
   # Create resource group
   az group create --name rg-azure-portal-analyzer --location eastus
   
   # Deploy ARM template
   az deployment group create \
     --resource-group rg-azure-portal-analyzer \
     --template-file deployment/azuredeploy.json \
     --parameters deployment/azuredeploy.parameters.json
   ```

2. **Configure GitHub Actions**:
   - Add the following secrets to your GitHub repository:
     - `AZURE_WEBAPP_PUBLISH_PROFILE_API`: Publish profile for the API app
     - `AZURE_WEBAPP_PUBLISH_PROFILE_WEB`: Publish profile for the web app

3. **Deploy Application**:
   - Push to main branch to trigger automatic deployment via GitHub Actions

## Configuration

### Required Azure Services

1. **Azure SQL Database**: For storing analysis requests and user data
2. **Azure Blob Storage**: For storing generated PDF documents
3. **Azure Communication Services**: For sending emails with analysis results
4. **Azure App Service**: For hosting the API and web applications

### Environment Variables

#### API Configuration (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=AzurePortalAnalyzer;...",
    "AzureBlobStorage": "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...",
    "AzureCommunicationServices": "endpoint=https://....communication.azure.com/;accesskey=..."
  },
  "Azure": {
    "BlobStorage": {
      "ContainerName": "documents"
    },
    "Communication": {
      "SenderAddress": "DoNotReply@your-domain.com"
    }
  }
}
```

#### Web Configuration (`appsettings.json`):
```json
{
  "ApiSettings": {
    "BaseUrl": "https://your-api-app.azurewebsites.net"
  }
}
```

## Usage

### Starting an Analysis

1. Navigate to the **Start Analysis** page
2. Fill in the required information:
   - Email address for receiving results
   - Azure subscription ID
   - Optional: specific resource group
   - Optional: select specific resource types to analyze
3. Submit the request
4. Monitor progress on the status page
5. Receive email notification when complete

### Analysis Results

The system generates comprehensive reports including:
- **Resource Inventory**: Complete list of discovered resources
- **Cost Analysis**: Estimated monthly costs and optimization recommendations
- **Security Recommendations**: Security best practices and compliance suggestions
- **Resource Distribution**: Breakdown by type, location, and resource group
- **Actionable Insights**: Specific recommendations for improvement

## API Endpoints

### Analysis Controller
- `POST /api/analysis` - Create new analysis request
- `GET /api/analysis/{id}` - Get analysis request status
- `POST /api/analysis/{id}/reprocess` - Retry failed analysis

### Health Controller
- `GET /api/health` - Health check endpoint

## Development

### Building the Solution
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Database Migrations
```bash
cd AzurePortalAnalyzer.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Security Considerations

- **Authentication**: Uses ASP.NET Core Identity for user management
- **Authorization**: All API endpoints require authentication
- **Data Protection**: Sensitive data encrypted at rest and in transit
- **Access Control**: Role-based access control implementation ready
- **Azure Security**: Leverages Azure security features (Key Vault, Managed Identity)

## Monitoring and Logging

- **Application Insights**: Integrated for telemetry and monitoring
- **Structured Logging**: Uses ILogger with JSON formatting
- **Health Checks**: Built-in health check endpoints
- **Performance Monitoring**: Request/response timing and error tracking

## Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/new-feature`
3. Commit your changes: `git commit -am 'Add new feature'`
4. Push to the branch: `git push origin feature/new-feature`
5. Create a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

For support and questions:
- Create an issue in this repository
- Check the documentation
- Review the deployment troubleshooting guide

---

**Built with ❤️ using Microsoft technologies and Azure cloud services**