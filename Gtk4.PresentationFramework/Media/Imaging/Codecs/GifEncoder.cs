using System.Collections.Generic;
using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>Managed GIF writer: each frame gets a local colour table and LZW data.</summary>
static class GifEncoder
{
	internal static byte[] Encode(IList<DecodedImage> frames)
	{
		using var o = new MemoryStream();
		int w = 0, h = 0;
		foreach (var f in frames)
		{
			w = Math.Max(w, f.Width);
			h = Math.Max(h, f.Height);
		}
		Write(o, System.Text.Encoding.ASCII.GetBytes("GIF89a"));
		Write(o, (byte)w, (byte)(w >> 8), (byte)h, (byte)(h >> 8), 0, 0, 0);
		if (frames.Count > 1)
			Write(o, 0x21, 0xFF, 11, (byte)'N', (byte)'E', (byte)'T', (byte)'S', (byte)'C', (byte)'A', (byte)'P',
				(byte)'E', (byte)'2', (byte)'.', (byte)'0', 3, 1, 0, 0, 0);
		foreach (var f in frames)
			WriteFrame(o, f);
		o.WriteByte(0x3B);
		return o.ToArray();
	}

	static void Write(Stream s, params byte[] bytes) => s.Write(bytes, 0, bytes.Length);

	static void WriteFrame(Stream o, DecodedImage f)
	{
		var (idx, table) = Indexed(f);
		int transparent = Array.FindIndex(table, c => c.A == 0);
		int bits = 1;
		while (1 << bits < table.Length)
			bits++;
		if (transparent >= 0)
			Write(o, 0x21, 0xF9, 4, 9, 0, 0, (byte)transparent, 0);
		Write(o, 0x2C, 0, 0, 0, 0, (byte)f.Width, (byte)(f.Width >> 8), (byte)f.Height, (byte)(f.Height >> 8),
			(byte)(0x80 | bits - 1));
		for (int i = 0; i < 1 << bits; i++)
		{
			var c = i < table.Length ? table[i] : default;
			Write(o, c.R, c.G, c.B);
		}
		int minCode = Math.Max(2, bits);
		Write(o, (byte)minCode);
		byte[] lzw = Lzw.Encode(idx, minCode);
		for (int p = 0; p < lzw.Length; p += 255)
		{
			int n = Math.Min(255, lzw.Length - p);
			o.WriteByte((byte)n);
			o.Write(lzw, p, n);
		}
		o.WriteByte(0);
	}

	/// <summary>Palette indices and colour table for a frame (own palette when indexed, else quantised).</summary>
	static (byte[] Indices, Color[] Table) Indexed(DecodedImage f)
	{
		int bpp = f.Format.BitsPerPixel;
		if (f.Palette != null && f.Format.Name.StartsWith("Indexed", StringComparison.Ordinal))
		{
			var idx = new byte[f.Width * f.Height];
			int stride = PixelConverter.MinStride(f.Width, f.Format);
			for (int y = 0; y < f.Height; y++)
			{
				for (int x = 0; x < f.Width; x++)
				{
					int bit = x * bpp;
					idx[y * f.Width + x] = (byte)(f.Data[y * stride + bit / 8] >> 8 - bpp - bit % 8 & (1 << bpp) - 1);
				}
			}
			return (idx, f.Palette);
		}
		return Quantise(f.ToBgra32(), f.Width * f.Height);
	}

	static (byte[], Color[]) Quantise(byte[] bgra, int count)
	{
		var table = new List<Color>();
		var lookup = new Dictionary<uint, byte>();
		var idx = new byte[count];
		bool exact = true;
		for (int i = 0; i < count && exact; i++)
		{
			uint rgb = (uint)(bgra[i * 4 + 2] << 16 | bgra[i * 4 + 1] << 8 | bgra[i * 4]);
			uint key = bgra[i * 4 + 3] < 128 ? 0 : 0xFF000000u | rgb;
			if (!lookup.TryGetValue(key, out byte index))
			{
				if (table.Count == 256)
				{
					exact = false;
					break;
				}
				index = (byte)table.Count;
				lookup[key] = index;
				table.Add(BitmapPalette.FromArgb(key));
			}
			idx[i] = index;
		}
		if (exact)
			return (idx, table.ToArray());
		var coarse = new Color[256];
		for (int i = 0; i < 256; i++)
			coarse[i] = Color.FromRgb((byte)((i >> 5) * 255 / 7), (byte)((i >> 2 & 7) * 255 / 7),
				(byte)((i & 3) * 255 / 3));
		for (int i = 0; i < count; i++)
			idx[i] = (byte)(bgra[i * 4 + 2] >> 5 << 5 | bgra[i * 4 + 1] >> 5 << 2 | bgra[i * 4] >> 6);
		return (idx, coarse);
	}
}
