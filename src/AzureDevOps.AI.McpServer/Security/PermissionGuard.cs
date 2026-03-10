using Microsoft.Extensions.Logging;

namespace AzureDevOps.AI.McpServer.Security;

/// <summary>Enforces operation-level restrictions to prevent destructive AI-initiated actions.</summary>
public sealed class PermissionGuard(ILogger<PermissionGuard> logger)
{
    private static readonly string[] BlockedKeywords =
    [
        "delete project",
        "remove backlog",
        "delete all work items",
        "bulk delete",
        "drop project"
    ];

    /// <summary>
    /// Throws <see cref="UnauthorizedAccessException"/> if the requested operation is blocked.
    /// </summary>
    /// <param name="operationDescription">Human-readable description of the requested operation.</param>
    public void AssertAllowed(string operationDescription)
    {
        foreach (var keyword in BlockedKeywords)
        {
            if (operationDescription.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning("Blocked dangerous operation: '{Operation}'", operationDescription);
                throw new UnauthorizedAccessException(
                    $"Operation '{operationDescription}' is blocked by PermissionGuard.");
            }
        }
    }
}
