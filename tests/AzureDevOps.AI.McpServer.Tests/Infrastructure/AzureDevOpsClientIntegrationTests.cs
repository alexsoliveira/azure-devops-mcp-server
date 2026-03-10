using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.Security;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Infrastructure;

/// <summary>Integration tests for AzureDevOpsClient that require Azure DevOps connectivity.</summary>
/// <remarks>These tests are skipped in CI/CD unless AZURE_DEVOPS_PAT is configured.</remarks>
public class AzureDevOpsClientIntegrationTests
{
    [Fact(Skip = "Integration test - requires AZURE_DEVOPS_PAT environment variable")]
    public async Task GetProjectClientAsync_WithRealConnection_ShouldSucceed()
    {
        // Only runs if explicitly enabled with proper Azure DevOps credentials
        var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        var org = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG");

        if (string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(org))
            return; // Skip silently

        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", pat);
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", org);

        var tokenProvider = new TokenProvider();
        await using var client = new AzureDevOpsClient(tokenProvider);

        // Act
        var projectClient = await client.GetProjectClientAsync();

        // Assert
        Assert.NotNull(projectClient);
    }

    [Fact(Skip = "Integration test - requires AZURE_DEVOPS_PAT environment variable")]
    public async Task GetOperationsClientAsync_WithRealConnection_ShouldSucceed()
    {
        var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        var org = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG");

        if (string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(org))
            return; // Skip silently

        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", pat);
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", org);

        var tokenProvider = new TokenProvider();
        await using var client = new AzureDevOpsClient(tokenProvider);

        // Act
        var opsClient = await client.GetOperationsClientAsync();

        // Assert
        Assert.NotNull(opsClient);
    }

    [Fact(Skip = "Integration test - requires AZURE_DEVOPS_PAT environment variable")]
    public async Task GetWorkItemTrackingClientAsync_WithRealConnection_ShouldSucceed()
    {
        var pat = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        var org = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG");

        if (string.IsNullOrEmpty(pat) || string.IsNullOrEmpty(org))
            return; // Skip silently

        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", pat);
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", org);

        var tokenProvider = new TokenProvider();
        await using var client = new AzureDevOpsClient(tokenProvider);

        // Act
        var witClient = await client.GetWorkItemTrackingClientAsync();

        // Assert
        Assert.NotNull(witClient);
    }
}
