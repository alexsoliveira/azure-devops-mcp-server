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
            new WorkItemResult(
                Id: 1,
                Url: "https://dev.azure.com/org/project/_apis/wit/workItems/1",
                Rev: 1,
                Fields: new Dictionary<string, object?> { { "System.Title", "Item 1" } },
                Links: new WorkItemLinks(
                    Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/1"),
                    Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=1"),
                    WorkItemUpdates: null,
                    WorkItemRevisions: null,
                    WorkItemHistory: null,
                    WorkItemType: null,
                    Fields: null)),
            new WorkItemResult(
                Id: 2,
                Url: "https://dev.azure.com/org/project/_apis/wit/workItems/2",
                Rev: 2,
                Fields: new Dictionary<string, object?> { { "System.Title", "Item 2" } },
                Links: new WorkItemLinks(
                    Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/2"),
                    Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=2"),
                    WorkItemUpdates: null,
                    WorkItemRevisions: null,
                    WorkItemHistory: null,
                    WorkItemType: null,
                    Fields: null))
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
        Assert.NotNull(results[0].Links);
        Assert.NotNull(results[0].Fields);
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
            new WorkItemResult(
                Id: 1,
                Url: "https://dev.azure.com/org/project/_apis/wit/workItems/1",
                Rev: 3,
                Fields: new Dictionary<string, object?> 
                { 
                    { "System.Title", "Feature 1" },
                    { "System.State", "Active" },
                    { "System.AssignedTo", "user@example.com" }
                },
                Links: new WorkItemLinks(
                    Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/1"),
                    Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=1"),
                    WorkItemUpdates: null,
                    WorkItemRevisions: null,
                    WorkItemHistory: null,
                    WorkItemType: null,
                    Fields: null))
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
        Assert.NotNull(results[0].Fields);
        Assert.Contains("System.State", results[0].Fields!.Keys);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task GetWorkItem_WithValidId_CallsWorkItemService()
    {
        // Arrange
        var workItemId = 42;
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(
            Id: workItemId,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/42",
            Rev: 5,
            Fields: new Dictionary<string, object?>
            {
                { "System.Title", "Important Feature" },
                { "System.State", "Active" },
                { "System.WorkItemType", "Feature" }
            },
            Links: new WorkItemLinks(
                Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/42"),
                Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=42"),
                WorkItemUpdates: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/42/updates"),
                WorkItemRevisions: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/42/revisions"),
                WorkItemHistory: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/42/history"),
                WorkItemType: null,
                Fields: null));

        _mockWorkItemService
            .Setup(x => x.GetWorkItemAsync(workItemId, cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _workItemTools.GetWorkItem(workItemId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workItemId, result.Id);
        Assert.Equal(5, result.Rev);
        Assert.NotNull(result.Fields);
        Assert.NotNull(result.Links);
        Assert.Equal("Important Feature", result.Fields["System.Title"]);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task UpdateWorkItem_WithValidId_CallsWorkItemService()
    {
        // Arrange
        var workItemId = 123;
        var title = "Updated Title";
        var cancellationToken = CancellationToken.None;

        var expectedResult = new WorkItemResult(
            Id: workItemId,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/123",
            Rev: 2,
            Fields: new Dictionary<string, object?> { { "System.Title", title } },
            Links: new WorkItemLinks(
                Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/123"),
                Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=123"),
                WorkItemUpdates: null,
                WorkItemRevisions: null,
                WorkItemHistory: null,
                WorkItemType: null,
                Fields: null));

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
        Assert.Equal(2, result.Rev);
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

        var expectedResult = new WorkItemResult(
            Id: workItemId,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/123",
            Rev: 3,
            Fields: new Dictionary<string, object?>
            {
                { "System.Title", title },
                { "System.Description", description },
                { "System.State", state },
                { "System.AssignedTo", assignedTo }
            },
            Links: new WorkItemLinks(
                Self: new LinkRef("https://dev.azure.com/org/project/_apis/wit/workItems/123"),
                Html: new LinkRef("https://dev.azure.com/org/project/web/wi.aspx?id=123"),
                WorkItemUpdates: null,
                WorkItemRevisions: null,
                WorkItemHistory: null,
                WorkItemType: null,
                Fields: null));

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
        Assert.NotNull(result.Fields);
        Assert.Equal(4, result.Fields.Count);
        Assert.Equal(state, result.Fields["System.State"]);
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

    [Fact]
    public async Task GetRelatedWorkItems_WithValidId_ReturnsRelatedItems()
    {
        // Arrange
        var workItemId = 100;
        var cancellationToken = CancellationToken.None;

        var parentItem = new WorkItemResult(
            Id: workItemId,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/100",
            Rev: 1,
            Fields: new Dictionary<string, object?> { { "System.Title", "Parent Task" } },
            Links: null);

        var childItem1 = new WorkItemResult(
            Id: 101,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/101",
            Rev: 1,
            Fields: new Dictionary<string, object?> { { "System.Title", "Child Task 1" } },
            Links: null);

        var childItem2 = new WorkItemResult(
            Id: 102,
            Url: "https://dev.azure.com/org/project/_apis/wit/workItems/102",
            Rev: 1,
            Fields: new Dictionary<string, object?> { { "System.Title", "Child Task 2" } },
            Links: null);

        var relations = new List<WorkItemRelation>
        {
            new WorkItemRelation("System.LinkTypes.Hierarchy-Forward", "https://dev.azure.com/org/project/_apis/wit/workItems/101", 101),
            new WorkItemRelation("System.LinkTypes.Hierarchy-Forward", "https://dev.azure.com/org/project/_apis/wit/workItems/102", 102)
        };

        var expectedResult = new WorkItemWithRelations(
            parentItem,
            relations,
            new List<WorkItemResult> { childItem1, childItem2 });

        _mockWorkItemService
            .Setup(x => x.GetRelatedWorkItemsAsync(workItemId, cancellationToken))
            .ReturnsAsync(expectedResult)
            .Verifiable();

        // Act
        var result = await _workItemTools.GetRelatedWorkItems(workItemId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workItemId, result.Item.Id);
        Assert.NotNull(result.Relations);
        Assert.Equal(2, result.Relations.Count);
        Assert.NotNull(result.RelatedItems);
        Assert.Equal(2, result.RelatedItems.Count);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task GetChildWorkItems_WithValidParentId_ReturnsChildTasks()
    {
        // Arrange
        var parentId = 62;
        var cancellationToken = CancellationToken.None;

        var expectedChildren = new List<WorkItemResult>
        {
            new WorkItemResult(
                Id: 63,
                Url: "https://dev.azure.com/org/project/_apis/wit/workItems/63",
                Rev: 1,
                Fields: new Dictionary<string, object?> { { "System.Title", "Task 1 for FEAT-01" } },
                Links: null),
            new WorkItemResult(
                Id: 64,
                Url: "https://dev.azure.com/org/project/_apis/wit/workItems/64",
                Rev: 1,
                Fields: new Dictionary<string, object?> { { "System.Title", "Task 2 for FEAT-01" } },
                Links: null)
        };

        _mockWorkItemService
            .Setup(x => x.GetChildWorkItemsAsync(parentId, cancellationToken))
            .ReturnsAsync(expectedChildren)
            .Verifiable();

        // Act
        var result = await _workItemTools.GetChildWorkItems(parentId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, item => item.Id == 63);
        Assert.Contains(result, item => item.Id == 64);
        _mockWorkItemService.Verify();
    }

    [Fact]
    public async Task GetChildWorkItems_WithNoChildren_ReturnsEmptyList()
    {
        // Arrange
        var parentId = 999;
        var cancellationToken = CancellationToken.None;

        _mockWorkItemService
            .Setup(x => x.GetChildWorkItemsAsync(parentId, cancellationToken))
            .ReturnsAsync(new List<WorkItemResult>())
            .Verifiable();

        // Act
        var result = await _workItemTools.GetChildWorkItems(parentId, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        _mockWorkItemService.Verify();
    }
}
