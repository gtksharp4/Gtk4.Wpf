namespace System.Windows.Media;

/// <summary>
/// The WPF TileBrush content-to-tile mapping, free of GTK. A content point <c>p</c> lands at
/// <c>Viewport.TopLeft + (p - Viewbox.TopLeft) * Scale + Offset</c> in brush space.
/// </summary>
readonly struct TileMapping
{
	/// <summary>The tile in brush space (absolute).</summary>
	internal readonly Rect Viewport;

	/// <summary>The content region shown in the tile (absolute content coordinates).</summary>
	internal readonly Rect Viewbox;

	internal readonly double ScaleX;
	internal readonly double ScaleY;

	/// <summary>Alignment offset of the scaled viewbox inside the viewport.</summary>
	internal readonly double OffsetX;
	internal readonly double OffsetY;

	/// <summary>False (the default value) for degenerate input: nothing is painted, never default black.</summary>
	internal readonly bool IsValid;

	TileMapping(Rect viewport, Rect viewbox, double sx, double sy, double ox, double oy)
	{
		Viewport = viewport;
		Viewbox = viewbox;
		ScaleX = sx;
		ScaleY = sy;
		OffsetX = ox;
		OffsetY = oy;
		IsValid = true;
	}

	/// <summary>Maps a content point into brush space.</summary>
	internal Point Map(Point p) => new Point(
		Viewport.X + (p.X - Viewbox.X) * ScaleX + OffsetX,
		Viewport.Y + (p.Y - Viewbox.Y) * ScaleY + OffsetY);

	static bool Bad(double v) => double.IsNaN(v) || double.IsInfinity(v);

	static bool Valid(Rect r) => !r.IsEmpty && !Bad(r.X) && !Bad(r.Y) && !Bad(r.Width) && !Bad(r.Height)
		&& r.Width > 0 && r.Height > 0;

	/// <summary>Computes the mapping, or the empty mapping when an input is degenerate.</summary>
	internal static TileMapping Compute(
		Rect content, Rect box, TileBrush brush)
	{
		var vb = brush.Viewbox;
		var vp = brush.Viewport;
		if (brush.ViewboxUnits == BrushMappingMode.RelativeToBoundingBox && Valid(content) && !vb.IsEmpty)
			vb = new Rect(content.X + vb.X * content.Width, content.Y + vb.Y * content.Height,
				vb.Width * content.Width, vb.Height * content.Height);
		if (brush.ViewportUnits == BrushMappingMode.RelativeToBoundingBox && Valid(box) && !vp.IsEmpty)
			vp = new Rect(box.X + vp.X * box.Width, box.Y + vp.Y * box.Height,
				vp.Width * box.Width, vp.Height * box.Height);
		if (!Valid(vb) || !Valid(vp) || !Valid(content) || !Valid(box))
			return default;

		double sx, sy;
		switch (brush.Stretch)
		{
			case Stretch.None: sx = sy = 1; break;
			case Stretch.Uniform: sx = sy = Math.Min(vp.Width / vb.Width, vp.Height / vb.Height); break;
			case Stretch.UniformToFill: sx = sy = Math.Max(vp.Width / vb.Width, vp.Height / vb.Height); break;
			default: sx = vp.Width / vb.Width; sy = vp.Height / vb.Height; break;
		}
		double free = vp.Width - vb.Width * sx;
		double freeY = vp.Height - vb.Height * sy;
		double ox = brush.AlignmentX == AlignmentX.Left ? 0 : brush.AlignmentX == AlignmentX.Right ? free : free / 2;
		double oy = brush.AlignmentY == AlignmentY.Top ? 0 : brush.AlignmentY == AlignmentY.Bottom ? freeY : freeY / 2;
		return new TileMapping(vp, vb, sx, sy, ox, oy);
	}

}
