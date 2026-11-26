using System;
using System.Collections.Generic;
using System.Globalization;

namespace System.Windows;

/// <summary>Culture-aware tokenising and length parsing shared by the struct converters.</summary>
static class ConverterParsing
{
	/// <summary>WPF's list separator: ';' when the culture's decimal separator is ',', else ','.</summary>
	internal static char ListSeparator(CultureInfo culture) =>
		culture.NumberFormat.NumberDecimalSeparator == "," ? ';' : ',';

	/// <summary>Splits on the list separator and/or whitespace.</summary>
	internal static string[] Tokenize(string text, CultureInfo culture)
	{
		char sep = ListSeparator(culture);
		var tokens = new List<string>();
		string s = text.Trim();
		int i = 0;
		while (i < s.Length)
		{
			while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
			int start = i;
			while (i < s.Length && s[i] != sep && !char.IsWhiteSpace(s[i])) i++;
			if (i > start) tokens.Add(s.Substring(start, i - start));
			while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
			if (i < s.Length && s[i] == sep) i++;
		}
		return tokens.ToArray();
	}

	/// <summary>Parses "Auto" (NaN) or a number with an optional px/in/cm/pt suffix into pixels.</summary>
	internal static double ParseLength(string token, CultureInfo culture)
	{
		string s = token.Trim();
		if (string.Equals(s, "Auto", StringComparison.OrdinalIgnoreCase)) return double.NaN;
		double factor = 1;
		if (s.Length > 2)
		{
			string unit = s.Substring(s.Length - 2).ToLowerInvariant();
			double f = unit == "px" ? 1 : unit == "in" ? 96 : unit == "cm" ? 96 / 2.54 : unit == "pt" ? 96.0 / 72 : 0;
			if (f != 0) { factor = f; s = s.Substring(0, s.Length - 2); }
		}
		if (!double.TryParse(s, NumberStyles.Float, culture, out double v))
			throw new FormatException($"'{token}' string cannot be converted to Length.");
		return v * factor;
	}

	/// <summary>Formats a length in round-trip form ("Auto" for NaN).</summary>
	internal static string FormatLength(double v, CultureInfo culture) =>
		double.IsNaN(v) ? "Auto" : v.ToString("R", culture);
}
