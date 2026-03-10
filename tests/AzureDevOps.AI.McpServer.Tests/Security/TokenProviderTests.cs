using AzureDevOps.AI.McpServer.Security;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Security;

public class TokenProviderTests
{
    [Fact]
    public void Constructor_WithValidEnvironmentVariables_LoadsTokenAndOrganizationUrl()
    {
        // Arrange
        var testToken = "test-pat-token-12345";
        var testOrg = "https://dev.azure.com/test-org";
        
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", testToken, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", testOrg, EnvironmentVariableTarget.Process);

        // Act
        var provider = new TokenProvider();

        // Assert
        Assert.Equal(testToken, provider.PersonalAccessToken);
        Assert.Equal(testOrg, provider.OrganizationUrl);
    }

    [Fact]
    public void Constructor_WithoutPatToken_ThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", null, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", "https://dev.azure.com/test-org", EnvironmentVariableTarget.Process);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new TokenProvider());
        Assert.Contains("AZURE_DEVOPS_PAT", exception.Message);
    }

    [Fact]
    public void Constructor_WithBothEnvironmentVariablesEmpty_ThrowsInvalidOperationException()
    {
        // Arrange
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_PAT", null, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("AZURE_DEVOPS_ORG", null, EnvironmentVariableTarget.Process);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => new TokenProvider());
        Assert.Contains("AZURE_DEVOPS_PAT", exception.Message);
    }
}
