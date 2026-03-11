# Configurando Cliente MCP com Docker Desktop

Este guia detalha como configurar clientes MCP (GitHub Copilot CLI, Claude Desktop, etc.) para usar o servidor Azure DevOps rodando em Docker Desktop.

---

## 📋 Visão Geral

```
┌──────────────────────────┐
│   Cliente MCP            │
│ (Copilot CLI/Claude)     │
└────────────┬─────────────┘
             │ lee .json config
             │
┌────────────▼─────────────┐
│  Docker (stdio)          │
│  azure-devops-mcp-server │
└────────────┬─────────────┘
             │ REST API
             │
┌────────────▼─────────────┐
│   Azure DevOps           │
│   REST API               │
└──────────────────────────┘
```

---

## ⚙️ Pré-requisitos

- ✅ Docker Desktop instalado e rodando
- ✅ Arquivo `.env` configurado (ver [README.md](../README.md#configuração-rápida))
- ✅ Imagem Docker build: `azure-devops-mcp-server:latest`
- ✅ Cliente MCP instalado (Copilot CLI, Claude Desktop, etc.)

---

## 🚀 Passo a Passo

### 1️⃣ Preparar o Arquivo `.env`

```bash
# Na raiz do projeto
cp .env.example .env

# Editar .env com suas credenciais:
# Abra em seu editor favorito e preencha:
```

**Exemplo:**
```bash
AZURE_DEVOPS_PAT=your-personal-access-token-here
AZURE_DEVOPS_ORG=https://dev.azure.com/sua-organizacao
AZURE_DEVOPS_PROJECT=seu-projeto
MCP_TRANSPORT=stdio
ASPNETCORE_ENVIRONMENT=Production
```

⚠️ **Segurança**: O arquivo `.env` está no `.gitignore` - nunca será commitado!

### 2️⃣ Build da Imagem Docker

```bash
# Navegar para raiz do projeto
cd DotnetMCPServer

# Build com tag
docker build -t azure-devops-mcp-server:latest .

# Verificar que build foi bem-sucedido
docker images | grep azure-devops-mcp-server
```

### 3️⃣ Configurar Arquivo MCP JSON

Dependendo do cliente, a configuração varia:

#### Para GitHub Copilot CLI

**Arquivo**: `.vscode/mcp.json` (no root do projeto)

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

#### Para Claude Desktop

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
**Linux**: `~/.config/Claude/claude_desktop_config.json`

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
        "/caminho/para/seu/projeto/.env",
        "-e",
        "MCP_TRANSPORT=stdio",
        "azure-devops-mcp-server:latest"
      ]
    }
  }
}
```

⚠️ **Formato de caminho relativo vs absoluto**:
- Copilot CLI: Use caminho relativo (`.env`)
- Claude Desktop: Use caminho absoluto (`/path/to/.env`)

#### Para Cursor Editor

**Arquivo**: `.cursor/settings.json`

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
        ".env",
        "-e",
        "MCP_TRANSPORT=stdio",
        "azure-devops-mcp-server:latest"
      ]
    }
  }
}
```

### 4️⃣ Verificar Conexão

#### GitHubCopilot CLI

```bash
# Abrir Copilot CLI
copilot

# Você deve ver no output:
# ✅ azure-devops-mcp-server: connected
# Available tools:
#   - ado_project_list
#   - ado_epic_create
#   - ado_backlog_generate
#   ... (e mais)
```

#### Claude Desktop

1. Abra Claude Desktop
2. Clique no ícone de **Settings** (⚙️)
3. Vá para **Developer** > **Model Contexts**
4. Você deve ver: **azure-devops-mcp-server** com status ✅

---

## 🔍 Debugging

### Verificar se Docker está rodando

```bash
docker ps
# Se vazio = Docker não está rodando
# Abra Docker Desktop
```

### Testar container manualmente

```bash
# Executar container interativo para debug
docker run -it \
  --env-file .env \
  -e "MCP_TRANSPORT=stdio" \
  azure-devops-mcp-server:latest

# Se funcionar, você verá JSON de ferramentas MCP no stdout
# Pressione Ctrl+C para sair
```

### Ver logs do container

```bash
# Se o container está rodando em background
docker logs -f azure-devops-mcp-server

# Ver últimas 50 linhas
docker logs --tail 50 azure-devops-mcp-server
```

### Erro: "Docker command not found"

**Causa**: Docker não está no PATH do sistema

**Solução**:
1. Verifique que Docker Desktop está instalado
2. Reinicie o terminal/IDE
3. Adicione Docker ao PATH:
   - Windows: Sistema > Variáveis de Ambiente > PATH > incluir pasta do Docker

### Erro: "No such file: .env"

**Causa**: Arquivo `.env` não existe ou caminho está errado

**Solução**:
```bash
# Verificar se existe
ls -la .env  # Linux/macOS
dir .env     # Windows

# Criar se não existir
cp .env.example .env
```

### Erro: "Connection refused" ou "401 Unauthorized"

**Causa**: Credenciais inválidas ou token expirado

**Solução**:
1. Regenere o Personal Access Token em Azure DevOps
2. Atualize o arquivo `.env`:
   ```bash
   AZURE_DEVOPS_PAT=novo-token-aqui
   ```
3. Reconstrua o container:
   ```bash
   docker stop azure-devops-mcp-server 2>/dev/null
   docker run -it --env-file .env -e MCP_TRANSPORT=stdio azure-devops-mcp-server:latest
   ```

### Verificar Credenciais no Container

```bash
# Entrar no container rodando
docker exec -it azure-devops-mcp-server bash

# Ver variáveis carregadas
env | grep AZURE_DEVOPS

# Sair
exit
```

---

## 📊 Tabela de Configurações

| Cliente | Arquivo Config | Caminho | Tipo de Caminho |
|---------|---|---|---|
| **Copilot CLI** | `.vscode/mcp.json` | Project root | Relativo |
| **Claude Desktop** | `claude_desktop_config.json` | User config dir | Absoluto |
| **Cursor Editor** | `.cursor/settings.json` | Project root | Relativo |
| **VSCode Extensions** | `.vscode/settings.json` | Project root | Relativo |

---

## 📝 Exemplo Completo: Fluxo de Uso

### 1. Copiar Arquivo `.env`

```bash
cp .env.example .env
```

### 2. Editar `.env`

```bash
vim .env  # ou seu editor
```

Conteúdo:
```
AZURE_DEVOPS_PAT=YOUR_PAT_TOKEN
AZURE_DEVOPS_ORG=https://dev.azure.com/your-org
AZURE_DEVOPS_PROJECT=your-project
MCP_TRANSPORT=stdio
ASPNETCORE_ENVIRONMENT=Production
```

### 3. Build Docker

```bash
docker build -t azure-devops-mcp-server:latest .
```

### 4. Configurar Cliente

Para Copilot CLI, criar `.vscode/mcp.json`:

```json
{
  "servers": {
    "azure-devops-mcp-server": {
      "command": "docker",
      "args": [
        "run", "--rm", "-i",
        "--name", "azure-devops-mcp-server",
        "--env-file", ".env",
        "-e", "MCP_TRANSPORT=stdio",
        "azure-devops-mcp-server:latest"
      ],
      "env": {}
    }
  }
}
```

### 5. Usar Cliente

```bash
copilot

# Agora você pode usar:
# "Create a backlog for OAuth2 authentication"
# Resposta: Cria Epic + Features + User Stories + Tasks automaticamente!
```

---

## ✅ Checklist de Verificação

- [ ] Docker Desktop instalado e rodando
- [ ] Arquivo `.env` criado e preenchido
- [ ] Imagem Docker build com sucesso
- [ ] Arquivo `.json` do cliente configurado
- [ ] Cliente MCP instalado
- [ ] Primeira execução testada (com debug se necessário)

---

## 🆘 Suporte

Se ainda tiver dúvidas:

1. **Verificar logs**: `docker logs azure-devops-mcp-server`
2. **Testar isolado**: Execute o container com `-t` e veja erros
3. **Consultar docs**:
   - [README.md](../README.md) - Visão geral
   - [DOCKER_MCP_SETUP.md](./DOCKER_MCP_SETUP.md) - Setup Docker
   - [MCP Oficial](https://modelcontextprotocol.io/) - Protocolo

---

## 📚 Referências

- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Docker Docs](https://docs.docker.com/)
- [GitHub Copilot CLI](https://github.com/github/copilot-cli)
- [Claude Desktop](https://claudedesktop.ai/)

---

**Última atualização**: Março 2024
**Status**: ✅ Production Ready
