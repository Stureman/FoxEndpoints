using Microsoft.AspNetCore.Http;

namespace FoxEndpoints;

/// <summary>
/// Thread-safe context holder for the current endpoint instance using AsyncLocal.
/// This allows the Send static class to access the current endpoint.
/// </summary>
internal static class EndpointContext<TRequest, TResponse>
{
    private static readonly AsyncLocal<EndpointBase?> _current = new();
    
    public static EndpointBase? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}