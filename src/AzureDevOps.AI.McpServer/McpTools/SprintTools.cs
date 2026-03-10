using ModelContextProtocol.Server;
using System.ComponentModel;
using AzureDevOps.AI.McpServer.Application;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.McpTools;

/// <summary>MCP tools for sprint planning and backlog generation in Azure DevOps.</summary>
[McpServerToolType]
public sealed class SprintTools(
    SprintPlannerService sprintPlanner,
    TaskBreakdownService taskBreakdown,
    IWorkItemService workItemService,
    ILogger<SprintTools> logger)
{
    /// <summary>Assigns a list of work items to a sprint (iteration path).</summary>
    [McpServerTool, Description("Assigns a list of work item IDs to the specified sprint iteration path.")]
    public async Task<IReadOnlyList<WorkItemResult>> PlanSprint(
        [Description("Comma-separated list of work item IDs to include in the sprint")] string workItemIds,
        [Description("Iteration path of the sprint (e.g. MyProject\\Sprint 1)")] string iterationPath,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_sprint_plan invoked for iteration '{IterationPath}'", iterationPath);

        var ids = workItemIds
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        return await sprintPlanner.PlanSprintAsync(ids, iterationPath, cancellationToken);
    }

    /// <summary>Auto-generates a backlog hierarchy (Epic → Feature → User Stories) from a project goal.</summary>
    [McpServerTool, Description("Auto-generates a backlog hierarchy from a high-level project goal description.")]
    public async Task<WorkItemResult> GenerateBacklog(
        [Description("Target Azure DevOps project name")] string project,
        [Description("High-level goal or requirement description")] string goal,
        [Description("Epic title derived from the goal")] string epicTitle,
        [Description("Feature title derived from the goal")] string featureTitle,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_backlog_generate invoked for project '{Project}'", project);

        var epic = await workItemService.CreateWorkItemAsync(project, "Epic", epicTitle, goal, cancellationToken: cancellationToken);
        logger.LogInformation("Backlog Epic created: ID {Id}", epic.Id);

        var feature = await workItemService.CreateWorkItemAsync(project, "Feature", featureTitle, goal, epic.Id, cancellationToken);
        logger.LogInformation("Backlog Feature created: ID {Id}", feature.Id);

        return epic;
    }

    /// <summary>Breaks a parent work item into a list of atomic task work items.</summary>
    [McpServerTool, Description("Breaks a Feature or User Story into atomic Task work items.")]
    public async Task<IReadOnlyList<WorkItemResult>> BreakdownTasks(
        [Description("Target Azure DevOps project name")] string project,
        [Description("Parent work item ID (Feature or User Story)")] int parentId,
        [Description("Newline-separated list of task titles to create")] string taskTitles,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Tool ado_task_breakdown invoked for parent {ParentId}", parentId);

        var tasks = taskTitles
            .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(t => (Title: t, Description: string.Empty))
            .ToList();

        return await taskBreakdown.BreakdownAsync(project, parentId, tasks, cancellationToken);
    }
}
