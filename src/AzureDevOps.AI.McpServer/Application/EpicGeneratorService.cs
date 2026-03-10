using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.Application;

/// <summary>Generates Epics in Azure DevOps from a project goal description.</summary>
public sealed class EpicGeneratorService(IWorkItemService workItemService, ILogger<EpicGeneratorService> logger)
{
    private readonly IWorkItemService _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
    private readonly ILogger<EpicGeneratorService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    /// <summary>Creates a single Epic work item from the supplied metadata.</summary>
    public async Task<WorkItemResult> GenerateEpicAsync(
        string project,
        string title,
        string description,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating Epic for project '{Project}'", project);
        return await _workItemService.CreateWorkItemAsync(project, "Epic", title, description, cancellationToken: cancellationToken);
    }
}
