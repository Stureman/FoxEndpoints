using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace FoxEndpoints.Internal;

/// <summary>
/// Handles creation of endpoint instances using dependency injection.
/// </summary>
internal static class EndpointFactory
{
    private static readonly Dictionary<Type, Func<IServiceProvider, object>> EndpointFactories = new();

    public static void RegisterFactory(Type endpointType, Func<IServiceProvider, object> factory)
    {
        EndpointFactories[endpointType] = factory;
    }

    public static object CreateInstance(Type endpointType, IServiceProvider services)
    {
        return EndpointFactories[endpointType](services);
    }

    /// <summary>
    /// Builds a compiled factory function for creating endpoint instances with dependency injection.
    /// Uses expression trees for optimal performance.
    /// </summary>
    public static Func<IServiceProvider, object> BuildFactory(Type endpointType)
    {
        var ctor = endpointType.GetConstructors()
            .OrderByDescending(c => c.GetParameters().Length)
            .First();

        var providerParam = Expression.Parameter(typeof(IServiceProvider), "sp");

        var args = ctor.GetParameters()
            .Select(p =>
                (Expression)Expression.Convert(
                    Expression.Call(
                        typeof(ActivatorUtilities).GetMethod(
                            nameof(ActivatorUtilities.GetServiceOrCreateInstance),
                            new[] { typeof(IServiceProvider), typeof(Type) })!,
                        providerParam,
                        Expression.Constant(p.ParameterType)
                    ),
                    p.ParameterType
                )
            )
            .ToArray();

        var body = Expression.New(ctor, args);

        var activator = Expression.Lambda<Func<IServiceProvider, object>>(body, providerParam).Compile();

        return sp =>
        {
            var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
            var scope = scopeFactory.CreateScope();
            try
            {
                return new ScopedEndpointWrapper(scope, activator(scope.ServiceProvider));
            }
            catch
            {
                scope.Dispose();
                throw;
            }
        };
    }

    /// <summary>
    /// Creates a design-time instance of an endpoint for configuration purposes.
    /// Uses default/null parameters if no parameterless constructor exists.
    /// </summary>
    public static object CreateDesignInstance(Type endpointType)
    {
        var parameterlessCtor = endpointType.GetConstructor(Type.EmptyTypes);
        if (parameterlessCtor != null)
            return Activator.CreateInstance(endpointType)!;

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

    internal sealed class ScopedEndpointWrapper : IDisposable, IAsyncDisposable
    {
        private readonly IServiceScope _scope;
        public object Endpoint { get; }

        public ScopedEndpointWrapper(IServiceScope scope, object endpoint)
        {
            _scope = scope;
            Endpoint = endpoint;
        }

        public void Dispose()
        {
            if (Endpoint is IDisposable disposable)
                disposable.Dispose();
            _scope.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (Endpoint is IAsyncDisposable asyncDisposable)
                await asyncDisposable.DisposeAsync();
            else if (Endpoint is IDisposable disposable)
                disposable.Dispose();

            if (_scope is IAsyncDisposable asyncScope)
                await asyncScope.DisposeAsync();
            else
                _scope.Dispose();
        }
    }
}