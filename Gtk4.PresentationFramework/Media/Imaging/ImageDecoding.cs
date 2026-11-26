using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>Routes a container to its decoder implementation.</summary>
static class ImageDecoding
{
	internal static List<DecodedImage> Decode(byte[] data, ImageContainer container)
	{
		switch (container)
		{
			case ImageContainer.Png:
				var png = PixbufCodec.Decode(data, "PNG");
				png.Text = PngText.Read(data);
				PngText.ReadDpi(data, png);
				return new List<DecodedImage> { png };
			case ImageContainer.Jpeg:
				return new List<DecodedImage> { PixbufCodec.Decode(data, "JPEG") };
			case ImageContainer.Tiff:
				return DecodeTiff(data);
			case ImageContainer.Gif:
				return GifDecoder.Decode(data);
			case ImageContainer.Bmp:
				return new List<DecodedImage> { BmpCodec.Decode(data) };
			default:
				throw new NotSupportedException(
					"JPEG XR (WDP) has no decoder in the GTK runtime, so WmpBitmapDecoder cannot read images.");
		}
	}

	static List<DecodedImage> DecodeTiff(byte[] data)
	{
		var images = new List<DecodedImage>();
		foreach (var page in TiffPages.Split(data))
			images.Add(PixbufCodec.Decode(page, "TIFF"));
		if (images.Count == 0)
			images.Add(PixbufCodec.Decode(data, "TIFF"));
		return images;
	}

	internal static BitmapMetadata? MetadataOf(DecodedImage image, ImageContainer container)
	{
		string? format = container switch
		{
			ImageContainer.Png => "png",
			ImageContainer.Jpeg => "jpg",
			ImageContainer.Gif => "gif",
			ImageContainer.Tiff => "tiff",
			_ => null,
		};
		if (format == null)
			return null;
		var meta = new BitmapMetadata(format);
		if (image.Text != null)
		{
			foreach (var kv in image.Text)
				meta.SetQuery("/Text/" + kv.Key, kv.Value);
		}
		meta.Freeze();
		return meta;
	}
}
