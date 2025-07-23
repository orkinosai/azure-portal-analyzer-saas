# Web Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["AzurePortalAnalyzer.Web/AzurePortalAnalyzer.Web.csproj", "AzurePortalAnalyzer.Web/"]
COPY ["AzurePortalAnalyzer.Core/AzurePortalAnalyzer.Core.csproj", "AzurePortalAnalyzer.Core/"]
RUN dotnet restore "AzurePortalAnalyzer.Web/AzurePortalAnalyzer.Web.csproj"
COPY . .
WORKDIR "/src/AzurePortalAnalyzer.Web"
RUN dotnet build "AzurePortalAnalyzer.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AzurePortalAnalyzer.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AzurePortalAnalyzer.Web.dll"]