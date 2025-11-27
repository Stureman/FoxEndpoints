using FoxEndpoints.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

public abstract class EndpointWithoutResponse<TRequest> : EndpointBase
{
    public abstract Task<IResult> HandleAsync(TRequest request, CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        var requiresFormData = ReflectionCache.RequiresFormDataConfiguration(endpointType);

        if (httpMethod == HttpMethods.Delete)
        {
            return async (HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
                ep.SetContext(ctx);
                try
                {
                    var request = ep.BindFromHttpContext<TRequest>(ctx);
                    return await ep.HandleAsync(request, ct);
                }
                catch (RequestBindingException ex)
                {
                    return Results.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request payload");
                }
            };
        }
        else if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
        {
            if (requiresFormData)
            {
                return async (HttpContext ctx, CancellationToken ct) =>
                {
                    var ep = (EndpointWithoutResponse<TRequest>)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
                    ep.SetContext(ctx);
                    try
                    {
                        var request = await ep.BindFromFormAsync<TRequest>(ctx);
                        return await ep.HandleAsync(request, ct);
                    }
                    catch (RequestBindingException ex)
                    {
                        return Results.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
                            title: "Invalid request payload");
                    }
                };
            }

            return async ([FromBody] TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
                ep.SetContext(ctx);
                try
                {
                    var mergedRequest = ep.MergeRouteParameters(req, ctx);
                    return await ep.HandleAsync(mergedRequest, ct);
                }
                catch (RequestBindingException ex)
                {
                    return Results.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
                        title: "Invalid request payload");
                }
            };
        }
        else
        {
            return async (TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
                ep.SetContext(ctx);
                return await ep.HandleAsync(req, ct);
            };
        }
    }
}