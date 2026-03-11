# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/AzureDevOps.AI.McpServer/AzureDevOps.AI.McpServer.csproj ./AzureDevOps.AI.McpServer/
RUN dotnet restore ./AzureDevOps.AI.McpServer/AzureDevOps.AI.McpServer.csproj

COPY src/AzureDevOps.AI.McpServer/ ./AzureDevOps.AI.McpServer/
RUN dotnet publish ./AzureDevOps.AI.McpServer/AzureDevOps.AI.McpServer.csproj \
    -c Release \
    -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Required environment variables (pass at runtime):
# AZURE_DEVOPS_PAT
# AZURE_DEVOPS_ORG
# AZURE_DEVOPS_PROJECT

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5050

ENTRYPOINT ["dotnet", "AzureDevOps.AI.McpServer.dll"]
