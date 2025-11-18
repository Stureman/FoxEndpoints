using FoxEndpoints;

namespace TestAPI.Endpoints;

/// <summary>
/// Endpoint without response - deletes a user (returns NoContent)
/// </summary>
public class DeleteUserEndpoint : EndpointWithoutResponse<DeleteUserRequest>
{
    private readonly ILogger<DeleteUserEndpoint> _logger;

    public DeleteUserEndpoint(ILogger<DeleteUserEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Delete("/users/{id}")
            .WithName("DeleteUser")
            .WithTags("Users")
            .Produces(204);
    }

    public override async Task<IResult> HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        // Simulate user deletion
        _logger.LogInformation("Deleting user with ID: {UserId}", request.Id);
        
        // In a real application, you would delete the user from the database here
        
        return await Send.NoContentAsync();
    }
    
}

public record DeleteUserRequest
{
    public int Id { get; init; }
}