using ModelContextProtocol.Server;
using System.ComponentModel;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.McpTools;

/// <summary>MCP tool for creating Feature work items in Azure DevOps.</summary>
[McpServerToolType]
public sealed class FeatureTools(IWorkItemService workItemService, ILogger<FeatureTools> logger)
{
    private readonly IWorkItemService _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
    private readonly ILogger<FeatureTools> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    /// <summary>Creates a Feature work item in Azure DevOps, optionally under a parent Epic.</summary>
    [McpServerTool, Description("Creates a Feature work item in Azure DevOps, optionally linked to a parent Epic.")]
    public async Task<WorkItemResult> CreateFeature(
        [Description("Target Azure DevOps project name")] string project,
        [Description("Feature title")] string title,
        [Description("Feature description")] string description,
        [Description("Optional parent Epic ID to establish hierarchy")] int? parentEpicId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool ado_feature_create invoked: '{Title}' in '{Project}'", title, project);
        var result = await _workItemService.CreateWorkItemAsync(project, "Feature", title, description, parentEpicId, cancellationToken);
        _logger.LogInformation("Feature created with ID {Id}", result.Id);
        return result;
    }
}
