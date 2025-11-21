using FoxEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace TestAPI.Endpoints;

/// <summary>
/// POST endpoint - demonstrates file upload with multipart/form-data
/// Automatically configured to accept multipart/form-data because request contains IFormFile
/// </summary>
public class UploadImageEndpoint : EndpointWithoutResponse<UploadImageRequest>
{
    private readonly ILogger<UploadImageEndpoint> _logger;

    public UploadImageEndpoint(ILogger<UploadImageEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/images/upload")
            .WithName("UploadImage")
            .WithTags("Images")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest);
    }

    public override async Task<IResult> HandleAsync(UploadImageRequest request, CancellationToken ct)
    {
        _logger.LogInformation("Uploading image: {FileName}, Size: {Size} bytes", 
            request.File?.FileName, request.File?.Length);

        // Validation
        if (request.File == null || request.File.Length == 0)
        {
            return await Send.BadRequestAsync("File is required");
        }

        // Check file extension
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
        {
            return await Send.BadRequestAsync($"Invalid file type. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Check file size (max 5MB)
        const long maxFileSize = 5 * 1024 * 1024;
        if (request.File.Length > maxFileSize)
        {
            return await Send.BadRequestAsync("File size exceeds 5MB limit");
        }

        // In a real application, you would save the file to disk or cloud storage
        // For demo purposes, we'll just log the details
        _logger.LogInformation(
            "Successfully received image upload: FileName={FileName}, ContentType={ContentType}, Size={Size} bytes, Description={Description}",
            request.File.FileName,
            request.File.ContentType,
            request.File.Length,
            request.Description ?? "(no description)");

        return await Send.NoContentAsync();
    }
}

public record UploadImageRequest
{
    [FromForm]
    public IFormFile? File { get; init; }
    
    [FromForm]
    public string? Description { get; init; }
}
