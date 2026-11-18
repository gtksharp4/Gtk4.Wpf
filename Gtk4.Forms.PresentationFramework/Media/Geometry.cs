using System;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// Abstract base for all WPF geometry types.
/// Mirrors <c>System.Windows.Media.Geometry</c>.
/// </summary>
public abstract class Geometry
{
	/// <summary>Optional transform applied to the geometry.</summary>
	public Transform? Transform { get; set; }

	/// <summary>
	/// The axis-aligned bounding box of this geometry (untransformed).
	/// Subclasses return their tight bounds.
	/// </summary>
	public abstract System.Windows.Rect Bounds { get; }

	/// <summary>Builds the Cairo path on <paramref name="cr"/>.</summary>
	internal abstract void BuildPath(Cairo.Context cr);

	/// <summary>An empty geometry with zero bounds.</summary>
	public static Geometry Empty { get; } = new RectangleGeometry(new System.Windows.Rect(0, 0, 0, 0));

	/// <summary>
	/// Parses the WPF Path Markup (mini-language) string and returns a <see cref="PathGeometry"/>.
	/// <para>
	/// Supported commands (absolute uppercase / relative lowercase):
	/// <list type="bullet">
	/// <item><c>M/m</c> — move-to (starts a new figure)</item>
	/// <item><c>L/l</c> — line-to</item>
	/// <item><c>H/h</c> — horizontal line-to</item>
	/// <item><c>V/v</c> — vertical line-to</item>
	/// <item><c>C/c</c> — cubic Bézier (6 params)</item>
	/// <item><c>Q/q</c> — quadratic Bézier (4 params)</item>
	/// <item><c>Z/z</c> — close figure</item>
	/// </list>
	/// Best-effort / unsupported: <c>A/a</c> (arc), <c>S/s</c> (smooth cubic), <c>T/t</c> (smooth quadratic).
	/// </para>
	/// </summary>
	/// <exception cref="FormatException">The string contains unrecognised commands or malformed numbers.</exception>
	public static Geometry Parse(string source) => PathMarkupParser.Parse(source);
}

/// <summary>
/// A rectangular geometry, optionally with rounded corners.
/// Mirrors <c>System.Windows.Media.RectangleGeometry</c>.
/// </summary>
public class RectangleGeometry : Geometry
{
	/// <summary>Creates an empty rectangle geometry.</summary>
	public RectangleGeometry() { }

	/// <summary>Creates a rectangle geometry from a <see cref="System.Windows.Rect"/>.</summary>
	public RectangleGeometry(System.Windows.Rect rect) => Rect = rect;

	/// <summary>Creates a rectangle geometry with rounded corners.</summary>
	public RectangleGeometry(System.Windows.Rect rect, double radiusX, double radiusY)
	{
		Rect = rect; RadiusX = radiusX; RadiusY = radiusY;
	}

	/// <summary>The rectangle.</summary>
	public System.Windows.Rect Rect { get; set; }
	/// <summary>Horizontal corner radius.</summary>
	public double RadiusX { get; set; }
	/// <summary>Vertical corner radius.</summary>
	public double RadiusY { get; set; }

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds => Rect;

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		double x = Rect.X, y = Rect.Y, w = Rect.Width, h = Rect.Height;
		double rx = Math.Min(RadiusX, w / 2), ry = Math.Min(RadiusY, h / 2);
		if (rx <= 0 || ry <= 0)
		{
			cr.Rectangle(x, y, w, h);
			return;
		}
		// Rounded rectangle: use the same elliptic-arc helper as Rectangle.BuildRoundedRectPath
		// so each corner uses rx horizontally and ry vertically.
		System.Windows.Shapes.Rectangle.BuildRoundedRectPath(cr, x, y, w, h, rx, ry);
	}
}

/// <summary>
/// An ellipse (or circle) geometry.
/// Mirrors <c>System.Windows.Media.EllipseGeometry</c>.
/// </summary>
public class EllipseGeometry : Geometry
{
	/// <summary>Creates a default ellipse.</summary>
	public EllipseGeometry() { }

	/// <summary>Creates an ellipse centred at <paramref name="center"/> with the given radii.</summary>
	public EllipseGeometry(System.Windows.Point center, double radiusX, double radiusY)
	{
		Center = center; RadiusX = radiusX; RadiusY = radiusY;
	}

	/// <summary>Centre point of the ellipse.</summary>
	public System.Windows.Point Center { get; set; }
	/// <summary>Horizontal radius.</summary>
	public double RadiusX { get; set; }
	/// <summary>Vertical radius.</summary>
	public double RadiusY { get; set; }

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds => new System.Windows.Rect(
		Center.X - RadiusX, Center.Y - RadiusY,
		RadiusX * 2, RadiusY * 2);

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		cr.Save();
		cr.Translate(Center.X, Center.Y);
		if (RadiusX > 0 && RadiusY > 0)
			cr.Scale(RadiusX, RadiusY);
		cr.Arc(0, 0, 1, 0, 2 * Math.PI);
		cr.Restore();
	}
}

/// <summary>
/// A line segment geometry from one point to another.
/// Mirrors <c>System.Windows.Media.LineGeometry</c>.
/// </summary>
public class LineGeometry : Geometry
{
	/// <summary>Creates a default line.</summary>
	public LineGeometry() { }

	/// <summary>Creates a line from <paramref name="startPoint"/> to <paramref name="endPoint"/>.</summary>
	public LineGeometry(System.Windows.Point startPoint, System.Windows.Point endPoint)
	{
		StartPoint = startPoint; EndPoint = endPoint;
	}

	/// <summary>Start point.</summary>
	public System.Windows.Point StartPoint { get; set; }
	/// <summary>End point.</summary>
	public System.Windows.Point EndPoint { get; set; }

	/// <inheritdoc/>
	public override System.Windows.Rect Bounds
	{
		get
		{
			double x = Math.Min(StartPoint.X, EndPoint.X);
			double y = Math.Min(StartPoint.Y, EndPoint.Y);
			double w = Math.Abs(EndPoint.X - StartPoint.X);
			double h = Math.Abs(EndPoint.Y - StartPoint.Y);
			return new System.Windows.Rect(x, y, w, h);
		}
	}

	/// <inheritdoc/>
	internal override void BuildPath(Cairo.Context cr)
	{
		cr.MoveTo(StartPoint.X, StartPoint.Y);
		cr.LineTo(EndPoint.X, EndPoint.Y);
	}
}