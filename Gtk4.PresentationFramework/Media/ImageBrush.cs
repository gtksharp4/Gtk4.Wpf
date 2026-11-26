using System.IO;
using System.Windows.Media.Imaging;

namespace System.Windows.Media;

/// <summary>
/// A brush that fills an area with an image, with optional stretching and tiling.
/// Mirrors <c>System.Windows.Media.ImageBrush</c>.
/// <para>
/// <b>What paints:</b> a local PNG file is loaded via <c>cairo_image_surface_create_from_png</c>;
/// any other <see cref="BitmapSource"/> (decoder frames, <c>BitmapSource.Create</c>, a <see cref="BitmapImage"/>
/// of any supported codec) is painted from its decoded pixels. The image is scaled according to
/// <see cref="TileBrush.Stretch"/>, and tiled if <see cref="TileBrush.TileMode"/> is not
/// <see cref="TileMode.None"/>. All four <see cref="Stretch"/> values are honoured.
/// </para>
/// <para>
/// <b>Object model only (no paint):</b> missing files and non-bitmap sources that are not a PNG are
/// accepted without error but produce no visible output — the fill area is left unpainted.
/// </para>
/// <para>
/// <b>Headless / no display:</b> constructing an <see cref="ImageBrush"/> and assigning it
/// to a <see cref="System.Windows.Shapes.Shape.Fill"/> is safe without a GTK surface;
/// <see cref="ApplyToCairo"/> is never called until an actual draw pass occurs.
/// </para>
/// </summary>
public partial class ImageBrush : TileBrush
{
	/// <summary>Creates an empty image brush.</summary>
	public ImageBrush() { }

	/// <summary>Creates an image brush with the given source.</summary>
	public ImageBrush(ImageSource imageSource) => ImageSource = imageSource;

	/// <summary>The image to paint with.</summary>
	public ImageSource? ImageSource { get; set; }

	/// <inheritdoc/>
	internal override void ApplyToCairo(Cairo.Context cr, double width, double height)
	{
		var surface = TryLoadSurface();
		if (surface == null) return;

		using (surface)
		{
			int sw = surface.Width;
			int sh = surface.Height;
			if (sw <= 0 || sh <= 0) return;

			var (dx, dy, dw, dh) = StretchMath.ComputeDestRect(sw, sh, width, height, Stretch);

			using var pat = new Cairo.SurfacePattern(surface);

			// Scale the pattern so the image fits into the computed destination rectangle.
			double scaleX = dw / sw;
			double scaleY = dh / sh;
			var m = new Cairo.Matrix();
			m.Scale(1.0 / scaleX, 1.0 / scaleY);
			m.Translate(-dx / scaleX, -dy / scaleY);
			pat.Matrix = m;

			pat.Extend = TileMode == TileMode.None ? Cairo.Extend.Pad : Cairo.Extend.Repeat;
			pat.Filter = Cairo.Filter.Bilinear;

			cr.SetSource(pat);
		}
	}

	/// <inheritdoc/>
	public override Gdk.RGBA ToGdkRgba() =>
		new Gdk.RGBA { Red = 0, Green = 0, Blue = 0, Alpha = (float)Opacity };

	// ── helpers ──────────────────────────────────────────────────────────────

	Cairo.ImageSurface? TryLoadSurface()
	{
		var path = ResolvePath();
		if (path != null && !(ImageSource is BitmapImage { HasDecodeOptions: true }) && File.Exists(path)
			&& IsPng(path))
		{
			try
			{
				// Fast path: cairo loads PNG natively.
				return new Cairo.ImageSurface(path);
			}
			catch
			{
			}
		}
		return ImageSource is BitmapSource bitmap ? SurfaceFromPixels(bitmap) : null;
	}

	static bool IsPng(string path)
	{
		try
		{
			using var s = File.OpenRead(path);
			var head = new byte[4];
			return s.Read(head, 0, 4) == 4 && head[0] == 0x89 && head[1] == 0x50 && head[2] == 0x4E && head[3] == 0x47;
		}
		catch
		{
			return false;
		}
	}

	/// <summary>Any decodable bitmap: straight BGRA32 pixels premultiplied into a Cairo Argb32 surface.</summary>
	static Cairo.ImageSurface? SurfaceFromPixels(BitmapSource bitmap)
	{
		try
		{
			int w = bitmap.PixelWidth, h = bitmap.PixelHeight;
			if (w <= 0 || h <= 0)
				return null;
			byte[] bgra = bitmap.ToBgra32();
			var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
			surface.Flush();
			var row = new byte[w * 4];
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w * 4; x += 4)
				{
					int a = bgra[y * w * 4 + x + 3];
					row[x] = (byte)((bgra[y * w * 4 + x] * a + 127) / 255);
					row[x + 1] = (byte)((bgra[y * w * 4 + x + 1] * a + 127) / 255);
					row[x + 2] = (byte)((bgra[y * w * 4 + x + 2] * a + 127) / 255);
					row[x + 3] = (byte)a;
				}
				System.Runtime.InteropServices.Marshal.Copy(row, 0, surface.DataPtr + y * surface.Stride, row.Length);
			}
			surface.MarkDirty();
			return surface;
		}
		catch
		{
			return null;
		}
	}

	internal string? ResolvePath()
	{
		var src = ImageSource;
		if (src == null) return null;

		string path = ImageSourceLocation.ResolveFilePath(src);
		return string.IsNullOrEmpty(path) ? null : path;
	}
}
