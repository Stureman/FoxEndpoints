using FoxEndpoints.Internal;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FoxEndpoints;

/// <summary>
/// Extension methods for registering and configuring FoxEndpoints.
/// This is the public API that delegates to internal implementation classes.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Discovers and registers all FoxEndpoints from the entry assembly.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to add the endpoints to.</param>
    /// <param name="configure">An optional action to configure global endpoint settings.</param>
    /// <returns>The <see cref="WebApplication"/> so that additional calls can be chained.</returns>
    public static WebApplication UseFoxEndpoints(this WebApplication app, Action<FoxEndpointsBuilder>? configure = null)
    {
        var builder = new FoxEndpointsBuilder(app);
        configure?.Invoke(builder);
        return builder.Build();
    }

    // Public methods used by endpoint base classes - delegate to internal implementations

    /// <summary>
    /// Creates an endpoint instance using dependency injection.
    /// Used internally by endpoint handler delegates.
    /// </summary>
    public static object CreateEndpointInstance(Type endpointType, IServiceProvider services)
        => EndpointFactory.CreateInstance(endpointType, services);

    /// <summary>
    /// Binds request data from HTTP context (route values and query string).
    /// Used internally by endpoint handler delegates for GET/DELETE requests.
    /// </summary>
    public static TRequest BindFromHttpContext<TRequest>(HttpContext context)
        => RequestBinder.BindFromHttpContext<TRequest>(context);

    /// <summary>
    /// Merges route parameters into an existing request object.
    /// Used internally by endpoint handler delegates for POST/PUT/PATCH requests.
    /// </summary>
    public static TRequest MergeRouteParameters<TRequest>(TRequest request, HttpContext context)
        => RequestBinder.MergeRouteParameters(request, context);

    /// <summary>
    /// Binds request data from multipart form data (including file uploads).
    /// Used internally by endpoint handler delegates for form data requests.
    /// </summary>
    public static async Task<TRequest> BindFromFormAsync<TRequest>(HttpContext context)
        => await RequestBinder.BindFromFormAsync<TRequest>(context);
}