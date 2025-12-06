using FoxEndpoints.Configuration;
using FoxEndpoints.Models;
using FoxEndpoints.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace FoxEndpoints.Abstractions;

/// <summary>
/// Base class for all FoxEndpoints. Provides HTTP verb helpers and access to the current HttpContext.
/// </summary>
public abstract class EndpointBase
{
	/// <summary>
	/// Gets the current HTTP context for this endpoint request.
	/// </summary>
	public HttpContext HttpContext { get; private set; } = default!;
	
	internal string? Route { get; private set; }
	internal string[] Methods { get; private set; } = Array.Empty<string>();
	internal List<Action<RouteHandlerBuilder>> Configurators { get; } = new();

	/// <summary>
	/// Provides access to response-sending methods.
	/// </summary>
	protected EndpointSend<object> Send { get; } = new();

	private FormOptions? _formOptions;
	private FileBindingMode _fileBindingMode = FileBindingMode.Buffered;

	internal void SetContext(HttpContext context)
	{
		HttpContext = context;
	}

	/// <summary>
	/// Configures form options for multipart/form-data requests on this endpoint.
	/// </summary>
	/// <param name="options">The form options to apply.</param>
	protected void ConfigureFormOptions(FormOptions options) => SetFormOptions(options);

	internal FormOptions? GetFormOptions() => _formOptions;
	internal void SetFormOptions(FormOptions options) => _formOptions = options;
	internal FileBindingMode GetFileBindingMode() => _fileBindingMode;
	internal void SetFileBindingMode(FileBindingMode mode) => _fileBindingMode = mode;

	/// <summary>
	/// Defines a GET endpoint at the specified route.
	/// </summary>
	/// <param name="route">The route pattern (e.g., "/users/{id}").</param>
	/// <returns>A fluent configuration builder for this endpoint.</returns>
	protected EndpointConfiguration Get(string route) => Verb(HttpMethods.Get, route);
	
	/// <summary>
	/// Defines a POST endpoint at the specified route.
	/// </summary>
	/// <param name="route">The route pattern (e.g., "/users").</param>
	/// <returns>A fluent configuration builder for this endpoint.</returns>
	protected EndpointConfiguration Post(string route) => Verb(HttpMethods.Post, route);
	
	/// <summary>
	/// Defines a PUT endpoint at the specified route.
	/// </summary>
	/// <param name="route">The route pattern (e.g., "/users/{id}").</param>
	/// <returns>A fluent configuration builder for this endpoint.</returns>
	protected EndpointConfiguration Put(string route) => Verb(HttpMethods.Put, route);
	
	/// <summary>
	/// Defines a DELETE endpoint at the specified route.
	/// </summary>
	/// <param name="route">The route pattern (e.g., "/users/{id}").</param>
	/// <returns>A fluent configuration builder for this endpoint.</returns>
	protected EndpointConfiguration Delete(string route) => Verb(HttpMethods.Delete, route);
	
	/// <summary>
	/// Defines a PATCH endpoint at the specified route.
	/// </summary>
	/// <param name="route">The route pattern (e.g., "/users/{id}").</param>
	/// <returns>A fluent configuration builder for this endpoint.</returns>
	protected EndpointConfiguration Patch(string route) => Verb(HttpMethods.Patch, route);

	private EndpointConfiguration Verb(string method, string route)
	{
		Route = route;
		Methods = new[] { method };
		return new EndpointConfiguration(this);
	}

	internal void AddConfigurator(Action<RouteHandlerBuilder> config)
		=> Configurators.Add(config);

	internal void ApplyConfigurators(RouteHandlerBuilder builder)
	{
		foreach (var config in Configurators)
			config(builder);
	}

	/// <summary>
	/// Override this method to configure your endpoint's route, metadata, and authorization.
	/// Called once at startup during endpoint discovery.
	/// </summary>
	public abstract void Configure();
}