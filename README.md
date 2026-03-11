# Azure DevOps AI Agent MCP Server

Um servidor **[Model Context Protocol (MCP)](https://modelcontextprotocol.io/)** construído em **.NET 10** que expõe operações de Azure DevOps como ferramentas para agentes de IA, como **GitHub Copilot CLI** e outros assistentes inteligentes.

> **MCP Server** = Camada de abstração que permite que agentes de IA interajam com Azure DevOps de forma estruturada e segura.

---

## 🎯 O que é Este Projeto?

Este servidor MCP permite que agentes de IA:

- ✅ Criar e gerenciar **projetos** em Azure DevOps
- ✅ Gerar **backlog automático** a partir de descrições de metas
- ✅ Criar **Epics, Features, User Stories e Tasks**
- ✅ Listar e atualizar **work items**
- ✅ Planejar **sprints** automaticamente
- ✅ Quebrar features em **tarefas atômicas**
- ✅ Gerenciar **links** entre work items

**Exemplo de uso**: Um agente de IA pode receber a descrição "Implementar autenticação OAuth" e automaticamente criar uma Epic, 3 Features, 8 User Stories e 20 Tasks em Azure DevOps.

---

## 📋 Pré-requisitos

### Obrigatório (Local)
- **.NET 10 SDK** ou superior ([download](https://dotnet.microsoft.com/en-us/download/dotnet/10.0))
- **Visual Studio Code**, **Visual Studio 2022** ou editor de sua preferência
- **PowerShell 5.1+** (Windows) ou **bash/zsh** (Linux/macOS)

### Obrigatório (Para usar MCP)
- Conta em **[Azure DevOps](https://dev.azure.com/)**
- **Personal Access Token (PAT)** com permissões de leitura/escrita em projetos
- Organização e projeto pré-criados em Azure DevOps

### Opcional (Docker)
- **Docker Desktop** ([download](https://www.docker.com/products/docker-desktop))
- **docker-compose** (incluído no Docker Desktop)

---

## 🚀 Configuração Rápida

### 1. Clonar o Repositório

```bash
git clone https://github.com/seu-usuario/MCP_Server.git
cd DotnetMCPServer
```

### 2. Obter Personal Access Token (PAT)

1. Acesse a organização em Azure DevOps: `https://dev.azure.com/{seu-org}`
2. Clique em **User Settings** (ícone de perfil no canto superior direito)
3. Acesse **Personal access tokens**
4. Clique em **+ New Token**
5. Configure:
   - **Name**: `MCP Server`
   - **Organization**: Selecione sua organização
   - **Expiration**: 30 dias ou mais
   - **Scopes**: Selecione `Work Items (Full)`, `Project & Team (Read)`
6. Clique em **Create** e **copie o token** (aparece apenas uma vez)

### 3. Configurar Variáveis de Ambiente

**Opção A: Usar Arquivo `.env` (Recomendado)**

```bash
# Copiar template
cp .env.example .env

# Editar .env com suas credenciais
# AZURE_DEVOPS_PAT=seu-token-aqui
# AZURE_DEVOPS_ORG=https://dev.azure.com/sua-organizacao
# etc.
```

**Opção B: Variáveis de Ambiente Diretas**

#### Windows (PowerShell)

```powershell
$env:AZURE_DEVOPS_PAT = "seu-token-aqui"
$env:AZURE_DEVOPS_ORG = "https://dev.azure.com/sua-organizacao"
$env:AZURE_DEVOPS_PROJECT = "seu-projeto-padrao"
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:MCP_TRANSPORT = "http"  # ou "stdio" para MCP CLI
```

#### Windows (Command Prompt)

```cmd
set AZURE_DEVOPS_PAT=seu-token-aqui
set AZURE_DEVOPS_ORG=https://dev.azure.com/sua-organizacao
set AZURE_DEVOPS_PROJECT=seu-projeto-padrao
set ASPNETCORE_ENVIRONMENT=Development
set MCP_TRANSPORT=http
```

#### Linux/macOS (Bash)

```bash
export AZURE_DEVOPS_PAT="seu-token-aqui"
export AZURE_DEVOPS_ORG="https://dev.azure.com/sua-organizacao"
export AZURE_DEVOPS_PROJECT="seu-projeto-padrao"
export ASPNETCORE_ENVIRONMENT="Development"
export MCP_TRANSPORT="http"
```

### 4. Restaurar Dependências e Executar

```bash
# Restaurar pacotes NuGet
dotnet restore

# Executar o servidor
dotnet run --project src/AzureDevOps.AI.McpServer

# Ou apenas compilar
dotnet build --configuration Release
```

O servidor iniciará em `http://localhost:5050` (modo HTTP).

---

## ⚡ Quick Start com MCP Client (Docker Desktop)

Para usar com **GitHub Copilot CLI** ou **Claude Desktop** rodando via Docker:

```bash
# 1. Criar arquivo .env (copiar do template)
cp .env.example .env
# Editar .env com suas credenciais

# 2. Build da imagem Docker
docker build -t azure-devops-mcp-server:latest .

# 3. Configurar cliente MCP
# Windows: .vscode/mcp.json
# macOS/Linux: ~/.config/Claude/claude_desktop_config.json
# (ver docs/MCP_CLIENT_SETUP.md para instruções completas)

# 4. Iniciar cliente (ex: Copilot CLI)
copilot
# ✅ Servidor conecta automaticamente via Docker
```

📖 **Guia completo**: [MCP_CLIENT_SETUP.md](docs/MCP_CLIENT_SETUP.md)

---

## 📁 Estrutura do Projeto

```
DotnetMCPServer/
├── 📄 README.md                                # Você está aqui
├── 📄 Dockerfile                               # Imagem Docker
├── 📄 DotnetMCPServer.sln                      # Solução do Visual Studio
│
├── docs/                                       # 📚 Documentação
│   ├── 00_COMECE_AQUI.md                       # Guia inicial
│   ├── QUICKSTART.md                           # Começar rápido
│   ├── STRUCTURE.md                            # Estrutura detalhada
│   ├── DOCKER_MCP_SETUP.md                     # Setup Docker
│   ├── EXTENSION_GUIDE.md                      # Expandir funcionalidades
│   ├── TESTS_CATALOG.md                        # Catálogo de testes
│   └── ...
│
├── src/                                        # ✨ Código-fonte
│   └── AzureDevOps.AI.McpServer/
│       ├── 🗂️  Host/
│       │   ├── Program.cs                      # Entry point, configuração MCP
│       │   └── ProjectsController.cs           # REST API (modo HTTP)
│       │
│       ├── 🔧 McpTools/                        # Ferramentas MCP (Interfaces com IA)
│       │   ├── ProjectTools.cs                 # Create/List projects
│       │   ├── EpicTools.cs                    # Create Epics
│       │   ├── FeatureTools.cs                 # Create Features
│       │   ├── UserStoryTools.cs               # Create User Stories
│       │   ├── TaskTools.cs                    # Create Tasks
│       │   ├── WorkItemTools.cs                # List/Update work items
│       │   └── SprintTools.cs                  # Sprint planning
│       │
│       ├── 💼 Application/                     # Serviços de negócio
│       │   ├── EpicGeneratorService.cs         # Lógica de geração de Epics
│       │   ├── FeatureGeneratorService.cs      # Lógica de geração de Features
│       │   ├── TaskBreakdownService.cs         # Quebra tasks em subtasks
│       │   └── SprintPlannerService.cs         # Planejamento de sprints
│       │
│       ├── 📊 Domain/                          # Modelos de domínio
│       │   └── Entities/
│       │       ├── Epic.cs                     # Entidade Epic
│       │       ├── Feature.cs                  # Entidade Feature
│       │       ├── UserStory.cs                # Entidade User Story
│       │       └── TaskItem.cs                 # Entidade Task
│       │
│       ├── 🔌 Infrastructure/                  # Integrações externas
│       │   └── AzureDevOps/
│       │       ├── AzureDevOpsClient.cs        # Cliente Azure DevOps
│       │       ├── WorkItemService.cs          # Serviço de Work Items
│       │       └── ProjectService.cs           # Serviço de Projetos
│       │
│       ├── 🔐 Security/                        # Segurança
│       │   ├── TokenProvider.cs                # Gerencia PAT token
│       │   └── PermissionGuard.cs              # Controle de permissões
│       │
│       ├── 📦 Properties/
│       │   └── AssemblyInfo.cs                 # Metadados do assembly
│       │
│       └── AzureDevOps.AI.McpServer.csproj     # Configuração do projeto
│
└── tests/                                      # 🧪 Testes unitários
    └── AzureDevOps.AI.McpServer.Tests/
        ├── 🔐 Security/
        │   └── TokenProviderTests.cs
        ├── 🏗️  Infrastructure/
        │   ├── AzureDevOpsClientTests.cs
        │   └── WorkItemServiceTests.cs
        ├── 💼 Application/
        │   ├── EpicGeneratorServiceTests.cs
        │   └── FeatureGeneratorServiceTests.cs
        ├── 🔧 McpTools/
        │   ├── ProjectToolsTests.cs
        │   ├── EpicToolsTests.cs
        │   ├── FeatureToolsTests.cs
        │   ├── UserStoryToolsTests.cs
        │   ├── TaskToolsTests.cs
        │   └── WorkItemToolsTests.cs
        ├── 🎁 Fixtures/
        │   └── MockLoggerFixture.cs
        └── AzureDevOps.AI.McpServer.Tests.csproj
```

### 📌 Explicação das Camadas

| Pasta | Responsabilidade | Exemplo |
|-------|------------------|---------|
| **McpTools** | Interfaces com agentes de IA | `CreateEpic()` recebe parâmetros JSON, valida e retorna ID |
| **Application** | Lógica de negócios | Gerar estrutura de Features a partir de um objetivo |
| **Domain** | Modelos de dados puros | Classes `Epic`, `Feature` sem dependências |
| **Infrastructure** | Integração com APIs externas | Chamar Azure DevOps REST API |
| **Security** | Autenticação e autorização | Validar PAT token, bloquear operações perigosas |
| **Host** | Configuração e startup | Registrar serviços, inicializar MCP |

---

## 🛠️ Stack de Tecnologias

| Tecnologia | Versão | Propósito |
|-----------|--------|----------|
| **.NET** | 10.0 | Runtime |
| **ASP.NET Core** | 10.0 | Framework web |
| **C#** | 14 | Linguagem |
| **ModelContextProtocol** | 0.2.0-preview.3 | SDK MCP |
| **Microsoft.TeamFoundationServer.Client** | 19.225.1 | Cliente Azure DevOps |
| **Microsoft.Extensions** | 9.0.0 | Logging, DI |

---

## 🏃 Como Executar

### Modo Local (HTTP)

```bash
# 1. Configurar variáveis de ambiente (ver seção Configuração)

# 2. Restaurar dependências
dotnet restore

# 3. Executar em desenvolvimento
dotnet run --project src/AzureDevOps.AI.McpServer

# 4. Testar
curl http://localhost:5050/health
# Resposta: {"status":"ok","timestamp":"2024-01-15T10:30:00Z"}
```

**Portas padrão:**
- `5050` - Servidor HTTP principal

### Modo Stdio (MCP CLI)

```bash
# Configurar para usar stdio (padrão MCP)
$env:MCP_TRANSPORT = "stdio"

# Executar
dotnet run --project src/AzureDevOps.AI.McpServer

# O servidor aguarda stdin/stdout para comunicação MCP
```

### Com Docker

```bash
# 1. Build da imagem
docker build -t azure-devops-mcp-server:latest .

# 2. Executar container
docker run -d \
  --name mcp-server \
  -p 5050:5050 \
  -e AZURE_DEVOPS_PAT="seu-token" \
  -e AZURE_DEVOPS_ORG="https://dev.azure.com/org" \
  -e AZURE_DEVOPS_PROJECT="projeto" \
  azure-devops-mcp-server:latest

# 3. Verificar logs
docker logs mcp-server

# 4. Parar container
docker stop mcp-server
```

### Com MCP Client (Copilot CLI) + Docker Desktop

Para usar o servidor MCP com **GitHub Copilot CLI** ou outro cliente MCP rodando localmente no Docker Desktop, siga os passos abaixo:

#### Passo 1: Criar Arquivo `.env`

Crie um arquivo `.env` **na raiz do projeto** com suas credenciais:

```bash
# .env (NUNCA commite este arquivo - adicione ao .gitignore)
AZURE_DEVOPS_PAT=seu-token-pat-aqui
AZURE_DEVOPS_ORG=https://dev.azure.com/seu-org
AZURE_DEVOPS_PROJECT=seu-projeto
MCP_TRANSPORT=stdio
ASPNETCORE_ENVIRONMENT=Production
```

**⚠️ Importante:**
- Adicione `.env` ao `.gitignore` para não expor suas credenciais
- O `MCP_TRANSPORT=stdio` é obrigatório para modo MCP Client
- Substitua os valores pelos seus dados reais

#### Passo 2: Build da Imagem Docker

```bash
# Navegar até a raiz do projeto
cd DotnetMCPServer

# Build da imagem
docker build -t azure-devops-mcp-server:latest .
```

#### Passo 3: Configurar Cliente MCP

Edite `.vscode/mcp.json` (ou arquivo `claude_desktop_config.json` para Claude Desktop):

**Arquivo: `.vscode/mcp.json`**

```json
{
  "servers": {
    "azure-devops-mcp-server": {
      "command": "docker",
      "args": [
        "run",
        "--rm",
        "-i",
        "--name",
        "azure-devops-mcp-server",
        "--env-file",
        ".env",
        "-e",
        "MCP_TRANSPORT=stdio",
        "azure-devops-mcp-server:latest"
      ],
      "env": {}
    }
  }
}
```

**Campo por campo:**
- `"command": "docker"` - Usa Docker para executar o servidor
- `"--env-file", ".env"` - Carrega variáveis do arquivo `.env`
- `"-i"` - Modo interativo para stdin/stdout
- `"--rm"` - Remove container automaticamente após parar
- `"MCP_TRANSPORT=stdio"` - Protocolo MCP padrão (entrada/saída)

#### Passo 4: Iniciar ClienteMCP

**Para GitHub Copilot CLI:**

```bash
# Abrir Copilot CLI
copilot

# O servidor MCP Docker será iniciado automaticamente
# Você pode usar as ferramentas:
# - ado_project_list
# - ado_epic_create
# - ado_backlog_generate
# etc.
```

**Para Claude Desktop (se usar integração):**

1. Localize `claude_desktop_config.json` no seu diretório de configuração:
   - **Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
   - **macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`

2. Adicione a configuração:

```json
{
  "mcpServers": {
    "azure-devops-mcp-server": {
      "command": "docker",
      "args": [
        "run",
        "--rm",
        "-i",
        "--name",
        "azure-devops-mcp-server",
        "--env-file",
        "path/to/.env",
        "-e",
        "MCP_TRANSPORT=stdio",
        "azure-devops-mcp-server:latest"
      ]
    }
  }
}
```

3. Reinicie o Claude Desktop

#### Passo 5: Verificar Conexão

O cliente MCP deve exibir as ferramentas disponíveis automaticamente:

```
✅ azure-devops-mcp-server conectado
   Ferramentas disponíveis:
   - ado_project_list
   - ado_project_create
   - ado_epic_create
   - ado_feature_create
   - ado_userstory_create
   - ado_task_create
   - ado_workitem_list
   - ado_workitem_update
   - ado_workitem_link
   - ado_backlog_generate
   - ado_task_breakdown
   - ado_sprint_plan
```

---

#### Troubleshooting - MCP Client com Docker

| Problema | Causa | Solução |
|----------|-------|---------|
| "Docker command not found" | Docker não está no PATH | Instale Docker Desktop ou adicione ao PATH |
| "No such file: .env" | Arquivo `.env` não existe | Crie `.env` na raiz do projeto |
| "Connection refused" | Servidor não iniciou | Verifique `docker logs` e credenciais |
| "Unauthorized 401" | PAT token inválido | Regenere o token em Azure DevOps |
| Container sai imediatamente | Erro durante startup | Execute `docker run -ti` para ver erros |

**Ver logs em tempo real:**

```bash
# Se o container ainda está rodando
docker logs -f azure-devops-mcp-server

# Se parou, reconstrua com output detalhado
docker run -ti \
  --env-file .env \
  -e MCP_TRANSPORT=stdio \
  azure-devops-mcp-server:latest
```

---

## 🧪 Executar Testes

### Testes Unitários

```bash
# Executar todos os testes
dotnet test

# Com detalhes verbosos
dotnet test -v detailed

# Apenas um arquivo de teste
dotnet test tests/AzureDevOps.AI.McpServer.Tests/Security/TokenProviderTests.cs

# Filtrar por categoria
dotnet test --filter "FullyQualifiedName~Security"

# Com cobertura de código
dotnet test /p:CollectCoverage=true
```

### Estrutura de Testes

```
✅ Security (4 testes)
   └─ TokenProvider: validação de tokens PAT

✅ Infrastructure (11 testes)
   ├─ AzureDevOpsClient: comunicação com API
   └─ WorkItemService: operações de work items

✅ Application (11 testes)
   ├─ EpicGeneratorService: geração de Epics
   └─ FeatureGeneratorService: geração de Features

✅ McpTools (31 testes)
   ├─ ProjectTools: create/list projects
   ├─ EpicTools: create Epics
   ├─ FeatureTools: create Features
   ├─ UserStoryTools: create User Stories
   └─ TaskTools: create Tasks

TOTAL: 57 testes ✅
```

---

## 🔌 Endpoints Disponíveis

### REST API (Modo HTTP)

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| `GET` | `/health` | Health check |
| `GET` | `/api/info` | Informações do servidor |

### MCP Tools

Ferramentas disponíveis para agentes de IA:

**Projetos:**
- `ado_project_list` - Listar todos os projetos
- `ado_project_get` - Obter detalhes de um projeto
- `ado_project_create` - Criar novo projeto

**Work Items:**
- `ado_epic_create` - Criar Epic
- `ado_feature_create` - Criar Feature
- `ado_userstory_create` - Criar User Story
- `ado_task_create` - Criar Task
- `ado_workitem_list` - Listar work items
- `ado_workitem_update` - Atualizar work item
- `ado_workitem_link` - Vincular work items

**Automação:**
- `ado_backlog_generate` - Gerar backlog automático
- `ado_task_breakdown` - Quebrar feature em tasks
- `ado_sprint_plan` - Planejar sprint

---

## 📖 Exemplo de Uso

### Cenário: Criar Backlog Automático

Um agente de IA recebe a solicitação: "Crie um backlog para implementar autenticação OAuth2"

**Fluxo automático:**

```
┌─────────────────────────────────────────┐
│ Agente IA                               │
│ "Implementar autenticação OAuth2"       │
└─────────────┬───────────────────────────┘
              │
              v
┌─────────────────────────────────────────┐
│ MCP Server (ado_backlog_generate)       │
│                                         │
│ Recebe: descrição de meta              │
│ Gerencia: AutoGeneratorService          │
└─────────────┬───────────────────────────┘
              │
              v
┌─────────────────────────────────────────┐
│ Azure DevOps REST API                   │
│                                         │
│ Cria:                                   │
│ ─ Epic: OAuth2 Authentication          │
│ ─ Feature: User Registration           │
│ ─ Feature: Social Login                │
│ ─ Feature: Token Management            │
│ ─ 12 User Stories                      │
│ ─ 25 Tasks                             │
└─────────────┬───────────────────────────┘
              │
              v
┌─────────────────────────────────────────┐
│ Resultado                               │
│ {                                       │
│   "epicId": 12345,                     │
│   "featureIds": [12346, 12347, ...],   │
│   "totalItems": 40,                    │
│   "url": "https://dev.azure.com/..."   │
│ }                                       │
└─────────────────────────────────────────┘
```

---

## 🔐 Segurança

### Práticas Implementadas

✅ **Token PAT isolado**: Variável de ambiente, nunca hardcoded
✅ **PermissionGuard**: Bloqueia operações perigosas
✅ **Validation**: Todas as entradas são validadas
✅ **Logging**: Auditoria de todas as operações mutantes
✅ **Erro seguro**: Mensagens de erro não expõem detalhes internos

### Restrições de Acesso

O `PermissionGuard` **bloqueia automaticamente:**
- ❌ Deletar projetos
- ❌ Deletar backlogs inteiros
- ❌ Bulk-delete de work items

---

## 🐳 Docker Compose

Para executar localmente com Docker Compose:

```yaml
# docker-compose.yml
version: '3.8'
services:
  mcp-server:
    build: .
    ports:
      - "5050:5050"
    environment:
      AZURE_DEVOPS_PAT: ${AZURE_DEVOPS_PAT}
      AZURE_DEVOPS_ORG: ${AZURE_DEVOPS_ORG}
      AZURE_DEVOPS_PROJECT: ${AZURE_DEVOPS_PROJECT}
      ASPNETCORE_ENVIRONMENT: Development
```

**Executar:**

```bash
docker-compose up -d
docker-compose logs -f
docker-compose down
```

---

## 📚 Documentação Adicional

| Documento | Descrição |
|-----------|-----------|
| [00_COMECE_AQUI.md](docs/00_COMECE_AQUI.md) | Guia de início para novos desenvolvedores |
| [QUICKSTART.md](docs/QUICKSTART.md) | Instruções rápidas de execução |
| [MCP_CLIENT_SETUP.md](docs/MCP_CLIENT_SETUP.md) | **Configurar cliente MCP com Docker Desktop** ⭐ |
| [STRUCTURE.md](docs/STRUCTURE.md) | Detalhes completos da estrutura |
| [DOCKER_MCP_SETUP.md](docs/DOCKER_MCP_SETUP.md) | Setup Docker para MCP |
| [EXTENSION_GUIDE.md](docs/EXTENSION_GUIDE.md) | Como expandir com novas ferramentas |
| [TESTS_CATALOG.md](docs/TESTS_CATALOG.md) | Catálogo completo de testes |

---

## 🤝 Contribuindo

1. Clone o repositório
2. Crie uma branch: `git checkout -b feature/sua-feature`
3. Commit suas mudanças: `git commit -m 'Add feature'`
4. Push para branch: `git push origin feature/sua-feature`
5. Abra um Pull Request

### Padrões de Código

- ✅ C# 14 com latest language features
- ✅ PascalCase para nomes de classe/método
- ✅ camelCase para variáveis locais
- ✅ XML docs para métodos públicos
- ✅ Unit tests para novas funcionalidades

---

## ❓ Troubleshooting

### Erro: "401 Unauthorized"

**Causa**: Token PAT inválido ou ausente

**Solução:**
```bash
# Verificar variável de ambiente
echo $env:AZURE_DEVOPS_PAT  # PowerShell
echo $AZURE_DEVOPS_PAT      # Bash

# Gerar novo token em https://dev.azure.com
```

### Erro: "Project not found"

**Causa**: Projeto não existe em Azure DevOps

**Solução:**
```bash
# Verificar nome do projeto
$env:AZURE_DEVOPS_PROJECT = "seu-projeto-exato"

# Pode conter espaços e caracteres especiais
```

### Server não inicia em Docker

**Causa**: Portas ocupadas ou variáveis de ambiente faltando

**Solução:**
```bash
# Liberar porta 5050
docker ps
docker stop <container-id>

# Ou usar porta diferente
docker run -p 5051:5050 azure-devops-mcp-server
```

---

## 📞 Suporte

- 🐛 Issues: GitHub Issues
- 💬 Discussões: GitHub Discussions
- 📖 Documentação: `/docs`

---

## 📄 Licença

Este projeto está licenciado sob a **MIT License** - veja o arquivo [LICENSE](LICENSE) para detalhes.

---

## 🙏 Agradecimentos

- [Model Context Protocol](https://modelcontextprotocol.io/) - Protocolo base
- [Azure DevOps](https://dev.azure.com/) - API backend
- [Microsoft .NET Team](https://dotnet.microsoft.com/) - Runtime
- Comunidade open source

---

**Versão**: 1.0.0  
**Última atualização**: Março 2024  
**Mantido por**: Seu Nome/Organização

---

## 🎉 O que foi adicionado (Março 2024)

### 📄 Novos Arquivos de Documentação

✅ **[docs/MCP_CLIENT_SETUP.md](docs/MCP_CLIENT_SETUP.md)** 
- Guia completo de configuração de cliente MCP com Docker Desktop
- Instruções para Copilot CLI, Claude Desktop, Cursor Editor
- Debugging e troubleshooting detalhado
- Exemplos práticos end-to-end

### 📋 Arquivos Atualizados

✅ **[.env.example](.env.example)**
- Melhorado com comentários explicativos
- Exemplo de como carregar em cada sistema operacional
- Documentação de cada variável de ambiente

✅ **[README.md](README.md)** (este arquivo)
- Adicionado **⚡ Quick Start com MCP Client** 
- Seção completa: **"Com MCP Client (Copilot CLI) + Docker Desktop"**
- Instruções passo a passo de configuração
- Tabela de troubleshooting
- Referência ao arquivo .env.example

### 🔑 Fluxo Recomendado para Iniciar

1. **Primeira vez?** → Leia [README.md](README.md) seção "Configuração Rápida"
2. **Quer usar MCP Client?** → Siga [docs/MCP_CLIENT_SETUP.md](docs/MCP_CLIENT_SETUP.md)
3. **Rodar com Docker?** → Use arquivo `.env` + `docker build` + `docker run`
4. **Testes e validação?** → Execute `dotnet test` (57 testes unitários)
