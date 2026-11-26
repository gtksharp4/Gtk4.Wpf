using System.ComponentModel;
using System.Globalization;
using System.Windows.Media.Media3D;

namespace System.Windows.Media;

/// <summary>Converts strings to <see cref="Int32Collection"/>.</summary>
public sealed class Int32CollectionConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Int32Collection.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}
