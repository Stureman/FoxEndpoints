using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

public abstract class EndpointWithoutRequest<TResponse> : EndpointBase
{
    private IResult? _result;
    
    public abstract Task HandleAsync(CancellationToken ct);

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
                await ep.HandleAsync(ct);

                return ep._result ?? Results.Ok();
            }
            finally
            {
                EndpointContext<object, TResponse>.Current = null;
            }
        };
    }

    /// <summary>
    /// Type-safe Send methods for returning responses from endpoints.
    /// All methods return Task to allow natural early termination via return statements.
    /// </summary>
    protected static class Send
    {
        private static EndpointWithoutRequest<TResponse> CurrentEndpoint
        {
            get => (EndpointContext<object, TResponse>.Current as EndpointWithoutRequest<TResponse>)
                   ?? throw new InvalidOperationException("Send can only be called from within HandleAsync");
        }

        /// <summary>
        /// Returns a 200 OK response with the specified response object.
        /// </summary>
        public static Task OkAsync(TResponse response)
        {
            CurrentEndpoint._result = Results.Ok(response);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 200 OK response with an empty body.
        /// </summary>
        public static Task OkAsync()
        {
            CurrentEndpoint._result = Results.Ok();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 201 Created response with the specified response object.
        /// </summary>
        public static Task CreatedAsync(TResponse response)
        {
            CurrentEndpoint._result = Results.Created(string.Empty, response);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 201 Created response with the specified URI and response object.
        /// </summary>
        public static Task CreatedAsync(string uri, TResponse response)
        {
            CurrentEndpoint._result = Results.Created(uri, response);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 204 No Content response.
        /// </summary>
        public static Task NoContentAsync()
        {
            CurrentEndpoint._result = Results.NoContent();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 404 Not Found response with an empty body.
        /// </summary>
        public static Task NotFoundAsync()
        {
            CurrentEndpoint._result = Results.NotFound();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 404 Not Found response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task NotFoundAsync(string message)
        {
            var problemDetails = new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = message
            };
            CurrentEndpoint._result = Results.NotFound(problemDetails);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 400 Bad Request response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task BadRequestAsync(string message)
        {
            var problemDetails = new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = message
            };
            CurrentEndpoint._result = Results.BadRequest(problemDetails);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 400 Bad Request response with custom ProblemDetails.
        /// </summary>
        public static Task BadRequestAsync(ProblemDetails problemDetails)
        {
            CurrentEndpoint._result = Results.BadRequest(problemDetails);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 401 Unauthorized response.
        /// </summary>
        public static Task UnauthorizedAsync()
        {
            CurrentEndpoint._result = Results.Unauthorized();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 401 Unauthorized response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task UnauthorizedAsync(string message)
        {
            var problemDetails = new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = message
            };
            CurrentEndpoint._result = Results.Problem(problemDetails);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 403 Forbidden response.
        /// </summary>
        public static Task ForbiddenAsync()
        {
            CurrentEndpoint._result = Results.Forbid();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 403 Forbidden response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task ForbiddenAsync(string message)
        {
            var problemDetails = new ProblemDetails
            {
                Status = 403,
                Title = "Forbidden",
                Detail = message
            };
            CurrentEndpoint._result = Results.Problem(problemDetails);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 409 Conflict response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task ConflictAsync(string message)
        {
            var problemDetails = new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = message
            };
            CurrentEndpoint._result = Results.Conflict(problemDetails);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a 200 OK response with a file stream.
        /// </summary>
        public static Task FileAsync(Stream fileStream, string contentType, string? fileName = null)
        {
            CurrentEndpoint._result = Results.File(fileStream, contentType, fileName);
            return Task.CompletedTask;
        }
    }
}