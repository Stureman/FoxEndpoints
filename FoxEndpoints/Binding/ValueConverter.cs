using System.Globalization;

namespace FoxEndpoints.Binding;

/// <summary>
/// Handles conversion of values to target types, including special types like Guid, DateOnly, TimeOnly.
/// </summary>
internal static class ValueConverter
{
	public static object? Convert(object? value, Type targetType)
	{
		if (value is null)
			return targetType.IsValueType && Nullable.GetUnderlyingType(targetType) is null
				? Activator.CreateInstance(targetType)
				: null;

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
			if (DateOnly.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateOnly))
				return dateOnly;
			throw new FormatException($"'{stringValue}' is not a valid DateOnly format");
		}

		// Handle TimeOnly (available in .NET 6+)
		if (targetType == typeof(TimeOnly))
		{
			if (TimeOnly.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timeOnly))
				return timeOnly;
			throw new FormatException($"'{stringValue}' is not a valid TimeOnly format");
		}

		if (targetType.IsEnum)
		{
			if (Enum.TryParse(targetType, stringValue, ignoreCase: true, out var enumValue))
				return enumValue!;
			throw new FormatException($"'{stringValue}' is not a valid {targetType.Name} value");
		}

		var underlyingNumeric = Nullable.GetUnderlyingType(targetType) ?? targetType;
		if (underlyingNumeric == typeof(double) || underlyingNumeric == typeof(float) ||
		    underlyingNumeric == typeof(decimal))
			return System.Convert.ChangeType(stringValue, underlyingNumeric, CultureInfo.InvariantCulture);

		if (underlyingNumeric == typeof(DateTime) || underlyingNumeric == typeof(DateTimeOffset))
		{
			if (underlyingNumeric == typeof(DateTime) && DateTime.TryParse(stringValue, CultureInfo.InvariantCulture,
				    DateTimeStyles.RoundtripKind, out var dt))
				return dt;
			if (underlyingNumeric == typeof(DateTimeOffset) && DateTimeOffset.TryParse(stringValue,
				    CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dto))
				return dto;
			throw new FormatException($"'{stringValue}' is not a valid {underlyingNumeric.Name} format");
		}

		return System.Convert.ChangeType(stringValue, targetType, CultureInfo.InvariantCulture);
	}
}