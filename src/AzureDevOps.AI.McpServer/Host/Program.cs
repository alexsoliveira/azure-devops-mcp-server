using AzureDevOps.AI.McpServer.Application;
using AzureDevOps.AI.McpServer.Host;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Security;

var builder = WebApplication.CreateBuilder(args);

// Determine transport mode from environment
var mcpTransportMode = Environment.GetEnvironmentVariable("MCP_TRANSPORT") ?? "http";

// Logging Configuration
builder.Logging.ClearProviders();

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
}
else
{
    // In production, ensure errors are logged
    builder.Logging.AddConsole();
    builder.Logging.SetMinimumLevel(LogLevel.Information);
}

// Always log Azure DevOps errors in detail
builder.Logging.AddFilter("AzureDevOps.AI.McpServer", LogLevel.Debug);

// Security
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddSingleton<PermissionGuard>();

// Infrastructure
builder.Services.AddSingleton<AzureDevOpsClient>();
builder.Services.AddSingleton<IAzureDevOpsClient>(sp => sp.GetRequiredService<AzureDevOpsClient>());
builder.Services.AddSingleton<IWorkItemService, WorkItemService>();
builder.Services.AddSingleton<IProjectService, ProjectService>();

// Application services
builder.Services.AddSingleton<EpicGeneratorService>();
builder.Services.AddSingleton<FeatureGeneratorService>();
builder.Services.AddSingleton<TaskBreakdownService>();
builder.Services.AddSingleton<SprintPlannerService>();

// Controllers for REST API (only in HTTP mode)
if (mcpTransportMode == "http")
{
    builder.Services.AddControllers();
}

// MCP Server - Configure transport based on mode
var mcpBuilder = builder.Services.AddMcpServer();

if (mcpTransportMode == "stdio")
{
    // Use stdio transport for MCP CLI integration
    mcpBuilder.WithStdioServerTransport();
}
else if (mcpTransportMode == "http")
{
    // HTTP mode is for testing and server-based deployment (not standard MCP)
    // Note: When using HTTP, MCP protocol is via REST endpoints, not stdio
    builder.Services.AddControllers();
}

mcpBuilder.WithToolsFromAssembly();

var app = builder.Build();

// Only map HTTP endpoints if in HTTP mode
if (mcpTransportMode == "http")
{
    app.MapControllers();

    // Health check
    app.MapGet("/health", () => new { status = "ok", timestamp = DateTime.UtcNow });

    // Server info
    app.MapGet("/api/info", () => new
    {
        name = "Azure DevOps AI Agent MCP Server",
        version = "1.0.0",
        transport = "stdio (for MCP Agent) + HTTP REST (for testing)",
        status = "running"
    });
}

await app.RunAsync();
