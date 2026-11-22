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
		var parts = trimmed.Substring(Prefix.Length)
			.Split(new[] { ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length != 3 && parts.Length != 4)
			throw new FormatException($"Cannot parse Color from \"{raw}\": scRGB needs 3 or 4 values.");

		var v = new float[4];
		v[0] = 1f;
		int offset = parts.Length == 4 ? 0 : 1;
		for (int i = 0; i < parts.Length; i++)
		{
			if (!float.TryParse(parts[i].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var f))
				throw new FormatException($"Cannot parse Color from \"{raw}\": bad scRGB number.");
			v[i + offset] = f;
		}

		return Color.FromArgb(Alpha(v[0]), Channel(v[1]), Channel(v[2]), Channel(v[3]));
	}

	static byte Alpha(float a) => ToByte(a);

	static byte Channel(float v)
	{
		if (!(v > 0f)) return 0;
		if (v <= 0.0031308f) return ToByte(v * 12.92f);
		if (v < 1f) return ToByte(1.055f * (float)Math.Pow(v, 1.0 / 2.4) - 0.055f);
		return 255;
	}

	static byte ToByte(float v)
	{
		float scaled = v * 255f;
		if (scaled <= 0f) return 0;
		if (scaled >= 255f) return 255;
		return (byte)Math.Round(scaled);
	}
}
