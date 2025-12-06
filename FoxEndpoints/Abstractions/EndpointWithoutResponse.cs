using FoxEndpoints.Binding;
using FoxEndpoints.Internal;
using FoxEndpoints.Internal.Discovery;
using FoxEndpoints.Internal.Factory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace FoxEndpoints.Abstractions;

/// <summary>
/// Base class for endpoints that accept a request but don't return a typed response.
/// Useful for command-style endpoints that return 204 No Content or simple success/error responses.
/// </summary>
/// <typeparam name="TRequest">The type of the request object that will be bound from route, query, body, or form data.</typeparam>
public abstract class EndpointWithoutResponse<TRequest> : EndpointBase
{
	/// <summary>
	/// Override this method to implement your endpoint's business logic.
	/// The request object is automatically bound from route parameters, query strings, and request body.
	/// </summary>
	/// <param name="request">The strongly-typed request object populated from the HTTP request.</param>
	/// <param name="ct">Cancellation token for the request.</param>
	/// <returns>An IResult representing the HTTP response (typically NoContent, Ok, or error responses).</returns>
	public abstract Task<IResult> HandleAsync(TRequest request, CancellationToken ct);

	internal static Delegate BuildHandler(Type endpointType, string httpMethod)
	{
		var requiresFormData = ReflectionCache.RequiresFormDataConfiguration(endpointType);

		if (httpMethod == HttpMethods.Delete)
		{
			Func<HttpContext, CancellationToken, Task<IResult>> handler = async (HttpContext ctx, CancellationToken ct) =>
			{
				var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
					ctx.RequestServices);
				await using var _ = wrapper;
				var ep = (EndpointWithoutResponse<TRequest>)wrapper.Endpoint;
				ep.SetContext(ctx);
				try
				{
					var request = ep.BindFromHttpContext<TRequest>(ctx);
					return await ep.HandleAsync(request, ct);
				}
				catch (RequestBindingException ex)
				{
					return HttpResults.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
						title: "Invalid request payload");
				}
			};
			return handler;
		}
		else if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
		{
			if (requiresFormData)
			{
				Func<HttpContext, CancellationToken, Task<IResult>> handler = async (HttpContext ctx, CancellationToken ct) =>
				{
					var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
						ctx.RequestServices);
					await using var _ = wrapper;
					var ep = (EndpointWithoutResponse<TRequest>)wrapper.Endpoint;
					ep.SetContext(ctx);
					try
					{
						var request = await ep.BindFromFormAsync<TRequest>(ctx);
						return await ep.HandleAsync(request, ct);
					}
					catch (RequestBindingException ex)
					{
						return HttpResults.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
							title: "Invalid request payload");
					}
				};
				return handler;
			}

			Func<TRequest, HttpContext, CancellationToken, Task<IResult>> bodyHandler = async ([FromBody] TRequest req, HttpContext ctx, CancellationToken ct) =>
			{
				var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
					ctx.RequestServices);
				await using var _ = wrapper;
				var ep = (EndpointWithoutResponse<TRequest>)wrapper.Endpoint;
				ep.SetContext(ctx);
				try
				{
					var mergedRequest = ep.MergeRouteParameters(req, ctx);
					return await ep.HandleAsync(mergedRequest, ct);
				}
				catch (RequestBindingException ex)
				{
					return HttpResults.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
						title: "Invalid request payload");
				}
			};
			return bodyHandler;
		}
		else
		{
			Func<TRequest, HttpContext, CancellationToken, Task<IResult>> handler = async (TRequest req, HttpContext ctx, CancellationToken ct) =>
			{
				var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
					ctx.RequestServices);
				await using var _ = wrapper;
				var ep = (EndpointWithoutResponse<TRequest>)wrapper.Endpoint;
				ep.SetContext(ctx);
				return await ep.HandleAsync(req, ct);
			};
			return handler;
		}
	}
}