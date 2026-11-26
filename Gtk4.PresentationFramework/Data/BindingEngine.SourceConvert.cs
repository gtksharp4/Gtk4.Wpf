using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace System.Windows.Data;

internal static partial class BindingEngine
{
	/// <summary>
	/// Converts a target value to the source property type like WPF's default converter:
	/// empty text to null for Nullable, strings via TypeConverter, IConvertible via Convert.
	/// Non-critical failures are returned in <paramref name="error"/>.
	/// </summary>
	internal static bool TryConvertToSourceType(
		object? value, Type propertyType, out object? result, out Exception? error, CultureInfo? culture = null)
	{
		result = value;
		error = null;
		var underlying = Nullable.GetUnderlyingType(propertyType);
		var t = underlying ?? propertyType;
		if (value == null || t.IsInstanceOfType(value)) return true;
		var cult = ResolveCulture(culture);
		try
		{
			if (value is string s)
			{
				if (underlying != null && string.IsNullOrWhiteSpace(s)) { result = null; return true; }
				result = TypeDescriptor.GetConverter(t).ConvertFrom(null, cult, s);
			}
			else if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(t) && !t.IsEnum)
				result = Convert.ChangeType(value, t, cult);
			else
			{
				var converter = TypeDescriptor.GetConverter(t);
				if (converter.CanConvertFrom(value.GetType()))
					result = converter.ConvertFrom(null, cult, value);
			}
			return true;
		}
		catch (Exception ex) when (!IsCritical(ex))
		{
			error = ex is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : ex;
			return false;
		}
	}
}
