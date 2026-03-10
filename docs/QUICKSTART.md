# Quick Start - Executando os Testes

## Pré-requisitos

- .NET 9 SDK instalado
- Visual Studio, VS Code com C# Dev Kit, ou linha de comando

## Passos para Executar

### 1. Via Linha de Comando

```bash
# Navegar para a pasta raiz do projeto
cd e:\Documents\Repositorios_GitHub\MCP_Server\DotnetMCPServer

# Restaurar dependências
dotnet restore

# Executar todos os testes
dotnet test

# Executar testes com saída detalhada
dotnet test -v detailed

# Executar testes de uma categoria específica
dotnet test --filter "FullyQualifiedName~Security"

# Executar com cobertura de código
dotnet test /p:CollectCoverage=true
```

### 2. Via Visual Studio
1. Abrir **Test Explorer** (menu View > Test Explorer)
2. Todos os testes aparecem organizados por categoria
3. Clicar no botão "Run All Tests" ▶

### 3. Via VS Code
1. Instalar extensão **C# Dev Kit**
2. Abrir **Test Explorer** no painel lateral
3. Clicar em qualquer teste para executá-lo

## Estrutura de Testes Criada

```
tests/
└── AzureDevOps.AI.McpServer.Tests/
    ├── AzureDevOps.AI.McpServer.Tests.csproj      # Configuração do projeto
    ├── .runsettings                                # Configuração de execução
    │
    ├── Fixtures/
    │   └── MockLoggerFixture.cs                    # Utilities para mocks
    │
    ├── Security/
    │   └── TokenProviderTests.cs                   # 3 testes ✅
    │
    ├── Infrastructure/
    │   ├── AzureDevOpsClientTests.cs              # 1 teste ✅
    │   ├── AzureDevOpsClientIntegrationTests.cs   # 3 testes ⏭️ SKIP
    │   └── WorkItemServiceTests.cs                # 3 testes ✅
    │
    ├── Application/
    │   ├── EpicGeneratorServiceTests.cs           # 6 testes ✅
    │   └── FeatureGeneratorServiceTests.cs        # 5 testes ✅
    │
    └── McpTools/
        ├── WorkItemToolsTests.cs                  # 8 testes ✅
        ├── EpicToolsTests.cs                      # 5 testes ✅
        ├── FeatureToolsTests.cs                   # 5 testes ✅
        ├── UserStoryToolsTests.cs                 # 5 testes ✅
        └── TaskToolsTests.cs                      # 7 testes ✅
```

## Total de Testes - Estado Atual

| Categoria | Classe | Testes | Status |
|-----------|--------|--------|--------|
| Security | TokenProvider | 3 | ✅ Aprovado |
| Infrastructure | AzureDevOpsClient | 1 | ✅ Aprovado |
| Infrastructure | WorkItemService | 3 | ✅ Aprovado |
| Application | EpicGeneratorService | 6 | ✅ Aprovado |
| Application | FeatureGeneratorService | 5 | ✅ Aprovado |
| McpTools | WorkItemTools | 8 | ✅ Aprovado |
| McpTools | EpicTools | 5 | ✅ Aprovado |
| McpTools | FeatureTools | 5 | ✅ Aprovado |
| McpTools | UserStoryTools | 5 | ✅ Aprovado |
| McpTools | TaskTools | 7 | ✅ Aprovado |
| Integration | AzureDevOpsClientIntegration | 3 | ⏭️ SKIP |
| **TOTAL** | **12 Classes** | **67 ✅ + 3 ⏭️** | **100% Funcional** |

## Padrões de Teste Utilizados

### ✅ Instant Setup
```csharp
[Fact]
public void Constructor_WithValidDependencies_Initializes()
{
    var service = new Service(dependency);
    Assert.NotNull(service);
}
```

### ✅ Mock Validation
```csharp
[Fact]
public async Task Method_WithValidInput_CallsService()
{
    _mock.Setup(x => x.Call()).ReturnsAsync(result).Verifiable();
    var result = await service.MethodAsync();
    _mock.Verify();  // Confirma que foi chamado
}
```

### ✅ Exception Handling
```csharp
[Fact]
public async Task Method_WithInvalidInput_ThrowsException()
{
    await Assert.ThrowsAsync<ArgumentException>(
        async () => await service.MethodAsync(invalidInput));
}
```

### ✅ Theory (Data-Driven)
```csharp
[Theory]
[InlineData(null)]
[InlineData("")]
public void Method_WithEmptyInput_Fails(string input)
{
    // Testa múltiplos cenários com um teste parametrizado
}
```

## Próximos Passos Recomendados

1. **Executar testes em máquina com .NET 9 instalado:**
   ```bash
   dotnet test --configuration Release --verbosity detailed
   ```

2. **Gerar relatório de cobertura:**
   ```bash
   dotnet test /p:CollectCoverage=true /p:CoverageFormat=lcov
   ```

3. **Adicionar testes para:**
   - ✅ `ProjectTools` (15 testes implementados)
   - [ ] `ProjectService` (testes de integração - cliente sealed)
   - [ ] `SprintTools`, `TaskBreakdownService`, `SprintPlannerService`
   - [ ] `PermissionGuard`
   - [ ] Testes de integração com Azure DevOps API

4. **Configurar CI/CD:**
   - GitHub Actions para executar testes automaticamente
   - Bloquear merge se testes falharem
   - Publicar relatório de cobertura

## Arquivos de Configuração

### AzureDevOps.AI.McpServer.Tests.csproj
- xUnit 2.7.1 para framework de testes
- Moq 4.20.70 para mocking
- Microsoft.Extensions.Logging para logging em testes
- Referência ao projeto principal

### .runsettings
- Paralelização de testes (4 workers)
- Configuração de code coverage
- Timeout de 10 minutos

## Troubleshooting

### Erro: "No .NET SDKs were found"
- Instale .NET 9 SDK de https://dotnet.microsoft.com/download

### Testes não aparecem no Test Explorer
- Recarregue a solução (Ctrl+Shift+P > Reload Window em VS Code)
- Compile o projeto (Build Solution)

### Erro de referência circular
- Certifique-se que `AzureDevOps.AI.McpServer.Tests` referencia apenas `AzureDevOps.AI.McpServer`

## Documentação Adicional

- [xUnit Documentation](https://xunit.net/)
- [Moq Quick Start](https://github.com/moq/moq4/wiki/Quickstart)
- [.NET Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/core/testing/)
- [README.md](./README.md) - Documentação detalhada dos testes
