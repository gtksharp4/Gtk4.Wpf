using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System.Windows.Media.Imaging;

/// <summary>Reads and writes PNG tEXt chunks (metadata queries of the form <c>/Text/key</c>).</summary>
static class PngText
{
	static readonly Encoding _latin1 = Encoding.GetEncoding(28591);

	/// <summary>All tEXt keywords in <paramref name="png"/>; empty when there are none.</summary>
	internal static Dictionary<string, string> Read(byte[] png)
	{
		var text = new Dictionary<string, string>();
		foreach (var (type, offset, length) in Chunks(png))
		{
			if (type != "tEXt")
				continue;
			int zero = Array.IndexOf(png, (byte)0, offset, length);
			if (zero > offset)
				text[_latin1.GetString(png, offset, zero - offset)] =
					_latin1.GetString(png, zero + 1, offset + length - zero - 1);
		}
		return text;
	}

	/// <summary>Returns <paramref name="png"/> with its tEXt chunks replaced by <paramref name="text"/>.</summary>
	internal static byte[] Write(byte[] png, IDictionary<string, string> text, double dpiX, double dpiY)
	{
		using var o = new MemoryStream();
		o.Write(png, 0, 8);
		bool inserted = false;
		foreach (var (type, offset, length) in Chunks(png))
		{
			if (type == "tEXt" || type == "pHYs")
				continue;
			if (!inserted && type != "IHDR")
			{
				WritePhys(o, dpiX, dpiY);
				WriteText(o, text);
				inserted = true;
			}
			o.Write(png, offset - 8, length + 12);
		}
		return o.ToArray();
	}

	static void WritePhys(Stream o, double dpiX, double dpiY)
	{
		var body = new byte[9];
		Big((int)Math.Round(dpiX / 0.0254)).CopyTo(body, 0);
		Big((int)Math.Round(dpiY / 0.0254)).CopyTo(body, 4);
		body[8] = 1;
		WriteChunk(o, "pHYs", body);
	}

	/// <summary>Sets the DPI of <paramref name="image"/> from a pHYs chunk when one is present.</summary>
	internal static void ReadDpi(byte[] png, DecodedImage image)
	{
		foreach (var (type, offset, length) in Chunks(png))
		{
			if (type != "pHYs" || length != 9 || png[offset + 8] != 1)
				continue;
			int x = png[offset] << 24 | png[offset + 1] << 16 | png[offset + 2] << 8 | png[offset + 3];
			int y = png[offset + 4] << 24 | png[offset + 5] << 16 | png[offset + 6] << 8 | png[offset + 7];
			if (x > 0 && y > 0)
			{
				image.DpiX = x * 0.0254;
				image.DpiY = y * 0.0254;
			}
		}
	}

	static void WriteText(Stream o, IDictionary<string, string> text)
	{
		foreach (var kv in text)
		{
			byte[] key = _latin1.GetBytes(kv.Key), value = _latin1.GetBytes(kv.Value);
			var body = new byte[key.Length + 1 + value.Length];
			key.CopyTo(body, 0);
			value.CopyTo(body, key.Length + 1);
			WriteChunk(o, "tEXt", body);
		}
	}

	static void WriteChunk(Stream o, string type, byte[] body)
	{
		var typed = new byte[4 + body.Length];
		Encoding.ASCII.GetBytes(type).CopyTo(typed, 0);
		body.CopyTo(typed, 4);
		o.Write(Big(body.Length), 0, 4);
		o.Write(typed, 0, typed.Length);
		o.Write(Big((int)Crc32(typed)), 0, 4);
	}

	static byte[] Big(int v) => new[] { (byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v };

	// Yields (type, body offset, body length); the chunk spans offset-8 .. offset+length+4.
	static IEnumerable<(string, int, int)> Chunks(byte[] png)
	{
		int p = 8;
		while (p + 12 <= png.Length)
		{
			int length = png[p] << 24 | png[p + 1] << 16 | png[p + 2] << 8 | png[p + 3];
			if (length < 0 || p + 12 + length > png.Length)
				yield break;
			yield return (Encoding.ASCII.GetString(png, p + 4, 4), p + 8, length);
			p += 12 + length;
		}
	}

	static uint Crc32(byte[] data)
	{
		uint crc = 0xFFFFFFFF;
		foreach (byte b in data)
		{
			crc ^= b;
			for (int k = 0; k < 8; k++)
				crc = (crc & 1) != 0 ? crc >> 1 ^ 0xEDB88320 : crc >> 1;
		}
		return ~crc;
	}
}
