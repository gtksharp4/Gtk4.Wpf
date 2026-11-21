using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>One decoded frame: pixels in <see cref="Format"/> with minimal-stride rows.</summary>
sealed class DecodedImage
{
	internal int Width;
	internal int Height;
	internal double DpiX = 96;
	internal double DpiY = 96;
	internal PixelFormat Format;
	internal Color[]? Palette;
	internal byte[] Data = Array.Empty<byte>();

	/// <summary>PNG tEXt entries (keyword to text).</summary>
	internal Dictionary<string, string>? Text;

	internal BitmapSource ToSource() => new MemoryBitmapSource(
		Width, Height, DpiX, DpiY, Format, Palette == null ? null : new BitmapPalette(Palette), Data);

	internal static DecodedImage FromSource(BitmapSource source) => new DecodedImage
	{
		Width = source.PixelWidth,
		Height = source.PixelHeight,
		DpiX = source.DpiX,
		DpiY = source.DpiY,
		Format = source.Format,
		Palette = source.Palette?.ColorArray,
		Data = source.GetPixelData(),
	};

	internal byte[] ToBgra32() => PixelConverter.ToBgra32(
		Data, PixelConverter.MinStride(Width, Format), Width, Height, Format, Palette);
}
