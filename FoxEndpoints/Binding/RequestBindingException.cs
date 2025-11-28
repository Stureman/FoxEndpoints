namespace FoxEndpoints.Binding;

/// <summary>
/// Signals that binding a request failed and translates to a 400 response.
/// </summary>
internal sealed class RequestBindingException : Exception
{
    public RequestBindingException(IReadOnlyDictionary<string, string[]> errors)
        : base("The request payload is invalid.")
    {
        Errors = errors;
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public static RequestBindingException FromErrors(Dictionary<string, List<string>> errors)
    {
        var flattened = errors.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToArray(),
            StringComparer.OrdinalIgnoreCase);

        return new RequestBindingException(flattened);
    }
}