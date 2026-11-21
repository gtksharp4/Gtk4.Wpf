using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>
/// Splits a multi-page TIFF into single-page files. gdk-pixbuf only reads the first directory, so each page
/// is the original bytes with the header pointing at that page's IFD and its next-IFD link cleared (strip
/// and tag data keep their absolute offsets).
/// </summary>
static class TiffPages
{
	const int MaxPages = 4096;

	internal static List<byte[]> Split(byte[] data)
	{
		var pages = new List<byte[]>();
		if (data.Length < 8)
			return pages;
		bool little = data[0] == 'I';
		var seen = new HashSet<long>();
		long ifd = ReadU32(data, 4, little);
		while (ifd >= 8 && ifd + 2 <= data.Length && seen.Add(ifd) && pages.Count < MaxPages)
		{
			int count = ReadU16(data, (int)ifd, little);
			long link = ifd + 2 + 12L * count;
			if (link + 4 > data.Length)
				break;
			var page = (byte[])data.Clone();
			WriteU32(page, 4, (uint)ifd, little);
			WriteU32(page, (int)link, 0, little);
			pages.Add(page);
			ifd = ReadU32(data, (int)link, little);
		}
		return pages;
	}

	static int ReadU16(byte[] b, int o, bool little) =>
		little ? b[o] | b[o + 1] << 8 : b[o] << 8 | b[o + 1];

	static long ReadU32(byte[] b, int o, bool little) => little
		? (uint)(b[o] | b[o + 1] << 8 | b[o + 2] << 16 | b[o + 3] << 24)
		: (uint)(b[o] << 24 | b[o + 1] << 16 | b[o + 2] << 8 | b[o + 3]);

	static void WriteU32(byte[] b, int o, uint v, bool little)
	{
		for (int i = 0; i < 4; i++)
			b[o + (little ? i : 3 - i)] = (byte)(v >> (8 * i));
	}
}
