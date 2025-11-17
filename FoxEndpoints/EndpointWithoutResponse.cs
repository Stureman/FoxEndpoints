using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

public abstract class EndpointWithoutResponse<TRequest> : EndpointBase
{
    public abstract Task HandleAsync(TRequest request, CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        // For DELETE with complex types, manually bind from HttpContext
        // For POST/PUT/PATCH with complex types, use FromBody and merge route params
        var requestType = typeof(TRequest);
        var isSimpleType = requestType.IsPrimitive || requestType == typeof(string) || requestType == typeof(Guid) || requestType == typeof(DateTime);
        
        if (httpMethod == HttpMethods.Delete && !isSimpleType)
        {
            // Manually bind from route/query for DELETE with complex types
            return async (HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;

                var request = EndpointExtensions.BindFromHttpContext<TRequest>(ctx);
                await ep.HandleAsync(request, ct);

                return Results.NoContent();
            };
        }
        else if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
        {
            // Bind from body for POST/PUT/PATCH, then merge route parameters
            return async ([FromBody] TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;

                // Merge route parameters into the request object (e.g., {id} from /users/{id}/status)
                var mergedRequest = EndpointExtensions.MergeRouteParameters(req, ctx);
                await ep.HandleAsync(mergedRequest, ct);

                return Results.NoContent();
            };
        }
        else
        {
            // For simple types or other methods, use standard binding
            return async (TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;

                await ep.HandleAsync(req, ct);

                return Results.NoContent();
            };
        }
    }
}