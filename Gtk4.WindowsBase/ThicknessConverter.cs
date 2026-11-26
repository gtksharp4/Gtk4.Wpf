using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>
/// Converts a <see cref="Thickness"/> to and from text ("1", "1,2" or "l,t,r,b"; "Auto", px/in/cm/pt units)
/// and from numbers. Mirrors <c>System.Windows.ThicknessConverter</c>.
/// </summary>
public class ThicknessConverter : TypeConverter
{
	static bool IsNumeric(Type t) =>
		t == typeof(int) || t == typeof(short) || t == typeof(long) || t == typeof(uint) || t == typeof(ushort)
		|| t == typeof(ulong) || t == typeof(float) || t == typeof(double) || t == typeof(decimal);

	/// <inheritdoc/>
	public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
		sourceType == typeof(string) || IsNumeric(sourceType);

	/// <inheritdoc/>
	public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
		destinationType == typeof(string);

	/// <inheritdoc/>
	public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
	{
		if (value == null) throw GetConvertFromException(value);
		culture ??= CultureInfo.InvariantCulture;
		if (value is string text) return FromString(text, culture);
		if (IsNumeric(value.GetType()))
			return new Thickness(Convert.ToDouble(value, CultureInfo.InvariantCulture));
		throw GetConvertFromException(value);
	}

	/// <inheritdoc/>
	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
		Type destinationType)
	{
		if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
		if (value is Thickness t && destinationType == typeof(string))
		{
			culture ??= CultureInfo.InvariantCulture;
			string sep = ConverterParsing.ListSeparator(culture).ToString();
			return string.Join(sep, ConverterParsing.FormatLength(t.Left, culture),
				ConverterParsing.FormatLength(t.Top, culture), ConverterParsing.FormatLength(t.Right, culture),
				ConverterParsing.FormatLength(t.Bottom, culture));
		}
		throw GetConvertToException(value, destinationType);
	}

	static Thickness FromString(string text, CultureInfo culture)
	{
		string[] p = ConverterParsing.Tokenize(text, culture);
		if (p.Length != 1 && p.Length != 2 && p.Length != 4)
			throw new FormatException(
				$"'{text}' value is not valid. It must contain one, two, or four delimited Lengths.");
		double[] v = new double[p.Length];
		for (int i = 0; i < p.Length; i++) v[i] = ConverterParsing.ParseLength(p[i], culture);
		if (v.Length == 1) return new Thickness(v[0]);
		if (v.Length == 2) return new Thickness(v[0], v[1], v[0], v[1]);
		return new Thickness(v[0], v[1], v[2], v[3]);
	}
}
