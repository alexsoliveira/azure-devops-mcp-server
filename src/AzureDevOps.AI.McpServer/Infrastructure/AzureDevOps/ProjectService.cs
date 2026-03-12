using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;

/// <summary>Azure DevOps implementation of <see cref="IProjectService"/>.</summary>
internal sealed class ProjectService(AzureDevOpsClient client, ILogger<ProjectService> logger) : IProjectService
{
    public async Task<IReadOnlyList<ProjectResult>> ListProjectsAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Listing all Azure DevOps projects");

        var projectClient = await client.GetProjectClientAsync();
        var projects = await projectClient.GetProjects();

        return projects.Select(p => new ProjectResult(
            p.Id.ToString(),
            p.Name,
            p.Description,
            p.Url,
            p.State.ToString(),
            p.Revision,
            p.Visibility.ToString(),
            p.LastUpdateTime)).ToList();
    }

    public async Task<ProjectResult> GetProjectAsync(string projectNameOrId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting project '{Project}'", projectNameOrId);

        var projectClient = await client.GetProjectClientAsync();
        var project = await projectClient.GetProject(projectNameOrId);

        return new ProjectResult(
            project.Id.ToString(),
            project.Name,
            project.Description,
            project.Url,
            project.State.ToString(),
            project.Revision,
            project.Visibility.ToString(),
            project.LastUpdateTime);
    }

    public async Task<ProjectResult> CreateProjectAsync(
        string name,
        string description,
        string processTemplate = "Agile",
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating project '{Name}' with template '{Template}'", name, processTemplate);

        var projectClient = await client.GetProjectClientAsync();

        var projectToCreate = new TeamProject
        {
            Name = name,
            Description = description,
            Capabilities = new Dictionary<string, Dictionary<string, string>>
            {
                ["versioncontrol"] = new() { ["sourceControlType"] = "Git" },
                ["processTemplate"] = new() { ["templateTypeId"] = processTemplate }
            }
        };

        var operation = await projectClient.QueueCreateProject(projectToCreate);

        // Poll until the operation completes
        var operationsClient = await client.GetOperationsClientAsync();
        Operation? completedOp = null;
        for (var i = 0; i < 30; i++)
        {
            await Task.Delay(2000, cancellationToken);
            completedOp = await operationsClient.GetOperation(operation.Id, cancellationToken: cancellationToken);
            if (completedOp.Status is OperationStatus.Succeeded or OperationStatus.Failed or OperationStatus.Cancelled)
                break;
        }

        if (completedOp?.Status != OperationStatus.Succeeded)
            throw new InvalidOperationException($"Project creation failed or timed out. Status: {completedOp?.Status}");

        var created = await projectClient.GetProject(name);
        logger.LogInformation("Project '{Name}' created with ID {Id}", name, created.Id);

        return new ProjectResult(
            created.Id.ToString(),
            created.Name,
            created.Description,
            created.Url,
            created.State.ToString(),
            created.Revision,
            created.Visibility.ToString(),
            created.LastUpdateTime);
    }
}
