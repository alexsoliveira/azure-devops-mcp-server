As informações técnicas usadas abaixo vêm do MCP oficial de Azure DevOps da Microsoft e de implementações .NET existentes que expõem operações de Azure DevOps (Boards, Repos, Pipelines etc.) como ferramentas MCP para agentes de IA.

---

# Azure DevOps AI Agent MCP Server Architecture

## Overview

This document describes the architecture of an **Azure DevOps AI Agent MCP Server**, designed to integrate:

* GitHub Copilot CLI
* Model Context Protocol (MCP)
* Azure DevOps
* AI Agents
* Automated backlog generation

The system enables AI assistants to interact with Azure DevOps resources such as:

* Projects
* Work Items
* Epics / Features / Tasks
* Repositories
* Pipelines
* Wikis

Azure DevOps MCP servers act as a **thin abstraction layer over Azure DevOps REST APIs**, allowing LLMs to invoke DevOps operations through structured tools.

---

# High-Level Architecture

```
+---------------------------+
|        AI CLIENTS         |
|---------------------------|
| Copilot CLI               |
| VSCode Copilot Agent      |
| AI Development Agents     |
+------------+--------------+
             |
             | MCP Protocol
             v
+---------------------------+
|        MCP SERVER         |
|---------------------------|
| Tool Registry             |
| Authentication Layer      |
| Rate Limiting             |
| Prompt Safety             |
+------------+--------------+
             |
             v
+---------------------------+
|     APPLICATION LAYER     |
|---------------------------|
| AI Product Owner Engine   |
| Epic Generator            |
| Feature Generator         |
| Task Breakdown Engine     |
| Sprint Planner            |
+------------+--------------+
             |
             v
+---------------------------+
|         DOMAIN            |
|---------------------------|
| Project                   |
| Epic                      |
| Feature                   |
| UserStory                 |
| Task                      |
+------------+--------------+
             |
             v
+---------------------------+
|       ADAPTER LAYER       |
|---------------------------|
| Azure DevOps REST Adapter |
| Azure DevOps SDK Adapter  |
| Azure DevOps CLI Adapter  |
+------------+--------------+
             |
             v
        Azure DevOps
```

---

# Core Architecture Principles

## 1. MCP Tool-Based Integration

Each Azure DevOps capability is exposed as a **tool**.

Example:

```
ado_project_list
ado_epic_create
ado_feature_create
ado_task_create
ado_workitem_update
```

LLMs call these tools using structured input.

---

## 2. Thin API Abstraction

The MCP server does **not implement complex business logic**.

Instead it:

* wraps Azure DevOps REST APIs
* exposes simple tools
* lets the AI agent perform reasoning

This design matches the approach used by the official Azure DevOps MCP implementation.

---

# Project Structure (.NET)

Recommended solution structure:

```
src/

AzureDevOps.AI.McpServer
│
├── Host
│   └── Program.cs
│
├── McpTools
│   ├── CreateEpicTool.cs
│   ├── CreateFeatureTool.cs
│   ├── CreateTaskTool.cs
│   ├── ListWorkItemsTool.cs
│   └── ProjectInfoTool.cs
│
├── Application
│   ├── EpicGeneratorService.cs
│   ├── FeatureGeneratorService.cs
│   ├── TaskBreakdownService.cs
│   └── SprintPlannerService.cs
│
├── Domain
│   ├── Entities
│   │   ├── Epic.cs
│   │   ├── Feature.cs
│   │   ├── UserStory.cs
│   │   └── TaskItem.cs
│
├── Infrastructure
│   ├── AzureDevOps
│   │   ├── AzureDevOpsClient.cs
│   │   ├── WorkItemService.cs
│   │   └── ProjectService.cs
│
└── Security
    ├── TokenProvider.cs
    └── PermissionGuard.cs
```

---

# MCP Tools Design

## Project Tools

```
ado_project_list
ado_project_get
ado_project_create
```

---

## Work Item Tools

```
ado_epic_create
ado_feature_create
ado_userstory_create
ado_task_create
ado_workitem_list
ado_workitem_update
ado_workitem_link
```

---

## Sprint Planning Tools

```
ado_sprint_plan
ado_backlog_generate
ado_task_breakdown
```

---

# Example MCP Tool

Example tool definition.

```
Tool: ado_epic_create

Input:

{
 "project": "AI Platform",
 "title": "AI Agent Infrastructure",
 "description": "Create infrastructure for AI Agents"
}

Output:

{
 "id": 12345,
 "url": "https://dev.azure.com/org/project/_workitems/edit/12345"
}
```

---

# AI Product Owner Engine

The AI engine generates backlog items automatically.

Example prompt:

```
INPUT

Project: AI Dev Platform

Goal:
Create an AI agent platform using MCP servers

OUTPUT

Epic
Feature
Tasks
```

---

### Example AI Output

```
Epic
AI Agent Platform

Feature
MCP Server Infrastructure

Tasks
Create MCP Server
Integrate Azure DevOps API
Create authentication system
Implement Copilot CLI tools
```

The MCP server then executes:

```
CreateEpic
CreateFeature
CreateTasks
```

---

# Copilot CLI Integration

Copilot CLI discovers MCP tools automatically.

Example interaction:

```
copilot

> create an epic for AI agent platform
```

Copilot calls:

```
ado_epic_create
```

---

# Deployment Architecture

## Local Development

Recommended for development.

```
Copilot CLI
     |
     v
Local MCP Server
     |
     v
Azure DevOps API
```

Transport:

```
stdio
```

---

## Production Deployment

Recommended architecture:

```
AI Agents
    |
    v
MCP Server
    |
Docker Container
    |
Azure DevOps API
```

---

# Docker Deployment Example

```
docker build -t azure-devops-mcp-server .

docker run -d \
 -p 5050:5050 \
 -e ASPNETCORE_ENVIRONMENT=Production \
 azure-devops-mcp-server
```

---

# MCP Endpoint

Example:

```
http://localhost:5050/mcp
```

Transport:

```
Server-Sent Events (SSE)
```

---

# Security Architecture

## 1. Personal Access Token (PAT)

Environment variable:

```
AZURE_DEVOPS_PAT
```

Scopes recommended:

```
Work Items Read/Write
Project Read
Build Read
```

---

## 2. Permission Guard

Prevent dangerous operations.

Example blocked prompts:

```
delete all work items
remove backlog
delete project
```

---

## 3. Audit Logging

Log all AI actions.

Example:

```
AI created Work Item
AI updated Work Item
AI linked Work Item
```

---

# Multi-MCP Platform Architecture

Advanced DevOps AI platform:

```
AI Agent
   |
   v
Tool Router
   |
   +----------------------+
   | MCP Servers          |
   |----------------------|
   | Azure DevOps MCP     |
   | GitHub MCP           |
   | Slack MCP            |
   | Jira MCP             |
   +----------------------+
```

---

# Future Architecture (AI DevOps Platform)

Full AI DevOps system:

```
Copilot CLI
     |
     v
MCP Gateway
     |
     +---------------------+
     | Azure DevOps MCP    |
     | GitHub MCP          |
     | Internal Tools MCP  |
     +---------------------+
```

This enables building autonomous agents such as:

* AI Product Owner
* AI Scrum Master
* AI Developer Agent
* AI DevOps Agent

---

# Technology Stack

Recommended stack.

```
.NET 8 / .NET 9
ASP.NET Core
ModelContextProtocol SDK
Azure DevOps REST API
Docker
GitHub Copilot CLI
```

---

# Important Notes

Current Azure DevOps MCP implementations are **still evolving and in preview**, meaning APIs and toolsets may change over time.

---

# Conclusion

This architecture enables the creation of a powerful **AI-driven DevOps automation platform** where:

* Copilot CLI interacts with MCP tools
* MCP tools control Azure DevOps
* AI agents generate backlog items automatically

Result:

A fully automated **AI Product Owner for Azure DevOps**.

---
