using System.Collections.Generic;
using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>TIFF LZW strip compressor (MSB-first codes with early change, as libtiff).</summary>
static class TiffLzw
{
	const int Clear = 256, Eoi = 257, First = 258, MaxEntry = 4094;

	internal static byte[] Compress(byte[] data)
	{
		var output = new MemoryStream();
		int bits = 0, acc = 0, width = 9, next = First;
		var dict = new Dictionary<int, int>();

		void Put(int code)
		{
			acc = acc << width | code;
			bits += width;
			while (bits >= 8)
			{
				bits -= 8;
				output.WriteByte((byte)(acc >> bits));
			}
			acc &= (1 << bits) - 1;
		}

		void Bump()
		{
			if (next == 512) width = 10;
			else if (next == 1024) width = 11;
			else if (next == 2048) width = 12;
		}

		Put(Clear);
		int cur = -1;
		foreach (byte c in data)
		{
			if (cur < 0)
			{
				cur = c;
				continue;
			}
			if (dict.TryGetValue(cur << 8 | c, out int hit))
			{
				cur = hit;
				continue;
			}
			Put(cur);
			dict[cur << 8 | c] = next++;
			if (next == MaxEntry)
			{
				Put(Clear);
				dict.Clear();
				next = First;
				width = 9;
			}
			else
				Bump();
			cur = c;
		}
		if (cur >= 0)
		{
			Put(cur);
			next++;
			if (next == MaxEntry)
			{
				Put(Clear);
				width = 9;
			}
			else
				Bump();
		}
		Put(Eoi);
		if (bits > 0)
			output.WriteByte((byte)(acc << (8 - bits)));
		return output.ToArray();
	}
}
