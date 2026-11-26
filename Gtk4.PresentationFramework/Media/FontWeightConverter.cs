using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Windows.Media;

namespace System.Windows;

/// <summary>
/// Converts a <see cref="FontWeight"/> to and from a name (Thin..UltraBlack) or an OpenType number (1..999).
/// </summary>
public class FontWeightConverter : TypeConverter
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
		// A number goes through FromOpenTypeWeight, which rejects 0 and 1000+ as WPF does.
		if (int.TryParse(text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out int n))
			return FontWeight.FromOpenTypeWeight(n);
		return FontConverters.ParseFontWeight(text);
	}

	/// <inheritdoc/>
	public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
		Type destinationType)
	{
		if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
		if (value is FontWeight weight)
		{
			if (destinationType == typeof(string)) return NameOf(weight);
			if (destinationType == typeof(InstanceDescriptor))
				return new InstanceDescriptor(typeof(FontWeight).GetMethod(nameof(FontWeight.FromOpenTypeWeight),
					new[] { typeof(int) }), new object[] { weight.ToOpenTypeWeight() });
		}
		return base.ConvertTo(context, culture, value, destinationType)!;
	}

	static string NameOf(FontWeight weight)
	{
		switch (weight.ToOpenTypeWeight())
		{
			case 100: return "Thin";
			case 200: return "ExtraLight";
			case 300: return "Light";
			case 400: return "Normal";
			case 500: return "Medium";
			case 600: return "SemiBold";
			case 700: return "Bold";
			case 800: return "ExtraBold";
			case 900: return "Black";
			case 950: return "ExtraBlack";
			default: return weight.ToOpenTypeWeight().ToString(CultureInfo.InvariantCulture);
		}
	}
}
