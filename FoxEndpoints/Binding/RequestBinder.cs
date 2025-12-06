using System.Reflection;
using FoxEndpoints.Binding.Attributes;
using FoxEndpoints.Internal;
using FoxEndpoints.Internal.Discovery;
using FoxEndpoints.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;

namespace FoxEndpoints.Binding;

/// <summary>
/// Handles binding of HTTP requests to strongly-typed request objects.
/// </summary>
internal static class RequestBinder
{
	public static TRequest BindFromHttpContext<TRequest>(HttpContext context)
	{
		var requestType = typeof(TRequest);
		var properties = ReflectionCache.GetCachedProperties(requestType);
		var allowlist = ReflectionCache.GetBindAllowlist(requestType);
		var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

		var routeValues = ToCaseInsensitiveDictionary(context.Request.RouteValues);
		var queryValues = context.Request.Query
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.OrdinalIgnoreCase);

		var propertyValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

		foreach (var property in properties)
		{
			if (!CanBindProperty(property, allowlist))
				continue;

			var propertyName = property.Name;
			object? valueToConvert = null;

			if (routeValues.TryGetValue(propertyName, out var routeValue))
			{
				valueToConvert = routeValue;
			}
			else if (queryValues.TryGetValue(propertyName, out var queryValue))
			{
				valueToConvert = queryValue;
			}

			if (valueToConvert == null)
				continue;

			if (!TryConvertValue(property, valueToConvert, propertyValues, errors))
				continue;
		}

		ThrowIfErrors(errors);

		return CreateInstance<TRequest>(requestType, properties, propertyValues);
	}

	public static TRequest MergeRouteParameters<TRequest>(TRequest request, HttpContext context)
	{
		if (request == null)
			throw new ArgumentNullException(nameof(request));

		var routeValues = ToCaseInsensitiveDictionary(context.Request.RouteValues);
		if (routeValues.Count == 0)
			return request;

		var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		var requestType = typeof(TRequest);
		var properties = ReflectionCache.GetCachedProperties(requestType);
		var allowlist = ReflectionCache.GetBindAllowlist(requestType);

		foreach (var property in properties)
		{
			if (!CanBindProperty(property, allowlist) || !property.CanWrite)
				continue;

			var propertyName = property.Name;
			if (!routeValues.TryGetValue(propertyName, out var routeValue) || routeValue is null)
				continue;

			var currentValue = property.GetValue(request);
			if (!IsDefaultValue(currentValue, property.PropertyType))
				continue;

			try
			{
				var converted = ConvertForAssignment(property, routeValue);
				property.SetValue(request, converted);
			}
			catch (Exception ex)
			{
				AddError(errors, propertyName, ex.Message);
			}
		}

		ThrowIfErrors(errors);
		return request;
	}

	public static async Task<TRequest> BindFromFormAsync<TRequest>(HttpContext context, FormOptions? formOptions,
		FileBindingMode? fileBindingMode = null)
	{
		var requestType = typeof(TRequest);
		var properties = ReflectionCache.GetCachedProperties(requestType);
		var allowlist = ReflectionCache.GetBindAllowlist(requestType);
		var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
		var propertyValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

		var effectiveBindingMode = fileBindingMode ?? FoxEndpointsSettings.FileBindingMode;

		var routeValues = ToCaseInsensitiveDictionary(context.Request.RouteValues);
		var queryValues = context.Request.Query
			.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(), StringComparer.OrdinalIgnoreCase);

		IFormCollection? form = null;
		Dictionary<string, string> formValues;
		if (context.Request.HasFormContentType)
		{
			var options = formOptions ?? FoxEndpointsSettings.FormOptions;
			form = await context.Request.ReadFormAsync(options);
			formValues = form.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(),
				StringComparer.OrdinalIgnoreCase);
		}
		else
		{
			formValues = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		foreach (var property in properties)
		{
			if (!CanBindProperty(property, allowlist))
				continue;

			var propertyName = property.Name;
			object? value = null;

			if (form != null && TryBindFileProperty(property, form, effectiveBindingMode, out value))
			{
				propertyValues[propertyName] = value;
				continue;
			}

			if (routeValues.TryGetValue(propertyName, out var routeValue))
			{
				value = routeValue;
			}
			else if (formValues.TryGetValue(propertyName, out var formValue))
			{
				value = formValue;
			}
			else if (queryValues.TryGetValue(propertyName, out var queryValue))
			{
				value = queryValue;
			}

			if (value == null)
				continue;

			if (!TryConvertValue(property, value, propertyValues, errors))
				continue;
		}

		ThrowIfErrors(errors);

		return CreateInstance<TRequest>(requestType, properties, propertyValues);
	}

	/// <summary>
	/// Creates an instance of TRequest, prioritizing primary constructors (records) and falling back to parameterless constructors.
	/// </summary>
	private static TRequest CreateInstance<TRequest>(Type requestType, PropertyInfo[] properties,
		Dictionary<string, object?> propertyValues)
	{
		var constructors = requestType.GetConstructors()
			.OrderByDescending(c => c.GetParameters().Length);

		foreach (var ctor in constructors)
		{
			var ctorParams = ctor.GetParameters();
			if (ctorParams.Length == 0) continue;

			var args = new object?[ctorParams.Length];
			bool canUse = true;

			for (int i = 0; i < ctorParams.Length; i++)
			{
				var param = ctorParams[i];
				var lookupName = param.Name ?? string.Empty;

				if (propertyValues.TryGetValue(lookupName, out var value) ||
				    propertyValues.TryGetValue(ToPascalCase(lookupName), out value))
				{
					args[i] = value;
				}
				else if (param.HasDefaultValue)
				{
					args[i] = param.DefaultValue;
				}
				else
				{
					// If a parameter has no value and no default, we can't use this constructor
					canUse = false;
					break;
				}
			}

			if (canUse)
			{
				try
				{
					return (TRequest)ctor.Invoke(args);
				}
				catch (Exception)
				{
					// This constructor failed, try the next one
					continue;
				}
			}
		}

		// Fallback to parameterless constructor if no other constructor worked
		var instance = Activator.CreateInstance(requestType);
		if (instance == null)
			throw new InvalidOperationException(
				$"Could not create an instance of {requestType.Name}. Ensure it has a parameterless constructor or a constructor whose parameters match the request properties.");

		foreach (var prop in properties)
		{
			if (prop.CanWrite && propertyValues.TryGetValue(prop.Name, out var value))
			{
				prop.SetValue(instance, value);
			}
		}

		return (TRequest)instance;
	}

	private static bool TryConvertValue(PropertyInfo property, object value, Dictionary<string, object?> propertyValues,
		Dictionary<string, List<string>> errors)
	{
		try
		{
			var converted = ConvertForAssignment(property, value);
			propertyValues[property.Name] = converted;
			return true;
		}
		catch (Exception ex)
		{
			AddError(errors, property.Name, ex.Message);
			return false;
		}
	}

	private static void ThrowIfErrors(Dictionary<string, List<string>> errors)
	{
		if (errors.Count == 0)
			return;

		throw RequestBindingException.FromErrors(errors);
	}

	private static Dictionary<string, object?> ToCaseInsensitiveDictionary(RouteValueDictionary values)
	{
		var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
		foreach (var pair in values)
		{
			dict[pair.Key] = pair.Value;
		}

		return dict;
	}

	private static object? ConvertForAssignment(PropertyInfo property, object value)
	{
		var targetType = property.PropertyType;
		var underlyingType = Nullable.GetUnderlyingType(targetType);

		if (underlyingType != null)
		{
			var stringValue = value?.ToString();
			if (string.IsNullOrWhiteSpace(stringValue))
				return null;

			return ValueConverter.Convert(stringValue, underlyingType);
		}

		if (targetType == typeof(string))
			return value?.ToString();

		return ValueConverter.Convert(value, targetType);
	}

	private static bool IsDefaultValue(object? currentValue, Type propertyType)
	{
		if (currentValue == null)
			return true;

		if (!propertyType.IsValueType)
			return false;

		var defaultValue = Activator.CreateInstance(propertyType);
		return currentValue.Equals(defaultValue);
	}

	private static void AddError(Dictionary<string, List<string>> errors, string propertyName, string message)
	{
		if (!errors.TryGetValue(propertyName, out var list))
		{
			list = new List<string>();
			errors[propertyName] = list;
		}

		list.Add(message);
	}

	private static bool TryBindFileProperty(PropertyInfo property, IFormCollection form, FileBindingMode bindingMode,
		out object? value)
	{
		value = null;

		if (property.PropertyType == typeof(IFormFile))
		{
			value = form.Files.GetFile(property.Name)
			        ?? form.Files.FirstOrDefault(f =>
				        string.Equals(f.Name, property.Name, StringComparison.OrdinalIgnoreCase));
			return value != null;
		}

		if (bindingMode == FileBindingMode.Stream && property.PropertyType == typeof(StreamFile))
		{
			var file = form.Files.GetFile(property.Name)
			           ?? form.Files.FirstOrDefault(f =>
				           string.Equals(f.Name, property.Name, StringComparison.OrdinalIgnoreCase));
			if (file != null)
			{
				value = StreamFile.FromFormFile(file);
				return true;
			}

			return false;
		}

		if (property.PropertyType == typeof(IFormFileCollection))
		{
			if (form.Files.Count > 0)
			{
				value = form.Files;
				return true;
			}

			return false;
		}

		if (bindingMode == FileBindingMode.Stream && property.PropertyType.IsGenericType &&
		    property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
		    && property.PropertyType.GetGenericArguments()[0] == typeof(StreamFile))
		{
			var files = form.Files
				.Where(f => string.Equals(f.Name, property.Name, StringComparison.OrdinalIgnoreCase))
				.Select(StreamFile.FromFormFile)
				.ToList();

			if (files.Count > 0)
			{
				value = files;
				return true;
			}
		}

		if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
		                                        && property.PropertyType.GetGenericArguments()[0] == typeof(IFormFile))
		{
			var files = form.Files
				.Where(f => string.Equals(f.Name, property.Name, StringComparison.OrdinalIgnoreCase))
				.Cast<IFormFile>()
				.ToList();

			if (files.Count > 0)
			{
				value = files;
				return true;
			}
		}

		return false;
	}

	private static string ToPascalCase(string value)
	{
		if (string.IsNullOrEmpty(value))
			return value;

		if (char.IsUpper(value[0]))
			return value;

		return char.ToUpperInvariant(value[0]) + value.Substring(1);
	}

	private static bool CanBindProperty(PropertyInfo property, HashSet<string>? allowlist)
	{
		if (property.GetCustomAttribute<BindNeverAttribute>(inherit: true) != null)
			return false;

		if (allowlist == null || allowlist.Count == 0)
			return true;

		return allowlist.Contains(property.Name);
	}
}