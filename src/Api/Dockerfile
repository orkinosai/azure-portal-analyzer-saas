# Use the official .NET 8 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ["AzurePortalAnalyzer.sln", "."]

# Copy project files
COPY ["src/Api/AzurePortalAnalyzer.Api/AzurePortalAnalyzer.Api.csproj", "src/Api/AzurePortalAnalyzer.Api/"]
COPY ["src/Shared/AzurePortalAnalyzer.Shared/AzurePortalAnalyzer.Shared.csproj", "src/Shared/AzurePortalAnalyzer.Shared/"]
COPY ["src/AI/AzurePortalAnalyzer.AI/AzurePortalAnalyzer.AI.csproj", "src/AI/AzurePortalAnalyzer.AI/"]

# Restore dependencies
RUN dotnet restore "src/Api/AzurePortalAnalyzer.Api/AzurePortalAnalyzer.Api.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/src/Api/AzurePortalAnalyzer.Api"
RUN dotnet build "AzurePortalAnalyzer.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AzurePortalAnalyzer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' dotnetuser && chown -R dotnetuser /app
USER dotnetuser

ENTRYPOINT ["dotnet", "AzurePortalAnalyzer.Api.dll"]