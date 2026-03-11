using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Integration;

/// <summary>Integration test for listing Azure DevOps projects with real credentials.</summary>
public class ProjectListingIntegrationTest
{
    [Fact]
    public async Task ListProjects_WithRealCredentials_ReturnsProjects()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(b => b.AddConsole().SetMinimumLevel(LogLevel.Information));
        services.AddSingleton<TokenProvider>();
        services.AddSingleton<PermissionGuard>();
        services.AddSingleton<AzureDevOpsClient>();
        services.AddSingleton<IProjectService, ProjectService>();
        services.AddSingleton<ProjectTools>();

        var provider = services.BuildServiceProvider();
        var projectTools = provider.GetRequiredService<ProjectTools>();

        // Act
        var projects = await projectTools.ListProjects();

        // Assert
        Assert.NotNull(projects);
        Assert.NotEmpty(projects);
        
        Console.WriteLine($"\n[SUCCESS] Found {projects.Count} projects:\n");
        foreach (var project in projects)
        {
            Console.WriteLine($"  • {project.Name} (ID: {project.Id})");
            Console.WriteLine($"    URL: {project.Url}\n");
        }
    }
}
