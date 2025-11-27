using FoxEndpoints.Internal;
using Microsoft.AspNetCore.Http;

namespace FoxEndpoints;

public abstract class EndpointWithoutRequest<TResponse> : EndpointBase
{
    /// <summary>
    /// Provides access to response-sending methods.
    /// </summary>
    protected new EndpointSend<TResponse> Send { get; } = new();
    
    public abstract Task<IResult> HandleAsync(CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        return async (HttpContext ctx, CancellationToken ct) =>
        {
            var ep = (EndpointWithoutRequest<TResponse>)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
            ep.SetContext(ctx);
            try
            {
                return await ep.HandleAsync(ct);
            }
            catch (RequestBindingException ex)
            {
                return Results.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid request payload");
            }
        };
    }
}