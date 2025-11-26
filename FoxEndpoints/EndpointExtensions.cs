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
    /// Registers FoxEndpoints for the application.
    /// Returns a builder for additional configuration.
    /// </summary>
    public static FoxEndpointsBuilder UseFoxEndpoints(this WebApplication app)
    {
        return new FoxEndpointsBuilder(app);
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