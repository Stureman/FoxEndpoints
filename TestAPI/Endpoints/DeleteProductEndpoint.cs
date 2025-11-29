using FoxEndpoints;
using FoxEndpoints.Abstractions;

namespace TestAPI.Endpoints;

/// <summary>
/// DELETE endpoint with response - for testing DELETE with Endpoint<TRequest, TResponse>
/// </summary>
public class DeleteProductEndpoint : Endpoint<DeleteProductRequest, DeleteProductResponse>
{
    public override void Configure()
    {
        Delete("/api/products/{id}")
            .WithName("DeleteProduct")
            .WithTags("Products")
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(DeleteProductRequest request, CancellationToken ct)
    {
        var message = string.IsNullOrEmpty(request.Reason)
            ? $"Product {request.Id} deleted successfully"
            : $"Product {request.Id} deleted successfully. Reason: {request.Reason}";

        var response = new DeleteProductResponse
        {
            Id = request.Id,
            Message = message,
            Reason = request.Reason,
            DeletedAt = DateTime.UtcNow
        };

        return await Send.OkAsync(response);
    }
}

public record DeleteProductRequest
{
    public int Id { get; init; }
    public string? Reason { get; init; }
}

public record DeleteProductResponse
{
    public int Id { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public DateTime DeletedAt { get; init; }
}
