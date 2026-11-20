using System.IO;
using System.Windows.Media.Imaging;

namespace System.Windows.Media;

/// <summary>
/// A brush that fills an area with an image, with optional stretching and tiling.
/// Mirrors <c>System.Windows.Media.ImageBrush</c>.
/// <para>
/// <b>What paints:</b> when <see cref="ImageSource"/> is an <see cref="ImageSource"/>
/// whose <see cref="ImageSource.FilePath"/> points to an accessible PNG file, the image
/// is rendered via <c>cairo_image_surface_create_from_png</c>, scaled according to
/// <see cref="TileBrush.Stretch"/>, and tiled if <see cref="TileBrush.TileMode"/> is not
/// <see cref="TileMode.None"/>. All four <see cref="Stretch"/> values are honoured.
/// </para>
/// <para>
/// <b>Object model only (no paint):</b> non-PNG sources, missing files, and
/// <see cref="BitmapImage"/> with a URI that is not a local file path are accepted without
/// error but produce no visible output — the fill area is left unpainted.
/// </para>
/// <para>
/// <b>Headless / no display:</b> constructing an <see cref="ImageBrush"/> and assigning it
/// to a <see cref="System.Windows.Shapes.Shape.Fill"/> is safe without a GTK surface;
/// <see cref="ApplyToCairo"/> is never called until an actual draw pass occurs.
/// </para>
/// </summary>
public class ImageBrush : TileBrush
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
		if (path == null) return null;
		if (!File.Exists(path)) return null;

		try
		{
			// Cairo can only load PNG natively.
			return new Cairo.ImageSurface(path);
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
