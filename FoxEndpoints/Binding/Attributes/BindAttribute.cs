namespace FoxEndpoints.Binding.Attributes;

/// <summary>
/// Restricts FoxEndpoints binding to the specified property names.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
public sealed class BindAttribute : Attribute
{
    public BindAttribute(params string[] properties)
    {
        Properties = properties ?? Array.Empty<string>();
    }

    public IReadOnlyCollection<string> Properties { get; }
}