using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Infrastructure;

public class WorkItemServiceTests
{
    private readonly Mock<IAzureDevOpsClient> _mockAzureDevOpsClient;
    private readonly Mock<ILogger<WorkItemService>> _mockLogger;
    private readonly WorkItemService _workItemService;

    public WorkItemServiceTests()
    {
        _mockAzureDevOpsClient = new Mock<IAzureDevOpsClient>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<WorkItemService>();
        _workItemService = new WorkItemService(_mockAzureDevOpsClient.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var service = new WorkItemService(
            _mockAzureDevOpsClient.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new WorkItemService(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new WorkItemService(_mockAzureDevOpsClient.Object, null!));
    }

}
