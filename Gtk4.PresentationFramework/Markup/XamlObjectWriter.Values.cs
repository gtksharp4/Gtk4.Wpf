using System;
using System.Globalization;
using System.Windows.Media;

namespace System.Windows.Markup;

internal sealed partial class XamlObjectWriter
{
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

	// The attribute text for a simple value (invariant culture), or null when it needs element syntax.
	string? Format(object value, Type declared)
	{
		switch (value)
		{
			case string s: return s;
			case bool b: return b ? "True" : "False";
			case double d: return Number(d);
			case float f: return f.ToString("R", Inv);
			case int or long or short or byte: return Convert.ToString(value, Inv);
			case Enum e: return e.ToString();
			case Thickness t: return Join(t.Left, t.Top, t.Right, t.Bottom);
			case CornerRadius c: return Join(c.TopLeft, c.TopRight, c.BottomRight, c.BottomLeft);
			case Point p: return Join(p.X, p.Y);
			case Size s2: return Join(s2.Width, s2.Height);
			case Rect r: return Join(r.X, r.Y, r.Width, r.Height);
			case GridLength g: return Grid(g);
			case Color color: return color.ToString();
			case SolidColorBrush brush when brush.Opacity == 1: return brush.Color.ToString();
			case FontFamily family: return family.Source;
			case Uri uri: return uri.OriginalString;
			case Type type: return XType(type);
		}
		return RoundTrips(value);
	}

	// Other converter-backed values: use ToString only when the converter parses it back to an equal value.
	static string? RoundTrips(object value)
	{
		Type type = value.GetType();
		if (!type.IsValueType) return null;
		string text = Convert.ToString(value, Inv) ?? "";
		try
		{
			return text.Length > 0 && value.Equals(XamlValueConverter.ConvertByType(type, text)) ? text : null;
		}
		catch (Exception ex) when (ex is FormatException || ex is InvalidCastException || ex is ArgumentException)
		{
			return null;
		}
	}

	static string Number(double d) =>
		double.IsNaN(d) ? "NaN" : double.IsPositiveInfinity(d) ? "Infinity"
		: double.IsNegativeInfinity(d) ? "-Infinity" : d.ToString("R", Inv);

	static string Join(params double[] parts) => string.Join(",", Array.ConvertAll(parts, Number));

	static string Grid(GridLength g) => g.GridUnitType switch
	{
		GridUnitType.Auto => "Auto",
		GridUnitType.Star => g.Value == 1 ? "*" : Number(g.Value) + "*",
		_ => Number(g.Value),
	};
}
