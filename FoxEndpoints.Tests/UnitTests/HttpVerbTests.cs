using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FoxEndpoints.Tests.UnitTests;

/// <summary>
/// Unit tests for EndpointBase HTTP verb methods
/// </summary>
public class HttpVerbTests
{
    [Fact]
    public void Get_ShouldSetHttpMethodToGet()
    {
        // Arrange
        var endpoint = new GetEndpoint();

        // Act
        endpoint.Configure();

        // Assert
        var methodsProp = typeof(EndpointBase).GetProperty("Methods", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var methods = methodsProp?.GetValue(endpoint) as string[];
        
        Assert.NotNull(methods);
        Assert.Single(methods);
        Assert.Equal(HttpMethods.Get, methods[0]);
    }

    [Fact]
    public void Post_ShouldSetHttpMethodToPost()
    {
        // Arrange
        var endpoint = new PostEndpoint();

        // Act
        endpoint.Configure();

        // Assert
        var methodsProp = typeof(EndpointBase).GetProperty("Methods", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var methods = methodsProp?.GetValue(endpoint) as string[];
        
        Assert.NotNull(methods);
        Assert.Single(methods);
        Assert.Equal(HttpMethods.Post, methods[0]);
    }

    [Fact]
    public void Put_ShouldSetHttpMethodToPut()
    {
        // Arrange
        var endpoint = new PutEndpoint();

        // Act
        endpoint.Configure();

        // Assert
        var methodsProp = typeof(EndpointBase).GetProperty("Methods", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var methods = methodsProp?.GetValue(endpoint) as string[];
        
        Assert.NotNull(methods);
        Assert.Single(methods);
        Assert.Equal(HttpMethods.Put, methods[0]);
    }

    [Fact]
    public void Delete_ShouldSetHttpMethodToDelete()
    {
        // Arrange
        var endpoint = new DeleteEndpoint();

        // Act
        endpoint.Configure();

        // Assert
        var methodsProp = typeof(EndpointBase).GetProperty("Methods", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var methods = methodsProp?.GetValue(endpoint) as string[];
        
        Assert.NotNull(methods);
        Assert.Single(methods);
        Assert.Equal(HttpMethods.Delete, methods[0]);
    }

    [Theory]
    [InlineData("/api/users")]
    [InlineData("/api/products/{id}")]
    [InlineData("/api/orders/{orderId}/items/{itemId}")]
    public void Endpoint_ShouldSetRoute_Correctly(string route)
    {
        // Arrange
        var endpoint = new DynamicRouteEndpoint(route);

        // Act
        endpoint.Configure();

        // Assert
        var routeProp = typeof(EndpointBase).GetProperty("Route", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var actualRoute = routeProp?.GetValue(endpoint) as string;
        
        Assert.Equal(route, actualRoute);
    }
}

// Test endpoints for HTTP verb tests
internal class GetEndpoint : Endpoint<object, object>
{
    public override void Configure() => Get("/test");
    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}

internal class PostEndpoint : Endpoint<object, object>
{
    public override void Configure() => Post("/test");
    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}

internal class PutEndpoint : Endpoint<object, object>
{
    public override void Configure() => Put("/test");
    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}

internal class DeleteEndpoint : Endpoint<object, object>
{
    public override void Configure() => Delete("/test");
    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}

internal class DynamicRouteEndpoint : Endpoint<object, object>
{
    private readonly string _route;

    public DynamicRouteEndpoint(string route)
    {
        _route = route;
    }

    public override void Configure() => Get(_route);
    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}