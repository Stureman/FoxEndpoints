using Microsoft.AspNetCore.Http;

namespace FoxEndpoints;

internal static class CachedResults
{
    internal static readonly Task<IResult> NoContent = Task.FromResult(Results.NoContent());
    internal static readonly Task<IResult> OkEmpty = Task.FromResult(Results.Ok());
    internal static readonly Task<IResult> NotFoundEmpty = Task.FromResult(Results.NotFound());
    internal static readonly Task<IResult> Unauthorized = Task.FromResult(Results.Unauthorized());
}