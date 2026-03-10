using AzureDevOps.AI.McpServer.Application;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

// Logging
builder.Logging.AddConsole();

// Security
builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddSingleton<PermissionGuard>();

// Infrastructure
builder.Services.AddSingleton<AzureDevOpsClient>();
builder.Services.AddSingleton<IWorkItemService, WorkItemService>();
builder.Services.AddSingleton<IProjectService, ProjectService>();

// Application services
builder.Services.AddSingleton<EpicGeneratorService>();
builder.Services.AddSingleton<FeatureGeneratorService>();
builder.Services.AddSingleton<TaskBreakdownService>();
builder.Services.AddSingleton<SprintPlannerService>();

// MCP Server — register all tool types
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();
