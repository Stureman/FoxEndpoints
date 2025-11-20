using FoxEndpoints;

namespace TestAPI.Endpoints;

/// <summary>
/// Test endpoint to reproduce the Guid binding issue
/// Tests that nullable Guid properties are correctly bound from route parameters
/// </summary>
public class GetLapsEndpoint : Endpoint<GetLapsRequest, GetLapsResponse>
{
    public override void Configure()
    {
        Get("/laps/moment/{MomentId?}/sub/{SubMomentId?}")
            .WithName("GetLaps")
            .WithTags("Testing")
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(GetLapsRequest request, CancellationToken ct)
    {
        var response = new GetLapsResponse
        {
            MomentId = request.MomentId,
            SubMomentId = request.SubMomentId,
            Message = $"MomentId: {request.MomentId?.ToString() ?? "null"}, SubMomentId: {request.SubMomentId?.ToString() ?? "null"}"
        };

        return await Send.OkAsync(response);
    }
}

public record GetLapsRequest
{
    public Guid? MomentId { get; init; }
    
    public Guid? SubMomentId { get; init; }
}

public record GetLapsResponse
{
    public Guid? MomentId { get; init; }
    public Guid? SubMomentId { get; init; }
    public string Message { get; init; } = string.Empty;
}
