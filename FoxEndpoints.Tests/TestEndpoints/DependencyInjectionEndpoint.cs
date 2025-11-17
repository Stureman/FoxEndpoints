namespace FoxEndpoints.Tests.TestEndpoints;

/// <summary>
/// Test endpoint with dependency injection
/// </summary>
public class DependencyInjectionEndpoint : Endpoint<DependencyRequest, DependencyResponse>
{
    private readonly ITestService _testService;

    public DependencyInjectionEndpoint(ITestService testService)
    {
        _testService = testService;
    }

    public override void Configure()
    {
        Get("/dependency")
            .WithName("DependencyInjectionEndpoint")
            .Produces<DependencyResponse>(200);
    }

    public override Task<DependencyResponse> HandleAsync(DependencyRequest request, CancellationToken ct)
    {
        var result = _testService.Process(request.Value);
        return Task.FromResult(new DependencyResponse { Result = result });
    }
}

public interface ITestService
{
    string Process(string value);
}

public class TestService : ITestService
{
    public string Process(string value)
    {
        return $"Processed: {value}";
    }
}

public record DependencyRequest
{
    public string Value { get; init; } = string.Empty;
}

public record DependencyResponse
{
    public string Result { get; init; } = string.Empty;
}