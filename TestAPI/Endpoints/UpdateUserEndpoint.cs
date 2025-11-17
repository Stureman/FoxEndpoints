using FoxEndpoints;

namespace TestAPI.Endpoints;

/// <summary>
/// PUT endpoint - updates a user combining route parameter (userId) with request body (update data)
/// This demonstrates binding from BOTH route and body
/// </summary>
public class UpdateUserEndpoint : Endpoint<UpdateUserRequest, UpdateUserResponse>
{
    private readonly ILogger<UpdateUserEndpoint> _logger;

    public UpdateUserEndpoint(ILogger<UpdateUserEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/users/{id}")
            .WithName("UpdateUser")
            .WithTags("Users")
            .Produces<UpdateUserResponse>(200);
    }

    public override Task<UpdateUserResponse> HandleAsync(UpdateUserRequest request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Updating user {UserId}: Name={Name}, Email={Email}, IsActive={IsActive}", 
            request.Id, 
            request.Name, 
            request.Email, 
            request.IsActive);

        // Simulate user update
        var response = new UpdateUserResponse
        {
            Id = request.Id,
            Name = request.Name ?? $"User {request.Id}",
            Email = request.Email ?? $"user{request.Id}@example.com",
            IsActive = request.IsActive ?? true,
            UpdatedAt = DateTime.UtcNow,
            Message = $"User {request.Id} updated successfully!"
        };

        return Task.FromResult(response);
    }
}

public record UpdateUserRequest
{
    // This will be bound from the route parameter {id}
    public int Id { get; init; }
    
    // These will be bound from the JSON request body
    public string? Name { get; init; }
    public string? Email { get; init; }
    public bool? IsActive { get; init; }
    public int? Age { get; init; }
}

public record UpdateUserResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string Message { get; init; } = string.Empty;
}