namespace AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;

/// <summary>Represents identity information for a work item field (e.g., assigned user).</summary>
public sealed record IdentityRef(
    string DisplayName,
    string Url,
    string Id,
    string UniqueName,
    string ImageUrl,
    string Descriptor);

/// <summary>Represents a link in the work item response (e.g., self, html, workItemType).</summary>
public sealed record LinkRef(string Href);

/// <summary>Represents the _links object in the work item response.</summary>
public sealed record WorkItemLinks(
    LinkRef? Self,
    LinkRef? WorkItemUpdates,
    LinkRef? WorkItemRevisions,
    LinkRef? WorkItemHistory,
    LinkRef? Html,
    LinkRef? WorkItemType,
    LinkRef? Fields);

/// <summary>Represents a relationship/link between two work items.</summary>
public sealed record WorkItemRelation(
    string Rel,
    string Url,
    int? TargetId = null);

/// <summary>Represents a work item with its related items (children, parent, etc.).</summary>
public sealed record WorkItemWithRelations(
    WorkItemResult Item,
    IReadOnlyList<WorkItemRelation>? Relations = null,
    IReadOnlyList<WorkItemResult>? RelatedItems = null);

/// <summary>Result returned for any created or updated work item, including rev, fields, and links.</summary>
public sealed record WorkItemResult(
    int Id,
    string Url,
    int? Rev = null,
    Dictionary<string, object?>? Fields = null,
    WorkItemLinks? Links = null);

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

    /// <summary>Gets a specific work item by ID.</summary>
    Task<WorkItemResult> GetWorkItemAsync(
        int id,
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

    /// <summary>Gets all related work items for a specific work item (children, parent, etc.).</summary>
    Task<WorkItemWithRelations> GetRelatedWorkItemsAsync(
        int id,
        CancellationToken cancellationToken = default);

    /// <summary>Gets child work items (tasks) linked to a parent work item.</summary>
    Task<IReadOnlyList<WorkItemResult>> GetChildWorkItemsAsync(
        int parentId,
        CancellationToken cancellationToken = default);
}
