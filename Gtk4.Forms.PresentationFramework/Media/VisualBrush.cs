namespace System.Windows.Media;

/// <summary>
/// A brush that fills an area with the content of a <see cref="Visual"/>,
/// with optional stretching and tiling (inherited from <see cref="TileBrush"/>).
/// Mirrors <c>System.Windows.Media.VisualBrush</c>.
/// </summary>
/// <remarks>
/// <b>What paints — <see cref="DrawingVisual"/>:</b> when <see cref="Visual"/> is a
/// <see cref="DrawingVisual"/> with a committed <see cref="DrawingVisual.Drawing"/>,
/// the recorded commands are replayed via <see cref="DrawingContextRenderer"/> into an
/// off-screen <see cref="Cairo.ImageSurface"/>, then scaled/tiled according to the
/// inherited <see cref="TileBrush.Stretch"/> and <see cref="TileBrush.TileMode"/> via
/// <see cref="StretchMath"/>.
/// <para>
/// <b>No-paint — general <see cref="Visual"/>:</b> for any <see cref="Visual"/> that is
/// not a <see cref="DrawingVisual"/> (e.g. a live <see cref="UIElement"/>), GTK cannot
/// snapshot an unrealized or foreign widget into a Cairo surface portably.  In this case
/// <see cref="ApplyToCairo"/> is a transparent no-op; the object model is fully usable.
/// </para>
/// <para>
/// <b>Headless / no display:</b> constructing a <see cref="VisualBrush"/> and assigning
/// it to a shape fill is safe without a GTK surface; <see cref="ApplyToCairo"/> is never
/// called until an actual draw pass occurs.  A null <see cref="Visual"/>, a
/// <see cref="DrawingVisual"/> with no committed drawing, or zero-size bounds all produce
/// a no-op paint.
/// </para>
/// </remarks>
public partial class VisualBrush : TileBrush
{
	/// <summary>Creates an empty visual brush.</summary>
	public VisualBrush() { }

	/// <summary>Creates a visual brush backed by <paramref name="visual"/>.</summary>
	public VisualBrush(Visual visual) => Visual = visual;

	/// <summary>
	/// The visual used as the brush content.
	/// Only <see cref="DrawingVisual"/> instances with a committed drawing produce
	/// painted output; all other visual types result in a transparent fill.
	/// </summary>
	public Visual? Visual { get; set; }

	/// <inheritdoc/>
	internal override void ApplyToCairo(Cairo.Context cr, double width, double height)
	{
		if (Visual is not DrawingVisual dv) return;  // general Visual: no-paint (see remarks)

		var ctx = dv.Drawing;
		if (ctx == null || ctx.Commands.Count == 0) return;

		var bounds = DrawingContextRenderer.ComputeBounds(ctx);
		double sw = bounds.Width;
		double sh = bounds.Height;
		if (sw <= 0 || sh <= 0) return;
		if (width <= 0 || height <= 0) return;

		int iw = (int)Math.Ceiling(sw);
		int ih = (int)Math.Ceiling(sh);
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, iw, ih);
		using (var drawCr = new Cairo.Context(surface))
		{
			drawCr.Translate(-bounds.X, -bounds.Y);
			DrawingContextRenderer.Render(ctx, drawCr, paintText: true);
		}

		var (dx, dy, dw, dh) = StretchMath.ComputeDestRect(sw, sh, width, height, Stretch);

		using var pat = new Cairo.SurfacePattern(surface);
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

	/// <inheritdoc/>
	public override Gdk.RGBA ToGdkRgba() =>
		new Gdk.RGBA { Red = 0, Green = 0, Blue = 0, Alpha = (float)Opacity };
}
