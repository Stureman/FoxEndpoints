namespace FoxEndpoints.Benchmarks.Models;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool InStock { get; set; }
}

public class ProductsResponse
{
    public List<ProductDto> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public string Version { get; set; } = "1.0";
}

public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
}

public class CreateProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool Success { get; set; }
}