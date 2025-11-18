using FoxEndpoints;

namespace TestAPI.Endpoints;

/// <summary>
/// Endpoint that requires authorization to access
/// Demonstrates how to protect endpoints with RequireAuthorization
/// </summary>
public class GetSecureDataEndpoint : Endpoint<GetSecureDataRequest, GetSecureDataResponse>
{
    private readonly ILogger<GetSecureDataEndpoint> _logger;

    public GetSecureDataEndpoint(ILogger<GetSecureDataEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/secure/data/{id}")
            .WithName("GetSecureData")
            .WithTags("Secure")
             // Opt-in authorization for this endpoint
            // OR use global auth: app.UseFoxEndpoints().RequireAuthorization()
            .Produces<GetSecureDataResponse>(200)
            .Produces(401); // Unauthorized
    }

    public override async Task<IResult> HandleAsync(GetSecureDataRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Accessing secure data with ID: {Id}", request.Id);

        // In a real application, you would check user claims and permissions here
        // For example: if (!HttpContext.User.IsInRole("Admin")) return Forbid();

        var response = new GetSecureDataResponse
        {
            Id = request.Id,
            SecretData = $"This is secret data for ID {request.Id}",
            AccessedAt = DateTime.UtcNow,
            AccessedBy = HttpContext.User.Identity?.Name ?? "Anonymous"
        };

        return await Send.OkAsync(response);
    }
}

public record GetSecureDataRequest
{
    public int Id { get; init; }
}

public record GetSecureDataResponse
{
    public int Id { get; init; }
    public string SecretData { get; init; } = string.Empty;
    public DateTime AccessedAt { get; init; }
    public string AccessedBy { get; init; } = string.Empty;
}