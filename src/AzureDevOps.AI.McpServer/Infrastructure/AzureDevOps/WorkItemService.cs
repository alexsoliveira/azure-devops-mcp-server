using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;

/// <summary>Azure DevOps implementation of <see cref="IWorkItemService"/>.</summary>
internal sealed class WorkItemService(IAzureDevOpsClient client, ILogger<WorkItemService> logger) : IWorkItemService
{
    private readonly IAzureDevOpsClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly ILogger<WorkItemService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Converts a WorkItem from the SDK to our WorkItemResult with full details.</summary>
    private static WorkItemResult ConvertToResult(WorkItem workItem)
    {
        var fields = new Dictionary<string, object?>();
        if (workItem.Fields != null)
        {
            foreach (var kvp in workItem.Fields)
            {
                fields[kvp.Key] = kvp.Value;
            }
        }

        var links = BuildLinksFromWorkItem(workItem);

        return new WorkItemResult(
            Id: workItem.Id!.Value,
            Url: workItem.Url,
            Rev: workItem.Rev,
            Fields: fields.Count > 0 ? fields : null,
            Links: links);
    }

    /// <summary>Extracts link references from the work item URL.</summary>
    private static WorkItemLinks? BuildLinksFromWorkItem(WorkItem workItem)
    {
        if (string.IsNullOrEmpty(workItem.Url))
            return null;

        // Construct standard links based on work item URL
        var self = new LinkRef(workItem.Url);
        var html = new LinkRef(workItem.Url.Replace("_apis/wit/workItems", "web/wi.aspx?pcguid=&id"));
        var updates = new LinkRef($"{workItem.Url}/updates");
        var revisions = new LinkRef($"{workItem.Url}/revisions");
        var history = new LinkRef($"{workItem.Url}/history");

        return new WorkItemLinks(
            Self: self,
            WorkItemUpdates: updates,
            WorkItemRevisions: revisions,
            WorkItemHistory: history,
            Html: html,
            WorkItemType: null,
            Fields: null);
    }

    public async Task<WorkItemResult> CreateWorkItemAsync(
        string project,
        string workItemType,
        string title,
        string description,
        int? parentId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating {WorkItemType} '{Title}' in project '{Project}'", workItemType, title, project);

        var witClient = await _client.GetWorkItemTrackingClientAsync();
        var patch = new JsonPatchDocument
        {
            new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Title", Value = title },
            new JsonPatchOperation { Operation = Operation.Add, Path = "/fields/System.Description", Value = description }
        };

        if (parentId.HasValue)
        {
            var orgUrl = _client.OrganizationUrl;
            patch.Add(new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/relations/-",
                Value = new
                {
                    rel = "System.LinkTypes.Hierarchy-Reverse",
                    url = $"{orgUrl}/_apis/wit/workItems/{parentId.Value}"
                }
            });
        }

        var workItem = await witClient.CreateWorkItemAsync(patch, project, workItemType, cancellationToken: cancellationToken);

        _logger.LogInformation("{WorkItemType} created with ID {Id}", workItemType, workItem.Id);
        return ConvertToResult(workItem);
    }

    public async Task<WorkItemResult> GetWorkItemAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting work item {Id}", id);

        var witClient = await _client.GetWorkItemTrackingClientAsync();
        var workItem = await witClient.GetWorkItemAsync(id, cancellationToken: cancellationToken);

        _logger.LogInformation("Work item {Id} retrieved", id);
        return ConvertToResult(workItem);
    }

    public async Task<IReadOnlyList<WorkItemResult>> ListWorkItemsAsync(
        string project,
        string? workItemType = null,
        string? state = null,
        string? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        var witClient = await _client.GetWorkItemTrackingClientAsync();

        var conditions = new List<string> { $"[System.TeamProject] = '{project}'" };
        if (!string.IsNullOrWhiteSpace(workItemType))
            conditions.Add($"[System.WorkItemType] = '{workItemType}'");
        if (!string.IsNullOrWhiteSpace(state))
            conditions.Add($"[System.State] = '{state}'");
        if (!string.IsNullOrWhiteSpace(assignedTo))
            conditions.Add($"[System.AssignedTo] = '{assignedTo}'");

        var wiql = new Wiql { Query = $"SELECT [System.Id] FROM WorkItems WHERE {string.Join(" AND ", conditions)}" };
        var result = await witClient.QueryByWiqlAsync(wiql, cancellationToken: cancellationToken);

        var ids = result.WorkItems.Select(w => w.Id).ToArray();
        if (ids.Length == 0) return [];

        var items = await witClient.GetWorkItemsAsync(ids, cancellationToken: cancellationToken);
        return items.Select(ConvertToResult).ToList();
    }

    public async Task<WorkItemResult> UpdateWorkItemAsync(
        int id,
        string? title = null,
        string? description = null,
        string? state = null,
        string? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating work item {Id}", id);

        var witClient = await _client.GetWorkItemTrackingClientAsync();
        var patch = new JsonPatchDocument();

        if (title is not null)
            patch.Add(new JsonPatchOperation { Operation = Operation.Replace, Path = "/fields/System.Title", Value = title });
        if (description is not null)
            patch.Add(new JsonPatchOperation { Operation = Operation.Replace, Path = "/fields/System.Description", Value = description });
        if (state is not null)
            patch.Add(new JsonPatchOperation { Operation = Operation.Replace, Path = "/fields/System.State", Value = state });
        if (assignedTo is not null)
            patch.Add(new JsonPatchOperation { Operation = Operation.Replace, Path = "/fields/System.AssignedTo", Value = assignedTo });

        var workItem = await witClient.UpdateWorkItemAsync(patch, id, cancellationToken: cancellationToken);

        _logger.LogInformation("Work item {Id} updated", id);
        return ConvertToResult(workItem);
    }

    public async Task<WorkItemResult> LinkWorkItemsAsync(
        int sourceId,
        int targetId,
        string relationType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Linking work item {SourceId} -> {TargetId} ({RelationType})", sourceId, targetId, relationType);

        var witClient = await _client.GetWorkItemTrackingClientAsync();
        var orgUrl = _client.OrganizationUrl;

        var patch = new JsonPatchDocument
        {
            new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/relations/-",
                Value = new
                {
                    rel = relationType,
                    url = $"{orgUrl}/_apis/wit/workItems/{targetId}"
                }
            }
        };

        var workItem = await witClient.UpdateWorkItemAsync(patch, sourceId, cancellationToken: cancellationToken);

        _logger.LogInformation("Work items {SourceId} and {TargetId} linked", sourceId, targetId);
        return ConvertToResult(workItem);
    }

    public async Task<WorkItemWithRelations> GetRelatedWorkItemsAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting related work items for {Id}", id);

        var witClient = await _client.GetWorkItemTrackingClientAsync();
        var workItem = await witClient.GetWorkItemAsync(id, expand: WorkItemExpand.Relations, cancellationToken: cancellationToken);

        var relations = ExtractRelations(workItem);
        var workItemResult = ConvertToResult(workItem);

        // Extract related work item IDs and fetch them
        var relatedIds = relations
            .Where(r => r.TargetId.HasValue)
            .Select(r => r.TargetId.Value)
            .Distinct()
            .ToList();

        IReadOnlyList<WorkItemResult> relatedItems = [];
        if (relatedIds.Count > 0)
        {
            try
            {
                var items = await witClient.GetWorkItemsAsync(relatedIds, cancellationToken: cancellationToken);
                relatedItems = items.Select(ConvertToResult).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch related work items for {Id}", id);
            }
        }

        _logger.LogInformation("Retrieved {Count} related work items for {Id}", relatedItems.Count, id);
        return new WorkItemWithRelations(workItemResult, relations, relatedItems);
    }

    public async Task<IReadOnlyList<WorkItemResult>> GetChildWorkItemsAsync(
        int parentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting child work items for parent {ParentId}", parentId);

        var workItemWithRelations = await GetRelatedWorkItemsAsync(parentId, cancellationToken);

        // Filter for child relationships (Hierarchy-Forward relations are children)
        var childIds = workItemWithRelations.Relations?
            .Where(r => r.Rel == "System.LinkTypes.Hierarchy-Forward" && r.TargetId.HasValue)
            .Select(r => r.TargetId.Value)
            .ToList() ?? [];

        var children = workItemWithRelations.RelatedItems?
            .Where(r => childIds.Contains(r.Id))
            .ToList() ?? [];

        _logger.LogInformation("Found {Count} child work items for parent {ParentId}", children.Count, parentId);
        return children;
    }

    /// <summary>Extracts relations from a work item.</summary>
    private static List<WorkItemRelation> ExtractRelations(WorkItem workItem)
    {
        var relations = new List<WorkItemRelation>();

        if (workItem.Relations == null)
            return relations;

        foreach (var relation in workItem.Relations)
        {
            var targetId = ExtractIdFromUrl(relation.Url);
            relations.Add(new WorkItemRelation(
                Rel: relation.Rel,
                Url: relation.Url,
                TargetId: targetId));
        }

        return relations;
    }

    /// <summary>Extracts work item ID from Azure DevOps API URL.</summary>
    private static int? ExtractIdFromUrl(string url)
    {
        // URL format: https://dev.azure.com/.../workItems/123
        if (string.IsNullOrEmpty(url))
            return null;

        var segments = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length > 0 && int.TryParse(segments[^1], out var id))
            return id;

        return null;
    }
}
