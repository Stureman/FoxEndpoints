using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace FoxEndpoints.Internal;

/// <summary>
/// Handles binding of HTTP requests to strongly-typed request objects.
/// </summary>
internal static class RequestBinder
{
    public static TRequest BindFromHttpContext<TRequest>(HttpContext context)
    {
        var requestType = typeof(TRequest);
        var properties = ReflectionCache.GetCachedProperties(requestType);

        // Collect values from route and query string
        var propertyValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in properties)
        {
            var propertyName = property.Name;
            object? valueToConvert = null;
            bool hasValue = false;

            // Try to get value from route values first
            if (context.Request.RouteValues.TryGetValue(propertyName, out var routeValue))
            {
                valueToConvert = routeValue;
                hasValue = true;
            }
            else
            {
                // Try case-insensitive route matching
                var routeKey = context.Request.RouteValues.Keys
                    .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

                if (routeKey != null)
                {
                    valueToConvert = context.Request.RouteValues[routeKey];
                    hasValue = true;
                }
                else
                {
                    // Try query string (case insensitive)
                    var queryKey = context.Request.Query.Keys
                        .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

                    if (queryKey != null)
                    {
                        valueToConvert = context.Request.Query[queryKey].ToString();
                        hasValue = true;
                    }
                }
            }

            if (hasValue && valueToConvert != null)
            {
                try
                {
                    var targetType = property.PropertyType;
                    var underlyingType = Nullable.GetUnderlyingType(targetType);

                    if (underlyingType != null)
                    {
                        // Nullable type
                        var stringValue = valueToConvert.ToString();
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            var convertedValue = ValueConverter.Convert(stringValue, underlyingType);
                            propertyValues[propertyName] = convertedValue;
                        }
                        else
                        {
                            propertyValues[propertyName] = null;
                        }
                    }
                    else
                    {
                        // Regular type
                        var convertedValue = ValueConverter.Convert(valueToConvert, targetType);
                        propertyValues[propertyName] = convertedValue;
                    }
                }
                catch (Exception)
                {
                    // Gracefully skip properties that cannot be converted
                }
            }
        }

        // Create instance using primary constructor if available, otherwise use parameterless constructor
        return CreateInstance<TRequest>(requestType, properties, propertyValues);
    }

    public static TRequest MergeRouteParameters<TRequest>(TRequest request, HttpContext context)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        var requestType = typeof(TRequest);
        var properties = ReflectionCache.GetCachedProperties(requestType);

        foreach (var property in properties)
        {
            if (!property.CanWrite) continue;

            var propertyName = property.Name;

            var routeKey = context.Request.RouteValues.Keys
                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

            if (routeKey != null)
            {
                var currentValue = property.GetValue(request);
                var isDefaultValue = currentValue == null ||
                                   (property.PropertyType.IsValueType &&
                                    currentValue.Equals(Activator.CreateInstance(property.PropertyType)));

                if (isDefaultValue)
                {
                    var routeValue = context.Request.RouteValues[routeKey];
                    var underlyingType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var convertedValue = Convert.ChangeType(routeValue, underlyingType);
                    property.SetValue(request, convertedValue);
                }
            }
        }

        return request;
    }

    public static async Task<TRequest> BindFromFormAsync<TRequest>(HttpContext context)
    {
        var requestType = typeof(TRequest);
        var properties = ReflectionCache.GetCachedProperties(requestType);

        // Collect values for constructor parameters
        var propertyValues = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (context.Request.HasFormContentType && context.Request.Form != null)
        {
            var form = await context.Request.ReadFormAsync();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                object? valueToSet = null;
                bool hasValue = false;

                // Check for IFormFile
                if (property.PropertyType == typeof(IFormFile))
                {
                    var file = form.Files.GetFile(propertyName)
                               ?? form.Files.FirstOrDefault(f => string.Equals(f.Name, propertyName, StringComparison.OrdinalIgnoreCase));
                    if (file != null)
                    {
                        valueToSet = file;
                        hasValue = true;
                    }
                }
                // Check for IFormFileCollection
                else if (property.PropertyType == typeof(IFormFileCollection))
                {
                    if (form.Files.Count > 0)
                    {
                        valueToSet = form.Files;
                        hasValue = true;
                    }
                }
                // Check for List<IFormFile>
                else if (property.PropertyType.IsGenericType &&
                         property.PropertyType.GetGenericTypeDefinition() == typeof(List<>) &&
                         property.PropertyType.GetGenericArguments()[0] == typeof(IFormFile))
                {
                    var filesForProperty = form.Files
                        .Where(f => string.Equals(f.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (filesForProperty.Count > 0)
                    {
                        valueToSet = filesForProperty;
                        hasValue = true;
                    }
                }
                else
                {
                    // Try route values first
                    if (context.Request.RouteValues.TryGetValue(propertyName, out var routeValue))
                    {
                        valueToSet = routeValue;
                        hasValue = true;
                    }
                    else
                    {
                        // Try case-insensitive route match
                        var routeKey = context.Request.RouteValues.Keys
                            .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

                        if (routeKey != null)
                        {
                            valueToSet = context.Request.RouteValues[routeKey];
                            hasValue = true;
                        }
                        else
                        {
                            // Try form fields
                            var formKey = form.Keys
                                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

                            if (formKey != null)
                            {
                                valueToSet = form[formKey].ToString();
                                hasValue = true;
                            }
                            else
                            {
                                // Try query string
                                var queryKey = context.Request.Query.Keys
                                    .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));

                                if (queryKey != null)
                                {
                                    valueToSet = context.Request.Query[queryKey].ToString();
                                    hasValue = true;
                                }
                            }
                        }
                    }

                    // Convert the value if needed
                    if (hasValue && valueToSet != null && property.PropertyType != typeof(IFormFile))
                    {
                        try
                        {
                            var targetType = property.PropertyType;
                            var underlyingType = Nullable.GetUnderlyingType(targetType);

                            if (underlyingType != null)
                            {
                                // Nullable type
                                var stringValue = valueToSet.ToString();
                                if (!string.IsNullOrWhiteSpace(stringValue))
                                    valueToSet = ValueConverter.Convert(stringValue, underlyingType);
                                else
                                    valueToSet = null;
                            }
                            else if (targetType != typeof(string))
                            {
                                // Non-string type - convert
                                valueToSet = ValueConverter.Convert(valueToSet, targetType);
                            }
                        }
                        catch (Exception)
                        {
                            // Skip properties that can't be converted
                            hasValue = false;
                        }
                    }
                }

                if (hasValue)
                {
                    propertyValues[propertyName] = valueToSet;
                }
            }
        }

        return CreateInstance<TRequest>(requestType, properties, propertyValues);
    }

    /// <summary>
    /// Creates an instance of TRequest, prioritizing primary constructors (records) and falling back to parameterless constructors.
    /// </summary>
    private static TRequest CreateInstance<TRequest>(Type requestType, PropertyInfo[] properties, Dictionary<string, object?> propertyValues)
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
                if (propertyValues.TryGetValue(param.Name!, out var value))
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
            throw new InvalidOperationException($"Could not create an instance of {requestType.Name}. Ensure it has a parameterless constructor or a constructor whose parameters match the request properties.");

        foreach (var prop in properties)
        {
            if (prop.CanWrite && propertyValues.TryGetValue(prop.Name, out var value))
            {
                prop.SetValue(instance, value);
            }
        }

        return (TRequest)instance;
    }
}