using System.IO;

using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>Managed GIF reader: gdk-pixbuf exposes neither the palette nor all frames as indices.</summary>
static class GifDecoder
{
	internal static List<DecodedImage> Decode(byte[] b)
	{
		try
		{
			return DecodeCore(b);
		}
		catch (IndexOutOfRangeException)
		{
			throw new FileFormatException("The GIF data is truncated.");
		}
	}

	static List<DecodedImage> DecodeCore(byte[] b)
	{
		var frames = new List<DecodedImage>();
		int flags = b[10];
		int p = 13;
		Color[]? global = null;
		if ((flags & 0x80) != 0)
		{
			global = ReadTable(b, p, 2 << (flags & 7));
			p += 3 * global.Length;
		}
		int transparent = -1;
		while (p < b.Length && b[p] != 0x3B)
		{
			if (b[p] == 0x21)
			{
				if (b[p + 1] == 0xF9 && b[p + 2] >= 4)
					transparent = (b[p + 3] & 1) != 0 ? b[p + 6] : -1;
				p = SkipBlocks(b, p + 2);
			}
			else if (b[p] == 0x2C)
			{
				p = ReadFrame(b, p, global, transparent, frames);
				transparent = -1;
			}
			else
			{
				throw new FileFormatException("Unexpected GIF block.");
			}
		}
		if (frames.Count == 0)
			throw new FileFormatException("The GIF contains no frames.");
		return frames;
	}

	static Color[] ReadTable(byte[] b, int p, int count)
	{
		var table = new Color[count];
		for (int i = 0; i < count; i++)
			table[i] = Color.FromRgb(b[p + i * 3], b[p + i * 3 + 1], b[p + i * 3 + 2]);
		return table;
	}

	static int SkipBlocks(byte[] b, int p)
	{
		while (b[p] != 0)
			p += b[p] + 1;
		return p + 1;
	}

	static int ReadFrame(byte[] b, int p, Color[]? global, int transparent, List<DecodedImage> frames)
	{
		int w = b[p + 5] | b[p + 6] << 8, h = b[p + 7] | b[p + 8] << 8, flags = b[p + 9];
		p += 10;
		Color[]? table = global;
		if ((flags & 0x80) != 0)
		{
			table = ReadTable(b, p, 2 << (flags & 7));
			p += 3 * table.Length;
		}
		if (table == null || w == 0 || h == 0)
			throw new FileFormatException("The GIF frame has no colour table.");
		int minCode = b[p++];
		var data = new List<byte>();
		while (b[p] != 0)
		{
			for (int i = 1; i <= b[p]; i++)
				data.Add(b[p + i]);
			p += b[p] + 1;
		}
		byte[] idx = Lzw.Decode(data.ToArray(), minCode, w * h);
		if ((flags & 0x40) != 0)
			idx = Deinterlace(idx, w, h);
		if (transparent >= 0 && transparent < table.Length)
		{
			table = (Color[])table.Clone();
			table[transparent] = Color.FromArgb(0, table[transparent].R, table[transparent].G, table[transparent].B);
		}
		frames.Add(new DecodedImage
		{
			Width = w,
			Height = h,
			Format = PixelFormats.Indexed8,
			Palette = table,
			Data = idx,
		});
		return p + 1;
	}

	static byte[] Deinterlace(byte[] src, int w, int h)
	{
		var o = new byte[src.Length];
		int row = 0;
		foreach (var (start, step) in new[] { (0, 8), (4, 8), (2, 4), (1, 2) })
		{
			for (int y = start; y < h; y += step)
				Buffer.BlockCopy(src, row++ * w, o, y * w, w);
		}
		return o;
	}
}
