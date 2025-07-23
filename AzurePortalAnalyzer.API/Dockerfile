# API Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AzurePortalAnalyzer.API/AzurePortalAnalyzer.API.csproj", "AzurePortalAnalyzer.API/"]
COPY ["AzurePortalAnalyzer.Core/AzurePortalAnalyzer.Core.csproj", "AzurePortalAnalyzer.Core/"]
RUN dotnet restore "AzurePortalAnalyzer.API/AzurePortalAnalyzer.API.csproj"
COPY . .
WORKDIR "/src/AzurePortalAnalyzer.API"
RUN dotnet build "AzurePortalAnalyzer.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AzurePortalAnalyzer.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Install system dependencies for QuestPDF
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzurePortalAnalyzer.API.dll"]