using AzureDevOps.AI.McpServer.Application;
using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Application;

public class EpicGeneratorServiceTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<EpicGeneratorService>> _mockLogger;
    private readonly EpicGeneratorService _epicGeneratorService;

    public EpicGeneratorServiceTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<EpicGeneratorService>();
        _epicGeneratorService = new EpicGeneratorService(_mockWorkItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var service = new EpicGeneratorService(
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
            new EpicGeneratorService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new EpicGeneratorService(_mockWorkItemService.Object, null!));
    }

    [Fact]
    public async Task GenerateEpicAsync_WithValidParameters_CallsWorkItemService()
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
        var result = await _epicGeneratorService.GenerateEpicAsync(
            project,
            title,
            description,
            cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Id, result.Id);
        Assert.Equal(expectedResult.Url, result.Url);
    }

    [Fact]
    public async Task GenerateEpicAsync_WhenWorkItemServiceThrowsException_PropagatesException()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Epic";
        var description = "Epic Description";
        var cancellationToken = CancellationToken.None;

        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Azure DevOps service unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _epicGeneratorService.GenerateEpicAsync(
                project,
                title,
                description,
                cancellationToken));
    }

    [Fact]
    public async Task GenerateEpicAsync_CallsLoggerWithProjectName()
    {
        // Arrange
        var project = "TestProject";
        var title = "New Epic";
        var description = "Epic Description";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(123, "https://dev.azure.com/org/project/_workitems/edit/123");
        
        _mockWorkItemService
            .Setup(x => x.CreateWorkItemAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _epicGeneratorService.GenerateEpicAsync(
            project,
            title,
            description,
            cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<Microsoft.Extensions.Logging.EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(project)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.AtLeastOnce);
    }
}
