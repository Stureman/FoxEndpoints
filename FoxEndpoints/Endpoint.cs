using FoxEndpoints.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

public abstract class Endpoint<TRequest, TResponse> : EndpointBase
{
    /// <summary>
    /// Provides access to response-sending methods.
    /// </summary>
    protected new EndpointSend<TResponse> Send { get; } = new();
    
    public abstract Task<IResult> HandleAsync(TRequest request, CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        var requiresFormData = ReflectionCache.RequiresFormDataConfiguration(endpointType);

        if ((httpMethod == HttpMethods.Get || httpMethod == HttpMethods.Delete))
        {
            return async (HttpContext ctx, CancellationToken ct) =>
            {
                var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
                await using var _ = wrapper;
                var ep = (Endpoint<TRequest, TResponse>)wrapper.Endpoint;
                ep.SetContext(ctx);
                try
                {
                    var request = ep.BindFromHttpContext<TRequest>(ctx);
                    return await ep.HandleAsync(request, ct);
                }
                catch (RequestBindingException ex)
                {
                    return CreateBindingProblem(ex);
                }
            };
        }
        if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
        {
            if (requiresFormData)
            {
                return async (HttpContext ctx, CancellationToken ct) =>
                {
                    var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
                    await using var _ = wrapper;
                    var ep = (Endpoint<TRequest, TResponse>)wrapper.Endpoint;
                    ep.SetContext(ctx);
                    try
                    {
                        var request = await ep.BindFromFormAsync<TRequest>(ctx);
                        return await ep.HandleAsync(request, ct);
                    }
                    catch (RequestBindingException ex)
                    {
                        return CreateBindingProblem(ex);
                    }
                };
            }

            return async ([FromBody] TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
                await using var _ = wrapper;
                var ep = (Endpoint<TRequest, TResponse>)wrapper.Endpoint;
                ep.SetContext(ctx);
                try
                {
                    var mergedRequest = ep.MergeRouteParameters(req, ctx);
                    return await ep.HandleAsync(mergedRequest, ct);
                }
                catch (RequestBindingException ex)
                {
                    return CreateBindingProblem(ex);
                }
            };
        }

        return async (TRequest req, HttpContext ctx, CancellationToken ct) =>
        {
            var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
            await using var _ = wrapper;
            var ep = (Endpoint<TRequest, TResponse>)wrapper.Endpoint;
            ep.SetContext(ctx);
            return await ep.HandleAsync(req, ct);
        };
    }

    private static IResult CreateBindingProblem(RequestBindingException ex)
        => Results.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid request payload");
}