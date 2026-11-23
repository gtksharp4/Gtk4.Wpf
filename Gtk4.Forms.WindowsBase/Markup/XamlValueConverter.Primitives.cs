using System;
using System.Globalization;

namespace System.Windows.Markup;

/// <summary>
/// Converters for primitive WPF types that live in <c>WindowsBase</c>:
/// <see cref="Thickness"/>, <see cref="GridLength"/>, <see cref="CornerRadius"/>,
/// <see cref="Point"/>, <see cref="Size"/>, <see cref="Rect"/>, numeric scalars,
/// <c>bool</c>, and arbitrary enums.
/// These are registered into <see cref="XamlValueConverter"/> at startup.
/// </summary>
static class PrimitiveConverters
{
	// ── Thickness ────────────────────────────────────────────────────────────

	/// <summary>
	/// WPF ThicknessConverter: one, two or four lengths separated by comma and/or whitespace; each has an
	/// optional px/in/cm/pt suffix, or is "Auto" (NaN).
	/// </summary>
	internal static Thickness ParseThickness(string raw) =>
		(Thickness)new ThicknessConverter().ConvertFromString(null, CultureInfo.InvariantCulture, raw)!;

	// ── CornerRadius ─────────────────────────────────────────────────────────

	/// <summary>
	/// WPF CornerRadiusConverter: one or four plain numbers (no unit suffixes) separated by comma and/or
	/// whitespace, invariant culture.
	/// </summary>
	internal static CornerRadius ParseCornerRadius(string raw)
	{
		var parts = ConverterParsing.Tokenize(raw, CultureInfo.InvariantCulture);
		if (parts.Length == 1)
			return new CornerRadius(ParseDouble(parts[0]));
		if (parts.Length == 4)
			return new CornerRadius(ParseDouble(parts[0]), ParseDouble(parts[1]),
				ParseDouble(parts[2]), ParseDouble(parts[3]));
		throw new FormatException($"Cannot parse CornerRadius from \"{raw}\".");
	}

	// ── GridLength ───────────────────────────────────────────────────────────

	/// <summary>
	/// Parses "Auto" | "*" | "2*" | "100" | "100px" (also in, cm, pt; invariant, case-insensitive Auto).
	/// </summary>
	internal static GridLength ParseGridLength(string raw)
	{
		string trimmed = raw.Trim();
		if (string.Equals(trimmed, "Auto", StringComparison.OrdinalIgnoreCase))
			return GridLength.Auto;
		if (trimmed == "*")
			return new GridLength(1.0, GridUnitType.Star);
		if (trimmed.EndsWith("*", StringComparison.Ordinal))
		{
			string numPart = trimmed.Substring(0, trimmed.Length - 1);
			double val = ParseDouble(numPart);
			return new GridLength(val, GridUnitType.Star);
		}
		double px = LengthUnits.Parse(trimmed);
		return new GridLength(px, GridUnitType.Pixel);
	}

	// ── Point / Size / Rect ──────────────────────────────────────────────────

	// WPF accepts commas and/or whitespace between the numbers.
	static string[] SplitNumbers(string raw) =>
		raw.Split(new[] { ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

	internal static Point ParsePoint(string raw)
	{
		var parts = SplitNumbers(raw);
		if (parts.Length == 2)
			return new Point(ParseDouble(parts[0].Trim()), ParseDouble(parts[1].Trim()));
		throw new FormatException($"Cannot parse Point from \"{raw}\".");
	}

	internal static Size ParseSize(string raw)
	{
		var parts = SplitNumbers(raw);
		if (parts.Length == 2)
			return new Size(ParseDouble(parts[0].Trim()), ParseDouble(parts[1].Trim()));
		throw new FormatException($"Cannot parse Size from \"{raw}\".");
	}

	internal static Rect ParseRect(string raw)
	{
		var parts = SplitNumbers(raw);
		if (parts.Length == 4)
			return new Rect(
				ParseDouble(parts[0].Trim()),
				ParseDouble(parts[1].Trim()),
				ParseDouble(parts[2].Trim()),
				ParseDouble(parts[3].Trim()));
		throw new FormatException($"Cannot parse Rect from \"{raw}\".");
	}

	// ── Scalars ──────────────────────────────────────────────────────────────

	internal static double ParseDouble(string raw) =>
		double.Parse(raw, CultureInfo.InvariantCulture);

	internal static float ParseFloat(string raw) =>
		float.Parse(raw, CultureInfo.InvariantCulture);

	internal static int ParseInt(string raw) =>
		int.Parse(raw, CultureInfo.InvariantCulture);

	internal static bool ParseBool(string raw) =>
		bool.Parse(raw);  // already case-insensitive
}