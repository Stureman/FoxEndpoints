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

    public override async Task<GetUserResponse> HandleAsync(GetUserRequest request, CancellationToken ct)
    {
        // Your endpoint logic here
        return new GetUserResponse
        {
            Id = request.Id,
            Name = "John Doe",
            Email = "john.doe@example.com"
        };
    }
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

    public override async Task<HealthResponse> HandleAsync(CancellationToken ct)
    {
        return new HealthResponse { Status = "Healthy" };
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

    public override async Task HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        // Delete user logic
        // Returns 204 No Content by default
    }
}
```

## License

MIT