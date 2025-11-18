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

                // Check if response was already sent via Send methods
                if (ctx.ResponseStarted())
                    return ep._result!;
                
                // Auto-send fallback if no Send method was called
                return Results.Ok();
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
        public static Task<Void> OkAsync(TResponse response)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.Ok(response);
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 200 OK response with an empty body.
        /// </summary>
        public static Task<Void> OkAsync()
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.Ok();
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 201 Created response with the specified response object.
        /// </summary>
        public static Task<Void> CreatedAsync(TResponse response)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.Created(string.Empty, response);
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 201 Created response with the specified URI and response object.
        /// </summary>
        public static Task<Void> CreatedAsync(string uri, TResponse response)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.Created(uri, response);
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 204 No Content response.
        /// </summary>
        public static Task<Void> NoContentAsync()
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.NoContent();
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 404 Not Found response with an empty body.
        /// </summary>
        public static Task<Void> NotFoundAsync()
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.NotFound();
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 404 Not Found response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task<Void> NotFoundAsync(string message)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            var problemDetails = new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = message
            };
            ep._result = Results.NotFound(problemDetails);
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 400 Bad Request response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task<Void> BadRequestAsync(string message)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            var problemDetails = new ProblemDetails
            {
                Status = 400,
                Title = "Bad Request",
                Detail = message
            };
            ep._result = Results.BadRequest(problemDetails);
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 400 Bad Request response with custom ProblemDetails.
        /// </summary>
        public static Task<Void> BadRequestAsync(ProblemDetails problemDetails)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.BadRequest(problemDetails);
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 401 Unauthorized response.
        /// </summary>
        public static Task<Void> UnauthorizedAsync()
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.Unauthorized();
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 401 Unauthorized response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task<Void> UnauthorizedAsync(string message)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            var problemDetails = new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized",
                Detail = message
            };
            ep._result = Results.Problem(problemDetails);
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 403 Forbidden response.
        /// </summary>
        public static Task<Void> ForbiddenAsync()
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.Forbid();
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 403 Forbidden response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task<Void> ForbiddenAsync(string message)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            var problemDetails = new ProblemDetails
            {
                Status = 403,
                Title = "Forbidden",
                Detail = message
            };
            ep._result = Results.Problem(problemDetails);
            return Task.FromResult(Void.Instance);
        }

        /// <summary>
        /// Returns a 409 Conflict response with a message wrapped in ProblemDetails.
        /// </summary>
        public static Task<Void> ConflictAsync(string message)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            var problemDetails = new ProblemDetails
            {
                Status = 409,
                Title = "Conflict",
                Detail = message
            };
            ep._result = Results.Conflict(problemDetails);
            return Task.FromResult(Void.Instance);
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