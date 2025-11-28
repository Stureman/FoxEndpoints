using Microsoft.AspNetCore.Http;

namespace FoxEndpoints.Models;

/// <summary>
/// Represents a streamed multipart file section. The <see cref="Body"/> stream must be disposed by the consumer.
/// </summary>
public sealed record StreamFile(string Name, string? FileName, string? ContentType, Stream Body)
{
    internal static StreamFile FromFormFile(IFormFile file)
        => new(file.Name, file.FileName, file.ContentType, file.OpenReadStream());
}