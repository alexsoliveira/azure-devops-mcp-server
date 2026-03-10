# Azure DevOps AI Agent MCP Server — Copilot Instructions

## Project Context

You are assisting in the development of an **Azure DevOps AI Agent MCP Server** built with **.NET 10** and **ASP.NET Core**. This server exposes Azure DevOps operations as **MCP (Model Context Protocol) tools**, enabling AI agents such as GitHub Copilot CLI to manage projects, work items, backlogs, and pipelines through structured tool calls.

The system acts as a thin abstraction layer over Azure DevOps REST APIs. Business logic lives in the AI agent, not in this server.

---

## Technology Stack

- **Runtime**: .NET 10
- **Framework**: ASP.NET Core
- **MCP SDK**: `ModelContextProtocol` NuGet SDK
- **Azure DevOps Client**: Azure DevOps REST API via `Microsoft.TeamFoundationServer.Client` or `Azure.DevOps` SDK
- **Containerization**: Docker
- **Transport (local)**: `stdio`
- **Transport (production)**: Server-Sent Events (SSE) over HTTP (`/mcp`)
- **Authentication**: Personal Access Token (PAT) via environment variable `AZURE_DEVOPS_PAT`

---

## Solution Structure

Always follow this canonical folder structure:

```
src/AzureDevOps.AI.McpServer/
├── Host/
│   └── Program.cs                  # Entry point, DI registration, MCP setup
├── McpTools/
│   ├── ProjectTools.cs             # ado_project_list, ado_project_get, ado_project_create
│   ├── EpicTools.cs                # ado_epic_create
│   ├── FeatureTools.cs             # ado_feature_create
│   ├── UserStoryTools.cs           # ado_userstory_create
│   ├── TaskTools.cs                # ado_task_create
│   ├── WorkItemTools.cs            # ado_workitem_list, ado_workitem_update, ado_workitem_link
│   └── SprintTools.cs              # ado_sprint_plan, ado_backlog_generate, ado_task_breakdown
├── Application/
│   ├── EpicGeneratorService.cs
│   ├── FeatureGeneratorService.cs
│   ├── TaskBreakdownService.cs
│   └── SprintPlannerService.cs
├── Domain/
│   └── Entities/
│       ├── Epic.cs
│       ├── Feature.cs
│       ├── UserStory.cs
│       └── TaskItem.cs
├── Infrastructure/
│   └── AzureDevOps/
│       ├── AzureDevOpsClient.cs
│       ├── WorkItemService.cs
│       └── ProjectService.cs
└── Security/
    ├── TokenProvider.cs
    └── PermissionGuard.cs
```

---

## MCP Tools Catalogue

### Project Tools
| Tool name              | Description                    |
|------------------------|--------------------------------|
| `ado_project_list`     | List all Azure DevOps projects |
| `ado_project_get`      | Get details of a project       |
| `ado_project_create`   | Create a new project           |

### Work Item Tools
| Tool name              | Description                              |
|------------------------|------------------------------------------|
| `ado_epic_create`      | Create an Epic work item                 |
| `ado_feature_create`   | Create a Feature work item               |
| `ado_userstory_create` | Create a User Story work item            |
| `ado_task_create`      | Create a Task work item                  |
| `ado_workitem_list`    | List work items with optional filters    |
| `ado_workitem_update`  | Update fields of a work item             |
| `ado_workitem_link`    | Link two work items (parent/child, etc.) |

### Sprint / Backlog Tools
| Tool name               | Description                                 |
|-------------------------|---------------------------------------------|
| `ado_sprint_plan`       | Plan and assign items to a sprint           |
| `ado_backlog_generate`  | Auto-generate backlog from a project goal   |
| `ado_task_breakdown`    | Break a feature or story into atomic tasks  |

---

## Tool Input/Output Conventions

All tools follow this pattern:

**Input**: flat JSON object with named parameters (string, int, bool).

**Output**: JSON object containing at minimum `id` and `url` for created/updated items.

```json
// Input example — ado_epic_create
{
  "project": "AI Platform",
  "title": "AI Agent Infrastructure",
  "description": "Create infrastructure for AI Agents"
}

// Output example
{
  "id": 12345,
  "url": "https://dev.azure.com/org/project/_workitems/edit/12345"
}
```

When generating tool implementations, always decorate the method with the `[McpServerTool]` attribute from the `ModelContextProtocol.Server` namespace.

---

## Architecture Principles

1. **Thin abstraction**: MCP tools wrap REST API calls. Do not embed AI reasoning or heavy business logic inside tools.
2. **Single Responsibility**: Each tool class handles one Azure DevOps resource type.
3. **DI-first**: All services are registered in `Program.cs` via `IServiceCollection`. Never use `new` to instantiate services inside tools.
4. **Async all the way**: All tool methods must be `async Task<T>`.
5. **Structured error handling**: Catch `VssServiceException` and surface clear error messages via tool output. Never throw unhandled exceptions to the MCP runtime.
6. **Immutable domain entities**: Domain entities (`Epic`, `Feature`, etc.) are records or read-only POCOs.

---

## Security Rules

- **PAT token** is loaded exclusively from the `AZURE_DEVOPS_PAT` environment variable. Never hardcode credentials.
- **PermissionGuard** must block the following classes of operations when invoked by AI agents:
  - Deleting projects
  - Deleting entire backlogs
  - Bulk-deleting work items
- **Audit logging** is mandatory for all mutating operations (`create`, `update`, `link`). Log: timestamp, tool name, parameters summary, result ID.
- Use `ILogger<T>` for all logging. Do not use `Console.WriteLine` in production code.

---

## Deployment Guidelines

### Local (stdio transport)
- Entry point: `Program.cs` configures MCP with `stdio` transport.
- Start command: `dotnet run`
- Used by Copilot CLI directly.

### Production (SSE transport)
- Expose endpoint: `http://localhost:5050/mcp` (Server-Sent Events).
- Container image: `azure-devops-mcp-server`.
- Required environment variables:
  - `AZURE_DEVOPS_PAT`
  - `AZURE_DEVOPS_ORG` (organization URL)
  - `AZURE_DEVOPS_PROJECT` (default project)
  - `ASPNETCORE_ENVIRONMENT=Production`

---

## Code Style & Conventions

- Use **C# 14** features where appropriate (primary constructors, collection expressions, and other modern language features).
- Use `record` types for immutable data transfer objects.
- Prefer `IReadOnlyList<T>` over `List<T>` in return types.
- XML doc comments (`///`) are required only on public tool methods and public service interfaces.
- File-scoped namespaces: `namespace AzureDevOps.AI.McpServer.McpTools;`
- One class per file. File name matches class name.

---

## Example: Tool Implementation Pattern

```csharp
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace AzureDevOps.AI.McpServer.McpTools;

[McpServerToolType]
public sealed class EpicTools(IWorkItemService workItemService, ILogger<EpicTools> logger)
{
    [McpServerTool, Description("Creates an Epic work item in Azure DevOps.")]
    public async Task<WorkItemResult> CreateEpic(
        [Description("Target Azure DevOps project name")] string project,
        [Description("Epic title")] string title,
        [Description("Epic description")] string description,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating Epic '{Title}' in project '{Project}'", title, project);
        var result = await workItemService.CreateWorkItemAsync(project, "Epic", title, description, cancellationToken);
        logger.LogInformation("Epic created with ID {Id}", result.Id);
        return result;
    }
}
```

---

## What NOT to Do

- Do not implement AI reasoning, prompt construction, or LLM calls inside this MCP server.
- Do not expose endpoints other than `/mcp` without explicit requirements.
- Do not store Azure DevOps data in a local database unless caching is explicitly requested.
- Do not use `HttpClient` directly; use the injected Azure DevOps SDK client.
- Do not block dangerous AI operations without routing them through `PermissionGuard`.
