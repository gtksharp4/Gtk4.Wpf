using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>Converts between <see cref="PropertyPath"/> and its string form.</summary>
public sealed class PropertyPathConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		value is string text ? new PropertyPath(text, (object[])null!) : base.ConvertFrom(context, culture, value);

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		destinationType == typeof(string) && value is PropertyPath path
			? path.Path
			: base.ConvertTo(context, culture, value, destinationType);
}
