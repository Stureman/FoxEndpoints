using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

public abstract class EndpointWithoutRequest<TResponse> : EndpointBase
{
    public abstract Task<IResult> HandleAsync(CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        return async (HttpContext ctx, CancellationToken ct) =>
        {
            var ep = (EndpointWithoutRequest<TResponse>)
                EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
            
            ep.HttpContext = ctx;
            EndpointContext<object, TResponse>.Current = ep;

            try
            {
                return await ep.HandleAsync(ct);
            }
            finally
            {
                EndpointContext<object, TResponse>.Current = null;
            }
        };
    }

    /// <summary>
    /// Type-safe Send methods for returning responses from endpoints.
    /// All methods return Task&lt;IResult&gt; to allow natural early termination via return statements.
    /// </summary>
    protected static class Send
    {
        /// <summary>
        /// Returns a 200 OK response with the specified response object.
        /// </summary>
        public static Task<IResult> OkAsync(TResponse response)
            => Task.FromResult<IResult>(Results.Ok(response));

        /// <summary>
        /// Returns a 200 OK response with an empty body.
        /// </summary>
        public static Task<IResult> OkAsync()
            => Task.FromResult<IResult>(Results.Ok());

        /// <summary>
        /// Returns a 201 Created response with the specified response object.
        /// </summary>
        public static Task<IResult> CreatedAsync(TResponse response)
            => Task.FromResult<IResult>(Results.Created(string.Empty, response));

        /// <summary>
        /// Returns a 201 Created response with the specified URI and response object.
        /// </summary>
        public static Task<IResult> CreatedAsync(string uri, TResponse response)
            => Task.FromResult<IResult>(Results.Created(uri, response));

        /// <summary>
        /// Returns a 204 No Content response.
        /// </summary>
        public static Task<IResult> NoContentAsync()
            => Task.FromResult<IResult>(Results.NoContent());

        /// <summary>
        /// Returns a 404 Not Found response with an empty body.
        /// </summary>
        public static Task<IResult> NotFoundAsync()
            => Task.FromResult<IResult>(Results.NotFound());

        /// <summary>
        /// Returns a 404 Not Found response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task<IResult> NotFoundAsync(string message)
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
        public static Task<IResult> BadRequestAsync(string message)
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
        public static Task<IResult> BadRequestAsync(ProblemDetails problemDetails)
            => Task.FromResult<IResult>(Results.BadRequest(problemDetails));

        /// <summary>
        /// Returns a 401 Unauthorized response.
        /// </summary>
        public static Task<IResult> UnauthorizedAsync()
            => Task.FromResult<IResult>(Results.Unauthorized());

        /// <summary>
        /// Returns a 401 Unauthorized response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task<IResult> UnauthorizedAsync(string message)
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
        public static Task<IResult> ForbiddenAsync()
            => Task.FromResult<IResult>(Results.Forbid());

        /// <summary>
        /// Returns a 403 Forbidden response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task<IResult> ForbiddenAsync(string message)
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
        public static Task<IResult> ConflictAsync(string message)
        {
            var problemDetails = new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = message
            };
            return Task.FromResult<IResult>(Results.Conflict(problemDetails));
        }
    }
}