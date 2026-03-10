using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.McpTools;

public class EpicToolsTestsFixed
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<EpicTools>> _mockLogger;
    private readonly EpicTools _epicTools;

    public EpicToolsTestsFixed()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<EpicTools>();
        _epicTools = new EpicTools(_mockWorkItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        var tools = new EpicTools(_mockWorkItemService.Object, _mockLogger.Object);
        Assert.NotNull(tools);
    }

    [Fact]
    public void Constructor_WithNullWorkItemService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EpicTools(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EpicTools(_mockWorkItemService.Object, null!));
    }

    [Fact]
    public async Task CreateEpic_WithValidParameters_ReturnsWorkItem()
    {
        var project = "TestProject";
        var title = "New Epic";
        var description = "Epic Description";
        var cancellationToken = CancellationToken.None;
        var expectedResult = new WorkItemResult(123, "https://dev.azure.com/org/project/_workitems/edit/123");

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(project, "Epic", title, description, null, cancellationToken))
            .ReturnsAsync(expectedResult);

        var result = await _epicTools.CreateEpic(project, title, description, cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
    }

    [Fact]
    public async Task CreateEpic_WhenServiceThrows_PropagatesException()
    {
        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                It.IsAny<string>(),
                "Epic",
                It.IsAny<string>(),
                It.IsAny<string>(),
                null,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _epicTools.CreateEpic("Project", "Title", "Description"));
    }
}
