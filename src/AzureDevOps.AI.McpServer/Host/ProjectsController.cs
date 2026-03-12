using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using Microsoft.AspNetCore.Mvc;

namespace AzureDevOps.AI.McpServer.Host;

[ApiController]
[Route("api")]
public sealed class ProjectsController(
    IProjectService projectService,
    ILogger<ProjectsController> logger) : ControllerBase
{
    [HttpGet("projects")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListProjects()
    {
        try
        {
            logger.LogInformation("API: ListProjects called");
            var projects = await projectService.ListProjectsAsync();
            logger.LogInformation("API: Found {Count} projects", projects.Count);
            
            return Ok(new
            {
                success = true,
                count = projects.Count,
                projects = projects.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Url,
                    p.State,
                    p.Revision,
                    p.Visibility,
                    p.LastUpdateTime
                }).ToList()
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error listing projects");
            return BadRequest(new
            {
                success = false,
                error = ex.Message
            });
        }
    }

    [HttpGet("projects/{projectNameOrId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProject(string projectNameOrId)
    {
        try
        {
            logger.LogInformation("API: GetProject called for '{Project}'", projectNameOrId);
            var project = await projectService.GetProjectAsync(projectNameOrId);
            
            return Ok(new
            {
                success = true,
                project = new
                {
                    project.Id,
                    project.Name,
                    project.Description,
                    project.Url,
                    project.State,
                    project.Revision,
                    project.Visibility,
                    project.LastUpdateTime
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "API: Error getting project '{Project}'", projectNameOrId);
            return BadRequest(new
            {
                success = false,
                error = ex.Message
            });
        }
    }
}
