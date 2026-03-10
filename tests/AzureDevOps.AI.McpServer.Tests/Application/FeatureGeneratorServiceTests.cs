using AzureDevOps.AI.McpServer.Application;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Application;

public class FeatureGeneratorServiceTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<FeatureGeneratorService>> _mockLogger;
    private readonly FeatureGeneratorService _featureGeneratorService;

    public FeatureGeneratorServiceTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<FeatureGeneratorService>();
        _featureGeneratorService = new FeatureGeneratorService(_mockWorkItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var service = new FeatureGeneratorService(
            _mockWorkItemService.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullWorkItemService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new FeatureGeneratorService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new FeatureGeneratorService(_mockWorkItemService.Object, null!));
    }

    [Fact]
    public async Task GenerateFeatureAsync_WithValidParameters_CallsWorkItemService()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Feature";
        var description = "Feature Description";
        var epicId = 123;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(456, "https://dev.azure.com/org/project/_workitems/edit/456");
        
        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                project,
                "Feature",
                title,
                description,
                epicId,
                cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _featureGeneratorService.GenerateFeatureAsync(
            project,
            title,
            description,
            epicId,
            cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
        Assert.Equal(expectedResult.Url, result.Url);
    }

    [Fact]
    public async Task GenerateFeatureAsync_WithNullEpicId_CallsWorkItemServiceWithoutParent()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Feature";
        var description = "Feature Description";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(456, "https://dev.azure.com/org/project/_workitems/edit/456");
        
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
        var result = await _featureGeneratorService.GenerateFeatureAsync(
            project,
            title,
            description,
            null,
            cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task GenerateFeatureAsync_WhenWorkItemServiceThrowsException_PropagatesException()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Feature";
        var description = "Description";
        var cancellationToken = CancellationToken.None;

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _featureGeneratorService.GenerateFeatureAsync(
                project,
                title,
                description,
                null,
                cancellationToken));
    }
}
