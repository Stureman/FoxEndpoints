namespace FoxEndpoints.Tests.TestEndpoints;

/// <summary>
/// Test endpoint with request and response
/// </summary>
public class TestEndpoint : Endpoint<TestRequest, TestResponse>
{
    public override void Configure()
    {
        Post("/test")
            .WithName("TestEndpoint")
            .WithTags("Test")
            .Produces<TestResponse>(200);
    }

    public override async Task<IResult> HandleAsync(TestRequest request, CancellationToken ct)
    {
        return await Send.OkAsync(new TestResponse
        {
            Message = $"Hello, {request.Name}!",
            Count = request.Count * 2
        });
    }
}

public record TestRequest
{
    public string Name { get; init; } = string.Empty;
    public int Count { get; init; }
}

public record TestResponse
{
    public string Message { get; init; } = string.Empty;
    public int Count { get; init; }
}