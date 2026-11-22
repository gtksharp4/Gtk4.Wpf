using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
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
		destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value == null) throw GetConvertFromException(value);
		if (!(value is string text)) throw new ArgumentException("Cannot convert from this type.", nameof(value));
		return FontConverters.ParseFontStyle(text);
	}

	/// <inheritdoc/>
	public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
		Type destinationType)
	{
		if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
		if (value is FontStyle style)
		{
			if (destinationType == typeof(string)) return style.ToString();
			if (destinationType == typeof(InstanceDescriptor)
				&& typeof(FontStyles).GetProperty(style.ToString()) is { } member)
				return new InstanceDescriptor(member, null);
		}
		return base.ConvertTo(context, culture, value, destinationType)!;
	}
}
