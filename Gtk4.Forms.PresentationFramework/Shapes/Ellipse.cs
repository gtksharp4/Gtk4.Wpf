using System;

namespace System.Windows.Shapes;

/// <summary>
/// Draws an ellipse that fills its allocated bounds.
/// Mirrors <c>System.Windows.Shapes.Ellipse</c>.
/// Backed by a <see cref="Gtk.DrawingArea"/> with Cairo rendering.
/// </summary>
public class Ellipse : Shape
{
	/// <inheritdoc/>
	protected override void BuildShapePath(Cairo.Context cr, int width, int height)
	{
		if (width <= 0 || height <= 0) return;
		double cx = width / 2.0, cy = height / 2.0;
		double rx = width / 2.0, ry = height / 2.0;
		cr.Save();
		cr.Translate(cx, cy);
		cr.Scale(rx, ry);
		cr.Arc(0, 0, 1, 0, 2 * Math.PI);
		cr.Restore();
	}
}