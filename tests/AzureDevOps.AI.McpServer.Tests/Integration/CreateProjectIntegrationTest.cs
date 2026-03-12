using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Integration;

/// <summary>Integration test for CreateProject functionality.</summary>
public class CreateProjectIntegrationTest
{
    [Fact(Skip = "Requires valid Azure DevOps credentials - set AZURE_DEVOPS_PAT and AZURE_DEVOPS_ORG environment variables")]
    public async Task CreateProject_DebugErrorResponse()
    {
        var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        var org = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG");

        Assert.NotNull(pat);
        Assert.NotNull(org);

        // Setup DI with verbose logging
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
        var logger = serviceProvider.GetRequiredService<ILogger<CreateProjectIntegrationTest>>();

        logger.LogInformation("=== CreateProject Debug Test ===");
        logger.LogInformation("Organization: {Org}", org);
        logger.LogInformation("PAT Length: {Length} chars", pat?.Length ?? 0);

        var projectName = $"TestProject_{Guid.NewGuid().ToString("N")[..8]}";

        try
        {
            logger.LogInformation("Creating project: {Name}", projectName);
            var result = await projectService.CreateProjectAsync(projectName, "Test Description");
            logger.LogInformation("✅ Success! Project ID: {Id}, URL: {Url}", result.Id, result.Url);

            Assert.NotNull(result);
            Assert.Equal(projectName, result.Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Exception ({Type}): {Message}", ex.GetType().Name, ex.Message);
            if (ex.InnerException != null)
            {
                logger.LogError(ex.InnerException, "Inner Exception: {Message}", ex.InnerException.Message);
            }
            throw;
        }
    }
}

