namespace AzureDevOps.AI.McpServer.Domain.Entities;

/// <summary>Represents a User Story work item in Azure DevOps.</summary>
public sealed record UserStory(
    int Id,
    string Title,
    string Description,
    string Project,
    int? ParentFeatureId,
    string? AcceptanceCriteria,
    string Url);
