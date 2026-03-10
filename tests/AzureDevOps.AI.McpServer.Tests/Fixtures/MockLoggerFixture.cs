using Microsoft.Extensions.Logging;
using Moq;

namespace AzureDevOps.AI.McpServer.Tests.Fixtures;

/// <summary>Provides mock logger instances for testing.</summary>
public class MockLoggerFixture
{
    public static ILogger<T> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>(MockBehavior.Default).Object;
    }

    public static Mock<ILogger<T>> CreateMockLoggerWithTracking<T>()
    {
        return new Mock<ILogger<T>>(MockBehavior.Default);
    }
}
