namespace FoxEndpoints.Tests.TestEndpoints;

/// <summary>
/// Test endpoint with authorization requirement
/// </summary>
public class AuthorizedEndpoint : Endpoint<AuthorizedRequest, AuthorizedResponse>
{
    public override void Configure()
    {
        Get("/authorized")
            .WithName("AuthorizedEndpoint")
            .RequireAuthorization()
            .Produces<AuthorizedResponse>(200)
            .Produces(401);
    }

    public override Task<AuthorizedResponse> HandleAsync(AuthorizedRequest request, CancellationToken ct)
    {
        return Task.FromResult(new AuthorizedResponse
        {
            Message = "You are authorized!",
            User = request.Username
        });
    }
}

public record AuthorizedRequest
{
    public string Username { get; init; } = string.Empty;
}

public record AuthorizedResponse
{
    public string Message { get; init; } = string.Empty;
    public string User { get; init; } = string.Empty;
}