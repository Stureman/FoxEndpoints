# FoxEndpoints

A lightweight, minimal endpoint library for .NET that provides a clean and simple way to define HTTP endpoints.

## Features

- Clean endpoint definition with minimal boilerplate
- Support for endpoints with and without request/response types
- Built-in dependency injection support
- Type-safe routing and parameter binding
- Works with .NET 10.0+

## Installation

Coming soon to NuGet.

## Usage

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

## License

MIT