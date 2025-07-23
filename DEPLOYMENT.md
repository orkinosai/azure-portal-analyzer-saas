# Azure Deployment Guide

This guide explains how to deploy the Azure Portal Analyzer SaaS application to Azure using Microsoft's best practices.

## Prerequisites

- Azure CLI installed and configured
- .NET 8 SDK
- Azure subscription with appropriate permissions

## Azure Resources Required

### 1. Resource Group
```bash
az group create --name rg-portal-analyzer --location eastus
```

### 2. Azure Storage Account
```bash
az storage account create \
  --name stportalanalyzer001 \
  --resource-group rg-portal-analyzer \
  --location eastus \
  --sku Standard_LRS \
  --kind StorageV2
```

Get the connection string:
```bash
az storage account show-connection-string \
  --name stportalanalyzer001 \
  --resource-group rg-portal-analyzer \
  --query connectionString \
  --output tsv
```

### 3. Azure SQL Database (Production)
```bash
# Create SQL Server
az sql server create \
  --name sql-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --location eastus \
  --admin-user sqladmin \
  --admin-password <YourSecurePassword>

# Create SQL Database
az sql db create \
  --name sqldb-portal-analyzer \
  --server sql-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --service-objective Basic
```

Configure firewall (allow Azure services):
```bash
az sql server firewall-rule create \
  --name AllowAzureServices \
  --server sql-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

### 4. App Service Plan
```bash
az appservice plan create \
  --name asp-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --location eastus \
  --sku B1 \
  --is-linux
```

### 5. Web App for API
```bash
az webapp create \
  --name app-portal-analyzer-api \
  --resource-group rg-portal-analyzer \
  --plan asp-portal-analyzer \
  --runtime "DOTNETCORE:8.0"
```

### 6. Static Web App for Client
```bash
az staticwebapp create \
  --name swa-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --source https://github.com/yourusername/azure-portal-analyzer-saas \
  --branch main \
  --app-location "src/AzurePortalAnalyzer.Client" \
  --api-location "src/AzurePortalAnalyzer.Web" \
  --output-location "wwwroot"
```

## Configuration

### 1. Configure App Service Settings
```bash
# Get SQL connection string
SQL_CONNECTION_STRING="Server=tcp:sql-portal-analyzer.database.windows.net,1433;Database=sqldb-portal-analyzer;User ID=sqladmin;Password=<YourSecurePassword>;Encrypt=true;Connection Timeout=30;"

# Get Storage connection string
STORAGE_CONNECTION_STRING=$(az storage account show-connection-string --name stportalanalyzer001 --resource-group rg-portal-analyzer --query connectionString --output tsv)

# Configure app settings
az webapp config appsettings set \
  --name app-portal-analyzer-api \
  --resource-group rg-portal-analyzer \
  --settings \
    "ConnectionStrings__DefaultConnection=$SQL_CONNECTION_STRING" \
    "ConnectionStrings__AzureStorage=$STORAGE_CONNECTION_STRING" \
    "ASPNETCORE_ENVIRONMENT=Production"
```

### 2. Configure CORS
```bash
az webapp cors add \
  --name app-portal-analyzer-api \
  --resource-group rg-portal-analyzer \
  --allowed-origins "https://swa-portal-analyzer.azurestaticapps.net"
```

## Deployment Options

### Option 1: GitHub Actions (Recommended)

Create `.github/workflows/deploy.yml`:

```yaml
name: Deploy to Azure

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal
      
    - name: Publish API
      run: dotnet publish src/AzurePortalAnalyzer.Web/AzurePortalAnalyzer.Web.csproj -c Release -o ./api-publish
      
    - name: Deploy API to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: 'app-portal-analyzer-api'
        slot-name: 'Production'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./api-publish
```

### Option 2: Azure CLI Direct Deployment

```bash
# Build and publish the API
dotnet publish src/AzurePortalAnalyzer.Web/AzurePortalAnalyzer.Web.csproj -c Release -o ./publish

# Create deployment zip
cd publish
zip -r ../deploy.zip .
cd ..

# Deploy to App Service
az webapp deployment source config-zip \
  --name app-portal-analyzer-api \
  --resource-group rg-portal-analyzer \
  --src deploy.zip
```

### Option 3: Visual Studio Publish

1. Right-click on `AzurePortalAnalyzer.Web` project
2. Select "Publish"
3. Choose "Azure"
4. Select "Azure App Service (Linux)"
5. Select your subscription and App Service
6. Click "Publish"

## Database Migration

### Run EF Core Migrations

```bash
# Install EF Core tools
dotnet tool install --global dotnet-ef

# Add migration (if needed)
cd src/AzurePortalAnalyzer.Infrastructure
dotnet ef migrations add InitialCreate --startup-project ../AzurePortalAnalyzer.Web

# Update database
dotnet ef database update --startup-project ../AzurePortalAnalyzer.Web --connection "$SQL_CONNECTION_STRING"
```

### Alternative: Automatic Migration in Production

The application is configured to automatically create the database on startup. However, for production, it's recommended to run migrations explicitly.

## Security Configuration

### 1. Key Vault (Recommended for Production)

```bash
# Create Key Vault
az keyvault create \
  --name kv-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --location eastus

# Add secrets
az keyvault secret set \
  --vault-name kv-portal-analyzer \
  --name "ConnectionStrings--DefaultConnection" \
  --value "$SQL_CONNECTION_STRING"

az keyvault secret set \
  --vault-name kv-portal-analyzer \
  --name "ConnectionStrings--AzureStorage" \
  --value "$STORAGE_CONNECTION_STRING"

# Configure App Service to use Key Vault
az webapp identity assign \
  --name app-portal-analyzer-api \
  --resource-group rg-portal-analyzer

# Get the principal ID and grant access to Key Vault
PRINCIPAL_ID=$(az webapp identity show --name app-portal-analyzer-api --resource-group rg-portal-analyzer --query principalId --output tsv)

az keyvault set-policy \
  --name kv-portal-analyzer \
  --object-id $PRINCIPAL_ID \
  --secret-permissions get list
```

### 2. Configure App Service to use Key Vault

Update app settings to reference Key Vault:
```bash
az webapp config appsettings set \
  --name app-portal-analyzer-api \
  --resource-group rg-portal-analyzer \
  --settings \
    "ConnectionStrings__DefaultConnection=@Microsoft.KeyVault(VaultName=kv-portal-analyzer;SecretName=ConnectionStrings--DefaultConnection)" \
    "ConnectionStrings__AzureStorage=@Microsoft.KeyVault(VaultName=kv-portal-analyzer;SecretName=ConnectionStrings--AzureStorage)"
```

## Monitoring and Logging

### 1. Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app ai-portal-analyzer \
  --location eastus \
  --resource-group rg-portal-analyzer

# Get instrumentation key
INSTRUMENTATION_KEY=$(az monitor app-insights component show --app ai-portal-analyzer --resource-group rg-portal-analyzer --query instrumentationKey --output tsv)

# Configure App Service
az webapp config appsettings set \
  --name app-portal-analyzer-api \
  --resource-group rg-portal-analyzer \
  --settings "APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY"
```

### 2. Log Analytics Workspace

```bash
# Create Log Analytics Workspace
az monitor log-analytics workspace create \
  --workspace-name law-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --location eastus
```

## Health Checks and Scaling

### 1. Configure Health Checks

The application includes built-in health checks. Configure App Service to use them:

```bash
az webapp config set \
  --name app-portal-analyzer-api \
  --resource-group rg-portal-analyzer \
  --generic-configurations '{"healthCheckPath": "/health"}'
```

### 2. Auto-scaling Rules

```bash
az monitor autoscale create \
  --name autoscale-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --resource /subscriptions/{subscription-id}/resourceGroups/rg-portal-analyzer/providers/Microsoft.Web/serverFarms/asp-portal-analyzer \
  --min-count 1 \
  --max-count 3 \
  --count 1

az monitor autoscale rule create \
  --name cpu-rule \
  --resource-group rg-portal-analyzer \
  --autoscale-name autoscale-portal-analyzer \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1
```

## Backup and Disaster Recovery

### 1. Database Backup

```bash
# Enable automated backups (included in Basic tier and above)
az sql db show \
  --name sqldb-portal-analyzer \
  --server sql-portal-analyzer \
  --resource-group rg-portal-analyzer \
  --query backupRetentionPeriod
```

### 2. Storage Account Backup

```bash
# Enable blob versioning and soft delete
az storage account blob-service-properties update \
  --account-name stportalanalyzer001 \
  --enable-versioning true \
  --enable-delete-retention true \
  --delete-retention-days 7
```

## Post-Deployment Validation

1. **Test API endpoints**: Visit `https://app-portal-analyzer-api.azurewebsites.net/swagger`
2. **Test authentication**: Try registering and logging in
3. **Test document generation**: Create a test document
4. **Test blob storage**: Verify documents are stored in Azure Storage
5. **Test secure downloads**: Verify SAS token generation and access

## Troubleshooting

### Common Issues

1. **Connection String Issues**
   - Verify connection strings in app settings
   - Check firewall rules for SQL Database
   - Ensure storage account access keys are correct

2. **Authentication Issues**
   - Verify CORS settings
   - Check JWT token configuration
   - Ensure HTTPS is enabled

3. **Deployment Issues**
   - Check deployment logs in App Service
   - Verify .NET runtime version
   - Check application logs in Application Insights

### Useful Commands

```bash
# View deployment logs
az webapp log tail --name app-portal-analyzer-api --resource-group rg-portal-analyzer

# Restart app service
az webapp restart --name app-portal-analyzer-api --resource-group rg-portal-analyzer

# View application settings
az webapp config appsettings list --name app-portal-analyzer-api --resource-group rg-portal-analyzer
```

## Cost Optimization

1. **Use appropriate service tiers** - Start with Basic/Standard tiers
2. **Configure auto-scaling** - Scale down during low usage
3. **Monitor storage costs** - Implement document lifecycle policies
4. **Use reserved capacity** - For predictable workloads
5. **Regular cost reviews** - Monitor Azure Cost Management

## Next Steps

1. Set up continuous monitoring with Application Insights
2. Implement automated testing in CI/CD pipeline
3. Configure staging slots for zero-downtime deployments
4. Set up Azure DevOps for advanced CI/CD features
5. Implement advanced security features like Azure AD integration