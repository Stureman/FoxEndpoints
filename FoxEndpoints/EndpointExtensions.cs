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

    internal static object CreateEndpointInstance(Type endpointType, IServiceProvider services)
        => EndpointFactory.CreateInstance(endpointType, services);

    internal static TRequest BindFromHttpContext<TRequest>(this EndpointBase endpoint, HttpContext context)
        => RequestBinder.BindFromHttpContext<TRequest>(context);

    internal static TRequest MergeRouteParameters<TRequest>(this EndpointBase endpoint, TRequest request, HttpContext context)
        => RequestBinder.MergeRouteParameters(request, context);

    internal static async Task<TRequest> BindFromFormAsync<TRequest>(this EndpointBase endpoint, HttpContext context)
        => await RequestBinder.BindFromFormAsync<TRequest>(context, endpoint.GetFormOptions());
}