using AzureDevOps.AI.McpServer.Infrastructure.AzureDevOps;
using AzureDevOps.AI.McpServer.Security;
using Xunit;

namespace AzureDevOps.AI.McpServer.Tests.Infrastructure;

public class AzureDevOpsClientTests
{
    [Fact]
    public void Constructor_WithNullTokenProvider_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AzureDevOpsClient(null!));
    }


}
