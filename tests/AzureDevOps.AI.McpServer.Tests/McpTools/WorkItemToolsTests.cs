using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.McpTools;

public class WorkItemToolsTests
{
    private readonly Mock<IWorkItemService> _mockWorkItemService;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<WorkItemTools>> _mockLogger;
    private readonly WorkItemTools _workItemTools;

    public WorkItemToolsTests()
    {
        _mockWorkItemService = new Mock<IWorkItemService>(MockBehavior.Strict);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<WorkItemTools>();
        _workItemTools = new WorkItemTools(_mockWorkItemService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var tools = new WorkItemTools(
            _mockWorkItemService.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(tools);
    }

    [Fact]
    public async Task ListWorkItems_WithValidProject_CallsWorkItemService()
    {
        // Arrange
        var project = "TestProject";
        var cancellationToken = CancellationToken.None;

        var expectedResults = new List<WorkItemResult>
        {
            new WorkItemResult(1, "https://dev.azure.com/org/project/_workitems/edit/1"),
            new WorkItemResult(2, "https://dev.azure.com/org/project/_workitems/edit/2")
        };

        _mockWorkItemService
            .Setup(x => x.ListWorkItemsAsync(
                project,
                null,
                null,
                null,
                cancellationToken))
            .ReturnsAsync(expectedResults)
            .Verifiable();

        // Act
        var results = await _workItemTools.ListWorkItems(project, null, null, null, cancellationToken);

        // Assert
        Assert.NotNull(results);
        Assert.Equal(expectedResults.Count, results.Count);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task ListWorkItems_WithFilters_CallsWorkItemServiceWithFilters()
    {
        // Arrange
        var project = "TestProject";
        var workItemType = "Feature";
        var state = "Active";
        var assignedTo = "user@example.com";
        var cancellationToken = CancellationToken.None;

        var expectedResults = new List<WorkItemResult>
        {
            new WorkItemResult(1, "https://dev.azure.com/org/project/_workitems/edit/1")
        };

        _mockWorkItemService
            .Setup(x => x.ListWorkItemsAsync(
                project,
                workItemType,
                state,
                assignedTo,
                cancellationToken))
            .ReturnsAsync(expectedResults)
            .Verifiable();

        // Act
        var results = await _workItemTools.ListWorkItems(
            project,
            workItemType,
            state,
            assignedTo,
            cancellationToken);

        // Assert
        Assert.NotNull(results);
        Assert.Single(results);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task UpdateWorkItem_WithValidId_CallsWorkItemService()
    {
        // Arrange
        var workItemId = 123;
        var title = "Updated Title";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(workItemId, "https://dev.azure.com/org/project/_workitems/edit/123");

        _mockWorkItemService
            .Setup(x => x.UpdateWorkItemAsync(
                workItemId,
                title,
                null,
                null,
                null,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _workItemTools.UpdateWorkItem(
            workItemId,
            title,
            null,
            null,
            null,
            cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workItemId, result.Id);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task UpdateWorkItem_WithMultipleFields_UpdatesAllFields()
    {
        // Arrange
        var workItemId = 123;
        var title = "Updated Title";
        var description = "Updated Description";
        var state = "Closed";
        var assignedTo = "user@example.com";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(workItemId, "https://dev.azure.com/org/project/_workitems/edit/123");

        _mockWorkItemService
            .Setup(x => x.UpdateWorkItemAsync(
                workItemId,
                title,
                description,
                state,
                assignedTo,
                cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _workItemTools.UpdateWorkItem(
            workItemId,
            title,
            description,
            state,
            assignedTo,
            cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task UpdateWorkItem_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var workItemId = 123;
        var cancellationToken = CancellationToken.None;

        _mockWorkItemService
            .Setup(x => x.UpdateWorkItemAsync(
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Item not found"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _workItemTools.UpdateWorkItem(
                workItemId,
                "Title",
                null,
                null,
                null,
                cancellationToken));
    }
}
