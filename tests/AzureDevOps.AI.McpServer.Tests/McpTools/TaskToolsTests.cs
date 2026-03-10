using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.McpTools;

public class TaskToolsTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<TaskTools>> _mockLogger;
    private readonly TaskTools _taskTools;

    public TaskToolsTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<TaskTools>();
        _taskTools = new TaskTools(_mockWorkItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var tools = new TaskTools(
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
            new TaskTools(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new TaskTools(_mockWorkItemService.Object, null!));
    }

    [Fact]
    public async Task CreateTask_WithValidParameters_CallsWorkItemService()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Task";
        var description = "Task Description";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(999, "https://dev.azure.com/org/project/_workitems/edit/999");

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "Task",
                title,
                description,
                null,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _taskTools.CreateTask(project, title, description, null, null, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task CreateTask_WithParentUserStoryId_CreatesTaskUnderUserStory()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Task";
        var description = "Description";
        var userStoryId = 789;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(999, "https://dev.azure.com/org/project/_workitems/edit/999");

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "Task",
                title,
                description,
                userStoryId,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _taskTools.CreateTask(project, title, description, userStoryId, null, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task CreateTask_WithEmptyDescription_ShouldStillWork()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Task";
        var description = "";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(999, "https://dev.azure.com/org/project/_workitems/edit/999");

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "Task",
                title,
                description,
                null,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _taskTools.CreateTask(project, title, description, null, null, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task CreateTask_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                It.IsAny<string>(),
                "Task",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _taskTools.CreateTask("Project", "Title", "Description", null));
    }
}
