using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>Decodes BMP files.</summary>
public sealed class BmpBitmapDecoder : BitmapDecoder
{
	internal BmpBitmapDecoder(byte[] data) : base(ImageContainer.Bmp, data) { }

	/// <summary>Decodes the BMP at <paramref name="bitmapUri"/>.</summary>
	public BmpBitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Bmp, ReadUri(bitmapUri)) { }

	/// <summary>Decodes the BMP in <paramref name="bitmapStream"/>.</summary>
	public BmpBitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Bmp, ReadStream(bitmapStream)) { }
}

/// <summary>Decodes GIF files; every frame is returned as Indexed8.</summary>
public sealed class GifBitmapDecoder : BitmapDecoder
{
	internal GifBitmapDecoder(byte[] data) : base(ImageContainer.Gif, data) { }

	/// <summary>Decodes the GIF at <paramref name="bitmapUri"/>.</summary>
	public GifBitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Gif, ReadUri(bitmapUri)) { }

	/// <summary>Decodes the GIF in <paramref name="bitmapStream"/>.</summary>
	public GifBitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Gif, ReadStream(bitmapStream)) { }
}

/// <summary>Decodes JPEG files.</summary>
public sealed class JpegBitmapDecoder : BitmapDecoder
{
	internal JpegBitmapDecoder(byte[] data) : base(ImageContainer.Jpeg, data) { }

	/// <summary>Decodes the JPEG at <paramref name="bitmapUri"/>.</summary>
	public JpegBitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Jpeg, ReadUri(bitmapUri)) { }

	/// <summary>Decodes the JPEG in <paramref name="bitmapStream"/>.</summary>
	public JpegBitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Jpeg, ReadStream(bitmapStream)) { }
}

/// <summary>Decodes PNG files, reading tEXt chunks into the frame metadata.</summary>
public sealed class PngBitmapDecoder : BitmapDecoder
{
	internal PngBitmapDecoder(byte[] data) : base(ImageContainer.Png, data) { }

	/// <summary>Decodes the PNG at <paramref name="bitmapUri"/>.</summary>
	public PngBitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Png, ReadUri(bitmapUri)) { }

	/// <summary>Decodes the PNG in <paramref name="bitmapStream"/>.</summary>
	public PngBitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Png, ReadStream(bitmapStream)) { }
}

/// <summary>Decodes TIFF files; every page of the IFD chain becomes one frame.</summary>
public sealed class TiffBitmapDecoder : BitmapDecoder
{
	internal TiffBitmapDecoder(byte[] data) : base(ImageContainer.Tiff, data) { }

	/// <summary>Decodes the TIFF at <paramref name="bitmapUri"/>.</summary>
	public TiffBitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Tiff, ReadUri(bitmapUri)) { }

	/// <summary>Decodes the TIFF in <paramref name="bitmapStream"/>.</summary>
	public TiffBitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Tiff, ReadStream(bitmapStream)) { }
}

/// <summary>Decodes JPEG XR (WDP); always throws <see cref="NotSupportedException"/> for valid data.</summary>
public sealed class WmpBitmapDecoder : BitmapDecoder
{
	internal WmpBitmapDecoder(byte[] data) : base(ImageContainer.Wmp, data) { }

	/// <summary>Decodes the WDP at <paramref name="bitmapUri"/>.</summary>
	public WmpBitmapDecoder(Uri bitmapUri, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Wmp, ReadUri(bitmapUri)) { }

	/// <summary>Decodes the WDP in <paramref name="bitmapStream"/>.</summary>
	public WmpBitmapDecoder(Stream bitmapStream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption)
		: base(ImageContainer.Wmp, ReadStream(bitmapStream)) { }
}
