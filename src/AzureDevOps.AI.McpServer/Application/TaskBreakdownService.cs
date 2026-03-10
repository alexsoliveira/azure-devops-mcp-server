using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.Application;

/// <summary>Breaks a User Story or Feature into atomic Task work items.</summary>
public sealed class TaskBreakdownService(IWorkItemService workItemService, ILogger<TaskBreakdownService> logger)
{
    /// <summary>Creates multiple Task work items under a parent work item.</summary>
    public async Task<IReadOnlyList<WorkItemResult>> BreakdownAsync(
        string project,
        int parentId,
        IReadOnlyList<(string Title, string Description)> tasks,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Breaking down work item {ParentId} into {Count} task(s)", parentId, tasks.Count);

        var results = new List<WorkItemResult>(tasks.Count);
        foreach (var (title, description) in tasks)
        {
            var result = await workItemService.CreateWorkItemAsync(project, "Task", title, description, parentId, cancellationToken);
            results.Add(result);
        }

        logger.LogInformation("Created {Count} task(s) under work item {ParentId}", results.Count, parentId);
        return results;
    }
}
