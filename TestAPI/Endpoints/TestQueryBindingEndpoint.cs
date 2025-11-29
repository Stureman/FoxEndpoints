using FoxEndpoints;
using FoxEndpoints.Abstractions;

namespace TestAPI.Endpoints;

/// <summary>
/// Comprehensive test endpoint demonstrating query string binding for GET endpoints
/// Tests multiple data types and optional parameters
/// </summary>
public class TestQueryBindingEndpoint : Endpoint<QueryBindingRequest, QueryBindingResponse>
{
    public override void Configure()
    {
        Get("/api/test-binding/{id}")
            .WithName("TestQueryBinding")
            .WithTags("Testing")
            .AllowAnonymous();
    }

    public override async Task<IResult> HandleAsync(QueryBindingRequest request, CancellationToken ct)
    {
        var response = new QueryBindingResponse
        {
            Id = request.Id,
            Name = request.Name,
            Age = request.Age,
            IsActive = request.IsActive,
            Score = request.Score,
            CreatedDate = request.CreatedDate,
            UniqueId = request.UniqueId,
            Tags = request.Tags,
            Message = "All parameters bound successfully from route and query string!"
        };

        return await Send.OkAsync(response);
    }
}

/// <summary>
/// Request DTO demonstrating various parameter types and sources
/// </summary>
public record QueryBindingRequest
{
    // From route parameter: /api/test-binding/{id}
    public int Id { get; init; }
    
    // From query string: ?Name=John
    public string? Name { get; init; }
    
    // From query string: ?Age=25
    public int? Age { get; init; }
    
    // From query string: ?IsActive=true
    public bool? IsActive { get; init; }
    
    // From query string: ?Score=98.5
    public decimal? Score { get; init; }
    
    // From query string: ?CreatedDate=2025-01-01
    public DateTime? CreatedDate { get; init; }
    
    // From query string: ?UniqueId=12345678-1234-1234-1234-123456789abc
    public Guid? UniqueId { get; init; }
    
    // From query string: ?Tags=tag1,tag2,tag3 (comma-separated string, not array binding)
    public string? Tags { get; init; }
}

public record QueryBindingResponse
{
    public int Id { get; init; }
    public string? Name { get; init; }
    public int? Age { get; init; }
    public bool? IsActive { get; init; }
    public decimal? Score { get; init; }
    public DateTime? CreatedDate { get; init; }
    public Guid? UniqueId { get; init; }
    public string? Tags { get; init; }
    public string Message { get; init; } = string.Empty;
}
