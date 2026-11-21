using System;

namespace System.Windows.Markup;

static class LengthUnits
{
	/// <summary>
	/// Parses a number with an optional case-insensitive <c>px</c>/<c>in</c>/<c>cm</c>/<c>pt</c> suffix into
	/// device-independent pixels (96 per inch). Whitespace before the suffix is tolerated.
	/// </summary>
	internal static double Parse(string raw)
	{
		string s = raw.Trim();
		double factor = 1.0;
		int cut = 0;
		if (s.Length > 2)
		{
			string unit = s.Substring(s.Length - 2);
			if (string.Equals(unit, "px", StringComparison.OrdinalIgnoreCase)) cut = 2;
			else if (string.Equals(unit, "in", StringComparison.OrdinalIgnoreCase)) { cut = 2; factor = 96.0; }
			else if (string.Equals(unit, "cm", StringComparison.OrdinalIgnoreCase)) { cut = 2; factor = 96.0 / 2.54; }
			else if (string.Equals(unit, "pt", StringComparison.OrdinalIgnoreCase)) { cut = 2; factor = 96.0 / 72.0; }
		}
		return PrimitiveConverters.ParseDouble(cut == 0 ? s : s.Substring(0, s.Length - cut).TrimEnd()) * factor;
	}
}
