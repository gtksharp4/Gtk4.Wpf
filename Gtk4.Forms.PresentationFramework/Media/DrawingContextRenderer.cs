namespace System.Windows.Media;

/// <summary>
/// Renders the commands recorded by a <see cref="DrawingContext"/> onto a live
/// <see cref="Cairo.Context"/>.  Used by <see cref="VisualBrush"/> to paint a
/// <see cref="DrawingVisual"/> into an off-screen surface.
/// </summary>
/// <remarks>
/// Only the primitive kinds defined by <see cref="DrawingContext.DrawCommandKind"/>
/// are rendered. <c>DrawText</c> is opt-in (<c>paintText</c>), painted with Cairo toy text, because page renderers
/// paint text themselves with Pango and a default pass would draw every line twice.
/// Each command saves/restores Cairo state around its draw.
/// </remarks>
internal static class DrawingContextRenderer
{
	/// <summary>
	/// Replays all commands in <paramref name="ctx"/> onto <paramref name="cr"/>.
	/// Returns the tight bounding rectangle of the rendered content, or
	/// <see cref="Rect.Empty"/> when there are no commands with geometry.
	/// </summary>
	internal static Rect Render(DrawingContext ctx, Cairo.Context cr, bool paintText = false)
	{
		Rect bounds = Rect.Empty;

		foreach (var cmd in ctx.Commands)
		{
			switch (cmd.Kind)
			{
				case DrawingContext.DrawCommandKind.DrawRectangle:
					if (cmd.Arg is Rect rect)
					{
						bounds = Expand(bounds, rect);
						RenderRect(cr, cmd.Brush, cmd.Pen, rect);
					}
					break;

				case DrawingContext.DrawCommandKind.DrawEllipse:
					if (cmd.Arg is ValueTuple<System.Windows.Point, double, double> ellipse)
					{
						var (center, rx, ry) = ellipse;
						var eb = new Rect(center.X - rx, center.Y - ry, rx * 2, ry * 2);
						bounds = Expand(bounds, eb);
						RenderEllipse(cr, cmd.Brush, cmd.Pen, center, rx, ry);
					}
					break;

				case DrawingContext.DrawCommandKind.DrawLine:
					if (cmd.Arg is ValueTuple<System.Windows.Point, System.Windows.Point> line)
					{
						var (p0, p1) = line;
						var lb = Rect.Union(new Rect(p0, new Size(0, 0)), new Rect(p1, new Size(0, 0)));
						bounds = Expand(bounds, lb);
						RenderLine(cr, cmd.Pen, p0, p1);
					}
					break;

				case DrawingContext.DrawCommandKind.DrawGeometry:
					if (cmd.Arg is Geometry geom)
					{
						var gb = geom.Bounds;
						if (!gb.IsEmpty) bounds = Expand(bounds, gb);
						RenderGeometry(cr, cmd.Brush, cmd.Pen, geom);
					}
					break;

				case DrawingContext.DrawCommandKind.DrawText:
					if (paintText && cmd.Arg is ValueTuple<FormattedText, System.Windows.Point> text)
						FormattedTextMetrics.Paint(cr, text.Item1, text.Item2);
					break;

				default:
					break;
			}
		}

		return bounds;
	}

	/// <summary>
	/// Computes the tight bounds of all commands without painting.
	/// Used to size the off-screen surface before rendering.
	/// </summary>
	internal static Rect ComputeBounds(DrawingContext ctx, bool includeText = false)
	{
		Rect bounds = Rect.Empty;
		foreach (var cmd in ctx.Commands)
		{
			switch (cmd.Kind)
			{
				case DrawingContext.DrawCommandKind.DrawRectangle:
					if (cmd.Arg is Rect r) bounds = Expand(bounds, r);
					break;
				case DrawingContext.DrawCommandKind.DrawEllipse:
					if (cmd.Arg is ValueTuple<System.Windows.Point, double, double> ellipse)
					{
						var (c, rx, ry) = ellipse;
						bounds = Expand(bounds, new Rect(c.X - rx, c.Y - ry, rx * 2, ry * 2));
					}
					break;
				case DrawingContext.DrawCommandKind.DrawLine:
					if (cmd.Arg is ValueTuple<System.Windows.Point, System.Windows.Point> line)
					{
						var (p0, p1) = line;
						bounds = Expand(bounds, Rect.Union(new Rect(p0, new Size(0, 0)), new Rect(p1, new Size(0, 0))));
					}
					break;
				case DrawingContext.DrawCommandKind.DrawGeometry:
					if (cmd.Arg is Geometry g && !g.Bounds.IsEmpty)
						bounds = Expand(bounds, g.Bounds);
					break;
				case DrawingContext.DrawCommandKind.DrawText:
					if (includeText && cmd.Arg is ValueTuple<FormattedText, System.Windows.Point> tx)
					{
						var sz = FormattedTextMetrics.Measure(tx.Item1);
						bounds = Expand(bounds, new Rect(tx.Item2, sz));
					}
					break;
			}
		}
		return bounds;
	}

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
