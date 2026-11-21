using System.Windows.Media.Imaging;

namespace System.Windows.Controls;

public partial class Image
{
	// File-backed images keep Gtk.Picture.Filename (GTK decodes them); everything else is shown
	// from its pixels as a Gdk.MemoryTexture.
	static bool IsInMemory(System.Windows.Media.ImageSource source)
	{
		if (source is not BitmapSource bitmap)
			return false;
		if (bitmap is BitmapImage image)
			return image.LoadedBytes != null || image.StreamSource != null || image.UriSource == null
				|| image.HasDecodeOptions;
		return true;
	}

	void ApplyBitmap(BitmapSource source)
	{
		int width = source.PixelWidth, height = source.PixelHeight;
		if (width <= 0 || height <= 0)
		{
			_picture.Paintable = null;
			return;
		}
		byte[] bgra = source.ToBgra32();
		using var bytes = new GLib.Bytes(bgra);
		_picture.Paintable = new Gdk.MemoryTexture(width, height, Gdk.MemoryFormat.B8g8r8a8, bytes,
			(ulong)(width * 4));
	}
}
