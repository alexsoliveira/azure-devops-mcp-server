namespace AzureDevOps.AI.McpServer.Domain.Entities;

/// <summary>Represents an Epic work item in Azure DevOps.</summary>
public sealed record Epic(
    int Id,
    string Title,
    string Description,
    string Project,
    string Url);
