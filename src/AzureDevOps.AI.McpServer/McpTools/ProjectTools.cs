using ModelContextProtocol.Server;
using System.ComponentModel;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.McpTools;

/// <summary>MCP tools for Azure DevOps project operations.</summary>
[McpServerToolType]
public sealed class ProjectTools(IProjectService projectService, ILogger<ProjectTools> logger)
{
    private readonly IProjectService _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
    private readonly ILogger<ProjectTools> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>Lists all Azure DevOps projects in the organization.</summary>
    [McpServerTool, Description("Lists all Azure DevOps projects in the organization.")]
    public async Task<IReadOnlyList<ProjectResult>> ListProjects(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool ado_project_list invoked");
        return await _projectService.ListProjectsAsync(cancellationToken);
    }

    /// <summary>Returns details of a specific Azure DevOps project.</summary>
    [McpServerTool, Description("Returns details of a specific Azure DevOps project by name or ID.")]
    public async Task<ProjectResult> GetProject(
        [Description("Project name or ID")] string project,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool ado_project_get invoked for '{Project}'", project);
        return await _projectService.GetProjectAsync(project, cancellationToken);
    }

    /// <summary>Creates a new Azure DevOps project.</summary>
    [McpServerTool, Description("Creates a new Azure DevOps project.")]
    public async Task<ProjectResult> CreateProject(
        [Description("Project name")] string name,
        [Description("Project description")] string description,
        [Description("Process template name (default: Agile)")] string processTemplate = "Agile",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool ado_project_create invoked: '{Name}'", name);
        return await _projectService.CreateProjectAsync(name, description, processTemplate, cancellationToken);
    }
}
