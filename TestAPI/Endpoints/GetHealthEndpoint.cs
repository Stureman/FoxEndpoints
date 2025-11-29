using FoxEndpoints;
using FoxEndpoints.Abstractions;

namespace TestAPI.Endpoints;

/// <summary>
/// Endpoint without request - returns health check status
/// </summary>
public class GetHealthEndpoint : EndpointWithoutRequest<HealthResponse>
{
    public override void Configure()
    {
        Get("/health")
            .WithName("HealthCheck")
            .WithTags("System")
            .Produces<HealthResponse>(200);
    }

    public override async Task<IResult> HandleAsync(CancellationToken ct)
    {
        var response = new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Uptime = TimeSpan.FromMinutes(Random.Shared.Next(1, 1000)),
            Version = "1.0.0"
        };

        return await Send.OkAsync(response);
    }
}

public record HealthResponse
{
    public string Status { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public TimeSpan Uptime { get; init; }
    public string Version { get; init; } = string.Empty;
}