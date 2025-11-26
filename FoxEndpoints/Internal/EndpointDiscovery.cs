namespace FoxEndpoints.Internal;

/// <summary>
/// Provides utilities for discovering and validating endpoint types.
/// </summary>
internal static class EndpointDiscovery
{
    public static bool IsEndpointType(Type t)
        => IsSubclassOfRawGeneric(t, typeof(Endpoint<,>))
           || IsSubclassOfRawGeneric(t, typeof(EndpointWithoutRequest<>))
           || IsSubclassOfRawGeneric(t, typeof(EndpointWithoutResponse<>));

    public static Type? GetEndpointBaseType(Type t)
    {
        while (t != null && t != typeof(object))
        {
            var g = t.IsGenericType ? t.GetGenericTypeDefinition() : t;
            if (g == typeof(Endpoint<,>)
                || g == typeof(EndpointWithoutRequest<>)
                || g == typeof(EndpointWithoutResponse<>))
                return t;

            t = t.BaseType!;
        }
        return null;
    }

    private static bool IsSubclassOfRawGeneric(Type toTest, Type generic)
    {
        while (toTest != null && toTest != typeof(object))
        {
            var current = toTest.IsGenericType ? toTest.GetGenericTypeDefinition() : toTest;
            if (current == generic)
                return true;

            toTest = toTest.BaseType!;
        }
        return false;
    }
}