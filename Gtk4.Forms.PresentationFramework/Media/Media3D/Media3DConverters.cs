using System.ComponentModel;
using System.Globalization;

namespace System.Windows.Media.Media3D;

/// <summary>Shared string conversion for the 3D type converters.</summary>
static class Media3DConverters
{
	internal static object From(object? value, Func<string, object> parse)
	{
		if (value is string s) return parse(s);
		throw new NotSupportedException(
			"Cannot convert from " + (value == null ? "(null)" : value.GetType().FullName) + ".");
	}

	internal static object? To(
		object? value, Type destinationType, CultureInfo? culture, Func<object?, object?> fallback)
	{
		if (destinationType == typeof(string) && value is IFormattable f)
			return f.ToString(null, culture);
		return fallback(null);
	}
}

/// <summary>Converts strings to <see cref="Point3D"/>.</summary>
public sealed class Point3DConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Point3D.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}

/// <summary>Converts strings to <see cref="Vector3D"/>.</summary>
public sealed class Vector3DConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Vector3D.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}

/// <summary>Converts strings to <see cref="Size3D"/>.</summary>
public sealed class Size3DConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Size3D.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}

/// <summary>Converts strings to <see cref="Point4D"/>.</summary>
public sealed class Point4DConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Point4D.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}

/// <summary>Converts strings to <see cref="Quaternion"/>.</summary>
public sealed class QuaternionConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Quaternion.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}

/// <summary>Converts strings to <see cref="Matrix3D"/>.</summary>
public sealed class Matrix3DConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Matrix3D.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}

/// <summary>Converts strings to <see cref="Rect3D"/>.</summary>
public sealed class Rect3DConverter : TypeConverter
{
	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) =>
		Media3DConverters.From(value, s => Rect3D.Parse(s));

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		Media3DConverters.To(value, destinationType, culture,
			_ => base.ConvertTo(context, culture, value, destinationType));
}
