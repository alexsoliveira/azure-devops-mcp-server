namespace AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;

/// <summary>Result returned for any created or updated work item.</summary>
public sealed record WorkItemResult(int Id, string Url);

/// <summary>Contract for work item CRUD operations against Azure DevOps.</summary>
public interface IWorkItemService
{
    /// <summary>Creates a work item of the specified type.</summary>
    Task<WorkItemResult> CreateWorkItemAsync(
        string project,
        string workItemType,
        string title,
        string description,
        int? parentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>Lists work items using a WIQL filter.</summary>
    Task<IReadOnlyList<WorkItemResult>> ListWorkItemsAsync(
        string project,
        string? workItemType = null,
        string? state = null,
        string? assignedTo = null,
        CancellationToken cancellationToken = default);

    /// <summary>Updates fields of an existing work item.</summary>
    Task<WorkItemResult> UpdateWorkItemAsync(
        int id,
        string? title = null,
        string? description = null,
        string? state = null,
        string? assignedTo = null,
        CancellationToken cancellationToken = default);

    /// <summary>Links two work items with the specified relation type.</summary>
    Task<WorkItemResult> LinkWorkItemsAsync(
        int sourceId,
        int targetId,
        string relationType,
        CancellationToken cancellationToken = default);
}
