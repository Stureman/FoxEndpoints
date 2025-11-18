using Microsoft.AspNetCore.Builder;
using NSubstitute;

namespace FoxEndpoints.Tests.UnitTests;

/// <summary>
/// Unit tests for EndpointBuilder fluent API
/// </summary>
public class EndpointBuilderTests
{
    [Fact]
    public void EndpointBuilder_WithName_ShouldAddNameConfigurator()
    {
        // Arrange
        var endpoint = new TestEndpointForBuilder();
        
        // Act
        endpoint.Configure();
        
        // Assert - verify configurators were added
        var configuratorsProp = typeof(EndpointBase).GetProperty("Configurators", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configurators = configuratorsProp?.GetValue(endpoint) as List<Action<RouteHandlerBuilder>>;
        
        Assert.NotNull(configurators);
        Assert.NotEmpty(configurators);
    }

    [Fact]
    public void EndpointBuilder_WithTags_ShouldAddTagsConfigurator()
    {
        // Arrange
        var endpoint = new TestEndpointForBuilder();
        
        // Act
        endpoint.Configure();
        
        // Assert
        var configuratorsProp = typeof(EndpointBase).GetProperty("Configurators", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configurators = configuratorsProp?.GetValue(endpoint) as List<Action<RouteHandlerBuilder>>;
        
        Assert.NotNull(configurators);
        Assert.True(configurators.Count >= 2); // WithName + WithTags
    }

    [Fact]
    public void EndpointBuilder_Produces_ShouldAddProducesConfigurator()
    {
        // Arrange
        var endpoint = new TestEndpointWithProduces();
        
        // Act
        endpoint.Configure();
        
        // Assert
        var configuratorsProp = typeof(EndpointBase).GetProperty("Configurators", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configurators = configuratorsProp?.GetValue(endpoint) as List<Action<RouteHandlerBuilder>>;
        
        Assert.NotNull(configurators);
        Assert.NotEmpty(configurators);
    }

    [Fact]
    public void EndpointBuilder_RequireAuthorization_ShouldAddAuthConfigurator()
    {
        // Arrange
        var endpoint = new TestEndpointWithAuth();
        
        // Act
        endpoint.Configure();
        
        // Assert
        var configuratorsProp = typeof(EndpointBase).GetProperty("Configurators", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configurators = configuratorsProp?.GetValue(endpoint) as List<Action<RouteHandlerBuilder>>;
        
        Assert.NotNull(configurators);
        Assert.NotEmpty(configurators);
    }

    [Fact]
    public void EndpointBuilder_ChainedCalls_ShouldReturnBuilder()
    {
        // Arrange
        var endpoint = new TestEndpointChained();
        
        // Act & Assert - should not throw
        endpoint.Configure();
        
        var configuratorsProp = typeof(EndpointBase).GetProperty("Configurators", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var configurators = configuratorsProp?.GetValue(endpoint) as List<Action<RouteHandlerBuilder>>;
        
        Assert.NotNull(configurators);
        Assert.True(configurators.Count >= 4); // Multiple chained calls
    }
}

// Test endpoints for builder tests
internal class TestEndpointForBuilder : Endpoint<object, object>
{
    public override void Configure()
    {
        Post("/builder")
            .WithName("BuilderTest")
            .WithTags("Test");
    }

    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}

internal class TestEndpointWithProduces : Endpoint<object, object>
{
    public override void Configure()
    {
        Get("/produces")
            .Produces<object>(200)
            .Produces(404);
    }

    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}

internal class TestEndpointWithAuth : Endpoint<object, object>
{
    public override void Configure()
    {
        Get("/auth")
            .RequireAuthorization();
    }

    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}

internal class TestEndpointChained : Endpoint<object, object>
{
    public override void Configure()
    {
        Post("/chained")
            .WithName("Chained")
            .WithTags("Test", "Builder")
            .Produces<object>(200)
            .RequireAuthorization();
    }

    public override Task<IResult> HandleAsync(object request, CancellationToken ct)
        => Task.FromResult<IResult>(Results.Ok(new { }));
}