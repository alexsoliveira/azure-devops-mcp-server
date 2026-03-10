using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.McpTools;
using AzureDevOps.AI.McpServer.Tests.Fixtures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.McpTools;

public class ProjectToolsTests
{
    private readonly Mock<IProjectService> _mockProjectService;
    private readonly Mock<ILogger<ProjectTools>> _mockLogger;
    private readonly ProjectTools _projectTools;

    public ProjectToolsTests()
    {
        _mockProjectService = new Mock<IProjectService>(MockBehavior.Loose);
        _mockLogger = MockLoggerFixture.CreateMockLoggerWithTracking<ProjectTools>();
        _projectTools = new ProjectTools(_mockProjectService.Object, _mockLogger.Object);
    }

    [Fact]
    public void Constructor_WithValidDependencies_Initializes()
    {
        // Arrange & Act
        var tools = new ProjectTools(
            _mockProjectService.Object,
            _mockLogger.Object);

        // Assert
        Assert.NotNull(tools);
    }

    [Fact]
    public void Constructor_WithNullProjectService_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ProjectTools(null!, _mockLogger.Object));
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ProjectTools(_mockProjectService.Object, null!));
    }

    [Fact]
    public async Task ListProjects_WithValidCall_ReturnsProjectList()
    {
        // Arrange
        var expectedProjects = new List<ProjectResult>
        {
            new("proj-1", "Project 1", "https://dev.azure.com/org/proj-1"),
            new("proj-2", "Project 2", "https://dev.azure.com/org/proj-2")
        };
        
        _mockProjectService
            .Setup(x => x.ListProjectsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProjects);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _projectTools.ListProjects(cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProjects.Count, result.Count);
    }

    [Fact]
    public async Task GetProject_WithValidProjectName_ReturnsProject()
    {
        // Arrange
        var projectName = "TestProject";
        var expectedResult = new ProjectResult("proj-123", "TestProject", "https://dev.azure.com/org/TestProject");
        
        _mockProjectService
            .Setup(x => x.GetProjectAsync(projectName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _projectTools.GetProject(projectName, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Name, result.Name);
        Assert.Equal(expectedResult.Id, result.Id);
    }

    [Fact]
    public async Task CreateProject_WithValidParameters_ReturnsCreatedProject()
    {
        // Arrange
        var name = "NewProject";
        var description = "This is a new project";
        var processTemplate = "Agile";
        var expectedResult = new ProjectResult("proj-456", "NewProject", "https://dev.azure.com/org/NewProject");
        
        _mockProjectService
            .Setup(x => x.CreateProjectAsync(
                name,
                description,
                processTemplate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _projectTools.CreateProject(name, description, processTemplate, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Name, result.Name);
        Assert.Equal(expectedResult.Id, result.Id);
    }

    [Fact]
    public async Task CreateProject_WithDefaultTemplate_UsesAgile()
    {
        // Arrange
        var name = "NewProject";
        var description = "This is a new project";
        var expectedResult = new ProjectResult("proj-456", "NewProject", "https://dev.azure.com/org/NewProject");
        
        _mockProjectService
            .Setup(x => x.CreateProjectAsync(
                name,
                description,
                "Agile",  // Default template
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var cancellationToken = CancellationToken.None;

        // Act - uses default template
        var result = await _projectTools.CreateProject(name, description, cancellationToken: cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResult.Name, result.Name);
    }

    [Fact]
    public async Task ListProjects_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        _mockProjectService
            .Setup(x => x.ListProjectsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service unavailable"));

        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _projectTools.ListProjects(cancellationToken));
    }

    [Fact]
    public async Task GetProject_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var projectName = "NonExistent";
        
        _mockProjectService
            .Setup(x => x.GetProjectAsync(projectName, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Project not found"));

        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _projectTools.GetProject(projectName, cancellationToken));
    }

    [Fact]
    public async Task CreateProject_WhenServiceThrowsException_PropagatesException()
    {
        // Arrange
        var name = "NewProject";
        var description = "This is a new project";
        var processTemplate = "Agile";
        
        _mockProjectService
            .Setup(x => x.CreateProjectAsync(
                name,
                description,
                processTemplate,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Creation failed"));

        var cancellationToken = CancellationToken.None;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _projectTools.CreateProject(name, description, processTemplate, cancellationToken));
    }

    [Fact]
    public async Task ListProjects_CallsLoggerMessage()
    {
        // Arrange
        var expectedProjects = new List<ProjectResult>
        {
            new("proj-1", "Project 1", "https://dev.azure.com/org/proj-1")
        };
        
        _mockProjectService
            .Setup(x => x.ListProjectsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedProjects);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _projectTools.ListProjects(cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("project_list")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetProject_CallsLoggerWithProjectName()
    {
        // Arrange
        var projectName = "TestProject";
        var expectedResult = new ProjectResult("proj-123", "TestProject", "https://dev.azure.com/org/TestProject");
        
        _mockProjectService
            .Setup(x => x.GetProjectAsync(projectName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _projectTools.GetProject(projectName, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(projectName)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateProject_CallsLoggerWithProjectName()
    {
        // Arrange
        var name = "NewProject";
        var description = "This is a new project";
        var processTemplate = "Agile";
        var expectedResult = new ProjectResult("proj-456", "NewProject", "https://dev.azure.com/org/NewProject");
        
        _mockProjectService
            .Setup(x => x.CreateProjectAsync(
                name,
                description,
                processTemplate,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var cancellationToken = CancellationToken.None;

        // Act
        var result = await _projectTools.CreateProject(name, description, processTemplate, cancellationToken);

        // Assert
        Assert.NotNull(result);
        _mockLogger.Verify(
            x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(name)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
