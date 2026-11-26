using System.Collections.Generic;

namespace System.Windows.Media.Imaging;

/// <summary>Describes a codec (WPF <c>BitmapCodecInfo</c>); values match the WIC built-in codecs.</summary>
public abstract class BitmapCodecInfo
{
	/// <summary>Creates a codec description for a derived class.</summary>
	protected BitmapCodecInfo() { }

	/// <summary>Codec author.</summary>
	public abstract string Author { get; }

	/// <summary>WIC container format GUID.</summary>
	public abstract Guid ContainerFormat { get; }

	/// <summary>Device manufacturer (empty for the built-in codecs).</summary>
	public abstract string DeviceManufacturer { get; }

	/// <summary>Device models (empty for the built-in codecs).</summary>
	public abstract string DeviceModels { get; }

	/// <summary>Comma separated file extensions.</summary>
	public abstract string FileExtensions { get; }

	/// <summary>Friendly codec name.</summary>
	public abstract string FriendlyName { get; }

	/// <summary>Comma separated MIME types.</summary>
	public abstract string MimeTypes { get; }

	/// <summary>Specification version.</summary>
	public abstract Version SpecificationVersion { get; }

	/// <summary>Whether the format supports animation.</summary>
	public abstract bool SupportsAnimation { get; }

	/// <summary>Whether the format is lossless.</summary>
	public abstract bool SupportsLossless { get; }

	/// <summary>Whether the format holds several frames.</summary>
	public abstract bool SupportsMultipleFrames { get; }

	/// <summary>Codec version.</summary>
	public abstract Version Version { get; }
}

/// <summary>The fixed description of one built-in codec.</summary>
sealed class BuiltInCodecInfo : BitmapCodecInfo
{
	readonly string _name, _ext, _mime;
	readonly Guid _container;
	readonly bool _animation, _lossless, _multiple;

	BuiltInCodecInfo(string name, string guid, string ext, string mime, bool animation, bool lossless, bool multiple)
	{
		_name = name;
		_container = new Guid(guid);
		_ext = ext;
		_mime = mime;
		_animation = animation;
		_lossless = lossless;
		_multiple = multiple;
	}

	internal static BitmapCodecInfo For(ImageContainer container, bool decoder)
	{
		string kind = decoder ? " Decoder" : " Encoder";
		switch (container)
		{
			case ImageContainer.Png:
				return new BuiltInCodecInfo("PNG" + kind, "1b7cfaf4-713f-473c-bbcd-6137425faeaf", ".png",
					"image/png", false, true, false);
			case ImageContainer.Jpeg:
				return new BuiltInCodecInfo("JPEG" + kind, "19e4a5aa-5662-4fc5-a0c0-1758028e1057",
					".jpeg,.jpe,.jpg,.jfif,.exif", "image/jpeg,image/jpe,image/jpg", false, false, false);
			case ImageContainer.Gif:
				return new BuiltInCodecInfo("GIF" + kind, "1f8a5601-7d4d-4cbd-9c82-1bc8d4eeb9a5", ".gif",
					"image/gif", true, true, true);
			case ImageContainer.Tiff:
				return new BuiltInCodecInfo("TIFF" + kind, "163bcc30-e2e9-4f0b-961d-a3e9fdb788a3", ".tiff,.tif",
					"image/tiff,image/tif", false, true, true);
			case ImageContainer.Bmp:
				return new BuiltInCodecInfo("BMP" + kind, "0af1d87e-fcfe-4188-bdeb-a7906471cbe3", ".bmp,.dib,.rle",
					"image/bmp", false, true, false);
			default:
				return new BuiltInCodecInfo("WMPhoto" + kind, "57a37caa-367a-4540-916b-f183c5093a4b", ".wdp,.jxr",
					"image/vnd.ms-photo", false, true, false);
		}
	}

	public override string Author => "Microsoft";

	public override Guid ContainerFormat => _container;

	public override string DeviceManufacturer => string.Empty;

	public override string DeviceModels => string.Empty;

	public override string FileExtensions => _ext;

	public override string FriendlyName => _name;

	public override string MimeTypes => _mime;

	public override Version SpecificationVersion => new Version(1, 0, 0, 0);

	public override bool SupportsAnimation => _animation;

	public override bool SupportsLossless => _lossless;

	public override bool SupportsMultipleFrames => _multiple;

	public override Version Version => new Version(1, 0, 0, 0);
}
