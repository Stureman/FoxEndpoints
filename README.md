# FoxEndpoints

A lightweight, minimal API endpoint framework for ASP.NET Core inspired by FastEndpoints. Supports .NET 9 and .NET 10 with natural IResult-based response handling, optional global authorization, and full API versioning support.

## Features

- ✨ Clean endpoint definition with minimal boilerplate
- 🎯 Support for endpoints with and without request/response types
- 💉 Built-in dependency injection support
- 🔒 Optional global authorization
- 📌 Type-safe routing and parameter binding
- 🔢 **Full API versioning support** (query string and headers)
- 🚀 Works with .NET 9.0 and .NET 10.0
- 📦 Zero configuration required

## Installation

```bash
dotnet add package FoxEndpoints
```

Or via NuGet Package Manager:
```
Install-Package FoxEndpoints
```

## Quick Start

### 1. Register FoxEndpoints in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Register FoxEndpoints (automatically discovers all endpoints)
app.UseFoxEndpoints();

app.Run();
```

### 2. Create Your First Endpoint

```csharp
public class GetUserEndpoint : Endpoint<GetUserRequest, GetUserResponse>
{
    public override void Configure()
    {
        Get("/users/{id}")
            .WithName("GetUser")
            .WithTags("Users");
    }

    public override async Task<IResult> HandleAsync(GetUserRequest request, CancellationToken ct)
    {
        var response = new GetUserResponse
        {
            Id = request.Id,
            Name = "John Doe",
            Email = "john.doe@example.com"
        };
        
        return await Send.OkAsync(response);
    }
}

public record GetUserRequest
{
    public int Id { get; init; }
}

public record GetUserResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}
```

That's it! FoxEndpoints will automatically discover and register all endpoints in your application.

## Usage

### Endpoint Configuration

FoxEndpoints supports various endpoint configurations:

#### HTTP Methods

```csharp
public override void Configure()
{
    Get("/users/{id}");      // GET request
    Post("/users");          // POST request
    Put("/users/{id}");      // PUT request
    Patch("/users/{id}");    // PATCH request
    Delete("/users/{id}");   // DELETE request
}
```

#### Endpoint Metadata

```csharp
public override void Configure()
{
    Get("/users/{id}")
        .WithName("GetUser")                    // OpenAPI operation ID
        .WithTags("Users")                      // OpenAPI tags
        .Produces<GetUserResponse>(200)         // Response type
        .AllowAnonymous();                      // Allow anonymous access
}
```

### Basic Endpoint

```csharp
public class GetUserEndpoint : Endpoint<GetUserRequest, GetUserResponse>
{
    public override void Configure()
    {
        Get("/users/{id}");
    }

    public override async Task<IResult> HandleAsync(GetUserRequest request, CancellationToken ct)
    {
        // Your endpoint logic here
        var response = new GetUserResponse
        {
            Id = request.Id,
            Name = "John Doe",
            Email = "john.doe@example.com"
        };
        
        return await Send.OkAsync(response);
    }
}
```

### Early Return with Validation

```csharp
public override async Task<IResult> HandleAsync(CreateUserRequest request, CancellationToken ct)
{
    // Validation with natural early return
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return await Send.BadRequestAsync("Name is required");
    }

    var response = CreateUser(request);
    return await Send.CreatedAsync(response);
}
```

### Endpoint Without Request

```csharp
public class GetHealthEndpoint : EndpointWithoutRequest<HealthResponse>
{
    public override void Configure()
    {
        Get("/health");
    }

    public override async Task<IResult> HandleAsync(CancellationToken ct)
    {
        var response = new HealthResponse { Status = "Healthy" };
        return await Send.OkAsync(response);
    }
}
```

### Endpoint Without Response

```csharp
public class DeleteUserEndpoint : EndpointWithoutResponse<DeleteUserRequest>
{
    public override void Configure()
    {
        Delete("/users/{id}");
    }

    public override async Task<IResult> HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        // Delete user logic
        return await Send.NoContentAsync();
    }
}
```

## API Versioning

FoxEndpoints has built-in support for API versioning using query strings and headers.

### Setup

Add the API versioning packages to your project:

```bash
dotnet add package Asp.Versioning.Http
dotnet add package Asp.Versioning.Mvc.ApiExplorer
```

Configure versioning in `Program.cs`:

```csharp
using Asp.Versioning;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Api-Version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

app.UseFoxEndpoints();
app.Run();
```

### Using Versioned Endpoints

Apply version attributes to your endpoints:

```csharp
[ApiVersion("1.0")]
[ApiExplorerSettings(GroupName = "v1")]
public class GetProductsV1Endpoint : Endpoint<GetProductsRequest, GetProductsV1Response>
{
    public override void Configure()
    {
        Get("/api/products")
            .WithName("GetProductsV1")
            .WithTags("Products");
    }

    public override async Task<IResult> HandleAsync(GetProductsRequest request, CancellationToken ct)
    {
        var response = new GetProductsV1Response
        {
            Products = GetProducts(),
            Version = "1.0"
        };
        return await Send.OkAsync(response);
    }
}

[ApiVersion("2.0")]
[ApiExplorerSettings(GroupName = "v2")]
public class GetProductsV2Endpoint : Endpoint<GetProductsRequest, GetProductsV2Response>
{
    public override void Configure()
    {
        Get("/api/products")  // Same route, different version
            .WithName("GetProductsV2")
            .WithTags("Products");
    }

    public override async Task<IResult> HandleAsync(GetProductsRequest request, CancellationToken ct)
    {
        var response = new GetProductsV2Response
        {
            Products = GetEnhancedProducts(),
            Version = "2.0",
            TotalCount = 10
        };
        return await Send.OkAsync(response);
    }
}
```

### Calling Versioned Endpoints

```http
# Query string versioning
GET /api/products?api-version=1.0
GET /api/products?api-version=2.0

# Header versioning
GET /api/products
X-Api-Version: 1.0

# Default version (when not specified)
GET /api/products  # Uses version 1.0
```

FoxEndpoints automatically:
- Discovers all `[ApiVersion]` attributes
- Creates an `ApiVersionSet` with all discovered versions
- Maps each endpoint to its specific version(s)
- Enables query string and header versioning
- Returns 400 Bad Request for invalid or missing versions (when required)

## Global Authorization

Require authorization for all endpoints by default:

```csharp
app.UseFoxEndpoints().RequireAuthorization();
```

Individual endpoints can opt-out using `.AllowAnonymous()`:

```csharp
public override void Configure()
{
    Get("/health")
        .AllowAnonymous();
}
```

## Advanced Features

### Dependency Injection

FoxEndpoints automatically resolves dependencies through constructor injection:

```csharp
public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    private readonly IUserService _userService;
    private readonly ILogger<CreateUserEndpoint> _logger;

    public CreateUserEndpoint(IUserService userService, ILogger<CreateUserEndpoint> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/users");
    }

    public override async Task<IResult> HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating user: {Name}", request.Name);
        var user = await _userService.CreateAsync(request, ct);
        return await Send.CreatedAsync(user);
    }
}
```

### Request Binding

FoxEndpoints automatically binds data from multiple sources:

```csharp
public record UpdateUserRequest
{
    public int Id { get; init; }              // From route: /users/{id}
    public string Name { get; init; }         // From JSON body
    public string Email { get; init; }        // From JSON body
    public bool? IsActive { get; init; }      // From query string (optional)
}
```

Request data is merged from:
1. Route parameters (`{id}`, `{name}`, etc.)
2. Query string parameters
3. JSON request body

## Why FoxEndpoints?

- **Simple**: Minimal boilerplate, clean syntax
- **Fast**: Optimized endpoint registration with cached factories
- **Flexible**: Support for various endpoint patterns
- **Type-safe**: Strong typing for requests and responses
- **Feature-rich**: Built-in versioning, authorization, DI support
- **Familiar**: Similar to FastEndpoints but lighter weight
- **Modern**: Uses latest .NET features and minimal APIs

## License

MIT