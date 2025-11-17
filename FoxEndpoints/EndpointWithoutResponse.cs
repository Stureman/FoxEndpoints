using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

public abstract class EndpointWithoutResponse<TRequest> : EndpointBase
{
    private IResult? _result;
    
    public abstract Task HandleAsync(TRequest request, CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        // For DELETE with complex types, manually bind from HttpContext
        // For POST/PUT/PATCH with complex types, use FromBody and merge route params
        var requestType = typeof(TRequest);
        var isSimpleType = requestType.IsPrimitive || requestType == typeof(string) || requestType == typeof(Guid) || requestType == typeof(DateTime);
        
        if (httpMethod == HttpMethods.Delete && !isSimpleType)
        {
            // Manually bind from route/query for DELETE with complex types
            return async (HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;
                EndpointContext<TRequest, object>.Current = ep;

                try
                {
                    var request = EndpointExtensions.BindFromHttpContext<TRequest>(ctx);
                    await ep.HandleAsync(request, ct);

                    return ep._result ?? Results.NoContent();
                }
                finally
                {
                    EndpointContext<TRequest, object>.Current = null;
                }
            };
        }
        else if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
        {
            // Bind from body for POST/PUT/PATCH, then merge route parameters
            return async ([FromBody] TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;
                EndpointContext<TRequest, object>.Current = ep;

                try
                {
                    // Merge route parameters into the request object (e.g., {id} from /users/{id}/status)
                    var mergedRequest = EndpointExtensions.MergeRouteParameters(req, ctx);
                    await ep.HandleAsync(mergedRequest, ct);

                    return ep._result ?? Results.NoContent();
                }
                finally
                {
                    EndpointContext<TRequest, object>.Current = null;
                }
            };
        }
        else
        {
            // For simple types or other methods, use standard binding
            return async (TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (EndpointWithoutResponse<TRequest>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;
                EndpointContext<TRequest, object>.Current = ep;

                try
                {
                    await ep.HandleAsync(req, ct);

                    return ep._result ?? Results.NoContent();
                }
                finally
                {
                    EndpointContext<TRequest, object>.Current = null;
                }
            };
        }
    }

    /// <summary>
    /// Send methods for returning responses from endpoints without a typed response.
    /// All methods return Task to allow natural early termination via return statements.
    /// </summary>
    protected static class Send
    {
        private static EndpointWithoutResponse<TRequest> CurrentEndpoint
        {
            get => (EndpointContext<TRequest, object>.Current as EndpointWithoutResponse<TRequest>)
                   ?? throw new InvalidOperationException("Send can only be called from within HandleAsync");
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
    }
}