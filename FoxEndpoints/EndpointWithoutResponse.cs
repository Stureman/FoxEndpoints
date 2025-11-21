using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace FoxEndpoints;

public abstract class EndpointWithoutResponse<TRequest> : EndpointBase
{
    public abstract Task<IResult> HandleAsync(TRequest request, CancellationToken ct);

    internal static Delegate BuildHandler(Type endpointType, string httpMethod)
    {
        // For DELETE with complex types, manually bind from HttpContext
        // For POST/PUT/PATCH with complex types, use FromBody and merge route params
        var requestType = typeof(TRequest);
        var isSimpleType = requestType.IsPrimitive || requestType == typeof(string) || requestType == typeof(Guid) || requestType == typeof(DateTime);
        var requiresFormData = RequiresFormDataBinding(requestType);
        
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
                    return await ep.HandleAsync(request, ct);
                }
                finally
                {
                    EndpointContext<TRequest, object>.Current = null;
                }
            };
        }
        else if (httpMethod == HttpMethods.Post || httpMethod == HttpMethods.Put || httpMethod == HttpMethods.Patch)
        {
            // For form data (file uploads), manually bind from HttpContext to avoid JSON inference
            if (requiresFormData)
            {
                return async (HttpContext ctx, CancellationToken ct) =>
                {
                    var ep = (EndpointWithoutResponse<TRequest>)
                        EndpointExtensions.CreateEndpointInstance(endpointType, ctx.RequestServices);
                    
                    ep.HttpContext = ctx;
                    EndpointContext<TRequest, object>.Current = ep;

                    try
                    {
                        // Manually bind from form data and route parameters
                        var request = await EndpointExtensions.BindFromFormAsync<TRequest>(ctx);
                        return await ep.HandleAsync(request, ct);
                    }
                    finally
                    {
                        EndpointContext<TRequest, object>.Current = null;
                    }
                };
            }
            
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
                    return await ep.HandleAsync(mergedRequest, ct);
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
                    return await ep.HandleAsync(req, ct);
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
    /// All methods return Task&lt;IResult&gt; to allow natural early termination via return statements.
    /// </summary>
    protected static class Send
    {
        /// <summary>
        /// Returns a 200 OK response with an empty body.
        /// </summary>
        public static Task<IResult> OkAsync()
            => Task.FromResult<IResult>(Results.Ok());

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
                property.PropertyType == typeof(IFormFileCollection) ||
                (property.PropertyType.IsGenericType && 
                 property.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>) &&
                 property.PropertyType.GetGenericArguments()[0] == typeof(IFormFile)))
            {
                return true;
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