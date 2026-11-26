using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>Converts strings to <see cref="Vector"/>.</summary>
public sealed class VectorConverter : TypeConverter
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
		if (value is string text) return Vector.Parse(text);
		throw new NotSupportedException(
			"Cannot convert from " + (value == null ? "(null)" : value.GetType().FullName) + ".");
	}

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
	{
		if (destinationType == typeof(string) && value is Vector v) return v.ToString(culture);
		return base.ConvertTo(context, culture, value, destinationType);
	}
}
