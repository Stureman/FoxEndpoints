using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

/// <summary>
/// Provides instance-based methods for sending responses from an endpoint.
/// </summary>
public class EndpointSend<TResponse>
{
    // Cache common responses to avoid repeated allocations
    private static readonly Task<IResult> CachedNoContent = Task.FromResult<IResult>(Results.NoContent());
    private static readonly Task<IResult> CachedOkEmpty = Task.FromResult<IResult>(Results.Ok());
    private static readonly Task<IResult> CachedNotFoundEmpty = Task.FromResult<IResult>(Results.NotFound());
    private static readonly Task<IResult> CachedUnauthorized = Task.FromResult<IResult>(Results.Unauthorized());

    /// <summary>
    /// Returns a 200 OK response with the specified response object.
    /// </summary>
    public Task<IResult> OkAsync(TResponse response)
        => Task.FromResult<IResult>(Results.Ok(response));

    /// <summary>
    /// Returns a 200 OK response with an empty body.
    /// </summary>
    public Task<IResult> OkAsync()
        => CachedOkEmpty;

    /// <summary>
    /// Returns a 201 Created response with the specified response object.
    /// </summary>
    public Task<IResult> CreatedAsync(TResponse response)
        => Task.FromResult<IResult>(Results.Created(string.Empty, response));

    /// <summary>
    /// Returns a 201 Created response with the specified URI and response object.
    /// </summary>
    public Task<IResult> CreatedAsync(string uri, TResponse response)
        => Task.FromResult<IResult>(Results.Created(uri, response));

    /// <summary>
    /// Returns a 204 No Content response.
    /// </summary>
    public Task<IResult> NoContentAsync()
        => CachedNoContent;

    /// <summary>
    /// Returns a 404 Not Found response with an empty body.
    /// </summary>
    public Task<IResult> NotFoundAsync()
        => CachedNotFoundEmpty;

    /// <summary>
    /// Returns a 404 Not Found response with a message wrapped in ProblemDetails.
    /// </summary>
    public Task<IResult> NotFoundAsync(string message)
    {
        var problemDetails = new ProblemDetails
        {
            Status = 404,
            Title = "Not Found",
            Detail = message
        };
        return Task.FromResult<IResult>(Results.NotFound(problemDetails));
    }

    /// <summary>
    /// Returns a 400 Bad Request response with a message wrapped in ProblemDetails.
    /// </summary>
    public Task<IResult> BadRequestAsync(string message)
    {
        var problemDetails = new ProblemDetails
        {
            Status = 400,
            Title = "Bad Request",
            Detail = message
        };
        return Task.FromResult<IResult>(Results.BadRequest(problemDetails));
    }

    /// <summary>
    /// Returns a 400 Bad Request response with custom ProblemDetails.
    /// </summary>
    public Task<IResult> BadRequestAsync(ProblemDetails problemDetails)
        => Task.FromResult<IResult>(Results.BadRequest(problemDetails));

    /// <summary>
    /// Returns a 401 Unauthorized response.
    /// </summary>
    public Task<IResult> UnauthorizedAsync()
        => CachedResults.Unauthorized;

    /// <summary>
    /// Returns a 401 Unauthorized response with a message wrapped in ProblemDetails.
    /// </summary>
    public Task<IResult> UnauthorizedAsync(string message)
    {
        var problemDetails = new ProblemDetails
        {
            Status = 401,
            Title = "Unauthorized",
            Detail = message
        };
        return Task.FromResult<IResult>(Results.Problem(problemDetails));
    }

    /// <summary>
    /// Returns a 403 Forbidden response.
    /// </summary>
    public Task<IResult> ForbiddenAsync()
        => Task.FromResult<IResult>(Results.Forbid());

    /// <summary>
    /// Returns a 403 Forbidden response with a message wrapped in ProblemDetails.
    /// </summary>
    public Task<IResult> ForbiddenAsync(string message)
    {
        var problemDetails = new ProblemDetails
        {
            Status = 403,
            Title = "Forbidden",
            Detail = message
        };
        return Task.FromResult<IResult>(Results.Problem(problemDetails));
    }

    /// <summary>
    /// Returns a 409 Conflict response with a message wrapped in ProblemDetails.
    /// </summary>
    public Task<IResult> ConflictAsync(string message)
    {
        var problemDetails = new ProblemDetails
        {
            Status = 409,
            Title = "Conflict",
            Detail = message
        };
        return Task.FromResult<IResult>(Results.Conflict(problemDetails));
    }

    /// <summary>
    /// Returns a 200 OK response with a file stream.
    /// </summary>
    public Task<IResult> FileAsync(Stream fileStream, string contentType, string? fileName = null)
        => Task.FromResult<IResult>(Results.File(fileStream, contentType, fileName));
}