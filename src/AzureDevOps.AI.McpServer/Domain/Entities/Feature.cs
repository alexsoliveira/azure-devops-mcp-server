namespace AzureDevOps.AI.McpServer.Domain.Entities;

/// <summary>Represents a Feature work item in Azure DevOps.</summary>
public sealed record Feature(
    int Id,
    string Title,
    string Description,
    string Project,
    int? ParentEpicId,
    string Url);
