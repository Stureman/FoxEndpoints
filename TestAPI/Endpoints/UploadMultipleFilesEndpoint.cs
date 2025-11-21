using FoxEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace TestAPI.Endpoints;

/// <summary>
/// POST endpoint - demonstrates explicit file upload configuration using .AllowFileUploads()
/// This endpoint handles multiple file uploads
/// </summary>
public class UploadMultipleFilesEndpoint : EndpointWithoutResponse<UploadMultipleFilesRequest>
{
    private readonly ILogger<UploadMultipleFilesEndpoint> _logger;

    public UploadMultipleFilesEndpoint(ILogger<UploadMultipleFilesEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/files/upload-multiple")
            .AllowFileUploads()  // Explicit configuration using the fluent API
            .AllowAnonymous()
            .WithName("UploadMultipleFiles")
            .WithTags("Files")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    public override async Task<IResult> HandleAsync(UploadMultipleFilesRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Uploading {Count} files", request.Files?.Count ?? 0);

        // Validation
        if (request.Files == null || request.Files.Count == 0)
        {
            return await Send.BadRequestAsync("At least one file is required");
        }

        if (request.Files.Count > 10)
        {
            return await Send.BadRequestAsync("Maximum 10 files allowed");
        }

        // Process files
        foreach (var file in request.Files)
        {
            if (file.Length == 0)
            {
                return await Send.BadRequestAsync($"File {file.FileName} is empty");
            }

            _logger.LogInformation(
                "Received file: FileName={FileName}, ContentType={ContentType}, Size={Size} bytes",
                file.FileName,
                file.ContentType,
                file.Length);
        }

        _logger.LogInformation("Successfully processed {Count} files", request.Files.Count);

        return await Send.NoContentAsync();
    }
}

public record UploadMultipleFilesRequest
{
    [FromForm]
    public List<IFormFile>? Files { get; init; }
    
    [FromForm]
    public string? Category { get; init; }
}
