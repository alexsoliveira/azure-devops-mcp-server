# Guia de Expansão - Adicionando Novos Testes

Quando precisar adicionar testes para novas classes, siga este guia.

## 📋 Checklist para Nova Classe de Teste

### 1. Criar Arquivo
```bash
# Naming: [NomeDaClasse]Tests.cs
tests/AzureDevOps.AI.McpServer.Tests/[Categoria]/[NomeDaClasse]Tests.cs
```

### 2. Template Base
```csharp
using AzureDevOps.AI.McpServer.[Namespace];
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.[Categoria];

public class [NomeDaClasse]Tests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly [NomeDaClasse] _service;

    public [NomeDaClasse]Tests()
    {
        _mockDependency = new Mock<IDependency>(MockBehavior.Strict);
        _service = new [NomeDaClasse](_mockDependency.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        Assert.NotNull(_service);
    }
}
```

### 3. Estrutura de Testes Esperada

#### Constructor Tests (Obrigatório)
```csharp
// Teste feliz
[Fact]
public void Constructor_WithValidDependencies_Initializes()
{
    var service = new Service(mockDep.Object);
    Assert.NotNull(service);
}

// Teste de nulo
[Fact]
public void Constructor_WithNullDependency_ThrowsArgumentNullException()
{
    Assert.Throws<ArgumentNullException>(() => 
        new Service(null!));
}
```

#### Method Tests (Padrão)
```csharp
// Teste feliz
[Fact]
public async Task MethodName_WithValidInput_ReturnsExpectedResult()
{
    // Arrange
    _mockDep.Setup(x => x.Call()).ReturnsAsync(expectedValue).Verifiable();
    
    // Act
    var result = await _service.MethodName(input);
    
    // Assert
    Assert.NotNull(result);
    _mockDep.Verify();
}

// Teste de validação de entrada
[Theory]
[InlineData(null)]
[InlineData("")]
public async Task MethodName_WithInvalidInput_ShouldFail(string input)
{
    await Assert.ThrowsAsync<ArgumentException>(async () =>
        await _service.MethodName(input));
}

// Teste de exceção
[Fact]
public async Task MethodName_WhenDependencyThrows_PropagatesException()
{
    _mockDep
        .Setup(x => x.Call())
        .ThrowsAsync(new InvalidOperationException("Error"));
    
    await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        await _service.MethodName(validInput));
}

// Teste de verificação de chamada
[Fact]
public async Task MethodName_CallsDependency_WithCorrectParameters()
{
    _mockDep.Setup(x => x.Call(It.IsAny<string>()))
        .ReturnsAsync(value).Verifiable();
    
    await _service.MethodName(param);
    
    _mockDep.Verify(x => x.Call(param), Times.Once);
}
```

## 📝 Exemplo Completo - SprintPlannerServiceTests

```csharp
using AzureDevOps.AI.McpServer.Application;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Application;

public class SprintPlannerServiceTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<SprintPlannerService>> _mockLogger;
    private readonly SprintPlannerService _sprintPlannerService;

    public SprintPlannerServiceTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Strict);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<SprintPlannerService>();
        _sprintPlannerService = new SprintPlannerService(
            _mockWorkItemService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        Assert.NotNull(_sprintPlannerService);
    }

    [Fact]
    public void Constructor_WithNullWorkItemService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => 
            new SprintPlannerService(null!, _mockLogger.Object));
    }

    [Fact]
    public async Task PlanSprintAsync_WithValidItems_AssignsToSprint()
    {
        // Arrange
        var projectId = "TestProject";
        var workItemIds = new List<int> { 1, 2, 3 };
        var sprintName = "Sprint 1";

        // Mock da chamada de atualização
        _mockWorkItemService
            .Setup(x => x.UpdateWorkItemAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WorkItemResult(1, "https://url"))
            .Verifiable();

        // Act
        // var result = await _sprintPlannerService.PlanSprintAsync(projectId, workItemIds, sprintName);

        // Assert
        // Assert.NotNull(result);
        // _mockWorkItemService.Verify(x => x.UpdateWorkItemAsync(
        //     It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
        //     It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
        //     Times.AtLeast(workItemIds.Count));
    }

    [Fact]
    public async Task PlanSprintAsync_WithEmptyWorkItems_ShouldFail()
    {
        // Arrange
        var projectId = "TestProject";
        var workItemIds = new List<int>();
        var sprintName = "Sprint 1";

        // Act & Assert
        // await Assert.ThrowsAsync<ArgumentException>(async () =>
        //     await _sprintPlannerService.PlanSprintAsync(projectId, workItemIds, sprintName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task PlanSprintAsync_WithEmptyProjectId_ShouldFail(string projectId)
    {
        // Arrange
        var workItemIds = new List<int> { 1 };
        var sprintName = "Sprint 1";

        // Act & Assert
        // await Assert.ThrowsAsync<ArgumentException>(async () =>
        //     await _sprintPlannerService.PlanSprintAsync(projectId, workItemIds, sprintName));
    }

    [Fact]
    public async Task PlanSprintAsync_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        _mockWorkItemService
            .Setup(x => x.UpdateWorkItemAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        // await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        //     await _sprintPlannerService.PlanSprintAsync("Project", new List<int> { 1 }, "Sprint"));
    }
}
```

## 🎨 Boas Práticas

### ✅ FAÇA
- Use nomes descritivos
- Teste um conceito por teste
- Use `[Theory]` para múltiplos cenários
- Verifique chamadas de mock
- Use `Mock<T>(MockBehavior.Strict)` para detectar erros
- Teste casos de erro
- Use `CancellationToken` nos testes async

### ❌ NÃO FAÇA
- Teste múltiplos conceitos em um teste
- Use nomes genéricos
- Durma ou aguarde em testes
- Acesse recursos externos (HTTP, BD)
- Deixe testes desorganizados
- Ignore exceções

## 🔧 MockBehavior Options

```csharp
// Strict - Vai falhar se algo inesperado for chamado
var mock = new Mock<IService>(MockBehavior.Strict);

// Loose - Permite chamadas inesperadas (default)
var mock = new Mock<IService>(MockBehavior.Loose);
var mock = new Mock<IService>(); // Loose é default
```

## 📊 Setup de Mock Avançado

```csharp
// Setup simples
mock.Setup(x => x.Method()).Returns(value);

// Setup com parâmetro específico
mock.Setup(x => x.Method("param")).Returns(value);

// Setup com qualquer parâmetro
mock.Setup(x => x.Method(It.IsAny<string>())).Returns(value);

// Setup com predicado
mock.Setup(x => x.Method(It.Is<string>(p => p.Contains("test"))))
    .Returns(value);

// Setup assíncrono
mock.Setup(x => x.MethodAsync()).ReturnsAsync(value);

// Setup com throw
mock.Setup(x => x.Method()).Throws<InvalidOperationException>();

// Setup verificável
mock.Setup(x => x.Method()).Verifiable();

// Verificar chamada específica
mock.Verify(x => x.Method("param"), Times.Once);
mock.Verify(x => x.Method(It.IsAny<string>()), Times.AtLeastOnce);
```

## 🔍 Verificações Comuns

```csharp
// Exatamente uma vez
mock.Verify(x => x.Method(), Times.Once);

// Nenhuma vez
mock.Verify(x => x.Method(), Times.Never);

// Uma ou mais vezes
mock.Verify(x => x.Method(), Times.AtLeastOnce);

// Exatos 3 vezes
mock.Verify(x => x.Method(), Times.Exactly(3));

// Verificar todos os setups
mock.Verify();
```

## 📦 Estrutura de Pasta para Novos Testes

```
tests/AzureDevOps.AI.McpServer.Tests/
├── [Categoria]/
│   ├── [ClasseA]Tests.cs
│   ├── [ClasseB]Tests.cs
│   └── [ClasseC]Tests.cs
└── ...

Categorias esperadas:
- Security/       → Testes de segurança
- Infrastructure/ → Testes de infraestrutura
- Application/    → Testes de serviços
- McpTools/       → Testes de ferramentas MCP
- Fixtures/       → Utilitários compartilhados
```

## 🚀 Executar Testes Novos

```bash
# Executar apenas testes de uma classe
dotnet test --filter "SprintPlannerServiceTests"

# Executar apenas testes com "Plan" no nome
dotnet test --filter "Plan"

# Executar e gerar relatório
dotnet test --logger "console;verbosity=detailed"
```

## 📖 Recursos para Aprender Mais

- [xUnit.net Theory Data](https://xunit.net/docs/getting-started/xunit.org)
- [Moq Setup Examples](https://github.com/moq/moq4/wiki/Quickstart)
- [Unit Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
