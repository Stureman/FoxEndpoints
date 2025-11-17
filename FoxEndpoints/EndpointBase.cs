using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FoxEndpoints;

public abstract class EndpointBase
{
    public HttpContext HttpContext { get; internal set; } = default!;
    internal string? Route { get; private set; }
    internal string[] Methods { get; private set; } = Array.Empty<string>();
    internal List<Action<RouteHandlerBuilder>> Configurators { get; } = new();

    protected EndpointBuilder Get(string route) => Verb(HttpMethods.Get, route);
    protected EndpointBuilder Post(string route) => Verb(HttpMethods.Post, route);
    protected EndpointBuilder Put(string route) => Verb(HttpMethods.Put, route);
    protected EndpointBuilder Delete(string route) => Verb(HttpMethods.Delete, route);
    
   


    private EndpointBuilder Verb(string method, string route)
    {
        Route = route;
        Methods = new[] { method };
        return new EndpointBuilder(this);
    }

    internal void AddConfigurator(Action<RouteHandlerBuilder> config)
        => Configurators.Add(config);

    internal void ApplyConfigurators(RouteHandlerBuilder builder)
    {
        foreach (var config in Configurators)
            config(builder);
    }

    public abstract void Configure();
}

public sealed class EndpointBuilder
{
    private readonly EndpointBase _ep;

    internal EndpointBuilder(EndpointBase ep)
    {
        _ep = ep;
    }

    public EndpointBuilder WithName(string name)
    {
        _ep.AddConfigurator(b => b.WithName(name));
        return this;
    }

    public EndpointBuilder WithTags(params string[] tags)
    {
        _ep.AddConfigurator(b => b.WithTags(tags));
        return this;
    }

    public EndpointBuilder Produces(int statusCode)
    {
        _ep.AddConfigurator(b => b.Produces(statusCode));
        return this;
    }

    public EndpointBuilder Produces<T>(int statusCode)
    {
        _ep.AddConfigurator(b => b.Produces<T>(statusCode));
        return this;
    }

    public EndpointBuilder RequireAuthorization()
    {
        _ep.AddConfigurator(b => b.RequireAuthorization());
        return this;
    }

    public EndpointBuilder RequireAuthorization(params string[] policies)
    {
        _ep.AddConfigurator(b => b.RequireAuthorization(policies));
        return this;
    }
}