namespace FoxEndpoints.Tests.TestEndpoints;

/// <summary>
/// Test endpoint without response (returns NoContent)
/// </summary>
public class NoResponseEndpoint : EndpointWithoutResponse<NoResponseRequest>
{
    public bool WasExecuted { get; private set; }

    public override void Configure()
    {
        Delete("/no-response/{id}")
            .WithName("NoResponseEndpoint");
    }

    public override Task HandleAsync(NoResponseRequest request, CancellationToken ct)
    {
        WasExecuted = true;
        return Task.CompletedTask;
    }
}

public record NoResponseRequest
{
    public int Id { get; init; }
}