using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.McpTools;

public class UserStoryToolsTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<UserStoryTools>> _mockLogger;
    private readonly UserStoryTools _userStoryTools;

    public UserStoryToolsTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<UserStoryTools>();
        _userStoryTools = new UserStoryTools(_mockWorkItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var tools = new UserStoryTools(
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
            new UserStoryTools(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new UserStoryTools(_mockWorkItemService.Object, null!));
    }

    [Fact]
    public async Task CreateUserStory_WithValidParameters_CallsWorkItemService()
    {
        // Arrange
        var project = "TestProject";
        var title = "New User Story";
        var description = "User Story Description";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(
            Id: 789,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/789",
            Rev: 1,
            Fields: new Dictionary<string, object?> { { "System.Title", title } },
            Links: new WorkItemLinks(
                Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/789"),
                Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=789"),
                WorkItemUpdates: null,
                WorkItemRevisions: null,
                WorkItemHistory: null,
                WorkItemType: null,
                Fields: null));

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "User Story",
                title,
                description,
                null,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _userStoryTools.CreateUserStory(project, title, description, null, null, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
        Assert.NotNull(result.Links);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task CreateUserStory_WithFeatureId_CreatesUserStoryUnderFeature()
    {
        // Arrange
        var project = "TestProject";
        var title = "New User Story";
        var description = "Description";
        var featureId = 456;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(
            Id: 789,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/789",
            Rev: 1,
            Fields: new Dictionary<string, object?> { { "System.Title", title } },
            Links: new WorkItemLinks(
                Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/789"),
                Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=789"),
                WorkItemUpdates: null,
                WorkItemRevisions: null,
                WorkItemHistory: null,
                WorkItemType: null,
                Fields: null));

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "User Story",
                title,
                description,
                featureId,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _userStoryTools.CreateUserStory(project, title, description, featureId, null, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(789, result.Id);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task CreateUserStory_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                It.IsAny<string>(),
                "User Story",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _userStoryTools.CreateUserStory("Project", "Title", "Description", null));
    }
}
