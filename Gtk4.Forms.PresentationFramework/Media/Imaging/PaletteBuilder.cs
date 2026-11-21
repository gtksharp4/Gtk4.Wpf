using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media.Imaging;

/// <summary>Builds an optimised palette from a bitmap's pixels (popularity of 3-3-2 colour buckets).</summary>
static class PaletteBuilder
{
	internal static Color[] Build(BitmapSource source, int maxColors)
	{
		byte[] px = source.ToBgra32();
		var exact = new Dictionary<uint, int>();
		for (int i = 0; i < px.Length; i += 4)
		{
			uint key = (uint)(px[i + 2] << 16 | px[i + 1] << 8 | px[i]);
			exact[key] = exact.TryGetValue(key, out int n) ? n + 1 : 1;
		}
		if (exact.Count == 0)
			return new[] { Colors.Black };
		if (exact.Count <= maxColors)
			return exact.Keys.OrderBy(k => k).Select(Opaque).ToArray();

		var buckets = new Dictionary<int, (long R, long G, long B, int N)>();
		foreach (var kv in exact)
		{
			int r = (int)(kv.Key >> 16 & 255), g = (int)(kv.Key >> 8 & 255), b = (int)(kv.Key & 255);
			int bucket = r >> 5 << 5 | g >> 5 << 2 | b >> 6;
			var cur = buckets.TryGetValue(bucket, out var v) ? v : default;
			buckets[bucket] = (cur.R + r * (long)kv.Value, cur.G + g * (long)kv.Value,
				cur.B + b * (long)kv.Value, cur.N + kv.Value);
		}
		return buckets.Values.OrderByDescending(v => v.N).Take(maxColors)
			.Select(v => Color.FromRgb((byte)(v.R / v.N), (byte)(v.G / v.N), (byte)(v.B / v.N))).ToArray();
	}

	static Color Opaque(uint rgb) => Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
}
