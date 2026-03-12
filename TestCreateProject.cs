using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Load environment variables from .env if it exists
var envFile = ".env";
if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            continue;

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            Environment.SetEnvironmentVariable(parts[0].Trim(), parts[1].Trim());
        }
    }
}

var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
var org = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG");

if (string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(org))
{
    Console.WriteLine("❌ Missing environment variables:");
    Console.WriteLine($"   AZURE_DEVOPS_PAT: {(string.IsNullOrEmpty(pat) ? "NOT SET" : "SET")}");
    Console.WriteLine($"   AZURE_DEVOPS_ORG: {(string.IsNullOrEmpty(org) ? "NOT SET" : "SET")}");
    return;
}

// Setup DI
var services = new ServiceCollection();
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});

services.AddSingleton<TokenProvider>();
services.AddSingleton<AzureDevOpsClient>();
services.AddSingleton<IAzureDevOpsClient>(sp => sp.GetRequiredService<AzureDevOpsClient>());
services.AddSingleton<IProjectService, ProjectService>();

var serviceProvider = services.BuildServiceProvider();
var projectService = serviceProvider.GetRequiredService<IProjectService>();
var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

logger.LogInformation("=== CreateProject Debug Test ===");
logger.LogInformation("Organization: {Org}", org);
logger.LogInformation("PAT Length: {Length} chars", pat?.Length ?? 0);

var projectName = $"TestProject_{Guid.NewGuid().ToString("N")[..8]}";

try
{
    logger.LogInformation("Creating project: {Name}...", projectName);
    var result = await projectService.CreateProjectAsync(projectName, "Test Description - Created by MCP Debug Tool");
    logger.LogInformation("✅ SUCCESS! Project created:");
    logger.LogInformation("   ID: {Id}", result.Id);
    logger.LogInformation("   Name: {Name}", result.Name);
    logger.LogInformation("   URL: {Url}", result.Url);
    Console.WriteLine("\n✅ Project creation succeeded!");
}
catch (Exception ex)
{
    logger.LogError(ex, "❌ FAILED to create project");
    logger.LogError("Exception Type: {Type}", ex.GetType().Name);
    logger.LogError("Message: {Message}", ex.Message);
    
    if (ex.InnerException != null)
    {
        logger.LogError("Inner Exception Type: {Type}", ex.InnerException.GetType().Name);
        logger.LogError("Inner Message: {Message}", ex.InnerException.Message);
    }
    
    Console.WriteLine("\n❌ Project creation FAILED!");
    Console.WriteLine($"Exception: {ex.GetType().Name}");
    Console.WriteLine($"Message: {ex.Message}");
}
