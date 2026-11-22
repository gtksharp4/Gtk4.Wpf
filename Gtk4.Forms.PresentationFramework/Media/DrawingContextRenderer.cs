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
internal static partial class DrawingContextRenderer
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
}
