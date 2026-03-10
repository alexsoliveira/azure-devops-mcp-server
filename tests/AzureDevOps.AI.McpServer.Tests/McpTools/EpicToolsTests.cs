using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.McpTools;

public class EpicToolsTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<EpicTools>> _mockLogger;
    private readonly EpicTools _epicTools;

    public EpicToolsTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<EpicTools>();
        _epicTools = new EpicTools(_mockWorkItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var tools = new EpicTools(
            _mockWorkItemService.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(tools);
    }

    [Fact]
    public void Constructor_WithNullWorkItemService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new EpicTools(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new EpicTools(_mockWorkItemService.Object, null!));
    }

    [Fact]
    public async Task CreateEpic_WithValidParameters_CallsWorkItemService()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Epic";
        var description = "Epic Description";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(123, "https://dev.azure.com/org/project/_workitems/edit/123");

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "Epic",
                title,
                description,
                null,
                cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _epicTools.CreateEpic(project, title, description, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
    }

    [Fact]
    public async Task CreateEpic_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                It.IsAny<string>(),
                "Epic",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _epicTools.CreateEpic("Project", "Title", "Description"));
    }
}
