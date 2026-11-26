using System;
using System.Globalization;
using System.Windows.Media;

namespace System.Windows.Markup;

/// <summary>Parses the scRGB colour form <c>sc#[a,]r,g,b</c> (floats, invariant culture).</summary>
static class ScRgbColor
{
	internal const string Prefix = "sc#";

	internal static bool IsScRgb(string trimmed) =>
		trimmed.StartsWith(Prefix, StringComparison.Ordinal);

	internal static Color Parse(string trimmed, string raw)
	{
		var parts = new System.Collections.Generic.List<string>();
		var ws = new[] { ' ', '\t', '\r', '\n' };
		foreach (var segment in trimmed.Substring(Prefix.Length).Split(','))
		{
			var tokens = segment.Split(ws, StringSplitOptions.RemoveEmptyEntries);
			// Like WPF's TokenizerHelper, a comma with nothing around it is an empty token, not a separator.
			if (tokens.Length == 0)
				throw new FormatException($"Cannot parse Color from \"{raw}\": empty scRGB value.");
			parts.AddRange(tokens);
		}
		if (parts.Count != 3 && parts.Count != 4)
			throw new FormatException($"Cannot parse Color from \"{raw}\": scRGB needs 3 or 4 values.");

		var v = new float[4];
		v[0] = 1f;
		int offset = parts.Count == 4 ? 0 : 1;
		for (int i = 0; i < parts.Count; i++)
		{
			if (!float.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
				throw new FormatException($"Cannot parse Color from \"{raw}\": bad scRGB number.");
			v[i + offset] = f;
		}

		return Color.FromScRgb(v[0], v[1], v[2], v[3]);
	}
}
