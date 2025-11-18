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

    public override async Task<IResult> HandleAsync(NoResponseRequest request, CancellationToken ct)
    {
        WasExecuted = true;
        return await Send.NoContentAsync();
    }
}

public record NoResponseRequest
{
    public int Id { get; init; }
}