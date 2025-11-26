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
        Get("/track/{trackId?}/car/{carId?}/laps")
            .WithName("GetLaps")
            .WithTags("Testing")
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(GetLapsRequest request, CancellationToken ct)
    {
        var response = new GetLapsResponse
        {
            TrackId = request.TrackId,
            CarId = request.CarId,
            Message = $"TrackId: {request.TrackId?.ToString() ?? "null"}, CarId: {request.CarId?.ToString() ?? "null"} has driven many laps"
        };

        return await Send.OkAsync(response);
    }
}

public record GetLapsRequest(Guid? TrackId, Guid? CarId);

public record GetLapsResponse
{
    public Guid? TrackId { get; init; }
    public Guid? CarId { get; init; }
    public string Message { get; init; } = string.Empty;
}