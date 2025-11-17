using FoxEndpoints.Tests.TestEndpoints;

namespace FoxEndpoints.Tests.UnitTests;

/// <summary>
/// Unit tests for EndpointWithoutRequest&lt;TResponse&gt; base class
/// </summary>
public class EndpointWithoutRequestTests
{
    [Fact]
    public void EndpointWithoutRequest_Configure_ShouldSetRouteAndMethod()
    {
        // Arrange
        var endpoint = new NoRequestEndpoint();

        // Act
        endpoint.Configure();

        // Assert
        var routeProp = typeof(EndpointBase).GetProperty("Route", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var methodsProp = typeof(EndpointBase).GetProperty("Methods", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var route = routeProp?.GetValue(endpoint) as string;
        var methods = methodsProp?.GetValue(endpoint) as string[];
        
        Assert.Equal("/no-request", route);
        Assert.NotNull(methods);
        Assert.Single(methods);
        Assert.Equal("GET", methods[0]);
    }
}