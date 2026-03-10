using ModelContextProtocol.Server;
using System.ComponentModel;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.McpTools;

/// <summary>MCP tool for creating Epic work items in Azure DevOps.</summary>
[McpServerToolType]
public sealed class EpicTools(IWorkItemService workItemService, ILogger<EpicTools> logger)
{
    private readonly IWorkItemService _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
    private readonly ILogger<EpicTools> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    /// <summary>Creates an Epic work item in Azure DevOps.</summary>
    [McpServerTool, Description("Creates an Epic work item in Azure DevOps.")]
    public async Task<WorkItemResult> CreateEpic(
        [Description("Target Azure DevOps project name")] string project,
        [Description("Epic title")] string title,
        [Description("Epic description")] string description,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool ado_epic_create invoked: '{Title}' in '{Project}'", title, project);
        var result = await _workItemService.CreateWorkItemAsync(project, "Epic", title, description, cancellationToken: cancellationToken);
        _logger.LogInformation("Epic created with ID {Id}", result.Id);
        return result;
    }
}
