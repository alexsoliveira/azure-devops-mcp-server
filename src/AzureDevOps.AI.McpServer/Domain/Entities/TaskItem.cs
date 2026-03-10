namespace AzureDevOps.AI.McpServer.Domain.Entities;

/// <summary>Represents a Task work item in Azure DevOps.</summary>
public sealed record TaskItem(
    int Id,
    string Title,
    string Description,
    string Project,
    int? ParentUserStoryId,
    string? AssignedTo,
    string Url);
