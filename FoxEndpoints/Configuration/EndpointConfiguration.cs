using FoxEndpoints.Abstractions;
using FoxEndpoints.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace FoxEndpoints.Configuration;

/// <summary>
/// Fluent builder for configuring a single endpoint's route, metadata, and authorization.
/// </summary>
public sealed class EndpointConfiguration
{
	private readonly EndpointBase _ep;

	internal EndpointConfiguration(EndpointBase ep)
	{
		_ep = ep;
	}

	/// <summary>
	/// Sets the operation name for this endpoint, used in OpenAPI/Swagger documentation.
	/// </summary>
	/// <param name="name">The operation name (e.g., "GetUser", "CreateProduct").</param>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration WithName(string name)
	{
		_ep.AddConfigurator(b => b.WithName(name));
		return this;
	}

	/// <summary>
	/// Adds tags to this endpoint for grouping in OpenAPI/Swagger documentation.
	/// </summary>
	/// <param name="tags">One or more tags (e.g., "Users", "Products").</param>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration WithTags(params string[] tags)
	{
		_ep.AddConfigurator(b => b.WithTags(tags));
		return this;
	}

	/// <summary>
	/// Declares that this endpoint can produce a response with the specified status code.
	/// Used for OpenAPI/Swagger documentation.
	/// </summary>
	/// <param name="statusCode">The HTTP status code (e.g., 200, 404).</param>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration Produces(int statusCode)
	{
		_ep.AddConfigurator(b => b.Produces(statusCode));
		return this;
	}

	/// <summary>
	/// Declares that this endpoint can produce a response with the specified type and status code.
	/// Used for OpenAPI/Swagger documentation.
	/// </summary>
	/// <typeparam name="T">The response type.</typeparam>
	/// <param name="statusCode">The HTTP status code (e.g., 200, 404).</param>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration Produces<T>(int statusCode)
	{
		_ep.AddConfigurator(b => b.Produces<T>(statusCode));
		return this;
	}

	/// <summary>
	/// Requires authentication for this endpoint using the default authorization policy.
	/// </summary>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration RequireAuthorization()
	{
		_ep.AddConfigurator(b => b.RequireAuthorization());
		return this;
	}

	/// <summary>
	/// Allows anonymous access to this endpoint, bypassing any global authorization requirements.
	/// </summary>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration AllowAnonymous()
	{
		_ep.AddConfigurator(b => b.AllowAnonymous());
		return this;
	}

	/// <summary>
	/// Requires authorization for this endpoint using one or more named policies.
	/// Policies must be registered in Program.cs via AddAuthorization().
	/// </summary>
	/// <param name="policies">One or more policy names (e.g., "AdminPolicy", "ManagerRole").</param>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration RequireAuthorization(params string[] policies)
	{
		_ep.AddConfigurator(b => b.RequireAuthorization(policies));
		return this;
	}

	/// <summary>
	/// Declares that this endpoint accepts multipart/form-data content type.
	/// Used for OpenAPI/Swagger documentation.
	/// </summary>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration AcceptsFormData()
	{
		_ep.AddConfigurator(b => b.Accepts<object>("multipart/form-data"));
		return this;
	}

	/// <summary>
	/// Disables antiforgery validation for this endpoint.
	/// Required when accepting form data from non-cookie-authenticated clients (e.g., bearer token auth).
	/// </summary>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration DisableAntiforgery()
	{
		_ep.AddConfigurator(b => b.DisableAntiforgery());
		return this;
	}

	/// <summary>
	/// Declares that this endpoint accepts file uploads via multipart/form-data.
	/// Adds the necessary content type metadata for OpenAPI/Swagger documentation.
	/// Note: Antiforgery is NOT automatically disabled - call DisableAntiforgery() separately if needed.
	/// </summary>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration AllowFileUploads()
	{
		_ep.AddConfigurator(b => { b.Accepts<object>("multipart/form-data"); });
		return this;
	}

	/// <summary>
	/// Configures form options for this endpoint (e.g., file size limits, buffer thresholds).
	/// </summary>
	/// <param name="options">The form options to apply.</param>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration WithFormOptions(FormOptions options)
	{
		_ep.SetFormOptions(options);
		return this;
	}

	/// <summary>
	/// Sets the file binding mode for this endpoint (Buffered or Stream).
	/// Buffered mode loads files into memory, while Stream mode provides direct stream access.
	/// </summary>
	/// <param name="mode">The file binding mode.</param>
	/// <returns>The configuration builder for fluent chaining.</returns>
	public EndpointConfiguration WithFileBindingMode(FileBindingMode mode)
	{
		_ep.SetFileBindingMode(mode);
		return this;
	}
}