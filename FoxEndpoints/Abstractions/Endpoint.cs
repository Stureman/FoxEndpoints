using FoxEndpoints.Binding;
using FoxEndpoints.Internal;
using FoxEndpoints.Internal.Discovery;
using FoxEndpoints.Internal.Factory;
using FoxEndpoints.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HttpResults = Microsoft.AspNetCore.Http.Results;

namespace FoxEndpoints.Abstractions;

/// <summary>
/// Base class for endpoints with both a request and response type.
/// </summary>
/// <typeparam name="TRequest">The type of the request object that will be bound from route, query, body, or form data.</typeparam>
/// <typeparam name="TResponse">The type of the response object that will be serialized to the HTTP response.</typeparam>
public abstract class Endpoint<TRequest, TResponse> : EndpointBase
{
	/// <summary>
	/// Provides access to typed response-sending methods for this endpoint's response type.
	/// </summary>
	protected new EndpointSend<TResponse> Send { get; } = new();

	/// <summary>
	/// Override this method to implement your endpoint's business logic.
	/// The request object is automatically bound from route parameters, query strings, and request body.
	/// </summary>
	/// <param name="request">The strongly-typed request object populated from the HTTP request.</param>
	/// <param name="ct">Cancellation token for the request.</param>
	/// <returns>An IResult representing the HTTP response.</returns>
	public abstract Task<IResult> HandleAsync(TRequest request, CancellationToken ct);

	internal static Delegate BuildHandler(Type endpointType, string httpMethod)
	{
		var requiresFormData = ReflectionCache.RequiresFormDataConfiguration(endpointType);

		if ((httpMethod == HttpMethods.Get || httpMethod == HttpMethods.Delete))
		{
			return async (HttpContext ctx, CancellationToken ct) =>
			{
				var wrapper =
					(EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
						ctx.RequestServices);
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
					var wrapper =
						(EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
							ctx.RequestServices);
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
				var wrapper =
					(EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
						ctx.RequestServices);
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
			var wrapper =
				(EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType,
					ctx.RequestServices);
			await using var _ = wrapper;
			var ep = (Endpoint<TRequest, TResponse>)wrapper.Endpoint;
			ep.SetContext(ctx);
			return await ep.HandleAsync(req, ct);
		};
	}

	private static IResult CreateBindingProblem(RequestBindingException ex)
		=> HttpResults.ValidationProblem(ex.Errors, statusCode: StatusCodes.Status400BadRequest,
			title: "Invalid request payload");
}

public abstract class Endpoint : EndpointBase
{
	/// <summary>
	/// Override this method to implement your endpoint's business logic.
	/// </summary>
	/// <param name="ct">Cancellation token for the request.</param>
	/// <returns>An IResult representing the HTTP response.</returns>
	public abstract Task<IResult> HandleAsync(CancellationToken ct);
	
	internal static Delegate BuildHandler(Type endpointType, string httpMethod) => 
		async (HttpContext ctx, CancellationToken ct) =>
	{
		var wrapper = (EndpointFactory.ScopedEndpointWrapper)EndpointFactory.CreateInstance(endpointType, ctx.RequestServices);
		await using var _ = wrapper;
		var ep = (Endpoint)wrapper.Endpoint;
		ep.SetContext(ctx);
		return await ep.HandleAsync(ct);
	};
}