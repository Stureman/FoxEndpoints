using Microsoft.AspNetCore.Http;

namespace FoxEndpoints;

public abstract class EndpointWithoutRequest<TResponse> : EndpointBase
{
    public abstract Task HandleAsync(CancellationToken ct);
    protected TResponse? Response { get; set; }

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        return async (HttpContext ctx, CancellationToken ct) =>
        {
            var ep = (EndpointWithoutRequest<TResponse>)
                EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
            
            ep.HttpContext = ctx;

            await ep.HandleAsync(ct);

            if (ctx.Response.HasStarted)
                return Results.Empty;

            return Results.Ok(ep.Response);
        };
    }
}