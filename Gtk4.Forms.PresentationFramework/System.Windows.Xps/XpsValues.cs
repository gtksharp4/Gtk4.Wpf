using System.Globalization;
using System.Windows.Media;

namespace System.Windows.Xps.Packaging;

/// <summary>Parsers for the attribute value formats of XPS markup (invariant culture).</summary>
static class XpsValues
{
	internal static double Double(string? text, double fallback = 0) =>
		double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double v) ? v : fallback;

	/// <summary>Parses <c>#RRGGBB</c>, <c>#AARRGGBB</c> and <c>sc#[a,]r,g,b</c>; null when malformed.</summary>
	internal static Color? Color(string? text)
	{
		if (string.IsNullOrWhiteSpace(text))
			return null;
		text = text!.Trim();
		if (text.StartsWith("sc#", StringComparison.OrdinalIgnoreCase))
			return ScColor(text.Substring(3));
		if (!text.StartsWith("#", StringComparison.Ordinal) || (text.Length != 7 && text.Length != 9))
			return null;
		if (!uint.TryParse(text.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint v))
			return null;
		byte a = text.Length == 9 ? (byte)(v >> 24) : (byte)255;
		return System.Windows.Media.Color.FromArgb(a, (byte)(v >> 16), (byte)(v >> 8), (byte)v);
	}

	static Color? ScColor(string body)
	{
		var parts = body.Split(',');
		if (parts.Length != 3 && parts.Length != 4)
			return null;
		var f = new float[4];
		f[0] = 1;
		int offset = parts.Length == 4 ? 0 : 1;
		for (int i = 0; i < parts.Length; i++)
			f[i + offset] = (float)Double(parts[i]);
		return System.Windows.Media.Color.FromScRgb(f[0], f[1], f[2], f[3]);
	}

	/// <summary>Parses <c>m11,m12,m21,m22,offsetX,offsetY</c>.</summary>
	internal static Matrix? Matrix(string? text)
	{
		var parts = (text ?? string.Empty).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 6)
			return null;
		return new Matrix(Double(parts[0]), Double(parts[1]), Double(parts[2]), Double(parts[3]), Double(parts[4]),
			Double(parts[5]));
	}

	internal static Point Point(string? text, Point fallback)
	{
		var parts = (text ?? string.Empty).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		return parts.Length == 2 ? new Point(Double(parts[0]), Double(parts[1])) : fallback;
	}

	internal static Rect? Rect(string? text)
	{
		var parts = (text ?? string.Empty).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		return parts.Length == 4
			? new Rect(Double(parts[0]), Double(parts[1]), Double(parts[2]), Double(parts[3])) : (Rect?)null;
	}

	internal static TEnum Enum<TEnum>(string? text, TEnum fallback) where TEnum : struct =>
		System.Enum.TryParse(text, true, out TEnum v) ? v : fallback;
}
