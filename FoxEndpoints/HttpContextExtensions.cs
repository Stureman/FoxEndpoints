using Microsoft.AspNetCore.Http;

namespace FoxEndpoints;

/// <summary>
/// Extension methods for HttpContext to track response state.
/// Uses HttpContext.Items for AWS Lambda compatibility (Response.HasStarted doesn't work there).
/// </summary>
internal static class HttpContextExtensions
{
    private const string ResponseStartedKey = "__FoxResponseStarted";

    /// <summary>
    /// Marks that a response has been started/sent.
    /// Call this before sending any response to prevent duplicate sends.
    /// </summary>
    public static void MarkResponseStart(this HttpContext ctx)
        => ctx.Items[ResponseStartedKey] = null;

    /// <summary>
    /// Checks if a response has already been started/sent.
    /// Returns true if MarkResponseStart was called OR if the actual HTTP response has started.
    /// </summary>
    public static bool ResponseStarted(this HttpContext ctx)
        => ctx.Response.HasStarted || ctx.Items.ContainsKey(ResponseStartedKey);
}