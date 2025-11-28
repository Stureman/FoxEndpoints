# FoxEndpoints

FoxEndpoints is a lightweight layer on top of ASP.NET Core minimal APIs inspired by FastEndpoints. It keeps the mental model of "one class = one endpoint" while staying close to the built-in primitives so it remains fast, dependency-light (Microsoft packages only), and predictable. Validation, formatting, and domain rules are intentionally left to the consumer for maximum control.

## Design Goals
- Minimal abstraction: re-use ASP.NET Core hosting, routing, DI, and results without introducing controllers.
- No third-party runtime dependencies: only Microsoft.AspNetCore.App framework reference plus `Asp.Versioning.Http` from the dotnet org for optional versioning support.
- Consumer-managed validation and behavioral policies; the library only handles binding plus basic 400 responses for malformed payloads.
- Faster startup and execution than MVC controllers by emitting delegates directly and caching activators per endpoint type.
- First-class support for typed requests/responses, without forcing FluentValidation/FastEndpoints pipelines.

## Feature Highlights
- Four base classes cover the common use cases: `Endpoint<TRequest, TResponse>`, `EndpointWithoutRequest<TResponse>`, `EndpointWithoutResponse<TRequest>`, and `Endpoint`.
- Automatic discovery of endpoint classes in the entry assembly via `app.UseFoxEndpoints()`.
- Constructor injection works out of the box; endpoints are resolved in scoped DI wrappers so scoped services behave like regular ASP.NET Core handlers.
- Request binding merges route values, query parameters, JSON bodies, and (optionally) multipart form data into strongly typed records/classes.
- File uploads can be buffered or streamed using `IFormFile`, `IFormFileCollection`, or the provided `StreamFile` abstraction.
- Optional global authorization, shared form options, and default file binding modes can be configured once per `WebApplication`.
- API versioning integrates with `Asp.Versioning` attributes and readers when you opt into `AddApiVersioning()` in your host.

## Quick Start

### Program setup
```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();
// Optional: builder.Services.AddApiVersioning(...).AddApiExplorer(...);

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.UseFoxEndpoints(); // discovers endpoints from the entry assembly

app.Run();
```

### Basic endpoint
```csharp
public sealed class GetUserEndpoint : Endpoint<GetUserRequest, GetUserResponse>
{
    public override void Configure()
    {
        Get("/users/{id}")
            .WithName("GetUser")
            .WithTags("Users")
            .RequireAuthorization();
    }

    public override async Task<IResult> HandleAsync(GetUserRequest request, CancellationToken ct)
    {
        var user = await _repository.GetAsync(request.Id, ct);
        return await Send.OkAsync(new GetUserResponse(user.Id, user.Name));
    }

    private readonly IUserRepository _repository;
    public GetUserEndpoint(IUserRepository repository) => _repository = repository;
}

public sealed record GetUserRequest(int Id);
public sealed record GetUserResponse(int Id, string Name);
```
Each endpoint class must select exactly one HTTP verb helper (`Get`, `Post`, `Put`, `Patch`, or `Delete`). Create separate endpoint classes when you need multiple verbs for the same resource.

## Endpoint Variants
- `Endpoint<TRequest, TResponse>`: route + request body/params + response payload.
- `EndpointWithoutRequest<TResponse>`: ex: listings without parameters.
- `EndpointWithoutResponse<TRequest>`: commands that return `204 No Content` or similar.
- `Endpoint`: health checks, or triggers that don't require a request body or response payload.

All base classes expose a typed `Send` helper for creating `IResult` instances without repeatedly calling `Results.*`.

## Request Binding & Validation
- Route values and query parameters are always inspected for matching property names (case-insensitive).
- For `POST`, `PUT`, and `PATCH` requests the JSON body is bound first via `[FromBody]`, then route values are merged into default-valued properties.
- For multipart form data, the binder inspects form fields plus files and honours custom `FormOptions` if provided.
- Use `[BindAttribute("PropA", "PropB")]` on the request type to create an allowlist of bindable properties.
- Use `[BindNever]` on individual properties to exclude them from binding.
- Validation frameworks (FluentValidation, DataAnnotations, custom logic, etc.) are not integrated. Perform validation inside `HandleAsync` and return the appropriate `Send.BadRequestAsync(...)`/`Send.Problem(...)` response yourself.

## File Uploads
When a request type exposes `IFormFile`, `List<IFormFile>`, `IFormFileCollection`, or `StreamFile`, FoxEndpoints automatically switches the binder to form mode.

```csharp
public sealed record UploadDocumentRequest
{
    public Guid Id { get; init; }
    public IFormFile? File { get; init; }
}

public sealed class UploadDocument : EndpointWithoutResponse<UploadDocumentRequest>
{
    public override void Configure()
    {
        Post("/estimates/{EstimateId}/documents")
            .AllowFileUploads()         // Adds Accepts("multipart/form-data") metadata
            .WithFormOptions(new FormOptions { MultipartBodyLengthLimit = 50 * 1024 * 1024 })
            .WithTags("Documents");
    }

    public override async Task<IResult> HandleAsync(UploadDocumentRequest request, CancellationToken ct)
    {
        if (request.File is null)
            return await Send.BadRequestAsync("File is required");

        await _storage.SaveAsync(request.Id, request.File, ct);
        return await Send.NoContentAsync();
    }

    private readonly IDocumentStorage _storage;
    public UploadDocument(IDocumentStorage storage) => _storage = storage;
}
```

Additional options:
- `.AllowFileUploads()` -> Adds `Accepts("multipart/form-data")` metadata only.
- `.DisableAntiforgery()` -> Call explicitly when you serve cookie-authenticated clients that post form data.
- `.WithFormOptions(FormOptions)` -> Override multipart thresholds per endpoint.
- `app.UseFoxEndpoints(c => c.UseFileBindingMode(FileBindingMode.Stream))` -> Switch the default to streaming `StreamFile` payloads.

## API Versioning (Optional)
The package references `Asp.Versioning.Http`, so you only need to opt into the services and add attributes.

```csharp
builder.Services
    .AddApiVersioning(o =>
    {
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.ReportApiVersions = true;
        o.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddApiExplorer(o =>
    {
        o.GroupNameFormat = "'v'VVV";
        o.SubstituteApiVersionInUrl = true;
    });
```

Annotate endpoints with `[ApiVersion("2024-10-01"]` (semantic or date-based) and optionally `[ApiExplorerSettings(GroupName = "v2024-10-01")]`. `UseFoxEndpoints` will:
- Build a version set from all discovered `ApiVersion` attributes.
- Map each endpoint to its declared versions.
- Attach the version set to the underlying route builder.

Consumers are responsible for deciding how clients specify the version (query string, header, etc.) by configuring the `ApiVersionReader`.

## Authorization
FoxEndpoints supports both global and endpoint-level authorization.

### Endpoint-Level Authorization
Use `.RequireAuthorization()` to require authentication, or pass policy/role names for fine-grained control:

```csharp
public override void Configure()
{
    Post("/admin/users")
        .RequireAuthorization("AdminPolicy")     // Single policy
        .WithTags("Admin");
        
    // Or multiple policies
    Put("/sensitive-data/{id}")
        .RequireAuthorization("DataAccess", "SeniorRole")
        .WithTags("Data");
}
```

Policies must be registered in `Program.cs`:
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("DataAccess", policy => policy.RequireClaim("DataAccess", "Read"));
});
```

### Global Authorization
Apply authorization to all endpoints by default, allowing individual endpoints to opt out:

```csharp
app.UseFoxEndpoints(config =>
{
    config.RequireAuthorization();  // All endpoints require auth by default
});
```

Endpoints can opt out using `.AllowAnonymous()`:
```csharp
public override void Configure()
{
    Get("/health")
        .AllowAnonymous();  // Public endpoint
}
```

## Global Configuration Hooks
```csharp
app.UseFoxEndpoints(config =>
{
    config.RequireAuthorization();                      // Force auth unless endpoints call .AllowAnonymous()
    config.ConfigureFormOptions(o => { ... });           // Global multipart settings
    config.UseFileBindingMode(FileBindingMode.Stream);   // Default to streaming uploads
});
```
`UseFoxEndpoints` returns the same `WebApplication` instance, so you can chain additional middleware registrations if desired.

## Known Behaviors & Limitations
- **One HTTP verb per endpoint class.** The last call to `Get/Post/Put/Patch/Delete` wins, so create one class per verb/route.
- **Only entry-assembly endpoints are discovered.** If you place endpoints in a referenced class library, make sure that library is the application entry assembly or load those endpoints into the entry assembly.
- **No automatic validation pipeline.** Consumers should validate inside `HandleAsync` or plug in their own middleware/decorators.
- **No automatic filters or behaviors.** Cross-cutting concerns should be handled via standard ASP.NET Core middleware or shared services.

## Why FoxEndpoints?
- Familiar FastEndpoints ergonomics without adopting a completely new runtime.
- Works inside existing minimal API projects; keeps middleware, filters, and hosting exactly as before.
- Encourages small, focused endpoint classes that are easy to test and reason about.
- Keeps controllers out of the hot path for scenarios where latency matters.

## You're more than welcome to contribute or create an issue!