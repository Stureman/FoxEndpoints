namespace FoxEndpoints;

/// <summary>
/// Prevents FoxEndpoints from binding to the decorated property.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class BindNeverAttribute : Attribute
{
}