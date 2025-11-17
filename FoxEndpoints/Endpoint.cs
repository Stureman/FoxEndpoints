using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

public abstract class Endpoint<TRequest, TResponse> : EndpointBase
{
    public abstract Task HandleAsync(TRequest request, CancellationToken ct);
    protected TResponse? Response { get; set; }

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        // For GET requests, bind from route/query by manually constructing the request object
        // For POST/PUT/PATCH/DELETE with complex types, bind from body (and merge route params if present)
        var requestType = typeof(TRequest);
        var isSimpleType = requestType.IsPrimitive || requestType == typeof(string) || requestType == typeof(Guid) || requestType == typeof(DateTime);
        
        if (httpMethod == HttpMethods.Get && !isSimpleType)
        {
            // For GET with complex types, manually bind from HttpContext (route + query)
            return async (HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (Endpoint<TRequest, TResponse>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;

                var request = EndpointExtensions.BindFromHttpContext<TRequest>(ctx);
                await ep.HandleAsync(request, ct);

                // if handler did NOT use Send, fallback to Response property
                return ctx.Response.HasStarted switch
                {
                    true => Results.Empty, // Send has already written
                    false => Results.Ok(ep.Response)
                };
            };
        }
        if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
        {
            // Bind from body for POST/PUT/PATCH, then merge route parameters
            return async ([FromBody] TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (Endpoint<TRequest, TResponse>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;

                // Merge route parameters into the request object (e.g., {id} from /users/{id})
                var mergedRequest = EndpointExtensions.MergeRouteParameters(req, ctx);
                await ep.HandleAsync(mergedRequest, ct);

                if (ctx.Response.HasStarted)
                    return Results.Empty;

                return Results.Ok(ep.Response);
            };
        }

        // For simple types or other methods, use standard binding
        return async (TRequest req, HttpContext ctx, CancellationToken ct) =>
        {
            var ep = (Endpoint<TRequest, TResponse>)
                EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
            ep.HttpContext = ctx;

            await ep.HandleAsync(req, ct);

            if (ctx.Response.HasStarted)
                return Results.Empty;

            return Results.Ok(ep.Response);
        };
    }
}