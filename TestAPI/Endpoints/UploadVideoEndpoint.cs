using FoxEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace TestAPI.Endpoints;

public sealed class UploadVideoEndpoint : EndpointWithoutResponse<UploadVideoRequest>
{
    private readonly ILogger<UploadVideoEndpoint> _logger;

    public UploadVideoEndpoint(ILogger<UploadVideoEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/videos/{id:guid}")
            .AllowAnonymous()
            .AllowFileUploads()
            .WithFileBindingMode(FileBindingMode.Stream)
            .WithName("StreamVideoUpload");
    }

    public override async Task<IResult> HandleAsync(UploadVideoRequest request, CancellationToken ct)
    {
        if (request.Video is null)
            return await Send.BadRequestAsync("Video file is required.");

        await using var fileStream = File.Create(Path.Combine(Path.GetTempPath(), request.Video.FileName ?? "upload.bin"));
        await request.Video.Body.CopyToAsync(fileStream, ct);
        return await Send.OkAsync();
    }
}

public sealed record UploadVideoRequest
{
    [FromRoute]
    public Guid Id { get; init; }

    [FromForm]
    public StreamFile? Video { get; init; }
}