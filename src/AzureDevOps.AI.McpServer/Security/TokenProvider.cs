namespace AzureDevOps.AI.McpServer.Security;

/// <summary>Provides the Azure DevOps PAT and organization URL from environment variables.</summary>
public sealed class TokenProvider
{
    /// <summary>Personal Access Token loaded from the <c>AZURE_DEVOPS_PAT</c> environment variable.</summary>
    public string PersonalAccessToken { get; }

    /// <summary>Organization URL loaded from the <c>AZURE_DEVOPS_ORG</c> environment variable.</summary>
    public string OrganizationUrl { get; }

    public TokenProvider()
    {
        PersonalAccessToken = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT")
            ?? throw new InvalidOperationException("Environment variable 'AZURE_DEVOPS_PAT' is not set.");

        OrganizationUrl = Environment.GetEnvironmentVariable("AZURE_DEVOPS_ORG")
            ?? throw new InvalidOperationException("Environment variable 'AZURE_DEVOPS_ORG' is not set.");
    }
}
