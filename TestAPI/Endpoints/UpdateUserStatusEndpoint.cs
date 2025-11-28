using FoxEndpoints;
using FoxEndpoints.Abstractions;

namespace TestAPI.Endpoints;

/// <summary>
/// PATCH endpoint - partial update of user status
/// Demonstrates EndpointWithoutResponse with POST/PATCH and request body
/// </summary>
public class UpdateUserStatusEndpoint : EndpointWithoutResponse<UpdateUserStatusRequest>
{
    private readonly ILogger<UpdateUserStatusEndpoint> _logger;

    public UpdateUserStatusEndpoint(ILogger<UpdateUserStatusEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/users/{id}/status")
            .WithName("UpdateUserStatus")
            .WithTags("Users")
            .Produces(204);
    }

    public override async Task<IResult> HandleAsync(UpdateUserStatusRequest request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Updating status for user {UserId}: IsActive={IsActive}, Reason={Reason}", 
            request.Id, 
            request.IsActive, 
            request.Reason);

        // In a real application, you would update the user status in the database here
        
        return await Send.NoContentAsync();
    }
}

public record UpdateUserStatusRequest
{
    // Bound from route parameter {id}
    public int Id { get; init; }
    
    // Bound from JSON request body
    public bool IsActive { get; init; }
    public string? Reason { get; init; }
    public DateTime? EffectiveDate { get; init; }
}