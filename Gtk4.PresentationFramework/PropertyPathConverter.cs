using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

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
	public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value == null) throw new ArgumentNullException("source");
		return value is string text ? new PropertyPath(text, (object[])null!) : base.ConvertFrom(context, culture, value);
	}

	/// <inheritdoc/>
	public override object? ConvertTo(
		ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) =>
		destinationType == typeof(string) && value is PropertyPath path
			? Qualify(path)
			: base.ConvertTo(context, culture, value, destinationType);

	// WPF also prefixes the owner's xmlns from the serializer context; there is no context here.
	static string Qualify(PropertyPath path)
	{
		if (path.PathParameters.Count == 0) return path.Path;
		return Regex.Replace(path.Path, @"\((\d+)\)", m =>
		{
			int n = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
			return n < path.PathParameters.Count && path.PathParameters[n] is DependencyProperty dp
				? "(" + dp.OwnerType.Name + "." + dp.Name + ")"
				: m.Value;
		});
	}
}
