using FoxEndpoints.Benchmarks.Models;

namespace FoxEndpoints.Benchmarks.Endpoints;

public class GetProductsFoxEndpoint : Endpoint<EmptyRequest, ProductsResponse>
{
    public override void Configure()
    {
        Get("/api/products")
            .WithName("GetProductsFox")
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(EmptyRequest request, CancellationToken ct)
    {
        var products = new List<ProductDto>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.99m, InStock = true, Category = "Electronics" },
            new() { Id = 2, Name = "Product 2", Price = 20.99m, InStock = true, Category = "Books" },
            new() { Id = 3, Name = "Product 3", Price = 30.99m, InStock = false, Category = "Clothing" },
            new() { Id = 4, Name = "Product 4", Price = 40.99m, InStock = true, Category = "Electronics" },
            new() { Id = 5, Name = "Product 5", Price = 50.99m, InStock = true, Category = "Sports" }
        };

        var response = new ProductsResponse
        {
            Products = products,
            TotalCount = products.Count,
            Version = "1.0"
        };

        return await Send.OkAsync(response);
    }
}

public class EmptyRequest { }