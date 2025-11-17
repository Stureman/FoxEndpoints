# Plan: Implement Send-based response pattern with early termination

This plan implements a new `Send` pattern that provides type-safe response methods (OkAsync, NotFoundAsync, BadRequestAsync, etc.) allowing natural early termination of endpoint handlers through return statements. The pattern ensures that code after a `return await Send.OkAsync(response)` call is not executed, and enforces compile-time type safety for response types.

## Steps

1. **Update `Endpoint.cs`** to keep `HandleAsync` signature as `Task` (not `Task<IResult>`), provide a nested `Send` static class with type-safe response methods that return `Task`, and store the IResult internally in the endpoint instance for retrieval by BuildHandler.

2. **Update `EndpointWithoutRequest.cs`** to keep `HandleAsync` signature as `Task` and provide a nested `Send` static class for type-safe responses with internal IResult storage.

3. **Update `EndpointWithoutResponse.cs`** to keep `HandleAsync` signature as `Task` and provide a nested `Send` static class (without typed Ok methods since there's no TResponse) with internal IResult storage.

4. **Update `BuildHandler` methods** in all endpoint base classes to check the internal IResult after HandleAsync completes and return it, providing a default response if none was set.

5. **Update all test endpoints** in `TestAPI/Endpoints/` to use new `Send` pattern with return statements (change from `return Task.FromResult(response)` to `return Send.OkAsync(response)`).

6. **Update unit tests** in `FoxEndpoints.Tests/` to reflect the new `Task` signature and test the Send pattern behavior including early termination scenarios.

## Further Considerations

1. **Should Send methods support both typed and untyped variants?** Yes - `Send.OkAsync()` for no response, `Send.OkAsync(TResponse value)` for typed response. Keep it simple and type-safe.

2. **ProblemDetails wrapping for error responses?** For `BadRequestAsync`, `NotFoundAsync` with string messages, should auto-wrap in ProblemDetails object following RFC 7807 for consistency with ASP.NET Core conventions.

3. **Default return behavior?** If HandleAsync completes without calling a Send method, BuildHandler should return a sensible default (e.g., 200 OK with empty body or 204 No Content).

## Implementation Details

### Example Usage Pattern

After implementation, endpoints should work like this:

```csharp
public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    private readonly ILogger<CreateUserEndpoint> _logger;

    public CreateUserEndpoint(ILogger<CreateUserEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/users");
    }

    public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating user: {Name}, {Email}", request.Name, request.Email);

        // Validation example with early return
        if (string.IsNullOrWhiteSpace(request.Name))
            return await Send.BadRequestAsync("Name is required");

        var newUserId = Random.Shared.Next(1000, 9999);

        var response = new CreateUserResponse
        {
            Id = newUserId,
            Name = request.Name,
            Email = request.Email,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            Message = $"User '{request.Name}' created successfully!"
        };

        return await Send.OkAsync(response); // Natural return - execution ends here
        
        Console.WriteLine("This should not be reached"); // Compiler warning: unreachable code
    }
}
```

### Supported Send Methods

All Send methods return `Task` for consistent return-based flow:

- `Send.OkAsync()` - Empty response with 200 status code
- `Send.OkAsync(TResponse response)` - Response DTO serialized with 200 status code (type-safe for TResponse)
- `Send.CreatedAsync(TResponse response)` - Response with 201 status code
- `Send.CreatedAsync(string uri, TResponse response)` - Response with 201 status code and Location header
- `Send.NoContentAsync()` - Empty response with 204 status code
- `Send.NotFoundAsync()` - Empty 404 response
- `Send.NotFoundAsync(string message)` - 404 with message wrapped in ProblemDetails
- `Send.BadRequestAsync(string message)` - 400 with message wrapped in ProblemDetails
- `Send.BadRequestAsync(ProblemDetails problem)` - 400 with custom ProblemDetails
- `Send.FileAsync(Stream fileStream, string contentType, string? fileName = null)` - 200 with file stream
- `Send.UnauthorizedAsync()` - 401 response
- `Send.UnauthorizedAsync(string message)` - 401 with message wrapped in ProblemDetails
- `Send.ForbiddenAsync()` - 403 response
- `Send.ForbiddenAsync(string message)` - 403 with message wrapped in ProblemDetails
- `Send.ConflictAsync(string message)` - 409 response with message wrapped in ProblemDetails

### Type Safety

The `Endpoint<TRequest, TResponse>` class provides a nested `Send` static class that enforces compile-time type checking. Send methods store the IResult internally in the endpoint instance:

```csharp
public abstract class Endpoint<TRequest, TResponse> : EndpointBase
{
    private IResult? _result;
    
    public abstract Task HandleAsync(TRequest request, CancellationToken ct);

    protected static class Send
    {
        // Type-safe: only accepts TResponse
        public static Task OkAsync(TResponse response)
        {
            // Access the current endpoint instance via AsyncLocal or similar
            CurrentEndpoint._result = Results.Ok(response);
            return Task.CompletedTask;
        }

        public static Task OkAsync()
        {
            CurrentEndpoint._result = Results.Ok();
            return Task.CompletedTask;
        }

        public static Task CreatedAsync(TResponse response)
        {
            CurrentEndpoint._result = Results.Created(string.Empty, response);
            return Task.CompletedTask;
        }

        public static Task CreatedAsync(string uri, TResponse response)
        {
            CurrentEndpoint._result = Results.Created(uri, response);
            return Task.CompletedTask;
        }

        public static Task NotFoundAsync()
        {
            CurrentEndpoint._result = Results.NotFound();
            return Task.CompletedTask;
        }

        public static Task NotFoundAsync(string message)
        {
            var problemDetails = new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = message
            };
            CurrentEndpoint._result = Results.NotFound(problemDetails);
            return Task.CompletedTask;
        }

        public static Task BadRequestAsync(string message)
        {
            var problemDetails = new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = message
            };
            CurrentEndpoint._result = Results.BadRequest(problemDetails);
            return Task.CompletedTask;
        }

        public static Task NoContentAsync()
        {
            CurrentEndpoint._result = Results.NoContent();
            return Task.CompletedTask;
        }

        public static Task UnauthorizedAsync()
        {
            CurrentEndpoint._result = Results.Unauthorized();
            return Task.CompletedTask;
        }

        public static Task ForbiddenAsync()
        {
            CurrentEndpoint._result = Results.Forbid();
            return Task.CompletedTask;
        }

        // Helper to get the current endpoint instance
        private static Endpoint<TRequest, TResponse> CurrentEndpoint
        {
            get => EndpointContext<TRequest, TResponse>.Current 
                   ?? throw new InvalidOperationException("Send can only be called from within HandleAsync");
        }

        // ... other methods
    }
}

// Thread-safe context holder using AsyncLocal
internal static class EndpointContext<TRequest, TResponse>
{
    private static readonly AsyncLocal<Endpoint<TRequest, TResponse>?> _current = new();
    
    public static Endpoint<TRequest, TResponse>? Current
    {
        get => _current.Value;
        set => _current.Value = value;
    }
}
```

This ensures that `Send.OkAsync(response)` will only compile if `response` is of type `TResponse`, providing compile-time type safety.

### Example Compilation Error

```csharp
public class GetUserEndpoint : Endpoint<GetUserRequest, GetUserResponse>
{
    public override async Task HandleAsync(GetUserRequest request, CancellationToken ct)
    {
        var wrongResponse = new CreateUserResponse(); // Different response type
        
        return await Send.OkAsync(wrongResponse); // ❌ Compilation error!
        // Error: cannot convert from 'CreateUserResponse' to 'GetUserResponse'
    }
}
```

### Control Flow Mechanism

The return-based pattern naturally terminates execution. The BuildHandler sets up the endpoint context, calls HandleAsync, then retrieves the stored IResult:

```csharp
internal static Delegate BuildHandler(Type endpointType, string httpMethod)
{
    return async (TRequest req, HttpContext ctx, CancellationToken ct) =>
    {
        var ep = (Endpoint<TRequest, TResponse>)
            EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
        
        ep.HttpContext = ctx;
        
        // Set the current endpoint context for Send methods to access
        EndpointContext<TRequest, TResponse>.Current = ep;
        
        try
        {
            // HandleAsync returns Task (not Task<IResult>)
            await ep.HandleAsync(req, ct);
            
            // After HandleAsync completes, check if a result was set
            if (ep._result != null)
                return ep._result;
            
            // Default response if no Send method was called
            return Results.NoContent();
        }
        finally
        {
            // Clean up context
            EndpointContext<TRequest, TResponse>.Current = null;
        }
    };
}
```

**Key Benefits:**
- Clean `Task` signature - no need to specify `Task<IResult>`
- No exception overhead - pure return-based flow
- Natural C# control flow - familiar `return` statement
- Unreachable code warnings - compiler warns if code follows a return statement
- Type safety enforced at compile time through generic constraints

### Validation Example

For endpoints that need validation:

```csharp
public override async Task HandleAsync(CreateUserRequest request, CancellationToken ct)
{
    // Early validation returns
    if (string.IsNullOrWhiteSpace(request.Name))
        return await Send.BadRequestAsync("Name is required");

    if (!IsValidEmail(request.Email))
        return await Send.BadRequestAsync("Invalid email format");

    // Proceed with business logic
    var user = await _userService.CreateAsync(request, ct);
    
    return await Send.CreatedAsync($"/users/{user.Id}", user);
}
```
## Testing Strategy

1. **Unit tests for Send methods** - Verify that each Send method returns the correct IResult type with proper status codes
2. **Early termination tests** - Verify that code after Send return statements is not executed (unreachable code)
3. **Type safety tests** - Verify that passing wrong response types causes compilation errors
4. **Integration tests** - Verify that endpoints return correct HTTP status codes and response bodies
5. **ProblemDetails tests** - Verify that error responses are properly wrapped in ProblemDetails format
6. **Validation flow tests** - Test multiple early returns in validation scenarios