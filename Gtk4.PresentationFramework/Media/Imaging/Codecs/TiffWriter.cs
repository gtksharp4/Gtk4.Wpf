using System.Collections.Generic;
using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>Managed baseline TIFF writer: one little-endian IFD per frame, 8-bit RGB(A), one strip each.</summary>
static class TiffWriter
{
	const int Short = 3, Long = 4, Rational = 5;

	internal static byte[] Encode(IList<DecodedImage> frames, TiffCompressOption compression)
	{
		int code = compression switch
		{
			TiffCompressOption.None => 1,
			_ => 5,
		};
		var o = new MemoryStream();
		o.Write(new byte[] { (byte)'I', (byte)'I', 42, 0, 0, 0, 0, 0 }, 0, 8);
		long linkAt = 4;
		foreach (var frame in frames)
		{
			long ifd = WriteFrame(o, frame, code);
			Patch(o, linkAt, (uint)ifd);
			linkAt = ifd + 2 + 12 * EntryCount(frame);
		}
		return o.ToArray();
	}

	static int EntryCount(DecodedImage f) => ImageEncoding.HasAlpha(f) ? 14 : 13;

	static long WriteFrame(MemoryStream o, DecodedImage f, int code)
	{
		bool alpha = ImageEncoding.HasAlpha(f);
		int spp = alpha ? 4 : 3;
		var bgra = f.ToBgra32();
		var raw = new byte[f.Width * f.Height * spp];
		for (int i = 0, s = 0, d = 0; i < f.Width * f.Height; i++, s += 4, d += spp)
		{
			raw[d] = bgra[s + 2];
			raw[d + 1] = bgra[s + 1];
			raw[d + 2] = bgra[s];
			if (alpha)
				raw[d + 3] = bgra[s + 3];
		}
		byte[] strip = code == 5 ? TiffLzw.Compress(raw) : raw;
		uint stripAt = (uint)o.Position;
		o.Write(strip, 0, strip.Length);
		Align(o);

		uint bpsAt = (uint)o.Position;
		for (int i = 0; i < spp; i++)
			Put16(o, 8);
		uint xAt = (uint)o.Position;
		PutRational(o, f.DpiX);
		uint yAt = (uint)o.Position;
		PutRational(o, f.DpiY);

		long ifd = o.Position;
		Put16(o, EntryCount(f));
		Entry(o, 256, Long, 1, (uint)f.Width);
		Entry(o, 257, Long, 1, (uint)f.Height);
		Entry(o, 258, Short, spp, bpsAt);
		Entry(o, 259, Short, 1, (uint)code);
		Entry(o, 262, Short, 1, 2);
		Entry(o, 273, Long, 1, stripAt);
		Entry(o, 277, Short, 1, (uint)spp);
		Entry(o, 278, Long, 1, (uint)f.Height);
		Entry(o, 279, Long, 1, (uint)strip.Length);
		Entry(o, 282, Rational, 1, xAt);
		Entry(o, 283, Rational, 1, yAt);
		Entry(o, 284, Short, 1, 1);
		Entry(o, 296, Short, 1, 2);
		if (alpha)
			Entry(o, 338, Short, 1, 2);
		Put32(o, 0);
		return ifd;
	}

	static void Entry(Stream o, int tag, int type, int count, uint value)
	{
		Put16(o, tag);
		Put16(o, type);
		Put32(o, (uint)count);
		Put32(o, value);
	}

	static void PutRational(Stream o, double dpi)
	{
		Put32(o, (uint)Math.Max(1, Math.Round(dpi * 1000)));
		Put32(o, 1000);
	}

	static void Align(Stream o)
	{
		if (o.Position % 2 != 0)
			o.WriteByte(0);
	}

	static void Put16(Stream o, int v)
	{
		o.WriteByte((byte)v);
		o.WriteByte((byte)(v >> 8));
	}

	static void Put32(Stream o, uint v)
	{
		for (int i = 0; i < 4; i++)
			o.WriteByte((byte)(v >> (8 * i)));
	}

	static void Patch(MemoryStream o, long at, uint v)
	{
		long end = o.Position;
		o.Position = at;
		Put32(o, v);
		o.Position = end;
	}
}
