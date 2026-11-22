using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Media;

namespace System.Windows;

/// <summary>Converts a <see cref="FontStyle"/> to and from its name (Normal, Italic, Oblique).</summary>
public class FontStyleConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (!(value is string text)) throw GetConvertFromException(value);
		return FontConverters.ParseFontStyle(text);
	}

	/// <inheritdoc/>
	public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
		Type destinationType)
	{
		if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
		if (destinationType == typeof(string) && value is FontStyle style) return style.ToString();
		throw GetConvertToException(value, destinationType);
	}
}
