using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary>Converts strings to <see cref="Point3DCollection"/>.</summary>
public sealed class Point3DCollectionConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Point3DCollection.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}

/// <summary>Converts strings to <see cref="Vector3DCollection"/>.</summary>
public sealed class Vector3DCollectionConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Vector3DCollection.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}
