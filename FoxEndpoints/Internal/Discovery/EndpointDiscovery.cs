using FoxEndpoints.Abstractions;

namespace FoxEndpoints.Internal.Discovery;

/// <summary>
/// Provides utilities for discovering and validating endpoint types.
/// </summary>
internal static class EndpointDiscovery
{
    public static bool IsEndpointType(Type t)
        => IsSubclassOfRawGeneric(t, typeof(Endpoint<,>))
           || IsSubclassOfRawGeneric(t, typeof(EndpointWithoutRequest<>))
           || IsSubclassOfRawGeneric(t, typeof(EndpointWithoutResponse<>))
           || IsSubclassOfRawGeneric(t, typeof(Endpoint));

    public static Type? GetEndpointBaseType(Type t)
    {
        while (t != typeof(object))
        {
            var g = t.IsGenericType ? t.GetGenericTypeDefinition() : t;
            if (g == typeof(Endpoint<,>)
                || g == typeof(EndpointWithoutRequest<>)
                || g == typeof(EndpointWithoutResponse<>)
                || t == typeof(Endpoint))
                return t;

            var baseType = t.BaseType;
            if (baseType == null)
                break;
			
            t = baseType;
        }

        return null;
    }

    private static bool IsSubclassOfRawGeneric(Type toTest, Type generic)
    {
        while (toTest != typeof(object))
        {
            var current = toTest.IsGenericType ? toTest.GetGenericTypeDefinition() : toTest;
            if (current == generic)
                return true;

            var baseType = toTest.BaseType;
            if (baseType == null)
                break;
			
            toTest = baseType;
        }

        return false;
    }
}