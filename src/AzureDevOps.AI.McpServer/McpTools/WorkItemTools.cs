using ModelContextProtocol.Server;
using System.ComponentModel;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.McpTools;

/// <summary>MCP tools for listing, updating, and linking work items in Azure DevOps.</summary>
[McpServerToolType]
public sealed class WorkItemTools(IWorkItemService workItemService, ILogger<WorkItemTools> logger)
{
    /// <summary>Lists work items in a project with optional filters.</summary>
    [McpServerTool, Description("Lists work items in an Azure DevOps project with optional type, state and assignee filters.")]
    public async Task<IReadOnlyList<WorkItemResult>> ListWorkItems(
        [Description("Target Azure DevOps project name")] string project,
        [Description("Work item type filter (e.g. Epic, Feature, User Story, Task)")] string? workItemType = null,
        [Description("State filter (e.g. Active, Closed, New)")] string? state = null,
        [Description("Assigned-to filter (display name or email)")] string? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_workitem_list invoked for project '{Project}'", project);
        return await workItemService.ListWorkItemsAsync(project, workItemType, state, assignedTo, cancellationToken);
    }

    /// <summary>Gets a specific work item by ID.</summary>
    [McpServerTool, Description("Gets details of a specific Azure DevOps work item by ID.")]
    public async Task<WorkItemResult> GetWorkItem(
        [Description("Work item ID to retrieve")] int id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_workitem_get invoked for ID {Id}", id);
        var result = await workItemService.GetWorkItemAsync(id, cancellationToken);
        logger.LogInformation("Work item {Id} retrieved", id);
        return result;
    }

    /// <summary>Updates one or more fields of an existing work item.</summary>
    [McpServerTool, Description("Updates fields of an existing Azure DevOps work item.")]
    public async Task<WorkItemResult> UpdateWorkItem(
        [Description("Work item ID to update")] int id,
        [Description("New title (optional)")] string? title = null,
        [Description("New description (optional)")] string? description = null,
        [Description("New state (optional, e.g. Active, Closed)")] string? state = null,
        [Description("Assign to user (optional, display name or email)")] string? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_workitem_update invoked for ID {Id}", id);
        var result = await workItemService.UpdateWorkItemAsync(id, title, description, state, assignedTo, cancellationToken);
        logger.LogInformation("Work item {Id} updated", id);
        return result;
    }

    /// <summary>Links two work items using a specified relation type.</summary>
    [McpServerTool, Description("Creates a link between two Azure DevOps work items.")]
    public async Task<WorkItemResult> LinkWorkItems(
        [Description("Source work item ID")] int sourceId,
        [Description("Target work item ID")] int targetId,
        [Description("Relation type (e.g. System.LinkTypes.Hierarchy-Forward, System.LinkTypes.Related)")] string relationType = "System.LinkTypes.Related",
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_workitem_link invoked: {SourceId} -> {TargetId}", sourceId, targetId);
        var result = await workItemService.LinkWorkItemsAsync(sourceId, targetId, relationType, cancellationToken);
        logger.LogInformation("Work items {SourceId} and {TargetId} linked", sourceId, targetId);
        return result;
    }

    /// <summary>Gets all related work items (children, parent, etc.) for a specific work item.</summary>
    [McpServerTool, Description("Gets all related work items (children, parent, related items) for a specific work item by ID.")]
    public async Task<WorkItemWithRelations> GetRelatedWorkItems(
        [Description("Work item ID to get relations for")] int id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_workitem_get_related invoked for ID {Id}", id);
        var result = await workItemService.GetRelatedWorkItemsAsync(id, cancellationToken);
        logger.LogInformation("Retrieved {Count} related work items for {Id}", result.RelatedItems?.Count ?? 0, id);
        return result;
    }

    /// <summary>Gets child work items (tasks) linked to a parent work item.</summary>
    [McpServerTool, Description("Gets child work items (tasks) linked to a parent work item. Filters for Hierarchy-Forward relations.")]
    public async Task<IReadOnlyList<WorkItemResult>> GetChildWorkItems(
        [Description("Parent work item ID")] int parentId,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_workitem_get_children invoked for parent {ParentId}", parentId);
        var result = await workItemService.GetChildWorkItemsAsync(parentId, cancellationToken);
        logger.LogInformation("Retrieved {Count} child work items for parent {ParentId}", result.Count, parentId);
        return result;
    }
}
