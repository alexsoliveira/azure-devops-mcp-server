using ModelContextProtocol.Server;
using System.ComponentModel;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.McpTools;

/// <summary>MCP tool for creating User Story work items in Azure DevOps.</summary>
[McpServerToolType]
public sealed class UserStoryTools(IWorkItemService workItemService, ILogger<UserStoryTools> logger)
{
    private readonly IWorkItemService _workItemService = workItemService ?? throw new ArgumentNullException(nameof(workItemService));
    private readonly ILogger<UserStoryTools> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    /// <summary>Creates a User Story work item in Azure DevOps, optionally under a parent Feature.</summary>
    [McpServerTool, Description("Creates a User Story work item in Azure DevOps, optionally linked to a parent Feature.")]
    public async Task<WorkItemResult> CreateUserStory(
        [Description("Target Azure DevOps project name")] string project,
        [Description("User Story title")] string title,
        [Description("User Story description")] string description,
        [Description("Optional parent Feature ID")] int? parentFeatureId = null,
        [Description("Optional acceptance criteria")] string? acceptanceCriteria = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool ado_userstory_create invoked: '{Title}' in '{Project}'", title, project);
        var result = await _workItemService.CreateWorkItemAsync(project, "User Story", title, description, parentFeatureId, cancellationToken);
        _logger.LogInformation("User Story created with ID {Id}", result.Id);
        return result;
    }
}
