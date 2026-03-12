namespace AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;

/// <summary>Result returned for project operations.</summary>
public sealed record ProjectResult(
    string Id,
    string Name,
    string? Description,
    string Url,
    string? State,
    long Revision,
    string? Visibility,
    DateTime? LastUpdateTime);

/// <summary>Contract for Azure DevOps project operations.</summary>
public interface IProjectService
{
    /// <summary>Returns all projects in the organization.</summary>
    Task<IReadOnlyList<ProjectResult>> ListProjectsAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns details of a single project by name or ID.</summary>
    Task<ProjectResult> GetProjectAsync(string projectNameOrId, CancellationToken cancellationToken = default);

    /// <summary>Creates a new Azure DevOps project with the specified name, description, and process template.</summary>
    /// <param name="name">Project name</param>
    /// <param name="description">Project description</param>
    /// <param name="processTemplate">Process template name: "Basic" (default), "Agile", "Scrum", or "CMMI"</param>
    Task<ProjectResult> CreateProjectAsync(
        string name,
        string description,
        string processTemplate = "Basic",
        CancellationToken cancellationToken = default);
}
