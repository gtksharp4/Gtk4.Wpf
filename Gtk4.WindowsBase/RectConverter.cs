using System;
using System.ComponentModel;
using System.Globalization;

namespace System.Windows;

/// <summary>
/// Converts a <see cref="Rect"/> to and from text ("x,y,width,height" or "Empty").
/// Mirrors <c>System.Windows.RectConverter</c>.
/// </summary>
public class RectConverter : TypeConverter
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
		culture ??= CultureInfo.InvariantCulture;
		string trimmed = text.Trim();
		if (trimmed == "Empty") return Rect.Empty;
		string[] p = ConverterParsing.Tokenize(trimmed, culture);
		if (p.Length < 4)
			throw new InvalidOperationException($"Premature string termination encountered while parsing '{text}'.");
		if (p.Length > 4)
			throw new InvalidOperationException($"Unexpected token after the last value in '{text}'.");
		double x = Num(p[0], culture), y = Num(p[1], culture), w = Num(p[2], culture), h = Num(p[3], culture);
		if (w < 0 || h < 0) throw new ArgumentException("Width and Height must be non-negative.");
		return new Rect(x, y, w, h);
	}

	/// <inheritdoc/>
	public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value,
		Type destinationType)
	{
		if (destinationType == null) throw new ArgumentNullException(nameof(destinationType));
		if (value is Rect r && destinationType == typeof(string))
		{
			if (r.IsEmpty) return "Empty";
			culture ??= CultureInfo.InvariantCulture;
			return string.Join(ConverterParsing.ListSeparator(culture).ToString(), r.X.ToString("R", culture),
				r.Y.ToString("R", culture), r.Width.ToString("R", culture), r.Height.ToString("R", culture));
		}
		throw GetConvertToException(value, destinationType);
	}

	static double Num(string token, CultureInfo culture) => double.Parse(token, NumberStyles.Float, culture);
}
