using ModelContextProtocol.Server;
using System.ComponentModel;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.McpTools;

/// <summary>MCP tool for creating Task work items in Azure DevOps.</summary>
[McpServerToolType]
public sealed class TaskTools(IWorkItemService workItemService, ILogger<TaskTools> logger)
{
    private readonly IWorkItemService _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
    private readonly ILogger<TaskTools> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    /// <summary>Creates a Task work item in Azure DevOps, optionally under a parent User Story.</summary>
    [McpServerTool, Description("Creates a Task work item in Azure DevOps, optionally linked to a parent User Story.")]
    public async Task<WorkItemResult> CreateTask(
        [Description("Target Azure DevOps project name")] string project,
        [Description("Task title")] string title,
        [Description("Task description")] string description,
        [Description("Optional parent User Story ID")] int? parentUserStoryId = null,
        [Description("Optional user to assign the task to")] string? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool ado_task_create invoked: '{Title}' in '{Project}'", title, project);
        var result = await _workItemService.CreateWorkItemAsync(project, "Task", title, description, parentUserStoryId, cancellationToken);
        _logger.LogInformation("Task created with ID {Id}", result.Id);
        return result;
    }
}
