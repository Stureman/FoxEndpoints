using FoxEndpoints;

namespace TestAPI.Endpoints;

/// <summary>
/// Endpoint with both request and response - retrieves user information
/// </summary>
public class GetUserEndpoint : Endpoint<GetUserRequest, GetUserResponse>
{
    public override void Configure()
    {
        Get("/users/{id}")
            .WithName("GetUser")
            .WithTags("Users")
            .Produces<GetUserResponse>(200);
    }

    public override Task<GetUserResponse> HandleAsync(GetUserRequest request, CancellationToken ct)
    {
        // Simulate user retrieval
        var response = new GetUserResponse
        {
            Id = request.Id,
            Name = $"User {request.Id}",
            Email = $"user{request.Id}@example.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-request.Id)
        };

        return Task.FromResult(response);
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
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
}