using System;
using System.Globalization;
using System.Reflection;
using System.Windows.Media;

namespace System.Windows.Markup;

/// <summary>
/// Converters for <see cref="Color"/> and <see cref="Brush"/> that live in
/// <c>PresentationFramework</c> (they depend on <see cref="Colors"/>/<see cref="Brushes"/>
/// which are defined here).
/// </summary>
static class MediaConverters
{
	// ── Color ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Parses a named colour ("Red") or hex string ("#RGB" | "#ARGB" | "#RRGGBB" | "#AARRGGBB").
	/// </summary>
	internal static Color ParseColor(string raw)
	{
		string trimmed = raw.Trim();

		if (trimmed.StartsWith("#", StringComparison.Ordinal))
			return ParseHexColor(trimmed);

		if (ScRgbColor.IsScRgb(trimmed))
			return ScRgbColor.Parse(trimmed, raw);

		// Named colour via Colors static properties.
		var prop = typeof(Colors).GetProperty(trimmed,
			BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
		if (prop != null)
			return (Color)prop.GetValue(null)!;

		throw new FormatException($"Cannot parse Color from \"{raw}\".");
	}

	static Color ParseHexColor(string hex)
	{
		// Strip the '#'.
		string h = hex.Substring(1);
		switch (h.Length)
		{
			case 3: // #RGB → #RRGGBB (opaque)
			{
				byte r = HexByte(h[0], h[0]);
				byte g = HexByte(h[1], h[1]);
				byte b = HexByte(h[2], h[2]);
				return Color.FromRgb(r, g, b);
			}
			case 4: // #ARGB
			{
				byte a = HexByte(h[0], h[0]);
				byte r = HexByte(h[1], h[1]);
				byte g = HexByte(h[2], h[2]);
				byte b = HexByte(h[3], h[3]);
				return Color.FromArgb(a, r, g, b);
			}
			case 6: // #RRGGBB (opaque)
			{
				byte r = HexByte(h[0], h[1]);
				byte g = HexByte(h[2], h[3]);
				byte b = HexByte(h[4], h[5]);
				return Color.FromRgb(r, g, b);
			}
			case 8: // #AARRGGBB
			{
				byte a = HexByte(h[0], h[1]);
				byte r = HexByte(h[2], h[3]);
				byte g = HexByte(h[4], h[5]);
				byte b = HexByte(h[6], h[7]);
				return Color.FromArgb(a, r, g, b);
			}
		}
		throw new FormatException($"Cannot parse Color from \"{hex}\": unexpected hex length {h.Length}.");
	}

	static byte HexByte(char hi, char lo) =>
		(byte)((HexNibble(hi) << 4) | HexNibble(lo));

	static int HexNibble(char c)
	{
		if (c >= '0' && c <= '9') return c - '0';
		if (c >= 'a' && c <= 'f') return c - 'a' + 10;
		if (c >= 'A' && c <= 'F') return c - 'A' + 10;
		throw new FormatException($"Invalid hex digit '{c}'.");
	}

	// ── Brush ────────────────────────────────────────────────────────────────

	/// <summary>
	/// Parses a brush: named ("Red") via <see cref="Brushes"/> statics, or hex colour.
	/// Returns a <see cref="SolidColorBrush"/>.
	/// </summary>
	internal static Brush ParseBrush(string raw)
	{
		string trimmed = raw.Trim();

		// Try named brush first (e.g. "Red" → Brushes.Red).
		var prop = typeof(Brushes).GetProperty(trimmed,
			BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
		if (prop != null)
			return (Brush)prop.GetValue(null)!;

		// Fall back to colour parse → SolidColorBrush.
		Color c = ParseColor(trimmed);
		return new SolidColorBrush(c);
	}
}