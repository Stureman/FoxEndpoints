using FoxEndpoints.Tests.TestEndpoints;
using NSubstitute;

namespace FoxEndpoints.Tests.UnitTests;

/// <summary>
/// Unit tests for dependency injection in endpoints
/// </summary>
public class DependencyInjectionTests
{
    [Fact]
    public async Task Endpoint_WithInjectedDependency_ShouldUseService()
    {
        // Arrange
        var mockService = Substitute.For<ITestService>();
        mockService.Process("TestValue").Returns("Mocked: TestValue");
        
        var endpoint = new DependencyInjectionEndpoint(mockService);
        var request = new DependencyRequest { Value = "TestValue" };

        // Act
        var result = await endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("Mocked: TestValue", result.Result);
        mockService.Received(1).Process("TestValue");
    }

    [Fact]
    public void Endpoint_WithConstructorDependencies_ShouldRequireDependencies()
    {
        // Arrange & Act
        var mockService = Substitute.For<ITestService>();
        var endpoint = new DependencyInjectionEndpoint(mockService);

        // Assert
        Assert.NotNull(endpoint);
        Assert.IsType<DependencyInjectionEndpoint>(endpoint);
    }

    [Fact]
    public async Task Endpoint_WithMockedService_ShouldReturnMockedResult()
    {
        // Arrange
        var mockService = Substitute.For<ITestService>();
        mockService.Process(Arg.Any<string>()).Returns(x => $"Processed: {x[0]}");
        
        var endpoint = new DependencyInjectionEndpoint(mockService);
        var request = new DependencyRequest { Value = "Input" };

        // Act
        var result = await endpoint.HandleAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal("Processed: Input", result.Result);
        mockService.Received(1).Process("Input");
    }
}