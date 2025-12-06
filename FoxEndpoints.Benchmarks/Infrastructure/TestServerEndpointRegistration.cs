using System.Reflection;
using FoxEndpoints.Abstractions;

namespace FoxEndpoints.Benchmarks.Infrastructure;

/// <summary>
/// Helper to manually register FoxEndpoints in TestServer environment using Minimal APIs
/// </summary>
public static class TestServerEndpointRegistration
{
    public static void MapFoxEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var endpointTypes = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && IsEndpointType(t))
            .ToList();

        foreach (var type in endpointTypes)
        {
            try
            {
                RegisterEndpoint(endpoints, type);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to register endpoint {type.Name}: {ex.Message}");
            }
        }
    }

    private static void RegisterEndpoint(IEndpointRouteBuilder endpoints, Type endpointType)
    {
        // Create endpoint instance
        var instance = Activator.CreateInstance(endpointType);
        if (instance == null) return;

        // Call Configure() method
        var configureMethod = endpointType.GetMethod("Configure");
        configureMethod?.Invoke(instance, null);

        // Use reflection to get Route and Methods
        var routeProperty = typeof(EndpointBase).GetProperty("Route", BindingFlags.NonPublic | BindingFlags.Instance);
        var methodsProperty = typeof(EndpointBase).GetProperty("Methods", BindingFlags.NonPublic | BindingFlags.Instance);

        var route = routeProperty?.GetValue(instance) as string;
        var methods = methodsProperty?.GetValue(instance) as string[];

        if (string.IsNullOrEmpty(route) || methods == null || methods.Length == 0)
            return;

        // Get HandleAsync method
        var handleAsyncMethod = endpointType.GetMethod("HandleAsync");
        if (handleAsyncMethod == null) return;

        // Get request type from base class generic arguments
        var genericBase = endpointType.BaseType;
        while (genericBase != null && (!genericBase.IsGenericType || !genericBase.Name.StartsWith("Endpoint")))
        {
            genericBase = genericBase.BaseType;
        }

        if (genericBase == null) return;

        var genericArgs = genericBase.GetGenericArguments();
        if (genericArgs.Length < 2) return;

        var requestType = genericArgs[0];
        var httpContextProp = typeof(EndpointBase).GetProperty("HttpContext");

        // Register based on HTTP method
        foreach (var method in methods)
        {
            if (method == "GET" || method == "DELETE")
            {
                // For GET/DELETE, use query/route binding
                endpoints.MapMethods(route, new[] { method }, async (HttpContext ctx, CancellationToken ct) =>
                {
                    var ep = Activator.CreateInstance(endpointType);
                    if (ep == null) return Microsoft.AspNetCore.Http.Results.Problem("Failed to create endpoint instance");

                    httpContextProp?.SetValue(ep, ctx);

                    var request = BindFromHttpContext(requestType, ctx);
                    
                    try
                    {
                        var result = handleAsyncMethod.Invoke(ep, new[] { request, ct });
                        
                        if (result is Task<IResult> task)
                        {
                            return await task;
                        }
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem($"Endpoint execution failed: {ex.Message}");
                    }

                    return Microsoft.AspNetCore.Http.Results.Problem("Invalid endpoint response");
                });
            }
            else // POST, PUT, PATCH
            {
                endpoints.MapMethods(route, new[] { method }, async (HttpContext ctx, CancellationToken ct) =>
                {
                    var ep = Activator.CreateInstance(endpointType);
                    if (ep == null) return Microsoft.AspNetCore.Http.Results.Problem("Failed to create endpoint instance");

                    httpContextProp?.SetValue(ep, ctx);

                    try
                    {
                        var request = await ctx.Request.ReadFromJsonAsync(requestType, ct);
                        if (request == null) return Microsoft.AspNetCore.Http.Results.Problem("Invalid request body");

                        var result = handleAsyncMethod.Invoke(ep, new[] { request, ct });
                        
                        if (result is Task<IResult> task)
                        {
                            return await task;
                        }
                    }
                    catch (Exception ex)
                    {
                        return Microsoft.AspNetCore.Http.Results.Problem($"Endpoint execution failed: {ex.Message}");
                    }

                    return Microsoft.AspNetCore.Http.Results.Problem("Invalid endpoint response");
                });
            }
        }
    }

    private static object BindFromHttpContext(Type requestType, HttpContext ctx)
    {
        var request = Activator.CreateInstance(requestType);
        if (request == null) return new object();

        var properties = requestType.GetProperties();
        foreach (var prop in properties)
        {
            object? value = null;
            
            // Try route values first (case-insensitive matching)
            var routeKey = ctx.Request.RouteValues.Keys
                .FirstOrDefault(k => string.Equals(k, prop.Name, StringComparison.OrdinalIgnoreCase));
            
            if (routeKey != null && ctx.Request.RouteValues.TryGetValue(routeKey, out var routeValue))
            {
                value = routeValue;
            }
            // Then query string
            else if (ctx.Request.Query.TryGetValue(prop.Name, out var queryValue))
            {
                value = queryValue.ToString();
            }

            // Set the property value if found
            if (value != null)
            {
                try
                {
                    var convertedValue = Convert.ChangeType(value, prop.PropertyType);
                    prop.SetValue(request, convertedValue);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to convert {prop.Name}: {ex.Message}");
                }
            }
        }

        return request;
    }

    private static bool IsEndpointType(Type type)
    {
        var current = type.BaseType;
        while (current != null)
        {
            if (current.IsGenericType && current.Name.StartsWith("Endpoint"))
                return true;
            current = current.BaseType;
        }
        return false;
    }
}