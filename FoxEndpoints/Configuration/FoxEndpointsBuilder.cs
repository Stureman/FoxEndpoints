using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.Builder;
using FoxEndpoints.Abstractions;
using FoxEndpoints.Internal;
using FoxEndpoints.Internal.Discovery;
using FoxEndpoints.Internal.Factory;
using FoxEndpoints.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;

namespace FoxEndpoints.Configuration;

/// <summary>
/// Builder for configuring global FoxEndpoints settings such as authorization, form options, and file binding modes.
/// </summary>
public class FoxEndpointsBuilder
{
	private readonly WebApplication _app;
	private bool _requireAuthorization;
	private bool _isBuilt;
	private Action<FormOptions>? _formOptionsConfigurator;
	private FileBindingMode _fileBindingMode = FileBindingMode.Buffered;

	internal FoxEndpointsBuilder(WebApplication app)
	{
		_app = app;
	}

	/// <summary>
	/// Requires authorization for all FoxEndpoints by default.
	/// Individual endpoints can opt out using .AllowAnonymous() in their Configure() method.
	/// </summary>
	/// <example>
	/// app.UseFoxEndpoints(c => c.RequireAuthorization());
	/// </example>
	public void RequireAuthorization()
	{
		_requireAuthorization = true;
	}

	/// <summary>
	/// Configures global form options for all endpoints that accept multipart/form-data.
	/// Individual endpoints can override these settings using WithFormOptions().
	/// </summary>
	/// <param name="configure">Action to configure the FormOptions (e.g., file size limits, buffer thresholds).</param>
	public void ConfigureFormOptions(Action<FormOptions> configure)
	{
		_formOptionsConfigurator = configure ?? throw new ArgumentNullException(nameof(configure));
	}

	/// <summary>
	/// Sets the default file binding mode for all endpoints that accept file uploads.
	/// Individual endpoints can override this setting using WithFileBindingMode().
	/// </summary>
	/// <param name="mode">The file binding mode (Buffered or Stream).</param>
	public void UseFileBindingMode(FileBindingMode mode)
	{
		_fileBindingMode = mode;
	}

	internal WebApplication Build()
	{
		// Prevent double-building
		if (_isBuilt)
			return _app;

		_isBuilt = true;

		if (_formOptionsConfigurator is not null)
		{
			var options = FoxEndpointsSettings.FormOptions;
			_formOptionsConfigurator(options);
			FoxEndpointsSettings.ConfigureFormOptions(options);
		}

		FoxEndpointsSettings.SetFileBindingMode(_fileBindingMode);

		var entryAssembly = Assembly.GetEntryAssembly()
		                    ?? throw new InvalidOperationException("EntryAssembly is null.");

		var endpointTypes = entryAssembly
			.GetTypes()
			.Where(t => !t.IsAbstract && EndpointDiscovery.IsEndpointType(t))
			.ToList();

		// Collect all API versions from endpoint attributes
		var allVersions = new HashSet<ApiVersion>();
		foreach (var type in endpointTypes)
		{
			var versionAttrs = type.GetCustomAttributes(true).OfType<ApiVersionAttribute>();
			foreach (var versionAttr in versionAttrs)
			{
				foreach (var version in versionAttr.Versions)
				{
					allVersions.Add(version);
				}
			}
		}

		// Create API version set if any versions were found
		ApiVersionSet? versionSet = null;
		if (allVersions.Any())
		{
			var versionSetBuilder = _app.NewApiVersionSet();
			foreach (var version in allVersions)
			{
				versionSetBuilder.HasApiVersion(version);
			}

			versionSet = versionSetBuilder.ReportApiVersions().Build();
		}

		foreach (var type in endpointTypes)
		{
			// 1) Create a design instance for Configure()
			var designInstance = (EndpointBase)EndpointFactory.CreateDesignInstance(type);
			designInstance.Configure();

			if (string.IsNullOrWhiteSpace(designInstance.Route) || designInstance.Methods.Length == 0)
				throw new InvalidOperationException(
					$"Endpoint {type.Name} must call Get/Post/Put/Delete inside Configure().");

			// 2) Create and cache DI factory
			var factory = EndpointFactory.BuildFactory(type);
			EndpointFactory.RegisterFactory(type, factory);

			// 3) Get static BuildHandler method
			var baseType = EndpointDiscovery.GetEndpointBaseType(type)!;

			var buildHandlerMethod = baseType.GetMethod(
				"BuildHandler",
				BindingFlags.NonPublic | BindingFlags.Static
			) ?? throw new InvalidOperationException($"Missing BuildHandler in {type.Name}");

			var httpMethod = designInstance.Methods[0];
			var handler = (Delegate)buildHandlerMethod.Invoke(null, new object[] { type, httpMethod })!;

			// 4) Map endpoint
			var builder = _app.MapMethods(designInstance.Route, designInstance.Methods, handler);

			builder.WithName(type.Name);

			// Apply API versioning if version set exists
			if (versionSet != null)
			{
				builder.WithApiVersionSet(versionSet);

				var versionAttrs = type.GetCustomAttributes(true).OfType<ApiVersionAttribute>();
				foreach (var versionAttr in versionAttrs)
				{
					foreach (var version in versionAttr.Versions)
					{
						builder.MapToApiVersion(version);
					}
				}
			}

			// Apply all attributes from the endpoint class as metadata
			var attrs = type.GetCustomAttributes(true);
			if (attrs.Length > 0)
				builder.WithMetadata(attrs);

			designInstance.ApplyConfigurators(builder);

			// Note: Form data binding is automatically handled in BuildHandler based on request type detection.
			// Developers should explicitly call .AllowFileUploads() or .AcceptsFormData() in Configure()
			// to add proper OpenAPI metadata, and .DisableAntiforgery() when needed for cookie auth scenarios.

			// Apply global authorization if enabled
			if (_requireAuthorization)
			{
				builder.RequireAuthorization();
			}
		}

		return _app;
	}
}