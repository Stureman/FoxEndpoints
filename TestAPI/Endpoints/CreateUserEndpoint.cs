using FoxEndpoints;
using FoxEndpoints.Abstractions;

namespace TestAPI.Endpoints;

/// <summary>
/// POST endpoint - creates a new user from request body
/// </summary>
public class CreateUserEndpoint : Endpoint<CreateUserRequest, CreateUserResponse>
{
    private readonly ILogger<CreateUserEndpoint> _logger;

    public CreateUserEndpoint(ILogger<CreateUserEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/users")
            .WithName("CreateUser")
            .WithTags("Users")
            .Produces<CreateUserResponse>(201);
    }

    public override async Task<IResult> HandleAsync(CreateUserRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Creating user: {Name}, {Email}", request.Name, request.Email);

        // Example validation with early return
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return await Send.BadRequestAsync("Name is required");
        }

        // Simulate user creation
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

        return await Send.CreatedAsync(response);
    }
}

public record CreateUserRequest
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Age { get; init; }
    public string? PhoneNumber { get; init; }
}

public record CreateUserResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public string Message { get; init; } = string.Empty;
}