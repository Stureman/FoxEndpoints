using FoxEndpoints.Abstractions;
using FoxEndpoints.Benchmarks.Models;

namespace FoxEndpoints.Benchmarks.Endpoints;

public class CreateProductFoxEndpoint : Endpoint<CreateProductRequest, CreateProductResponse>
{
    public override void Configure()
    {
        Post("/api/products")
            .WithName("CreateProductFox")
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(CreateProductRequest request, CancellationToken ct)
    {
        // Simulate some work
        var response = new CreateProductResponse
        {
            Id = Random.Shared.Next(1000, 9999),
            Name = request.Name,
            Price = request.Price,
            Success = true
        };

        return await Send.OkAsync(response);
    }
}