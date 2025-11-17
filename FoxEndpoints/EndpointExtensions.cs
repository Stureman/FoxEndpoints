using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FoxEndpoints;

public static class EndpointExtensions
{
    private static readonly Dictionary<Type, Func<IServiceProvider, object>> _endpointFactories = new();

    public static WebApplication UseFoxEndpoints(this WebApplication app)
    {
        var entryAssembly = Assembly.GetEntryAssembly()
                            ?? throw new InvalidOperationException("EntryAssembly is null.");

        var endpointTypes = entryAssembly
            .GetTypes()
            .Where(t => !t.IsAbstract && IsEndpointType(t))
            .ToList();

        foreach (var type in endpointTypes)
        {
            // -----------------------------------------------
            // 1) Skapa en design-instans för Configure()
            //    Denna "kastas" sedan bort, används bara för att hämta Route och Methods
            // -----------------------------------------------
            var designInstance = (EndpointBase)CreateDesignInstance(type);
            designInstance.Configure();

            if (string.IsNullOrWhiteSpace(designInstance.Route) || designInstance.Methods.Length == 0)
                throw new InvalidOperationException(
                    $"Endpoint {type.Name} must call Get/Post/Put/Delete inside Configure().");

            // -----------------------------------------------
            // 2) Skapa en DI-factory och cache:a den
            //    => snabbaste sättet att skapa endpoints
            // -----------------------------------------------
            var factory = CreateFactory(type);
            _endpointFactories[type] = factory;

            // -----------------------------------------------
            // 3) Hämta static BuildHandler(Type, string)
            // -----------------------------------------------
            var baseType = GetEndpointBaseType(type)!;

            var buildHandlerMethod = baseType.GetMethod(
                "BuildHandler",
                BindingFlags.NonPublic | BindingFlags.Static
            ) ?? throw new InvalidOperationException($"Missing BuildHandler in {type.Name}");

            var httpMethod = designInstance.Methods[0]; // Get the HTTP method (GET, POST, DELETE, etc.)
            var handler = (Delegate)buildHandlerMethod.Invoke(null, new object[] { type, httpMethod })!;

            // -----------------------------------------------
            // 4) Mappa endpoint
            // -----------------------------------------------
            var builder = app.MapMethods(designInstance.Route, designInstance.Methods, handler);

            builder.WithName(type.Name);

            var attrs = type.GetCustomAttributes(true).Cast<object>().ToArray();
            if (attrs.Length > 0)
                builder.WithMetadata(attrs);

            designInstance.ApplyConfigurators(builder);
        }

        return app;
    }

    // ---------------------------------------
    // Factory builder — "Hämtar" den konstruktorn med mest parametrar
    // Bygger en expression med param ServiceProvider sp som skickas i runtime
    // För varje parameter i konstruktorn skapar vi ett expression som hämtar eller skapar servicen
    // Och sedan "castar" den till till den exakta typen av parameter.
    // Slutligen retunerar vi en kompilerad "delegate" som kommer cacheas i _endpointFactories
    // ---------------------------------------
    private static Func<IServiceProvider, object> CreateFactory(Type endpointType)
    {
        // Hämta bästa matchande konstruktor
        var ctor = endpointType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        var providerParam = Expression.Parameter(typeof(IServiceProvider), "sp");

        var args = ctor.GetParameters()
            .Select(p =>
                (Expression)Expression.Convert(
                    Expression.Call(
                        typeof(ActivatorUtilities).GetMethod(nameof(ActivatorUtilities.GetServiceOrCreateInstance), new[] { typeof(IServiceProvider), typeof(Type) })!,
                        providerParam,
                        Expression.Constant(p.ParameterType)
                    ),
                    p.ParameterType
                )
            )
            .ToArray();

        var body = Expression.New(ctor, args);

        return Expression.Lambda<Func<IServiceProvider, object>>(body, providerParam).Compile();
    }

    // ---------------------------------------
    // Helpers
    // ---------------------------------------

    private static object CreateDesignInstance(Type endpointType)
    {
        // Försök med "parameterless" konstruktor först.
        var parameterlessCtor = endpointType.GetConstructor(Type.EmptyTypes);
        if (parameterlessCtor != null)
        {
            return Activator.CreateInstance(endpointType)!;
        }

        // Om ingen "parameterless" konstruktor, skapa en instans med default/null parameterar
        var ctor = endpointType.GetConstructors()
            .OrderBy(c => c.GetParameters().Length)
            .First();

        var parameters = ctor.GetParameters()
            .Select(p => p.ParameterType.IsValueType 
                ? Activator.CreateInstance(p.ParameterType) 
                : null)
            .ToArray();

        return ctor.Invoke(parameters);
    }

    public static object CreateEndpointInstance(Type endpointType, IServiceProvider services)
        => _endpointFactories[endpointType](services);

    public static TRequest BindFromHttpContext<TRequest>(HttpContext context)
    {
        var requestType = typeof(TRequest);
        
        // Hämta alla properties från request typen
        var properties = requestType.GetProperties();
        
        // Skapa en instans med "parameterless" konstruktor eller default
        var request = Activator.CreateInstance(requestType);
        if (request == null)
        {
            throw new InvalidOperationException($"Cannot create instance of {requestType.Name}");
        }
        
        // Bind varje property från route values eller query string
        foreach (var property in properties)
        {
            if (!property.CanWrite) continue;
            
            var propertyName = property.Name;
            object? valueToConvert = null;
            bool hasValue = false;
            
            // Försök hämta värdena från route values först
            if (context.Request.RouteValues.TryGetValue(propertyName, out var routeValue))
            {
                valueToConvert = routeValue;
                hasValue = true;
            }
            else
            {
                // Försök med case-insensitive matchining
                var routeKey = context.Request.RouteValues.Keys
                    .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));
                
                if (routeKey != null)
                {
                    valueToConvert = context.Request.RouteValues[routeKey];
                    hasValue = true;
                }
                else
                {
                    // Försök med query string (case insensitive)
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
                    // Hantera nullable typer
                    var targetType = property.PropertyType;
                    var underlyingType = Nullable.GetUnderlyingType(targetType);
                    
                    if (underlyingType != null)
                    {
                        // Det är en nullable type (int?, bool?, etc.)
                        var stringValue = valueToConvert.ToString();
                        if (!string.IsNullOrWhiteSpace(stringValue))
                        {
                            var convertedValue = Convert.ChangeType(stringValue, underlyingType);
                            property.SetValue(request, convertedValue);
                        }
                        // Om null eller empty, lämna som null (default for nullable)
                    }
                    else
                    {
                        // Vanlig typ
                        var convertedValue = Convert.ChangeType(valueToConvert, targetType);
                        property.SetValue(request, convertedValue);
                    }
                }
                catch (Exception)
                {
                    // Hoppa över properties som inte kan konverteras
                    // Gracefull error handling
                }
            }
        }
        
        return (TRequest)request;
    }

    public static TRequest MergeRouteParameters<TRequest>(TRequest request, HttpContext context)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = typeof(TRequest);
        
        // If it's a value type or immutable type (like records), we need to create a new instance
        // For records, we'll use reflection to set properties
        var properties = requestType.GetProperties();
        
        foreach (var property in properties)
        {
            if (!property.CanWrite) continue;
            
            var propertyName = property.Name;
            
            // Check if there's a route value that matches this property
            var routeKey = context.Request.RouteValues.Keys
                .FirstOrDefault(k => string.Equals(k, propertyName, StringComparison.OrdinalIgnoreCase));
            
            if (routeKey != null)
            {
                // Get current value - if it's default/null, replace with route value
                var currentValue = property.GetValue(request);
                var isDefaultValue = currentValue == null || 
                                   (property.PropertyType.IsValueType && currentValue.Equals(Activator.CreateInstance(property.PropertyType)));
                
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

    private static bool IsEndpointType(Type t)
        => IsSubclassOfRawGeneric(t, typeof(Endpoint<,>))
           || IsSubclassOfRawGeneric(t, typeof(EndpointWithoutRequest<>))
           || IsSubclassOfRawGeneric(t, typeof(EndpointWithoutResponse<>));

    private static Type? GetEndpointBaseType(Type t)
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