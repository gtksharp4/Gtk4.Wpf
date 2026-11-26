using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>
/// The comparison of a data trigger: a string <c>Value</c> is converted to the type of the bound value (type
/// converter, enum parse or <see cref="IConvertible"/>, invariant culture) before the equality test, so
/// <c>Value="2"</c> matches a boxed int and <c>Value="True"</c> a bool. Null matches only null.
/// </summary>
internal static class TriggerValueComparer
{
	internal static bool AreEqual(object? current, object? triggerValue)
	{
		if (current == null || triggerValue == null) return current == null && triggerValue == null;
		if (triggerValue is string text && current is not string
		    && TryConvert(text, current.GetType(), out var converted))
			triggerValue = converted;
		return Equals(current, triggerValue);
	}

	static bool TryConvert(string text, Type type, out object? value)
	{
		value = null;
		try
		{
			if (type.IsEnum)
			{
				value = Enum.Parse(type, text, ignoreCase: true);
				return true;
			}
			var converter = TypeDescriptor.GetConverter(type);
			if (converter.CanConvertFrom(typeof(string)))
			{
				value = converter.ConvertFromInvariantString(text);
				return true;
			}
			if (typeof(IConvertible).IsAssignableFrom(type))
			{
				value = Convert.ChangeType(text, type, CultureInfo.InvariantCulture);
				return true;
			}
		}
		catch (Exception ex) when (ex is FormatException || ex is ArgumentException || ex is NotSupportedException
			|| ex is InvalidCastException || ex is OverflowException)
		{
		}
		return false;
	}
}
