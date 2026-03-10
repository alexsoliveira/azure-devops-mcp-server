using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.Application;

/// <summary>Generates Feature work items under a parent Epic.</summary>
public sealed class FeatureGeneratorService(IWorkItemService workItemService, ILogger<FeatureGeneratorService> logger)
{
    private readonly IWorkItemService _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
    private readonly ILogger<FeatureGeneratorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    /// <summary>Creates a Feature work item optionally linked to a parent Epic.</summary>
    public async Task<WorkItemResult> GenerateFeatureAsync(
        string project,
        string title,
        string description,
        int? parentEpicId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Feature '{Title}' in project '{Project}'", title, project);
        return await _workItemService.CreateWorkItemAsync(project, "Feature", title, description, parentEpicId, cancellationToken);
    }
}
