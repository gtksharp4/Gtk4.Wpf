using System;
using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>
/// Draws an ellipse that fills its allocated bounds.
/// Mirrors <c>System.Windows.Shapes.Ellipse</c>.
/// Backed by a <see cref="Gtk.DrawingArea"/> with Cairo rendering.
/// </summary>
public class Ellipse : Shape
{
	static Ellipse() =>
		StretchProperty.OverrideMetadata(typeof(Ellipse), new FrameworkPropertyMetadata(Stretch.Fill));

	internal override bool UsesStretchMatrix => false;

	/// <inheritdoc/>
	protected override Size MeasureOverride(Size constraint) => MeasureRectLike(constraint);

	/// <inheritdoc/>
	protected override Size ArrangeOverride(Size finalSize) => finalSize;

	/// <inheritdoc/>
	protected override void BuildShapePath(Cairo.Context cr, int width, int height)
	{
		var r = GetRectLikeRect(width, height);
		if (Stretch == Stretch.None || r.Width <= 0 || r.Height <= 0) return;
		cr.Save();
		cr.Translate(r.X + r.Width / 2, r.Y + r.Height / 2);
		cr.Scale(r.Width / 2, r.Height / 2);
		cr.NewSubPath();
		cr.Arc(0, 0, 1, 0, 2 * Math.PI);
		cr.Restore();
	}
}
