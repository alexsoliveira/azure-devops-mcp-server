using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.VisualStudio.Services.WebApi;
using AzureDevOps.AI.McpServer.Security;

namespace AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;

/// <summary>Manages the VssConnection and typed client access to Azure DevOps REST APIs.</summary>
public sealed class AzureDevOpsClient : IAzureDevOpsClient
{
    private readonly VssConnection _connection;

    public string OrganizationUrl { get; }

    public AzureDevOpsClient(TokenProvider tokenProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenProvider);
        OrganizationUrl = tokenProvider.OrganizationUrl ?? throw new ArgumentNullException(nameof(tokenProvider.OrganizationUrl));
        var credentials = new VssBasicCredential(string.Empty, tokenProvider.PersonalAccessToken);
        _connection = new VssConnection(new Uri(OrganizationUrl), credentials);
    }

    public async Task<WorkItemTrackingHttpClient> GetWorkItemTrackingClientAsync()
        => await _connection.GetClientAsync<WorkItemTrackingHttpClient>();

    public async Task<ProjectHttpClient> GetProjectClientAsync()
        => await _connection.GetClientAsync<ProjectHttpClient>();

    public async Task<OperationsHttpClient> GetOperationsClientAsync()
        => await _connection.GetClientAsync<OperationsHttpClient>();

    public async ValueTask DisposeAsync()
        => await Task.Run(_connection.Disconnect);
}
