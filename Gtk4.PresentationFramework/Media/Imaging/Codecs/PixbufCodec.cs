using System.IO;
using System.Runtime.InteropServices;

namespace System.Windows.Media.Imaging;

/// <summary>PNG, JPEG, TIFF (and static GIF) through gdk-pixbuf loaders and savers.</summary>
static class PixbufCodec
{
	internal static DecodedImage Decode(byte[] bytes, string container)
	{
		Gdk.Pixbuf? pb;
		try
		{
			using var loader = new Gdk.PixbufLoader();
			loader.Write(bytes);
			loader.Close();
			pb = loader.Pixbuf;
		}
		catch (Exception e) when (e is GLib.GException || e is NullReferenceException)
		{
			throw new FileFormatException($"The {container} data could not be decoded.", e);
		}
		if (pb == null)
			throw new FileFormatException($"The {container} data could not be decoded.");

		int w = pb.Width, h = pb.Height, channels = pb.NChannels, stride = pb.Rowstride;
		bool alpha = pb.HasAlpha;
		int bytesPerPixel = alpha ? 4 : 3;
		var native = new byte[stride * h];
		Marshal.Copy(pb.Pixels, native, 0, native.Length);
		var data = new byte[w * h * bytesPerPixel];
		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				int s = y * stride + x * channels, d = (y * w + x) * bytesPerPixel;
				data[d] = native[s + 2];
				data[d + 1] = native[s + 1];
				data[d + 2] = native[s];
				if (alpha)
					data[d + 3] = native[s + 3];
			}
		}
		return new DecodedImage
		{
			Width = w,
			Height = h,
			Format = alpha ? PixelFormats.Bgra32 : PixelFormats.Bgr24,
			Data = data,
		};
	}

	/// <summary>Whether the runtime's gdk-pixbuf can write <paramref name="type"/> (png, jpeg, tiff).</summary>
	internal static bool CanWrite(string type)
	{
		foreach (var f in Gdk.Pixbuf.Formats)
		{
			if (f.Name == type)
				return f.IsWritable;
		}
		return false;
	}

	/// <summary>Encodes straight BGRA32 pixels (width x height) with the named gdk-pixbuf saver.</summary>
	internal static byte[] Encode(byte[] bgra, int width, int height, string type, bool alpha,
		string[] keys, string[] values)
	{
		if (!CanWrite(type))
			throw new NotSupportedException($"This GTK runtime has no gdk-pixbuf '{type}' writer.");
		int channels = alpha ? 4 : 3;
		using var pb = new Gdk.Pixbuf(Gdk.Colorspace.Rgb, alpha, 8, width, height);
		var native = new byte[pb.Rowstride * height];
		for (int y = 0; y < height; y++)
		{
			for (int x = 0; x < width; x++)
			{
				int s = (y * width + x) * 4, d = y * pb.Rowstride + x * channels;
				native[d] = bgra[s + 2];
				native[d + 1] = bgra[s + 1];
				native[d + 2] = bgra[s];
				if (alpha)
					native[d + 3] = bgra[s + 3];
			}
		}
		Marshal.Copy(native, 0, pb.Pixels, native.Length);

		string path = Path.Combine(Path.GetTempPath(), "gtk4wpf-" + Guid.NewGuid().ToString("N") + "." + type);
		try
		{
			pb.Savev(path, type, keys, values);
			return File.ReadAllBytes(path);
		}
		finally
		{
			try { File.Delete(path); } catch (IOException) { }
		}
	}
}
