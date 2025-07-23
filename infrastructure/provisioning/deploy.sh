#!/bin/bash

# Azure Portal Analyzer SaaS - Infrastructure Deployment Script
# This script deploys the Azure infrastructure using Bicep templates

set -e  # Exit on any error

# Configuration
RESOURCE_GROUP_NAME="azure-portal-analyzer-saas-rg"
LOCATION="East US"
TEMPLATE_FILE="main.bicep"
PARAMETERS_FILE="main.parameters.json"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if Azure CLI is installed
check_azure_cli() {
    if ! command -v az &> /dev/null; then
        print_error "Azure CLI is not installed. Please install it first."
        print_error "Visit: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli"
        exit 1
    fi
    print_success "Azure CLI is installed"
}

# Function to check if user is logged in to Azure
check_azure_login() {
    if ! az account show &> /dev/null; then
        print_error "You are not logged in to Azure. Please run 'az login' first."
        exit 1
    fi
    
    local account_name=$(az account show --query name -o tsv)
    print_success "Logged in to Azure account: $account_name"
}

# Function to create resource group if it doesn't exist
create_resource_group() {
    print_status "Checking if resource group '$RESOURCE_GROUP_NAME' exists..."
    
    if az group show --name "$RESOURCE_GROUP_NAME" &> /dev/null; then
        print_warning "Resource group '$RESOURCE_GROUP_NAME' already exists"
    else
        print_status "Creating resource group '$RESOURCE_GROUP_NAME' in '$LOCATION'..."
        az group create --name "$RESOURCE_GROUP_NAME" --location "$LOCATION"
        print_success "Resource group created successfully"
    fi
}

# Function to validate Bicep template
validate_template() {
    print_status "Validating Bicep template..."
    
    if az deployment group validate \
        --resource-group "$RESOURCE_GROUP_NAME" \
        --template-file "$TEMPLATE_FILE" \
        --parameters "$PARAMETERS_FILE" &> /dev/null; then
        print_success "Template validation passed"
    else
        print_error "Template validation failed"
        az deployment group validate \
            --resource-group "$RESOURCE_GROUP_NAME" \
            --template-file "$TEMPLATE_FILE" \
            --parameters "$PARAMETERS_FILE"
        exit 1
    fi
}

# Function to deploy the infrastructure
deploy_infrastructure() {
    print_status "Deploying infrastructure..."
    
    local deployment_name="azure-portal-analyzer-deployment-$(date +%Y%m%d-%H%M%S)"
    
    print_status "Deployment name: $deployment_name"
    
    if az deployment group create \
        --resource-group "$RESOURCE_GROUP_NAME" \
        --name "$deployment_name" \
        --template-file "$TEMPLATE_FILE" \
        --parameters "$PARAMETERS_FILE" \
        --verbose; then
        print_success "Infrastructure deployment completed successfully"
        return 0
    else
        print_error "Infrastructure deployment failed"
        return 1
    fi
}

# Function to retrieve deployment outputs
get_deployment_outputs() {
    print_status "Retrieving deployment outputs..."
    
    local latest_deployment=$(az deployment group list \
        --resource-group "$RESOURCE_GROUP_NAME" \
        --query "[0].name" -o tsv)
    
    if [ -n "$latest_deployment" ]; then
        print_status "Latest deployment: $latest_deployment"
        
        echo ""
        echo "=== Deployment Outputs ==="
        az deployment group show \
            --resource-group "$RESOURCE_GROUP_NAME" \
            --name "$latest_deployment" \
            --query properties.outputs \
            --output table
        echo ""
        
        # Save outputs to file for easy access
        az deployment group show \
            --resource-group "$RESOURCE_GROUP_NAME" \
            --name "$latest_deployment" \
            --query properties.outputs > deployment-outputs.json
        
        print_success "Deployment outputs saved to deployment-outputs.json"
    else
        print_warning "No deployments found"
    fi
}

# Function to display configuration instructions
show_configuration_instructions() {
    echo ""
    echo "=== Next Steps ==="
    echo "1. Update your appsettings.Development.json files with the values from deployment-outputs.json"
    echo "2. Set up your environment variables or configuration as needed"
    echo "3. Test the local application setup"
    echo "4. Deploy your application code to the created Azure resources"
    echo ""
    echo "Configuration files to update:"
    echo "- src/Api/AzurePortalAnalyzer.Api/appsettings.Development.json"
    echo "- src/Web/AzurePortalAnalyzer.Web/appsettings.Development.json"
    echo ""
}

# Main deployment function
main() {
    echo "=================================="
    echo "Azure Portal Analyzer SaaS"
    echo "Infrastructure Deployment Script"
    echo "=================================="
    echo ""
    
    # Change to the directory containing the script
    cd "$(dirname "$0")"
    
    # Check prerequisites
    check_azure_cli
    check_azure_login
    
    # Verify template files exist
    if [[ ! -f "$TEMPLATE_FILE" ]]; then
        print_error "Template file '$TEMPLATE_FILE' not found"
        exit 1
    fi
    
    if [[ ! -f "$PARAMETERS_FILE" ]]; then
        print_error "Parameters file '$PARAMETERS_FILE' not found"
        exit 1
    fi
    
    print_status "Using template: $TEMPLATE_FILE"
    print_status "Using parameters: $PARAMETERS_FILE"
    print_status "Target resource group: $RESOURCE_GROUP_NAME"
    print_status "Location: $LOCATION"
    echo ""
    
    # Deployment steps
    create_resource_group
    validate_template
    
    if deploy_infrastructure; then
        get_deployment_outputs
        show_configuration_instructions
        print_success "Deployment completed successfully!"
    else
        print_error "Deployment failed. Check the error messages above."
        exit 1
    fi
}

# Help function
show_help() {
    echo "Usage: $0 [OPTIONS]"
    echo ""
    echo "Deploy Azure Portal Analyzer SaaS infrastructure"
    echo ""
    echo "Options:"
    echo "  -h, --help     Show this help message"
    echo "  -r, --rg       Resource group name (default: $RESOURCE_GROUP_NAME)"
    echo "  -l, --location Location (default: $LOCATION)"
    echo ""
    echo "Examples:"
    echo "  $0                                    # Deploy with default settings"
    echo "  $0 -r my-rg -l 'West US 2'          # Deploy to custom resource group and location"
    echo ""
}

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -h|--help)
            show_help
            exit 0
            ;;
        -r|--rg)
            RESOURCE_GROUP_NAME="$2"
            shift 2
            ;;
        -l|--location)
            LOCATION="$2"
            shift 2
            ;;
        *)
            print_error "Unknown parameter: $1"
            show_help
            exit 1
            ;;
    esac
done

# Run main function
main