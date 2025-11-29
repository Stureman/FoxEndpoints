using FoxEndpoints.Abstractions;
using FoxEndpoints.Benchmarks.Models;

namespace FoxEndpoints.Benchmarks.Endpoints;

public class GetProductByIdRequest
{
    public int Id { get; set; }
}

public class GetProductByIdFoxEndpoint : Endpoint<GetProductByIdRequest, ProductDto>
{
    public override void Configure()
    {
        Get("/api/products/{id}")
            .WithName("GetProductByIdFox")
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(GetProductByIdRequest request, CancellationToken ct)
    {
        var product = new ProductDto
        {
            Id = request.Id,
            Name = $"Product {request.Id}",
            Price = 10.99m * request.Id,
            InStock = true,
            Category = "Electronics",
            Description = "A sample product"
        };

        return await Send.OkAsync(product);
    }
}