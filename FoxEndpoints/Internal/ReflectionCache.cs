using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace FoxEndpoints.Internal;

/// <summary>
/// Provides thread-safe caching for reflection metadata to improve performance.
/// Uses private lock object per Microsoft guidelines (not typeof or this).
/// </summary>
internal static class ReflectionCache
{
    private static readonly Dictionary<Type, PropertyInfo[]> PropertyCache = new();
    private static readonly Dictionary<Type, bool> FormDataBindingCache = new();
    private static readonly object CacheLock = new(); // Private lock - following Microsoft guidelines

    /// <summary>
    /// Gets properties for a type with caching to avoid repeated reflection calls.
    /// Thread-safe using double-check locking pattern.
    /// </summary>
    public static PropertyInfo[] GetCachedProperties(Type type)
    {
        if (PropertyCache.TryGetValue(type, out var cachedProperties))
            return cachedProperties;

        lock (CacheLock)
        {
            // Double-check after acquiring lock
            if (PropertyCache.TryGetValue(type, out cachedProperties))
                return cachedProperties;

            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyCache[type] = properties;
            return properties;
        }
    }

    /// <summary>
    /// Determines if an endpoint type requires form data configuration with caching.
    /// Thread-safe using double-check locking pattern.
    /// </summary>
    public static bool RequiresFormDataConfiguration(Type endpointType)
    {
        if (FormDataBindingCache.TryGetValue(endpointType, out var cachedResult))
            return cachedResult;

        lock (CacheLock)
        {
            // Double-check after acquiring lock
            if (FormDataBindingCache.TryGetValue(endpointType, out cachedResult))
                return cachedResult;

            var result = CheckFormDataRequirement(endpointType);
            FormDataBindingCache[endpointType] = result;
            return result;
        }
    }

    private static bool CheckFormDataRequirement(Type endpointType)
    {
        var baseType = EndpointDiscovery.GetEndpointBaseType(endpointType);
        if (baseType == null) return false;

        Type? requestType = null;

        if (baseType.IsGenericType)
        {
            var genericArgs = baseType.GetGenericArguments();
            if (genericArgs.Length > 0)
                requestType = genericArgs[0];
        }

        if (requestType == null) return false;

        var properties = GetCachedProperties(requestType);

        foreach (var property in properties)
        {
            if (property.PropertyType == typeof(IFormFile) ||
                property.PropertyType == typeof(IFormFileCollection))
                return true;

            if (property.PropertyType.IsGenericType)
            {
                var genericTypeDef = property.PropertyType.GetGenericTypeDefinition();
                var genericArgs = property.PropertyType.GetGenericArguments();

                if (genericArgs.Length > 0 && genericArgs[0] == typeof(IFormFile))
                {
                    if (genericTypeDef == typeof(List<>) ||
                        genericTypeDef == typeof(IEnumerable<>) ||
                        genericTypeDef == typeof(IList<>) ||
                        genericTypeDef == typeof(ICollection<>))
                        return true;
                }
            }

            if (property.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromFormAttribute>() != null)
                return true;
        }

        return false;
    }
}