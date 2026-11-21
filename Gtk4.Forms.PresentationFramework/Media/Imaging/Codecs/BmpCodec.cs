using System.Collections.Generic;
using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>Managed BMP reader (1/4/8/24/32 bpp, uncompressed) and writer; gdk-pixbuf has no BMP module here.</summary>
static class BmpCodec
{
	static int I32(byte[] b, int p) => BitConverter.ToInt32(b, p);

	internal static DecodedImage Decode(byte[] b)
	{
		try
		{
			return DecodeCore(b);
		}
		catch (IndexOutOfRangeException)
		{
			throw new FileFormatException("The BMP data is truncated.");
		}
		catch (ArgumentException)
		{
			throw new FileFormatException("The BMP data is truncated.");
		}
	}

	static DecodedImage DecodeCore(byte[] b)
	{
		int dataOffset = I32(b, 10);
		int headerSize = I32(b, 14);
		if (headerSize < 40)
			throw new NotSupportedException("BMP files with a core (OS/2) header are not supported.");
		int width = I32(b, 18);
		int rawHeight = I32(b, 22);
		int bpp = BitConverter.ToUInt16(b, 28);
		int compression = I32(b, 30);
		int colorsUsed = I32(b, 46);
		bool topDown = rawHeight < 0;
		int height = Math.Abs(rawHeight);
		if (width <= 0 || height == 0)
			throw new FileFormatException("The BMP dimensions are invalid.");
		if (compression != 0 && !(compression == 3 && bpp == 32))
			throw new NotSupportedException("Compressed BMP files are not supported.");

		PixelFormat format;
		switch (bpp)
		{
			case 1: format = PixelFormats.Indexed1; break;
			case 4: format = PixelFormats.Indexed4; break;
			case 8: format = PixelFormats.Indexed8; break;
			case 24: format = PixelFormats.Bgr24; break;
			case 32: format = PixelFormats.Bgr32; break;
			default: throw new NotSupportedException($"BMP files with {bpp} bits per pixel are not supported.");
		}
		var img = new DecodedImage { Width = width, Height = height, Format = format };
		int ppmX = I32(b, 38), ppmY = I32(b, 42);
		if (ppmX > 0 && ppmY > 0)
		{
			img.DpiX = ppmX * 0.0254;
			img.DpiY = ppmY * 0.0254;
		}
		if (bpp <= 8)
			img.Palette = ReadPalette(b, 14 + headerSize, colorsUsed == 0 ? 1 << bpp : Math.Min(colorsUsed, 1 << bpp));

		int rowBytes = PixelConverter.MinStride(width, format);
		int srcStride = (width * bpp + 31) / 32 * 4;
		img.Data = new byte[rowBytes * height];
		for (int y = 0; y < height; y++)
		{
			int srcRow = dataOffset + (topDown ? y : height - 1 - y) * srcStride;
			Buffer.BlockCopy(b, srcRow, img.Data, y * rowBytes, rowBytes);
		}
		return img;
	}

	static Color[] ReadPalette(byte[] b, int offset, int count)
	{
		var palette = new Color[count];
		for (int i = 0; i < count; i++)
			palette[i] = Color.FromRgb(b[offset + i * 4 + 2], b[offset + i * 4 + 1], b[offset + i * 4]);
		return palette;
	}

	internal static byte[] Encode(DecodedImage img)
	{
		int bpp = img.Palette != null && (img.Format == PixelFormats.Indexed1 || img.Format == PixelFormats.Indexed4
			|| img.Format == PixelFormats.Indexed8)
			? img.Format.BitsPerPixel : 24;
		byte[] data = img.Data;
		if (bpp == 24 && img.Format != PixelFormats.Bgr24)
			data = ToBgr24(img);
		int colors = bpp <= 8 ? 1 << bpp : 0;
		int stride = (img.Width * bpp + 31) / 32 * 4;
		int rowBytes = (img.Width * bpp + 7) / 8;
		int dataOffset = 54 + colors * 4;
		var o = new byte[dataOffset + stride * img.Height];
		o[0] = (byte)'B';
		o[1] = (byte)'M';
		Put(o, 2, o.Length);
		Put(o, 10, dataOffset);
		Put(o, 14, 40);
		Put(o, 18, img.Width);
		Put(o, 22, img.Height);
		o[26] = 1;
		o[28] = (byte)bpp;
		Put(o, 34, stride * img.Height);
		Put(o, 38, (int)Math.Round(img.DpiX / 0.0254));
		Put(o, 42, (int)Math.Round(img.DpiY / 0.0254));
		Put(o, 46, colors);
		for (int i = 0; i < colors && img.Palette != null && i < img.Palette.Length; i++)
		{
			o[54 + i * 4] = img.Palette[i].B;
			o[55 + i * 4] = img.Palette[i].G;
			o[56 + i * 4] = img.Palette[i].R;
		}
		for (int y = 0; y < img.Height; y++)
			Buffer.BlockCopy(data, y * rowBytes, o, dataOffset + (img.Height - 1 - y) * stride, rowBytes);
		return o;
	}

	static byte[] ToBgr24(DecodedImage img)
	{
		byte[] bgra = img.ToBgra32();
		var o = new byte[img.Width * img.Height * 3];
		for (int i = 0, j = 0; i < bgra.Length; i += 4, j += 3)
		{
			o[j] = bgra[i];
			o[j + 1] = bgra[i + 1];
			o[j + 2] = bgra[i + 2];
		}
		return o;
	}

	static void Put(byte[] o, int p, int v) => BitConverter.GetBytes(v).CopyTo(o, p);

}
