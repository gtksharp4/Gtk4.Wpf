using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>Writes PNG through the gdk-pixbuf saver plus managed tEXt/pHYs chunks.</summary>
public sealed class PngBitmapEncoder : BitmapEncoder
{
	/// <summary>Creates the encoder.</summary>
	public PngBitmapEncoder() : base(ImageContainer.Png) { }

	/// <summary>Interlacing; stored only (the gdk-pixbuf PNG saver has no interlace option).</summary>
	public PngInterlaceOption Interlace { get; set; }

	internal override byte[] Encode(IList<DecodedImage> frames)
	{
		RequireSingle(frames, "PNG");
		var f = frames[0];
		byte[] png = PixbufCodec.Encode(f.ToBgra32(), f.Width, f.Height, "png", ImageEncoding.HasAlpha(f),
			new string[0], new string[0]);
		return PngText.Write(png, f.Text ?? new Dictionary<string, string>(), f.DpiX, f.DpiY);
	}
}

/// <summary>Writes JPEG through the gdk-pixbuf saver.</summary>
public sealed class JpegBitmapEncoder : BitmapEncoder
{
	int _quality = 75;

	/// <summary>Creates the encoder.</summary>
	public JpegBitmapEncoder() : base(ImageContainer.Jpeg) { }

	/// <summary>Quality from 1 to 100 (default 75).</summary>
	/// <exception cref="ArgumentOutOfRangeException">Outside 1..100.</exception>
	public int QualityLevel
	{
		get => _quality;
		set
		{
			if (value < 1 || value > 100)
				throw new ArgumentOutOfRangeException(nameof(value));
			_quality = value;
		}
	}

	/// <summary>Rotation applied to the pixels on save.</summary>
	public Rotation Rotation { get; set; }

	/// <summary>Mirror horizontally on save.</summary>
	public bool FlipHorizontal { get; set; }

	/// <summary>Mirror vertically on save.</summary>
	public bool FlipVertical { get; set; }

	internal override byte[] Encode(IList<DecodedImage> frames)
	{
		RequireSingle(frames, "JPEG");
		var f = frames[0];
		var (bgra, w, h) = PixelTransforms.Apply(f.ToBgra32(), f.Width, f.Height, Rotation, FlipHorizontal,
			FlipVertical);
		return PixbufCodec.Encode(bgra, w, h, "jpeg", false, new[] { "quality" },
			new[] { _quality.ToString(System.Globalization.CultureInfo.InvariantCulture) });
	}
}

/// <summary>Writes TIFF with the managed writer: one 8-bit RGB(A) page per frame.</summary>
public sealed class TiffBitmapEncoder : BitmapEncoder
{
	/// <summary>Creates the encoder.</summary>
	public TiffBitmapEncoder() : base(ImageContainer.Tiff) { }

	/// <summary>Compression: None and Lzw are written; Default, Zip (the bundled libtiff cannot read it), Rle and
	/// CCITT are written as LZW.</summary>
	public TiffCompressOption Compression { get; set; }

	internal override byte[] Encode(IList<DecodedImage> frames) => TiffWriter.Encode(frames, Compression);
}

/// <summary>Writes BMP with the managed writer (24 bit, or palette for indexed frames).</summary>
public sealed class BmpBitmapEncoder : BitmapEncoder
{
	/// <summary>Creates the encoder.</summary>
	public BmpBitmapEncoder() : base(ImageContainer.Bmp) { }

	internal override byte[] Encode(IList<DecodedImage> frames)
	{
		RequireSingle(frames, "BMP");
		return BmpCodec.Encode(frames[0]);
	}
}

/// <summary>Writes GIF with the managed LZW writer; several frames are allowed.</summary>
public sealed class GifBitmapEncoder : BitmapEncoder
{
	/// <summary>Creates the encoder.</summary>
	public GifBitmapEncoder() : base(ImageContainer.Gif) { }

	internal override byte[] Encode(IList<DecodedImage> frames) => GifEncoder.Encode(frames);
}

/// <summary>JPEG XR encoder; the GTK runtime has no JPEG XR writer, so saving throws.</summary>
public sealed class WmpBitmapEncoder : BitmapEncoder
{
	/// <summary>Creates the encoder.</summary>
	public WmpBitmapEncoder() : base(ImageContainer.Wmp) { }

	/// <summary>Image quality (stored only).</summary>
	public float ImageQualityLevel { get; set; } = 0.9f;

	/// <summary>Lossless mode (stored only).</summary>
	public bool Lossless { get; set; }

	/// <summary>Quality level 1..255 (stored only).</summary>
	public byte QualityLevel { get; set; }

	/// <summary>Rotation (stored only).</summary>
	public Rotation Rotation { get; set; }

	/// <summary>Horizontal flip (stored only).</summary>
	public bool FlipHorizontal { get; set; }

	/// <summary>Vertical flip (stored only).</summary>
	public bool FlipVertical { get; set; }

	internal override byte[] Encode(IList<DecodedImage> frames) =>
		throw new NotSupportedException("JPEG XR (WDP) has no encoder in the GTK runtime.");
}

static class ImageEncoding
{
	internal static bool HasAlpha(DecodedImage f)
	{
		switch (f.Format.Name)
		{
			case "Bgra32":
			case "Pbgra32":
			case "Rgba64":
			case "Prgba64":
			case "Rgba128Float":
			case "Prgba128Float":
				return true;
		}
		if (f.Palette == null)
			return false;
		foreach (var c in f.Palette)
		{
			if (c.A < 255)
				return true;
		}
		return false;
	}
}
