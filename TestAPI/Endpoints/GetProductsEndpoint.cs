using Asp.Versioning;
using FoxEndpoints;
using FoxEndpoints.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace TestAPI.Endpoints;

/// <summary>
/// Example endpoint demonstrating API versioning support
/// Access with: /api/products?api-version=1.0
/// </summary>
[ApiVersion(ApiVersion.V1)]
[ApiExplorerSettings(GroupName = ApiVersion.V1)]
public class GetProductsV1Endpoint : Endpoint<GetProductsRequest, GetProductsResponse>
{
    public override void Configure()
    {
        Get("/api/products")
            .WithName("GetProductsV1")
            .WithTags("Products")
            .Produces<GetProductsResponse>(200)
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(GetProductsRequest request, CancellationToken ct)
    {
        // V1 returns simple product list
        var products = new List<ProductDto>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.99m },
            new() { Id = 2, Name = "Product 2", Price = 20.99m },
            new() { Id = 3, Name = "Product 3", Price = 30.99m }
        };

        var response = new GetProductsResponse
        {
            Products = products,
            Version = "1.0"
        };

        return await Send.OkAsync(response);
    }
}

/// <summary>
/// Version 2 of the products endpoint with enhanced response
/// Access with: /api/products?api-version=2.0
/// </summary>
[ApiVersion(ApiVersion.V2)]
[ApiExplorerSettings(GroupName = ApiVersion.V2)]
public class GetProductsV2Endpoint : Endpoint<GetProductsRequest, GetProductsV2Response>
{
    public override void Configure()
    {
        Get("/api/products")
            .WithName("GetProductsV2")
            .WithTags("Products")
            .Produces<GetProductsV2Response>(200)
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(GetProductsRequest request, CancellationToken ct)
    {
        // V2 returns enhanced product list with additional metadata
        var products = new List<ProductDtoV2>
        {
            new() { Id = 1, Name = "Product 1", Price = 10.99m, Category = "Electronics", InStock = true },
            new() { Id = 2, Name = "Product 2", Price = 20.99m, Category = "Books", InStock = true },
            new() { Id = 3, Name = "Product 3", Price = 30.99m, Category = "Clothing", InStock = false }
        };

        var response = new GetProductsV2Response
        {
            Products = products,
            Version = "2.0",
            TotalCount = products.Count,
            Timestamp = DateTime.UtcNow
        };

        return await Send.OkAsync(response);
    }
}

public record GetProductsRequest
{
    // Can add filtering parameters here if needed
}

public record GetProductsResponse
{
    public List<ProductDto> Products { get; init; } = new();
    public string Version { get; init; } = string.Empty;
}

public record GetProductsV2Response
{
    public List<ProductDtoV2> Products { get; init; } = new();
    public string Version { get; init; } = string.Empty;
    public int TotalCount { get; init; }
    public DateTime Timestamp { get; init; }
}

public record ProductDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
}

public record ProductDtoV2
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Category { get; init; } = string.Empty;
    public bool InStock { get; init; }
}