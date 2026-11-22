using System;
using System.Globalization;

namespace System.Windows.Media;

/// <summary>
/// XAML string parsing for font values, shared by <see cref="Markup.XamlValueConverter"/> and the public
/// <c>FontWeightConverter</c> and <c>FontStyleConverter</c>, which add the WPF exception types and
/// <c>InstanceDescriptor</c> support. Failures here throw <see cref="FormatException"/>.
/// </summary>
internal static class FontConverters
{
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

	internal static FontFamily ParseFontFamily(string raw)
	{
		string s = raw.Trim();
		if (s.Length == 0) throw new FormatException("FontFamily cannot be empty.");
		return new FontFamily(s);
	}

	internal static FontWeight ParseFontWeight(string raw)
	{
		string s = raw.Trim();
		if (int.TryParse(s, NumberStyles.None, Inv, out int n))
		{
			if (n < 1 || n > 999) throw new FormatException($"FontWeight number {n} is outside 1..999.");
			return FontWeight.FromOpenTypeWeight(n);
		}
		switch (s.ToLowerInvariant())
		{
			case "thin": return FontWeights.Thin;
			case "extralight": return FontWeights.ExtraLight;
			case "ultralight": return FontWeights.UltraLight;
			case "light": return FontWeights.Light;
			case "normal": return FontWeights.Normal;
			case "regular": return FontWeights.Regular;
			case "medium": return FontWeights.Medium;
			case "demibold": return FontWeights.DemiBold;
			case "semibold": return FontWeights.SemiBold;
			case "bold": return FontWeights.Bold;
			case "extrabold": return FontWeights.ExtraBold;
			case "ultrabold": return FontWeights.UltraBold;
			case "black": return FontWeights.Black;
			case "heavy": return FontWeights.Heavy;
			case "extrablack": return FontWeights.ExtraBlack;
			case "ultrablack": return FontWeights.UltraBlack;
			default: throw new FormatException($"Cannot parse FontWeight from \"{raw}\".");
		}
	}

	internal static FontStyle ParseFontStyle(string raw)
	{
		switch (raw.Trim().ToLowerInvariant())
		{
			case "normal": return FontStyles.Normal;
			case "italic": return FontStyles.Italic;
			case "oblique": return FontStyles.Oblique;
			default: throw new FormatException($"Cannot parse FontStyle from \"{raw}\".");
		}
	}

	internal static FontStretch ParseFontStretch(string raw)
	{
		string s = raw.Trim();
		for (int i = 1; i <= 9; i++)
		{
			if (string.Equals(FontStretches.NameOf(i), s, StringComparison.OrdinalIgnoreCase))
				return FontStretch.FromOpenTypeStretch(i);
		}
		if (string.Equals(s, "Medium", StringComparison.OrdinalIgnoreCase)) return FontStretches.Medium;
		throw new FormatException($"Cannot parse FontStretch from \"{raw}\".");
	}

	/// <summary>
	/// Parses a font size with WPF <c>FontSizeConverter</c> units: bare = px, <c>px</c>,
	/// <c>in</c> (x96), <c>cm</c> (x96/2.54), <c>pt</c> (x96/72). Result is in device-independent px.
	/// </summary>
	internal static double ParseFontSize(string raw) => ParseWithUnits(raw, "FontSize", allowNonFinite: false);

	/// <summary>Parses a number with an optional px/in/cm/pt suffix into device-independent px.</summary>
	internal static double ParseWithUnits(string raw, string what, bool allowNonFinite,
		NumberStyles styles = NumberStyles.Float)
	{
		string s = raw.Trim();
		double factor = 1.0;
		string number = s;
		if (EndsWith(s, "px")) number = s.Substring(0, s.Length - 2);
		else if (EndsWith(s, "in")) { factor = 96.0; number = s.Substring(0, s.Length - 2); }
		else if (EndsWith(s, "cm")) { factor = 96.0 / 2.54; number = s.Substring(0, s.Length - 2); }
		else if (EndsWith(s, "pt")) { factor = 96.0 / 72.0; number = s.Substring(0, s.Length - 2); }

		if (!double.TryParse(number.Trim(), styles, Inv, out double value)
			|| (!allowNonFinite && (double.IsNaN(value) || double.IsInfinity(value))))
			throw new FormatException($"Cannot parse {what} from \"{raw}\".");
		return value * factor;
	}

	static bool EndsWith(string s, string unit) => s.EndsWith(unit, StringComparison.OrdinalIgnoreCase);
}
