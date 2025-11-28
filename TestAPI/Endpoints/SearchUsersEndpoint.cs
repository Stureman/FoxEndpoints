using FoxEndpoints;
using FoxEndpoints.Abstractions;

namespace TestAPI.Endpoints;

/// <summary>
/// GET endpoint with multiple query parameters
/// Demonstrates binding from query string
/// </summary>
public class SearchUsersEndpoint : Endpoint<SearchUsersRequest, SearchUsersResponse>
{
    private readonly ILogger<SearchUsersEndpoint> _logger;

    public SearchUsersEndpoint(ILogger<SearchUsersEndpoint> logger)
    {
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/users/search")
            .WithName("SearchUsers")
            .WithTags("Users")
            .Produces<SearchUsersResponse>(200);
    }

    public override async Task<IResult> HandleAsync(SearchUsersRequest request, CancellationToken ct)
    {
        _logger.LogInformation(
            "Searching users: Name={Name}, MinAge={MinAge}, MaxAge={MaxAge}, IsActive={IsActive}, Page={Page}, PageSize={PageSize}", 
            request.Name, 
            request.MinAge, 
            request.MaxAge, 
            request.IsActive, 
            request.Page, 
            request.PageSize);

        // Simulate user search
        var users = new List<UserSummary>();
        
        // Generate some mock results based on the search criteria
        var resultCount = request.PageSize ?? 10;
        for (int i = 1; i <= resultCount; i++)
        {
            users.Add(new UserSummary
            {
                Id = i + ((request.Page ?? 1) - 1) * resultCount,
                Name = !string.IsNullOrEmpty(request.Name) ? $"{request.Name} {i}" : $"User {i}",
                Email = $"user{i}@example.com",
                Age = request.MinAge ?? 25,
                IsActive = request.IsActive ?? true
            });
        }

        var response = new SearchUsersResponse
        {
            Users = users,
            TotalCount = 100, // Mock total count
            Page = request.Page ?? 1,
            PageSize = request.PageSize ?? 10,
            SearchCriteria = $"Name={request.Name}, Age={request.MinAge}-{request.MaxAge}, Active={request.IsActive}"
        };

        return await Send.OkAsync(response);
    }
}

public record SearchUsersRequest
{
    public string? Name { get; init; }
    public int? MinAge { get; init; }
    public int? MaxAge { get; init; }
    public bool? IsActive { get; init; }
    public int? Page { get; init; }
    public int? PageSize { get; init; }
}

public record SearchUsersResponse
{
    public List<UserSummary> Users { get; init; } = new();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public string SearchCriteria { get; init; } = string.Empty;
}

public record UserSummary
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int Age { get; init; }
    public bool IsActive { get; init; }
}