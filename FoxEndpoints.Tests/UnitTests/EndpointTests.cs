using FoxEndpoints.Tests.TestEndpoints;

namespace FoxEndpoints.Tests.UnitTests;

/// <summary>
/// Unit tests for Endpoint&lt;TRequest, TResponse&gt; base class
/// </summary>
public class EndpointTests
{
    [Fact]
    public void Endpoint_Configure_ShouldSetRouteAndMethod()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        // Act
        endpoint.Configure();

        // Assert - using reflection to access internal properties
        var routeProp = typeof(EndpointBase).GetProperty("Route", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var methodsProp = typeof(EndpointBase).GetProperty("Methods", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var route = routeProp?.GetValue(endpoint) as string;
        var methods = methodsProp?.GetValue(endpoint) as string[];
        
        Assert.Equal("/test", route);
        Assert.NotNull(methods);
        Assert.Single(methods);
        Assert.Equal("POST", methods[0]);
    }

    [Fact]
    public async Task Endpoint_HandleAsync_ShouldReturnIResult()
    {
        // Arrange
        var endpoint = new TestEndpoint();
        var request = new TestRequest { Name = "Test", Count = 5 };
        var ct = CancellationToken.None;

        // Act
        var result = await endpoint.HandleAsync(request, ct);

        // Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IResult>(result);
    }

    [Fact]
    public void Endpoint_Configure_ShouldAddConfigurators()
    {
        // Arrange
        var endpoint = new TestEndpoint();

        // Act
        endpoint.Configure();

        // Assert - verify configurators were added
        var configuratorsProp = typeof(EndpointBase).GetProperty("Configurators", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configurators = configuratorsProp?.GetValue(endpoint) as List<Action<Microsoft.AspNetCore.Builder.RouteHandlerBuilder>>;
        
        Assert.NotNull(configurators);
        Assert.True(configurators.Count >= 2); // WithName and WithTags at minimum
    }
}