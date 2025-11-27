using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace FoxEndpoints;

public abstract class EndpointBase
{
    public HttpContext HttpContext { get; private set; } = default!;
    internal string? Route { get; private set; }
    internal string[] Methods { get; private set; } = Array.Empty<string>();
    internal List<Action<RouteHandlerBuilder>> Configurators { get; } = new();

    /// <summary>
    /// Provides access to response-sending methods.
    /// </summary>
    protected EndpointSend<object> Send { get; } = new();
    private FormOptions? _formOptions;
    private FileBindingMode _fileBindingMode = FileBindingMode.Buffered;

    internal void SetContext(HttpContext context)
    {
        HttpContext = context;
    }
    protected void ConfigureFormOptions(FormOptions options) => SetFormOptions(options);

    internal FormOptions? GetFormOptions() => _formOptions;
    internal void SetFormOptions(FormOptions options) => _formOptions = options;
    internal FileBindingMode GetFileBindingMode() => _fileBindingMode;
    internal void SetFileBindingMode(FileBindingMode mode) => _fileBindingMode = mode;

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

    public EndpointBuilder AllowAnonymous()
    {
        _ep.AddConfigurator(b => b.AllowAnonymous());
        return this;
    }

    public EndpointBuilder RequireAuthorization(params string[] policies)
    {
        _ep.AddConfigurator(b => b.RequireAuthorization(policies));
        return this;
    }

    public EndpointBuilder AcceptsFormData()
    {
        _ep.AddConfigurator(b => b.Accepts<object>("multipart/form-data"));
        return this;
    }

    public EndpointBuilder DisableAntiforgery()
    {
        _ep.AddConfigurator(b => b.DisableAntiforgery());
        return this;
    }

    public EndpointBuilder AllowFileUploads()
    {
        _ep.AddConfigurator(b =>
        {
            b.Accepts<object>("multipart/form-data");
        });
        return this;
    }

    public EndpointBuilder WithFormOptions(FormOptions options)
    {
        _ep.SetFormOptions(options);
        return this;
    }

    public EndpointBuilder WithFileBindingMode(FileBindingMode mode)
    {
        _ep.SetFileBindingMode(mode);
        return this;
    }
}