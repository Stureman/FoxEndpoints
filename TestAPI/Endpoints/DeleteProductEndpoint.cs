using FoxEndpoints;

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
        var response = new DeleteProductResponse
        {
            Id = request.Id,
            Message = $"Product {request.Id} deleted successfully",
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
    public DateTime DeletedAt { get; init; }
}
