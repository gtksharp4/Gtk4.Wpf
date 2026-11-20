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
	/// Parses "5" | "5,10" | "l,t,r,b" (invariant culture).
	/// </summary>
	internal static Thickness ParseThickness(string raw)
	{
		var parts = raw.Split(',');
		if (parts.Length == 1)
		{
			double v = ParseDouble(parts[0].Trim());
			return new Thickness(v);
		}
		if (parts.Length == 2)
		{
			double h = ParseDouble(parts[0].Trim());
			double vv = ParseDouble(parts[1].Trim());
			return new Thickness(h, vv, h, vv);
		}
		if (parts.Length == 4)
		{
			return new Thickness(
				ParseDouble(parts[0].Trim()),
				ParseDouble(parts[1].Trim()),
				ParseDouble(parts[2].Trim()),
				ParseDouble(parts[3].Trim()));
		}
		throw new FormatException($"Cannot parse Thickness from \"{raw}\".");
	}

	// ── CornerRadius ─────────────────────────────────────────────────────────

	/// <summary>
	/// Parses "5" | "tl,tr,br,bl" (invariant culture).
	/// </summary>
	internal static CornerRadius ParseCornerRadius(string raw)
	{
		var parts = raw.Split(',');
		if (parts.Length == 1)
		{
			double v = ParseDouble(parts[0].Trim());
			return new CornerRadius(v);
		}
		if (parts.Length == 4)
		{
			return new CornerRadius(
				ParseDouble(parts[0].Trim()),
				ParseDouble(parts[1].Trim()),
				ParseDouble(parts[2].Trim()),
				ParseDouble(parts[3].Trim()));
		}
		throw new FormatException($"Cannot parse CornerRadius from \"{raw}\".");
	}

	// ── GridLength ───────────────────────────────────────────────────────────

	/// <summary>
	/// Parses "Auto" | "*" | "2*" | "100" (invariant, case-insensitive Auto).
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
		double px = ParseDouble(trimmed);
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