using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace FoxEndpoints;

public abstract class Endpoint<TRequest, TResponse> : EndpointBase
{
    public abstract Task<IResult> HandleAsync(TRequest request, CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        // For GET/DELETE requests, bind from route/query by manually constructing the request object
        // For POST/PUT/PATCH with complex types, bind from body (and merge route params if present)
        var requestType = typeof(TRequest);
        var isSimpleType = requestType.IsPrimitive || requestType == typeof(string) || requestType == typeof(Guid) || requestType == typeof(DateTime);
        var requiresFormData = RequiresFormDataBinding(requestType);
        
        if ((httpMethod == HttpMethods.Get || httpMethod == HttpMethods.Delete) && !isSimpleType)
        {
            // For GET/DELETE with complex types, manually bind from HttpContext (route + query)
            return async (HttpContext ctx, CancellationToken ct) =>
            {
                var ep = (Endpoint<TRequest, TResponse>)
                    EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                
                ep.HttpContext = ctx;
                EndpointContext<TRequest, TResponse>.Current = ep;

                try
                {
                    var request = EndpointExtensions.BindFromHttpContext<TRequest>(ctx);
                    return await ep.HandleAsync(request, ct);
                }
                finally
                {
                    EndpointContext<TRequest, TResponse>.Current = null;
                }
            };
        }
        if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
        {
            // For form data (file uploads), manually bind from HttpContext to avoid JSON inference
            if (requiresFormData)
            {
                return async (HttpContext ctx, CancellationToken ct) =>
                {
                    var ep = (Endpoint<TRequest, TResponse>)
                        EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                    
                    ep.HttpContext = ctx;
                    EndpointContext<TRequest, TResponse>.Current = ep;

                    try
                    {
                        // Manually bind from form data and route parameters
                        var request = await EndpointExtensions.BindFromFormAsync<TRequest>(ctx);
                        return await ep.HandleAsync(request, ct);
                    }
                    finally
                    {
                        EndpointContext<TRequest, TResponse>.Current = null;
                    }
                };
            }
            
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
                    return await ep.HandleAsync(mergedRequest, ct);
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
                return await ep.HandleAsync(req, ct);
            }
            finally
            {
                EndpointContext<TRequest, TResponse>.Current = null;
            }
        };
    }

    /// <summary>
    /// Type-safe Send methods for returning responses from endpoints.
    /// All methods return Task&lt;IResult&gt; to allow natural early termination via return statements.
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

        /// <summary>
        /// Returns a 200 OK response with a file stream.
        /// </summary>
        public static Task<IResult> FileAsync(Stream fileStream, string contentType, string? fileName = null)
            => Task.FromResult<IResult>(Results.File(fileStream, contentType, fileName));
    }

    /// <summary>
    /// Determines if a request type requires form data binding (multipart/form-data).
    /// Returns true if the type contains IFormFile properties or properties with [FromForm] attribute.
    /// </summary>
    private static bool RequiresFormDataBinding(Type requestType)
    {
        var properties = requestType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var property in properties)
        {
            // Check if property is IFormFile or IFormFileCollection
            if (property.PropertyType == typeof(IFormFile) || 
                property.PropertyType == typeof(IFormFileCollection))
            {
                return true;
            }
            
            // Check if property is a collection of IFormFile (List<IFormFile>, IEnumerable<IFormFile>, etc.)
            if (property.PropertyType.IsGenericType)
            {
                var genericTypeDef = property.PropertyType.GetGenericTypeDefinition();
                var genericArgs = property.PropertyType.GetGenericArguments();
                
                if (genericArgs.Length > 0 && genericArgs[0] == typeof(IFormFile))
                {
                    // Check for common collection types
                    if (genericTypeDef == typeof(List<>) ||
                        genericTypeDef == typeof(IEnumerable<>) ||
                        genericTypeDef == typeof(IList<>) ||
                        genericTypeDef == typeof(ICollection<>))
                    {
                        return true;
                    }
                }
            }
            
            // Check if property has [FromForm] attribute
            if (property.GetCustomAttribute<FromFormAttribute>() != null)
            {
                return true;
            }
        }
        
        return false;
    }
}