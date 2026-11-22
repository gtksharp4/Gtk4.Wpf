namespace System.Windows.Media;

internal static partial class DrawingContextRenderer
{
	static Rect Expand(Rect current, Rect add) =>
		current.IsEmpty ? add : Rect.Union(current, add);

	static void RenderRect(Cairo.Context cr, Brush? brush, Pen? pen, Rect rect)
	{
		double w = rect.Width, h = rect.Height;
		cr.Save();
		cr.Rectangle(rect.X, rect.Y, w, h);
		if (brush != null)
		{
			brush.ApplyToCairo(cr, w, h);
			if (pen != null) cr.FillPreserve(); else cr.Fill();
		}
		if (pen?.Brush != null)
		{
			pen.ApplyToCairo(cr, w, h);
			cr.Stroke();
		}
		else if (brush == null)
			cr.NewPath();
		cr.Restore();
	}

	static void RenderEllipse(
		Cairo.Context cr, Brush? brush, Pen? pen,
		System.Windows.Point center, double rx, double ry)
	{
		double w = rx * 2, h = ry * 2;
		cr.Save();
		cr.Save();
		cr.Translate(center.X, center.Y);
		cr.Scale(rx, ry);
		cr.Arc(0, 0, 1, 0, 2 * Math.PI);
		cr.Restore();
		if (brush != null)
		{
			brush.ApplyToCairo(cr, w, h);
			if (pen != null) cr.FillPreserve(); else cr.Fill();
		}
		if (pen?.Brush != null)
		{
			pen.ApplyToCairo(cr, w, h);
			cr.Stroke();
		}
		else if (brush == null)
			cr.NewPath();
		cr.Restore();
	}

	static void RenderLine(
		Cairo.Context cr, Pen? pen, System.Windows.Point p0, System.Windows.Point p1)
	{
		if (pen?.Brush == null) return;
		cr.Save();
		cr.MoveTo(p0.X, p0.Y);
		cr.LineTo(p1.X, p1.Y);
		pen.ApplyToCairo(cr, Math.Abs(p1.X - p0.X), Math.Abs(p1.Y - p0.Y));
		cr.Stroke();
		cr.Restore();
	}

	static void RenderGeometry(Cairo.Context cr, Brush? brush, Pen? pen, Geometry geom)
	{
		var b = geom.Bounds;
		double w = b.Width, h = b.Height;
		cr.Save();
		geom.BuildPath(cr);
		if (brush != null)
		{
			brush.ApplyToCairo(cr, w, h);
			if (pen != null) cr.FillPreserve(); else cr.Fill();
		}
		if (pen?.Brush != null)
		{
			pen.ApplyToCairo(cr, w, h);
			cr.Stroke();
		}
		else if (brush == null)
			cr.NewPath();
		cr.Restore();
	}
}
