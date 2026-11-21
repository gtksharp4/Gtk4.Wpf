namespace System.Windows.Media.Imaging;

/// <summary>The image container formats, recognised by their magic bytes.</summary>
enum ImageContainer
{
	Unknown,
	Png,
	Jpeg,
	Gif,
	Bmp,
	Tiff,
	Wmp,
}

static class ImageSniffer
{
	internal static ImageContainer Sniff(byte[] b)
	{
		if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47)
			return ImageContainer.Png;
		if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
			return ImageContainer.Jpeg;
		if (b.Length >= 6 && b[0] == 'G' && b[1] == 'I' && b[2] == 'F' && b[3] == '8')
			return ImageContainer.Gif;
		if (b.Length >= 14 && b[0] == 'B' && b[1] == 'M')
			return ImageContainer.Bmp;
		if (b.Length >= 4 && b[0] == 'I' && b[1] == 'I' && b[2] == 0xBC)
			return ImageContainer.Wmp;
		if (b.Length >= 4 && (b[0] == 'I' && b[1] == 'I' && b[2] == 0x2A && b[3] == 0
			|| b[0] == 'M' && b[1] == 'M' && b[2] == 0 && b[3] == 0x2A))
			return ImageContainer.Tiff;
		return ImageContainer.Unknown;
	}
}
