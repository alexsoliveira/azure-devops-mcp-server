using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Operations;
using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;

/// <summary>Azure DevOps implementation of <see cref="IProjectService"/>.</summary>
public sealed class ProjectService(AzureDevOpsClient client, ILogger<ProjectService> logger) : IProjectService
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
        string processTemplate = "Basic",
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating project '{Name}' with template '{Template}'", name, processTemplate);

        var projectClient = await client.GetProjectClientAsync();

        // Map template names to their GUIDs
        // Source: https://learn.microsoft.com/en-us/azure/devops/boards/work-items/guidance/choose-process
        var templateTypeIdMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Agile"] = "6b724908-ef14-45cf-84f8-768b5384da45",
            ["Scrum"] = "6e102d7d-a855-4364-9562-41f0830e5e65",
            ["CMMI"] = "27450541-8e31-4150-9947-dc59f998fc01",
            ["Basic"] = "b8a3a935-7ecd-443f-9226-2f2d4979f2f9"
        };

        var templateTypeId = templateTypeIdMap.TryGetValue(processTemplate, out var id) 
            ? id 
            : processTemplate; // Fall back to provided value if it's already a GUID

        logger.LogDebug("Template '{Template}' resolved to GUID: {TemplateTypeId}", processTemplate, templateTypeId);

        var projectToCreate = new TeamProject
        {
            Name = name,
            Description = description,
            Capabilities = new Dictionary<string, Dictionary<string, string>>
            {
                ["versioncontrol"] = new() { ["sourceControlType"] = "Git" },
                ["processTemplate"] = new() { ["templateTypeId"] = templateTypeId }
            }
        };

        try
        {
            var operation = await projectClient.QueueCreateProject(projectToCreate);
            logger.LogInformation("Project creation queued with operation ID {OperationId}", operation.Id);

            // Poll until the operation completes
            var operationsClient = await client.GetOperationsClientAsync();
            Operation? completedOp = null;
            for (var i = 0; i < 30; i++)
            {
                await Task.Delay(2000, cancellationToken);
                completedOp = await operationsClient.GetOperation(operation.Id, cancellationToken: cancellationToken);
                
                logger.LogDebug("Project creation operation poll #{Attempt}: Status={Status}", i + 1, completedOp.Status);
                
                if (completedOp.Status is OperationStatus.Succeeded or OperationStatus.Failed or OperationStatus.Cancelled)
                    break;
            }

            if (completedOp?.Status != OperationStatus.Succeeded)
            {
                var errorMsg = $"Project creation failed or timed out. Status: {completedOp?.Status}";
                logger.LogError("Project creation failed for '{Name}': {Message}", name, errorMsg);
                throw new InvalidOperationException(errorMsg);
            }

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
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            logger.LogError(ex, "Error creating project '{Name}': {Message}", name, ex.Message);
            throw;
        }
    }
}
