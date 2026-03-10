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
        return new WorkItemResult(workItem.Id!.Value, workItem.Url);
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
        return items.Select(w => new WorkItemResult(w.Id!.Value, w.Url)).ToList();
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
        return new WorkItemResult(workItem.Id!.Value, workItem.Url);
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
        return new WorkItemResult(workItem.Id!.Value, workItem.Url);
    }
}
