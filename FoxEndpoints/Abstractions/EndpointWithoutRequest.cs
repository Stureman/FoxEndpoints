using FoxEndpoints.Binding;
using FoxEndpoints.Internal.Factory;
using FoxEndpoints.Results;
using Microsoft.AspNetCore.Http;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace FoxEndpoints.Abstractions;

/// <summary>
/// Base class for endpoints that don't accept a request body but return a response.
/// Useful for health checks, status endpoints, or simple GET endpoints without parameters.
/// </summary>
/// <typeparam name="TResponse">The type of the response object that will be serialized to the HTTP response.</typeparam>
public abstract class EndpointWithoutRequest<TResponse> : EndpointBase
{
    /// <summary>
    /// Provides access to typed response-sending methods for this endpoint's response type.
    /// </summary>
    protected new EndpointSend<TResponse> Send { get; } = new();

    /// <summary>
    /// Override this method to implement your endpoint's business logic.
    /// No request object is provided - use HttpContext directly if you need to access request data.
    /// </summary>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>An IResult representing the HTTP response.</returns>
    public abstract Task<IResult> HandleAsync(CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        Func<HttpContext, CancellationToken, Task<IResult>> handler = async (HttpContext ctx, CancellationToken ct) =>
        {
            var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
                ctx.RequestServices);
            await using var _ = wrapper;
            var ep = (EndpointWithoutRequest<TResponse>)wrapper.Endpoint;
            ep.SetContext(ctx);
            try
            {
                return await ep.HandleAsync(ct);
            }
            catch (RequestBindingException ex)
            {
                return HttpResults.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid request payload");
            }
        };
        return handler;
    }
}