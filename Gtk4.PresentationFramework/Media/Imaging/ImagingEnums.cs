namespace System.Windows.Media.Imaging;

/// <summary>When a bitmap is decoded and cached.</summary>
public enum BitmapCacheOption
{
	/// <summary>Cache on demand (same as <see cref="OnDemand"/>).</summary>
	Default = 0,

	/// <summary>Cache on demand.</summary>
	OnDemand = 0,

	/// <summary>Cache the whole image when it is loaded.</summary>
	OnLoad = 1,

	/// <summary>Do not cache.</summary>
	None = 2,
}

/// <summary>Options that control how a bitmap is created.</summary>
[Flags]
public enum BitmapCreateOptions
{
	/// <summary>No options.</summary>
	None = 0,

	/// <summary>Keep the source pixel format.</summary>
	PreservePixelFormat = 1,

	/// <summary>Delay creating the bitmap until it is needed.</summary>
	DelayCreation = 2,

	/// <summary>Ignore an embedded colour profile.</summary>
	IgnoreColorProfile = 4,

	/// <summary>Bypass the image cache.</summary>
	IgnoreImageCache = 8,
}

/// <summary>Rotation applied when an image is decoded or encoded.</summary>
public enum Rotation
{
	/// <summary>No rotation.</summary>
	Rotate0,

	/// <summary>90 degrees clockwise.</summary>
	Rotate90,

	/// <summary>180 degrees.</summary>
	Rotate180,

	/// <summary>270 degrees clockwise.</summary>
	Rotate270,
}

/// <summary>PNG interlacing.</summary>
public enum PngInterlaceOption
{
	/// <summary>Encoder default.</summary>
	Default,

	/// <summary>Interlaced.</summary>
	On,

	/// <summary>Not interlaced.</summary>
	Off,
}

/// <summary>TIFF compression.</summary>
public enum TiffCompressOption
{
	/// <summary>Encoder default.</summary>
	Default,

	/// <summary>No compression.</summary>
	None,

	/// <summary>CCITT3.</summary>
	Ccitt3,

	/// <summary>CCITT4.</summary>
	Ccitt4,

	/// <summary>LZW.</summary>
	Lzw,

	/// <summary>Run-length encoding.</summary>
	Rle,

	/// <summary>Zip (deflate).</summary>
	Zip,
}
