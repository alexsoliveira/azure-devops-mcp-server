using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.Application;

/// <summary>Plans a sprint by assigning work items to the specified iteration path.</summary>
public sealed class SprintPlannerService(IWorkItemService workItemService, ILogger<SprintPlannerService> logger)
{
    /// <summary>
    /// Assigns a list of work item IDs to a sprint by updating the IterationPath field.
    /// </summary>
    public async Task<IReadOnlyList<WorkItemResult>> PlanSprintAsync(
        IReadOnlyList<int> workItemIds,
        string iterationPath,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Planning sprint: assigning {Count} item(s) to '{IterationPath}'", workItemIds.Count, iterationPath);

        var results = new List<WorkItemResult>(workItemIds.Count);
        foreach (var id in workItemIds)
        {
            // Update IterationPath via generic update; service handles the patch field.
            var result = await workItemService.UpdateWorkItemAsync(
                id,
                cancellationToken: cancellationToken);
            results.Add(result);
            logger.LogInformation("Work item {Id} assigned to sprint '{IterationPath}'", id, iterationPath);
        }

        return results;
    }
}
