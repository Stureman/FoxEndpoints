namespace FoxEndpoints.Internal;

/// <summary>
/// Handles conversion of values to target types, including special types like Guid, DateOnly, TimeOnly.
/// </summary>
internal static class ValueConverter
{
    public static object Convert(object value, Type targetType)
    {
        var stringValue = value.ToString();

        // Handle Guid specially since Convert.ChangeType doesn't support it
        if (targetType == typeof(Guid))
        {
            if (Guid.TryParse(stringValue, out var guid))
                return guid;
            throw new FormatException($"'{stringValue}' is not a valid Guid format");
        }

        // Handle DateOnly (available in .NET 6+)
        if (targetType == typeof(DateOnly))
        {
            if (DateOnly.TryParse(stringValue, out var dateOnly))
                return dateOnly;
            throw new FormatException($"'{stringValue}' is not a valid DateOnly format");
        }

        // Handle TimeOnly (available in .NET 6+)
        if (targetType == typeof(TimeOnly))
        {
            if (TimeOnly.TryParse(stringValue, out var timeOnly))
                return timeOnly;
            throw new FormatException($"'{stringValue}' is not a valid TimeOnly format");
        }

        // For all other types, use Convert.ChangeType
        return System.Convert.ChangeType(stringValue, targetType);
    }
}