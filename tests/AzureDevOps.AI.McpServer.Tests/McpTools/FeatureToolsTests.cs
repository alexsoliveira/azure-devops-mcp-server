using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.McpTools;

public class FeatureToolsTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<FeatureTools>> _mockLogger;
    private readonly FeatureTools _featureTools;

    public FeatureToolsTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<FeatureTools>();
        _featureTools = new FeatureTools(_mockWorkItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var tools = new FeatureTools(
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
            new FeatureTools(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new FeatureTools(_mockWorkItemService.Object, null!));
    }

    [Fact]
    public async Task CreateFeature_WithValidParameters_CallsWorkItemService()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Feature";
        var description = "Feature Description";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(
            Id: 456,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/456",
            Rev: 1,
            Fields: new Dictionary<string, object?> { { "System.Title", title } },
            Links: new WorkItemLinks(
                Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/456"),
                Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=456"),
                WorkItemUpdates: null,
                WorkItemRevisions: null,
                WorkItemHistory: null,
                WorkItemType: null,
                Fields: null));

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "Feature",
                title,
                description,
                null,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _featureTools.CreateFeature(project, title, description, null, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
        Assert.NotNull(result.Links);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task CreateFeature_WithEpicId_CreatesFeatureUnderEpic()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Feature";
        var description = "Feature Description";
        var epicId = 123;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(
            Id: 456,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/456",
            Rev: 1,
            Fields: new Dictionary<string, object?> { { "System.Title", title } },
            Links: new WorkItemLinks(
                Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/456"),
                Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=456"),
                WorkItemUpdates: null,
                WorkItemRevisions: null,
                WorkItemHistory: null,
                WorkItemType: null,
                Fields: null));

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "Feature",
                title,
                description,
                epicId,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _featureTools.CreateFeature(project, title, description, epicId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(456, result.Id);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task CreateFeature_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                It.IsAny<string>(),
                "Feature",
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _featureTools.CreateFeature("Project", "Title", "Description", null));
    }
}
