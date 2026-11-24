using System;

namespace System.Windows.Media;

// Pen-inflated and transformed bounds. Cairo stroke extents are exact for lines, curves and
// miter/bevel/round joins; Cairo has one cap for both ends, so the larger of start/end is used.
public abstract partial class Geometry
{
	/// <summary>
	/// Bounds of the area covered by stroking this geometry with <paramref name="pen"/>, including
	/// <see cref="Transform"/>. A null pen, or one with no thickness, gives the fill bounds. The dash
	/// array is ignored, as in WPF.
	/// </summary>
	public System.Windows.Rect GetRenderBounds(Pen? pen)
	{
		var bounds = GetTransformedBounds();
		if (pen == null || !(pen.Thickness > 0) || double.IsInfinity(pen.Thickness)) return bounds;
		double half = pen.Thickness / 2;
		var inflated = new System.Windows.Rect(
			bounds.X - half, bounds.Y - half, bounds.Width + pen.Thickness, bounds.Height + pen.Thickness);
		try
		{
			using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 1, 1);
			using var cr = new Cairo.Context(surface);
			BuildTransformedPath(cr);
			cr.LineWidth = pen.Thickness;
			cr.LineCap = CairoCap(pen);
			cr.LineJoin = CairoJoin(pen.LineJoin);
			cr.MiterLimit = Math.Max(1, pen.MiterLimit);
			var e = cr.StrokeExtents();
			// A lone point has no stroke; fall back to its bounds grown by the half thickness.
			return e.Width <= 0 && e.Height <= 0 ? inflated : new System.Windows.Rect(e.X, e.Y, e.Width, e.Height);
		}
		catch (DllNotFoundException)
		{
			return inflated;
		}
	}

	/// <summary><see cref="Bounds"/> with <see cref="Transform"/> applied (WPF's <c>Geometry.Bounds</c>).</summary>
	internal System.Windows.Rect GetTransformedBounds()
	{
		var bounds = Bounds;
		var matrix = Transform.Value;
		if (matrix.IsIdentity) return bounds;
		try
		{
			using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 1, 1);
			using var cr = new Cairo.Context(surface);
			BuildTransformedPath(cr);
			var e = cr.PathExtents();
			if (e.Width > 0 || e.Height > 0) return new System.Windows.Rect(e.X, e.Y, e.Width, e.Height);
		}
		catch (DllNotFoundException)
		{
		}
		return TransformCorners(bounds, matrix);
	}

	static System.Windows.Rect TransformCorners(System.Windows.Rect r, Matrix m)
	{
		var a = m.Transform(new System.Windows.Point(r.Left, r.Top));
		var b = m.Transform(new System.Windows.Point(r.Right, r.Top));
		var c = m.Transform(new System.Windows.Point(r.Left, r.Bottom));
		var d = m.Transform(new System.Windows.Point(r.Right, r.Bottom));
		double x0 = Math.Min(Math.Min(a.X, b.X), Math.Min(c.X, d.X));
		double x1 = Math.Max(Math.Max(a.X, b.X), Math.Max(c.X, d.X));
		double y0 = Math.Min(Math.Min(a.Y, b.Y), Math.Min(c.Y, d.Y));
		double y1 = Math.Max(Math.Max(a.Y, b.Y), Math.Max(c.Y, d.Y));
		return new System.Windows.Rect(x0, y0, x1 - x0, y1 - y0);
	}

	/// <summary>Builds the path with <see cref="Transform"/> applied to its coordinates.</summary>
	internal void BuildTransformedPath(Cairo.Context cr)
	{
		var m = Transform.Value;
		if (m.IsIdentity)
		{
			BuildPath(cr);
			return;
		}
		cr.Save();
		cr.Transform(new Cairo.Matrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY));
		BuildPath(cr);
		cr.Restore();
	}

	static Cairo.LineCap CairoCap(Pen pen)
	{
		static int Rank(PenLineCap c) => c switch
		{
			PenLineCap.Square => 3, PenLineCap.Round => 2, _ => 0
		};
		var cap = Rank(pen.StartLineCap) >= Rank(pen.EndLineCap) ? pen.StartLineCap : pen.EndLineCap;
		return Shapes.StrokeStyle.ToCairoLineCap(cap);
	}

	static Cairo.LineJoin CairoJoin(PenLineJoin join) => Shapes.StrokeStyle.ToCairoLineJoin(join);
}
