namespace FoxEndpoints;

/// <summary>
/// A zero-size struct used as a return type to enable natural control flow with return statements.
/// Following FastEndpoints pattern - allows "return await Send.OkAsync()" syntax without allocations.
/// </summary>
public readonly struct Void
{
    /// <summary>
    /// Singleton instance of Void. Since it's a struct with no fields, all instances are identical.
    /// </summary>
    public static readonly Void Instance = default;
}