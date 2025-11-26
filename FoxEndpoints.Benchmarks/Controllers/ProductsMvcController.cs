using FoxEndpoints.Benchmarks.Models;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints.Benchmarks.Controllers;

[ApiController]
[Route("api/mvc/products")]
public class ProductsMvcController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProducts()
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

        return Ok(response);
    }

    [HttpGet("{id}")]
    public IActionResult GetProductById(int id)
    {
        var product = new ProductDto
        {
            Id = id,
            Name = $"Product {id}",
            Price = 10.99m * id,
            InStock = true,
            Category = "Electronics",
            Description = "A sample product"
        };

        return Ok(product);
    }

    [HttpPost]
    public IActionResult CreateProduct([FromBody] CreateProductRequest request)
    {
        var response = new CreateProductResponse
        {
            Id = Random.Shared.Next(1000, 9999),
            Name = request.Name,
            Price = request.Price,
            Success = true
        };

        return Ok(response);
    }
}