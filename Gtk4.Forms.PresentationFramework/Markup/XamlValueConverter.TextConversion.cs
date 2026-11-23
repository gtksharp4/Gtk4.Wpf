using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Markup;

public static partial class XamlValueConverter
{
	// ConvertByType returns the text for a type without a registered converter; a CLR property of such a type
	// (TimeSpan, Guid, ...) still takes its TypeConverter, as XAML does.
	static object? ConvertText(Type type, string raw)
	{
		object? value = ConvertByType(type, raw);
		Type target = Nullable.GetUnderlyingType(type) ?? type;
		if (!(value is string) || target == typeof(string) || target == typeof(object)) return value;
		TypeConverter converter = TypeDescriptor.GetConverter(target);
		return converter.CanConvertFrom(typeof(string))
			? converter.ConvertFromString(null, CultureInfo.InvariantCulture, raw) : value;
	}
}
