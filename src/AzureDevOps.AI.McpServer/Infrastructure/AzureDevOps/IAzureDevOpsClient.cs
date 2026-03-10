using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Operations;

namespace AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;

/// <summary>Interface for Azure DevOps client operations.</summary>
public interface IAzureDevOpsClient : IAsyncDisposable
{
    /// <summary>Gets the organization URL.</summary>
    string OrganizationUrl { get; }

    /// <summary>Gets the work item tracking HTTP client.</summary>
    Task<WorkItemTrackingHttpClient> GetWorkItemTrackingClientAsync();

    /// <summary>Gets the project HTTP client.</summary>
    Task<ProjectHttpClient> GetProjectClientAsync();

    /// <summary>Gets the operations HTTP client.</summary>
    Task<OperationsHttpClient> GetOperationsClientAsync();
}
