using FoxEndpoints.Tests.TestEndpoints;

namespace FoxEndpoints.Tests.UnitTests;

/// <summary>
/// Unit tests for EndpointWithoutResponse&lt;TRequest&gt; base class
/// </summary>
public class EndpointWithoutResponseTests
{
    [Fact]
    public void EndpointWithoutResponse_Configure_ShouldSetRouteAndMethod()
    {
        // Arrange
        var endpoint = new NoResponseEndpoint();

        // Act
        endpoint.Configure();

        // Assert
        var routeProp = typeof(EndpointBase).GetProperty("Route", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var methodsProp = typeof(EndpointBase).GetProperty("Methods", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var route = routeProp?.GetValue(endpoint) as string;
        var methods = methodsProp?.GetValue(endpoint) as string[];
        
        Assert.Equal("/no-response/{id}", route);
        Assert.NotNull(methods);
        Assert.Single(methods);
        Assert.Equal("DELETE", methods[0]);
    }

    [Fact]
    public async Task EndpointWithoutResponse_HandleAsync_ShouldExecuteWithoutReturningValue()
    {
        // Arrange
        var endpoint = new NoResponseEndpoint();
        var request = new NoResponseRequest { Id = 123 };
        var ct = CancellationToken.None;

        // Act
        await endpoint.HandleAsync(request, ct);

        // Assert
        Assert.True(endpoint.WasExecuted);
    }
}