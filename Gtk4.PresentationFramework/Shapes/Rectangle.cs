using System;
using System.Windows.Media;

namespace System.Windows.Shapes;

/// <summary>
/// Draws a rectangle, optionally with rounded corners.
/// Mirrors <c>System.Windows.Shapes.Rectangle</c>.
/// Backed by a <see cref="Gtk.DrawingArea"/> with Cairo rendering.
/// </summary>
public class Rectangle : Shape
{
	static Rectangle() =>
		StretchProperty.OverrideMetadata(typeof(Rectangle), new FrameworkPropertyMetadata(Stretch.Fill));

	internal override bool UsesStretchMatrix => false;

	/// <inheritdoc/>
	protected override Size MeasureOverride(Size constraint) => MeasureRectLike(constraint);

	/// <inheritdoc/>
	protected override Size ArrangeOverride(Size finalSize) => finalSize;

	// ── RadiusX DP ───────────────────────────────────────────────────────────

	/// <summary>Horizontal corner radius.</summary>
	public static readonly DependencyProperty RadiusXProperty =
		DependencyProperty.Register(nameof(RadiusX), typeof(double), typeof(Rectangle),
			new PropertyMetadata(0.0, (d, _) => ((Rectangle)d).QueueRedraw()));

	/// <summary>Gets or sets the horizontal corner radius.</summary>
	public double RadiusX
	{
		get => (double)GetValue(RadiusXProperty)!;
		set => SetValue(RadiusXProperty, value);
	}

	// ── RadiusY DP ───────────────────────────────────────────────────────────

	/// <summary>Vertical corner radius.</summary>
	public static readonly DependencyProperty RadiusYProperty =
		DependencyProperty.Register(nameof(RadiusY), typeof(double), typeof(Rectangle),
			new PropertyMetadata(0.0, (d, _) => ((Rectangle)d).QueueRedraw()));

	/// <summary>Gets or sets the vertical corner radius.</summary>
	public double RadiusY
	{
		get => (double)GetValue(RadiusYProperty)!;
		set => SetValue(RadiusYProperty, value);
	}

	/// <inheritdoc/>
	protected override void BuildShapePath(Cairo.Context cr, int width, int height)
	{
		var r = GetRectLikeRect(width, height);
		if (Stretch == Stretch.None || r.Width <= 0 || r.Height <= 0) return;
		double rx = Math.Min(RadiusX, r.Width / 2.0);
		double ry = Math.Min(RadiusY, r.Height / 2.0);

		if (rx <= 0 || ry <= 0)
		{
			cr.Rectangle(r.X, r.Y, r.Width, r.Height);
			return;
		}

		BuildRoundedRectPath(cr, r.X, r.Y, r.Width, r.Height, rx, ry);
	}

	/// <summary>
	/// Builds an elliptically-cornered rectangle path into <paramref name="cr"/>.
	/// Each corner arc is drawn in a saved context scaled so the unit arc spans
	/// <paramref name="rx"/> horizontally and <paramref name="ry"/> vertically.
	/// Coverage limit: the pixel render is not unit-tested; the pure helper
	/// <see cref="CornerArcCentre"/> is tested instead.
	/// </summary>
	internal static void BuildRoundedRectPath(
		Cairo.Context cr, double x, double y, double w, double h, double rx, double ry)
	{
		// straight top edge
		cr.MoveTo(x + rx, y);
		cr.LineTo(x + w - rx, y);
		// top-right corner
		EllipticArc(cr, x + w - rx, y + ry, rx, ry, -Math.PI / 2, 0);
		// straight right edge
		cr.LineTo(x + w, y + h - ry);
		// bottom-right corner
		EllipticArc(cr, x + w - rx, y + h - ry, rx, ry, 0, Math.PI / 2);
		// straight bottom edge
		cr.LineTo(x + rx, y + h);
		// bottom-left corner
		EllipticArc(cr, x + rx, y + h - ry, rx, ry, Math.PI / 2, Math.PI);
		// straight left edge
		cr.LineTo(x, y + ry);
		// top-left corner
		EllipticArc(cr, x + rx, y + ry, rx, ry, Math.PI, Math.PI * 3 / 2);
		cr.ClosePath();
	}

	/// <summary>
	/// Appends an elliptical arc centred at (<paramref name="cx"/>, <paramref name="cy"/>)
	/// with horizontal half-axis <paramref name="rx"/> and vertical half-axis
	/// <paramref name="ry"/>, sweeping from <paramref name="angle1"/> to
	/// <paramref name="angle2"/> in the unit-circle parameterisation.
	/// The context is saved and restored so the scale does not leak.
	/// </summary>
	static void EllipticArc(
		Cairo.Context cr,
		double cx, double cy,
		double rx, double ry,
		double angle1, double angle2)
	{
		cr.Save();
		cr.Translate(cx, cy);
		cr.Scale(rx, ry);
		cr.Arc(0, 0, 1, angle1, angle2);
		cr.Restore();
	}

	/// <summary>
	/// Returns the arc centre for a given corner of a rounded rectangle, used by
	/// tests to verify that horizontal extent uses <paramref name="rx"/> and
	/// vertical extent uses <paramref name="ry"/>.
	/// <paramref name="corner"/>: 0=top-right, 1=bottom-right, 2=bottom-left, 3=top-left.
	/// </summary>
	internal static (double cx, double cy) CornerArcCentre(
		double x, double y, double w, double h, double rx, double ry, int corner) =>
		corner switch
		{
			0 => (x + w - rx, y + ry),
			1 => (x + w - rx, y + h - ry),
			2 => (x + rx,     y + h - ry),
			3 => (x + rx,     y + ry),
			_ => throw new ArgumentOutOfRangeException(nameof(corner))
		};
}