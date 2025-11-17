using Microsoft.AspNetCore.Http;

namespace FoxEndpoints.Tests.TestEndpoints;

/// <summary>
/// Test endpoint that returns IResult directly
/// </summary>
public class IResultEndpoint : Endpoint<IResultRequest, IResult>
{
    public override void Configure()
    {
        Put("/iresult/{id}")
            .WithName("IResultEndpoint")
            .Produces(200)
            .Produces(404);
    }

    public override Task<IResult> HandleAsync(IResultRequest request, CancellationToken ct)
    {
        if (request.Id <= 0)
        {
            return Task.FromResult<IResult>(Results.NotFound(new { Error = "Invalid ID" }));
        }

        return Task.FromResult<IResult>(Results.Ok(new { Id = request.Id, Updated = true }));
    }
}

public record IResultRequest
{
    public int Id { get; init; }
}