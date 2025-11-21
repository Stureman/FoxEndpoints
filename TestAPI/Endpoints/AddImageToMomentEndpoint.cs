using FoxEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace TestAPI.Endpoints;

/// <summary>
/// POST endpoint - demonstrates file upload with route parameter
/// This matches the use case from the issue description
/// </summary>
public class AddImageToMomentEndpoint : EndpointWithoutResponse<AddImageRequest>
{
    private readonly ILogger<AddImageToMomentEndpoint> _logger;

    public AddImageToMomentEndpoint(ILogger<AddImageToMomentEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/image/moment/{MomentId}")
            .AllowAnonymous()
            .WithName("AddImage")
            .WithTags("Images")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    public override async Task<IResult> HandleAsync(AddImageRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Adding image to moment {MomentId}: {FileName}", 
            request.MomentId, request.File?.FileName);

        // Validation
        if (request.MomentId == Guid.Empty)
        {
            return await Send.BadRequestAsync("Invalid MomentId");
        }

        if (request.File == null || request.File.Length == 0)
        {
            return await Send.BadRequestAsync("File is required");
        }

        // In a real application, you would:
        // 1. Verify the moment exists
        // 2. Save the file to storage
        // 3. Create a database record linking the file to the moment
        
        _logger.LogInformation(
            "Successfully added image to moment {MomentId}: FileName={FileName}, Size={Size} bytes",
            request.MomentId,
            request.File.FileName,
            request.File.Length);

        return await Send.NoContentAsync();
    }
}

public record AddImageRequest
{
    [FromRoute]
    public Guid MomentId { get; init; }

    [FromForm]
    public IFormFile? File { get; init; }
}
