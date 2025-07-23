# CI/CD Pipeline Configurations

This directory contains configuration files for different CI/CD systems that can be used with the Azure Portal Analyzer SaaS project.

## GitHub Actions (Primary)

The primary CI/CD system is GitHub Actions, with workflows located in `.github/workflows/`:

- `ci-cd.yml` - Main CI/CD pipeline for build, test, and deployment
- `pr-validation.yml` - Pull request validation and checks

## Azure DevOps (Alternative)

For teams using Azure DevOps, pipeline configurations will be provided here:

### Azure Pipelines YAML

```yaml
# azure-pipelines.yml (example structure)
trigger:
  branches:
    include:
    - main
    - develop

pool:
  vmImage: 'ubuntu-latest'

variables:
  buildConfiguration: 'Release'
  dotNetVersion: '8.0.x'

stages:
- stage: Build
  jobs:
  - job: BuildAndTest
    steps:
    - task: UseDotNet@2
      inputs:
        version: $(dotNetVersion)
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
    - task: DotNetCoreCLI@2
      displayName: 'Build solution'
      inputs:
        command: 'build'
        configuration: $(buildConfiguration)
    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        configuration: $(buildConfiguration)

- stage: Deploy
  condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
  jobs:
  - deployment: DeployToAzure
    environment: 'production'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureRmWebAppDeployment@4
            inputs:
              azureSubscription: 'Azure Subscription'
              appType: 'webAppContainer'
              WebAppName: 'azure-portal-analyzer'
```

## Jenkins (Alternative)

For teams using Jenkins:

### Jenkinsfile

```groovy
pipeline {
    agent any
    
    environment {
        DOTNET_VERSION = '8.0'
        BUILD_CONFIGURATION = 'Release'
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
        stage('Restore') {
            steps {
                sh 'dotnet restore'
            }
        }
        
        stage('Build') {
            steps {
                sh "dotnet build --configuration ${BUILD_CONFIGURATION} --no-restore"
            }
        }
        
        stage('Test') {
            steps {
                sh "dotnet test --configuration ${BUILD_CONFIGURATION} --no-build --verbosity normal"
            }
        }
        
        stage('Publish') {
            when {
                branch 'main'
            }
            steps {
                sh "dotnet publish --configuration ${BUILD_CONFIGURATION} --no-build --output ./publish"
            }
        }
        
        stage('Deploy') {
            when {
                branch 'main'
            }
            steps {
                // Deployment steps would go here
                echo 'Deploying to Azure...'
            }
        }
    }
    
    post {
        always {
            cleanWs()
        }
    }
}
```

## GitLab CI (Alternative)

For teams using GitLab:

### .gitlab-ci.yml

```yaml
image: mcr.microsoft.com/dotnet/sdk:8.0

variables:
  DOTNET_VERSION: "8.0"
  BUILD_CONFIGURATION: "Release"

stages:
  - build
  - test
  - deploy

before_script:
  - dotnet --version

build:
  stage: build
  script:
    - dotnet restore
    - dotnet build --configuration $BUILD_CONFIGURATION --no-restore
  artifacts:
    paths:
      - "*/bin/"
      - "*/obj/"
    expire_in: 1 hour

test:
  stage: test
  script:
    - dotnet test --configuration $BUILD_CONFIGURATION --no-build --verbosity normal
  dependencies:
    - build

deploy:
  stage: deploy
  script:
    - echo "Deploying to Azure..."
    # Deployment scripts would go here
  only:
    - main
  dependencies:
    - build
    - test
```

## Configuration Notes

### Required Secrets/Variables

All CI/CD systems will need the following secrets/variables configured:

- `AZURE_CREDENTIALS` - Azure service principal for deployment
- `ACR_LOGIN_SERVER` - Azure Container Registry URL
- `ACR_USERNAME` - Azure Container Registry username
- `ACR_PASSWORD` - Azure Container Registry password
- `AZURE_RESOURCE_GROUP` - Target Azure resource group
- `CODECOV_TOKEN` - Code coverage reporting token (if using Codecov)

### Environment-Specific Configuration

Each CI/CD system should support multiple environments:

- **Development** - Automatic deployment from feature branches
- **Staging** - Automatic deployment from develop branch
- **Production** - Manual approval required for main branch

### Deployment Strategies

Supported deployment strategies:
- **Blue-Green Deployment** - Zero downtime deployments
- **Rolling Deployment** - Gradual rollout
- **Canary Deployment** - Risk mitigation through gradual rollout

## Choosing Your CI/CD System

1. **GitHub Actions** (Recommended)
   - Native integration with GitHub
   - Free for public repositories
   - Extensive marketplace of actions

2. **Azure DevOps**
   - Tight integration with Azure services
   - Enterprise features and reporting
   - Free tier available

3. **Jenkins**
   - Self-hosted option
   - Extensive plugin ecosystem
   - Full control over environment

4. **GitLab CI**
   - Integrated with GitLab repositories
   - Built-in container registry
   - Comprehensive DevOps platform