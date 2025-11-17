namespace FoxEndpoints.Tests.TestEndpoints;

/// <summary>
/// Test endpoint without request
/// </summary>
public class NoRequestEndpoint : EndpointWithoutRequest<NoRequestResponse>
{
    public override void Configure()
    {
        Get("/no-request")
            .WithName("NoRequestEndpoint")
            .Produces<NoRequestResponse>(200);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(new NoRequestResponse
        {
            Timestamp = DateTime.UtcNow,
            Status = "OK"
        });
    }
}

public record NoRequestResponse
{
    public DateTime Timestamp { get; init; }
    public string Status { get; init; } = string.Empty;
}