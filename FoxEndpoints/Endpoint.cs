using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoxEndpoints;

public abstract class Endpoint<TRequest, TResponse> : EndpointBase
{
    private IResult? _result;
    
    public abstract Task HandleAsync(TRequest request, CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        // For GET requests, bind from route/query by manually constructing the request object
        // For POST/PUT/PATCH/DELETE with complex types, bind from body (and merge route params if present)
        var requestType = typeof(TRequest);
        var isSimpleType = requestType.IsPrimitive || requestType == typeof(string) || requestType == typeof(Guid) || requestType == typeof(DateTime);
        
        if (httpMethod == HttpMethods.Get && !isSimpleType)
        {
            // For GET with complex types, manually bind from HttpContext (route + query)
            return async (HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (Endpoint<TRequest, TResponse>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;
                EndpointContext<TRequest, TResponse>.Current = ep;

                try
                {
                    var request = EndpointExtensions.BindFromHttpContext<TRequest>(ctx);
                    await ep.HandleAsync(request, ct);

                    // Check if response was already sent via Send methods
                    if (ctx.ResponseStarted())
                        return ep._result!;
                    
                    // Auto-send fallback if no Send method was called
                    return Results.Ok();
                }
                finally
                {
                    EndpointContext<TRequest, TResponse>.Current = null;
                }
            };
        }
        if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
        {
            // Bind from body for POST/PUT/PATCH, then merge route parameters
            return async ([FromBody] TRequest req, HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (Endpoint<TRequest, TResponse>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;
                EndpointContext<TRequest, TResponse>.Current = ep;

                try
                {
                    // Merge route parameters into the request object (e.g., {id} from /users/{id})
                    var mergedRequest = EndpointExtensions.MergeRouteParameters(req, ctx);
                    await ep.HandleAsync(mergedRequest, ct);

                    // Check if response was already sent via Send methods
                    if (ctx.ResponseStarted())
                        return ep._result!;
                    
                    // Auto-send fallback if no Send method was called
                    return Results.Ok();
                }
                finally
                {
                    EndpointContext<TRequest, TResponse>.Current = null;
                }
            };
        }

        // For simple types or other methods, use standard binding
        return async (TRequest req, HttpContext ctx, CancellationToken ct) =>
        {
            var ep = (Endpoint<TRequest, TResponse>)
                EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
            ep.HttpContext = ctx;
            EndpointContext<TRequest, TResponse>.Current = ep;

            try
            {
                await ep.HandleAsync(req, ct);

                // Check if response was already sent via Send methods
                if (ctx.ResponseStarted())
                    return ep._result!;
                
                // Auto-send fallback if no Send method was called
                return Results.Ok();
            }
            finally
            {
                EndpointContext<TRequest, TResponse>.Current = null;
            }
        };
    }

    /// <summary>
    /// Type-safe Send methods for returning responses from endpoints.
    /// All methods return Task to allow natural early termination via return statements.
    /// </summary>
    protected static class Send
    {
        private static Endpoint<TRequest, TResponse> CurrentEndpoint
        {
            get => (EndpointContext<TRequest, TResponse>.Current as Endpoint<TRequest, TResponse>)
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
        public static Task<Void> FileAsync(Stream fileStream, string contentType, string? fileName = null)
        {
            var ep = CurrentEndpoint;
            ep.HttpContext.MarkResponseStart();
            ep._result = Results.File(fileStream, contentType, fileName);
            return Task.FromResult(Void.Instance);
        }
    }
}