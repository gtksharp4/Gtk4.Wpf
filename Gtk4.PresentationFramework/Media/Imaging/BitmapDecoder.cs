using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace System.Windows.Media.Imaging;

/// <summary>Decodes an image container into frames (WPF <c>BitmapDecoder</c>).</summary>
public abstract class BitmapDecoder
{
	readonly ImageContainer _container;
	readonly ReadOnlyCollection<BitmapFrame> _frames;

	/// <summary>Creates a decoder for a derived class; override <see cref="Frames"/> and the other members.</summary>
	protected BitmapDecoder() => _frames = new List<BitmapFrame>().AsReadOnly();

	internal BitmapDecoder(ImageContainer expected, byte[] data)
	{
		_container = expected;
		var found = ImageSniffer.Sniff(data);
		if (found == ImageContainer.Unknown)
			throw new NotSupportedException("No image codec recognises the data.");
		if (found != expected)
			throw new FileFormatException("The data is not in the format this decoder reads.");
		var frames = new List<BitmapFrame>();
		foreach (var image in ImageDecoding.Decode(data, found))
			frames.Add(new SourceBitmapFrame(image.ToSource(), null, ImageDecoding.MetadataOf(image, found), null,
				this));
		_frames = frames.AsReadOnly();
	}

	/// <summary>The decoded frames.</summary>
	public virtual ReadOnlyCollection<BitmapFrame> Frames => _frames;

	/// <summary>Describes the codec.</summary>
	public virtual BitmapCodecInfo CodecInfo => _container == ImageContainer.Unknown
		? throw new NotSupportedException("A derived decoder must override CodecInfo.")
		: BuiltInCodecInfo.For(_container, true);

	/// <summary>Container level metadata (none for the built-in codecs).</summary>
	public virtual BitmapMetadata? Metadata => null;

	/// <summary>Container level palette (none for the built-in codecs).</summary>
	public virtual BitmapPalette? Palette => null;

	/// <summary>Container level thumbnail (none for the built-in codecs).</summary>
	public virtual BitmapSource? Thumbnail => null;

	/// <summary>Container level preview (none for the built-in codecs).</summary>
	public virtual BitmapSource? Preview => null;

	/// <summary>Container level colour profiles (none for the built-in codecs).</summary>
	public virtual ReadOnlyCollection<ColorContext>? ColorContexts => null;

	/// <summary>Creates the decoder that matches the content of <paramref name="bitmapUri"/>.</summary>
	public static BitmapDecoder Create(Uri bitmapUri, BitmapCreateOptions createOptions,
		BitmapCacheOption cacheOption) => Pick(ReadUri(bitmapUri));

	/// <summary>Creates the decoder that matches the content of <paramref name="bitmapStream"/>.</summary>
	public static BitmapDecoder Create(Stream bitmapStream, BitmapCreateOptions createOptions,
		BitmapCacheOption cacheOption) => Pick(ReadStream(bitmapStream));

	internal static byte[] ReadUri(Uri uri)
	{
		if (uri == null)
			throw new ArgumentNullException(nameof(uri));
		return File.ReadAllBytes(ImageSourceLocation.ResolveUri(uri));
	}

	internal static byte[] ReadStream(Stream stream)
	{
		if (stream == null)
			throw new ArgumentNullException(nameof(stream));
		using var copy = new MemoryStream();
		stream.CopyTo(copy);
		return copy.ToArray();
	}

	internal static BitmapDecoder Create(byte[] data) => Pick(data);

	static BitmapDecoder Pick(byte[] data)
	{
		switch (ImageSniffer.Sniff(data))
		{
			case ImageContainer.Png: return new PngBitmapDecoder(data);
			case ImageContainer.Jpeg: return new JpegBitmapDecoder(data);
			case ImageContainer.Gif: return new GifBitmapDecoder(data);
			case ImageContainer.Bmp: return new BmpBitmapDecoder(data);
			case ImageContainer.Tiff: return new TiffBitmapDecoder(data);
			case ImageContainer.Wmp: return new WmpBitmapDecoder(data);
			default: throw new NotSupportedException("No image codec recognises the data.");
		}
	}
}
