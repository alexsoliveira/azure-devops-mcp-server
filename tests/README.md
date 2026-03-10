# Azure DevOps MCP Server - Tests

## Estrutura de Testes

Esta pasta contém todos os testes de unidade e integração do projeto Azure DevOps AI MCP Server.

```
tests/
└── AzureDevOps.AI.McpServer.Tests/
    ├── AzureDevOps.AI.McpServer.Tests.csproj
    ├── Fixtures/                       # Utilitários compartilhados para testes
    │   └── MockLoggerFixture.cs        # Factory para criar mock loggers
    ├── Security/                       # Testes de camada de segurança
    │   └── TokenProviderTests.cs       # Testes de carregamento de PAT e variáveis de ambiente
    ├── Infrastructure/                 # Testes de camada de infraestrutura
    │   ├── AzureDevOpsClientTests.cs   # Testes de inicialização e acesso a clientes
    │   ├── AzureDevOpsClientIntegrationTests.cs  # Testes de integração com mocks
    │   └── WorkItemServiceTests.cs     # Testes CRUD de work items
    ├── Application/                    # Testes de serviços de aplicação
    │   ├── EpicGeneratorServiceTests.cs      # Testes de geração de Epics
    │   └── FeatureGeneratorServiceTests.cs   # Testes de geração de Features
    └── McpTools/                       # Testes de ferramentas MCP
        ├── ProjectToolsTests.cs        # Testes de projeto (list, get, create)
        ├── EpicToolsTests.cs           # Testes de criação de Epics
        ├── FeatureToolsTests.cs        # Testes de criação de Features
        ├── UserStoryToolsTests.cs      # Testes de criação de User Stories
        ├── TaskToolsTests.cs           # Testes de criação de Tasks
        └── WorkItemToolsTests.cs       # Testes de listagem, update, link
```

### Notas Importantes

- **Testes Unitários**: Usam mocks com `Moq` para isolar componentes individuais
- **Testes de Integração**: Testam múltiplos componentes juntos (veja `AzureDevOpsClientIntegrationTests.cs`)
- **Azure DevOps SDK**: Os mocks devem simular corretamente o comportamento do VssConnection e clientes REST
- **Structured Logging**: Todos os testes verificam logging via `ILogger<T>` mockado

## Stack de Testes

- **Framework**: xUnit 2.7.1
- **Mocking**: Moq 4.20.70
- **Runtime**: .NET 10.0
- **SDK**: Microsoft.NET.Test.SDK 17.9.0
- **Test Runner**: xunit.runner.visualstudio 2.5.6

## Convenções de Teste

### Nomenclatura
```
[NomeDaClasse]Tests.cs          // Arquivo de teste
MethodName_Condition_Expected    // Método de teste
```

### Padrão AAA (Arrange-Act-Assert)
```csharp
[Fact]
public async Task MethodName_Condition_Expected()
{
    // Arrange - Setup inicial
    var dependency = new Mock<IDependency>();
    var service = new Service(dependency.Object);
    
    // Act - Executar o código sob teste
    var result = await service.MethodAsync(parameter);
    
    // Assert - Verificar resultado
    Assert.NotNull(result);
    dependency.Verify();
}
```

### Characteristics dos Testes
- Um conceito por teste
- Nomes descritivos
- Sem dependências de estado compartilhado
- Uso de `Mock<T>` para objetos externos
- Verificação com `Verify()`

## Exemplos de Padrões Implementados

### 1. Testes de Segurança (TokenProviderTests)
Valida carregamento de variáveis de ambiente:
```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
public void Constructor_WithInvalidEnvVar_ThrowsException(string value)
```

### 2. Testes de Infraestrutura (AzureDevOpsClientTests)
Testa inicialização e acesso a clientes Azure DevOps:
```csharp
[Fact]
public async Task GetWorkItemTrackingClientAsync_ReturnsClient()
```

### 3. Testes de Aplicação (EpicGeneratorServiceTests)
Valida lógica de negócio com mocks:
```csharp
[Fact]
public async Task GenerateEpicAsync_WithValidParameters_CallsWorkItemService()
{
    // Mock setup
    _mockWorkItemService
        .Setup(x => x.CreateWorkItemAsync(...))
        .ReturnsAsync(expectedResult)
        .Verifiable();
    
    // Act
    var result = await _epicGeneratorService.GenerateEpicAsync(...);
    
    // Assert
    _mockWorkItemService.Verify();
}
```

### 4. Testes de MCP Tools (WorkItemToolsTests)
Testa integração de ferramentas MCP:
```csharp
[Fact]
public async Task ListWorkItems_WithFilters_CallsWorkItemService()
```

## Executando os Testes

### Linha de Comando
```bash
# Executar todos os testes
dotnet test

# Executar testes do projeto específico
dotnet test tests/AzureDevOps.AI.McpServer.Tests/AzureDevOps.AI.McpServer.Tests.csproj

# Executar com detalhes verbose
dotnet test -v detailed

# Executar uma classe de teste específica
dotnet test --filter "ClassName"

# Executar múltiplos filtros
dotnet test --filter "ClassName|MethodName"

# Executar com cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover

# Executar testes e gerar relatório HTML
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover /p:ExcludeByAttribute=GeneratedCodeAttribute
```

### Exemplos Práticos
```bash
# Rodar testes de TokenProvider
dotnet test --filter "TokenProviderTests"

# Rodar testes de segurança
dotnet test --filter "Security"

# Rodar testes de MCP Tools
dotnet test --filter "McpTools"

# Rodar testes com pattern no nome do método
dotnet test --filter "Constructor"
```

## Cobertura de Testes

As classes atualmente com testes implementados:

### Security
- ✅ `TokenProvider` - 100%

### Infrastructure
- ✅ `AzureDevOpsClient` - Testes unitários + integração
- ✅ `WorkItemService` - Testes com mocking

### Application
- ✅ `EpicGeneratorService` - Completo
- ✅ `FeatureGeneratorService` - Completo

### MCP Tools
- ✅ `ProjectTools` - Completo
- ✅ `EpicTools` - Completo
- ✅ `FeatureTools` - Completo
- ✅ `UserStoryTools` - Completo
- ✅ `TaskTools` - Completo
- ✅ `WorkItemTools` - Completo

### Pendente de Testes
- ⚠️ `TaskBreakdownService` - Estrutura pronta
- ⚠️ `SprintPlannerService` - Estrutura pronta
- ⚠️ `ProjectService` - Estrutura pronta
- ⚠️ `PermissionGuard` - Não implementado
- ⚠️ `SprintTools` - Não implementado

## Próximos Passos

1. **Completar cobertura de testes para:**
   - `TaskBreakdownService` - Implementar testes
   - `SprintPlannerService` - Implementar testes
   - `ProjectService` - Implementar testes
   - `PermissionGuard` - Criar teste para bloqueio de operações perigosas
   - `SprintTools` - Implementar testes MCP

2. **Melhorar cobertura existente com:**
   - Testes de cenários de erro e exceções
   - Testes de edge cases
   - Testes de validação de entrada

3. **Implementar testes de integração:**
   - `WorkItemService` com Azure DevOps REST API simulado
   - `ProjectService` com mocking de clientes Azure DevOps
   - Fluxos completos: Epic → Feature → User Story → Task

4. **Configurar CI/CD:**
   - GitHub Actions com execução automática de testes
   - Relatório de cobertura de código (OpenCover)
   - Bloqueio de merge se testes falharem

5. **Adicionar testes de performance:**
   - Validar tempo de resposta de operações MCP
   - Testar comportamento sob carga

6. **Documentação adicional:**
   - Adicionar exemplos de testes com Azure DevOps SDK reais
   - Documentar como fazer mocking de VssConnection

## Debugging de Testes

### Breakpoints
1. Colocar breakpoint na linha desejada
2. Clicar "Debug Test" no Test Explorer
3. Depurador abrirá automaticamente

### Via Linha de Comando
```bash
# Executar teste com debugging (aguarda debugger se VSCode está aberto)
dotnet test --logger "console;verbosity=detailed"

# Executar com parado em exceção
dotnet test -v detailed 2>&1 | Select-String "FAILED|ERROR"
```

### Logging em Testes
```csharp
// Criar mock logger com tracking
var logger = MockLoggerFixture.CreateMockLoggerWithTracking<MyService>();

// Verificar logs
logger.Verify(x => x.Log(
    It.IsAny<LogLevel>(),
    It.IsAny<EventId>(),
    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Expected message")),
    It.IsAny<Exception>(),
    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
    Times.AtLeastOnce);
```

### Inspecionando Falhas
```bash
# Executar teste com output detalhado
dotnet test --logger "console;verbosity=detailed" 

# Capturar output em arquivo
dotnet test > test-output.txt 2>&1

# Filtrar apenas erros
dotnet test 2>&1 | findstr /I "FAILED ERROR PASSED"
```

## Troubleshooting de Testes

### Problema: "DLL not found" ou Import errors
**Solução:**
```bash
dotnet restore tests/AzureDevOps.AI.McpServer.Tests/
dotnet clean
dotnet build
```

### Problema: Testes falhando com "Package not found"
**Solução:**
```bash
# Verificar se o projeto principal foi compilado
dotnet build src/AzureDevOps.AI.McpServer/

# Restaurar nugets
dotnet restore
```

### Problema: Timeout em testes de integração
**Solução:**
```bash
# Aumentar timeout
dotnet test --logger "console;verbosity=detailed" -- RunConfiguration.TestSessionTimeout=30000
```

### Problema: Mock não está funcionando com VssConnection
**Causa:** VssConnection é sealed, difícil fazer mock direto

**Solução:** 
```csharp
// Mockar os clientes HTTP clients, não VssConnection
var mockWorkItemClient = new Mock<WorkItemTrackingHttpClient>(
    new Uri("https://dev.azure.com/"), 
    new VssCredentials());

// Ou usar padrão de Interface wrapper
var mockAzureDevOpsClient = new Mock<AzureDevOpsClient>();
mockAzureDevOpsClient
    .Setup(x => x.GetWorkItemTrackingClientAsync())
    .ReturnsAsync(mockWorkItemClient.Object);
```

## Recursos Úteis

- [xUnit.net Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4/wiki/Quickstart)
- [Azure DevOps REST API](https://docs.microsoft.com/en-us/rest/api/azure/devops/)
- [Microsoft Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [MCP Protocol Specification](https://modelcontextprotocol.io/)

## Padrões Específicos para Testes MCP

### Testando Tools MCP
```csharp
[Fact]
public async Task CreateEpic_WithValidInput_ReturnsWorkItemResult()
{
    // Arrange
    var mockService = new Mock<IWorkItemService>();
    mockService
        .Setup(x => x.CreateWorkItemAsync(
            It.IsAny<string>(),      // project
            It.IsAny<string>(),      // workItemType
            It.IsAny<string>(),      // title
            It.IsAny<string>(),      // description
            It.IsAny<int?>(),        // parentId
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(new WorkItemResult(123, "https://example.com/item/123"));
    
    var logger = new Mock<ILogger<EpicTools>>();
    var tool = new EpicTools(mockService.Object, logger.Object);
    
    // Act
    var result = await tool.CreateEpic("MyProject", "Epic Title", "Epic Desc");
    
    // Assert
    Assert.NotNull(result);
    Assert.Equal(123, result.Id);
    mockService.Verify(x => x.CreateWorkItemAsync(
        "MyProject", "Epic", "Epic Title", "Epic Desc", null, default), Times.Once);
}
```

### Testando Services de Aplicação
```csharp
[Fact]
public async Task GenerateEpicAsync_CallsWorkItemService_AndReturnsResult()
{
    // Arrange
    var mockWorkItemService = new Mock<IWorkItemService>();
    var mockLogger = new Mock<ILogger<EpicGeneratorService>>();
    
    mockWorkItemService
        .Setup(x => x.CreateWorkItemAsync(
            "TestProject", "Epic", "GeneratedEpic", "Description", null, default))
        .ReturnsAsync(new WorkItemResult(456, "https://example.com/item/456"));
    
    var service = new EpicGeneratorService(mockWorkItemService.Object, mockLogger.Object);
    
    // Act
    var result = await service.GenerateEpicAsync("TestProject", "GeneratedEpic", "Description");
    
    // Assert
    Assert.Equal(456, result.Id);
}
```

### Verificação de Logging
```csharp
[Fact]
public async Task CreateEpic_LogsAppropriateLevels()
{
    var mockLogger = new Mock<ILogger<EpicTools>>();
    var mockService = new Mock<IWorkItemService>();
    
    mockService
        .Setup(x => x.CreateWorkItemAsync(
            It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), 
            null, default))
        .ReturnsAsync(new WorkItemResult(789, "https://example.com/item/789"));
    
    var tool = new EpicTools(mockService.Object, mockLogger.Object);
    
    // Act
    await tool.CreateEpic("Project", "Title", "Desc");
    
    // Assert - Verifica que LogInformation foi chamado
    mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.AtLeastOnce);
}
```
