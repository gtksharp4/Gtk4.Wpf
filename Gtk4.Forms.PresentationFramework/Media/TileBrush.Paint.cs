namespace System.Windows.Media;

public abstract partial class TileBrush
{
	const double MaxTilePixels = 4096.0 * 4096.0;

	/// <summary>Sets an explicit transparent source, so a degenerate brush never paints Cairo's default black.</summary>
	internal static void SetTransparentSource(Cairo.Context cr) => cr.SetSourceRGBA(0, 0, 0, 0);

	/// <summary>
	/// Sets <paramref name="cr"/>'s source to this brush's tile pattern for a fill area of
	/// <paramref name="width"/> x <paramref name="height"/>. <paramref name="draw"/> paints the content in its own
	/// coordinates; <paramref name="content"/> are its bounds.
	/// </summary>
	internal void ApplyTile(Cairo.Context cr, double width, double height, Rect content, Action<Cairo.Context> draw)
	{
		var map = TileMapping.Compute(content, new Rect(0, 0, width, height), this);
		if (!map.IsValid)
		{
			SetTransparentSource(cr);
			return;
		}

		int cols = TileMode == TileMode.FlipX || TileMode == TileMode.FlipXY ? 2 : 1;
		int rows = TileMode == TileMode.FlipY || TileMode == TileMode.FlipXY ? 2 : 1;
		double cellW = map.Viewport.Width * cols;
		double cellH = map.Viewport.Height * rows;
		var cellToUser = Matrix.Identity;
		cellToUser.Translate(map.Viewport.X, map.Viewport.Y);
		cellToUser.Append(RelativeToAbsolute(new Rect(0, 0, width, height)));
		cellToUser.Append(Transform?.Value ?? Matrix.Identity);
		// Raster at device resolution: the cell's own axes (after the brush transforms), carried through the CTM.
		double ax = cellW * cellToUser.M11, ay = cellW * cellToUser.M12;
		double bx = cellH * cellToUser.M21, by = cellH * cellToUser.M22;
		cr.UserToDeviceDistance(ref ax, ref ay);
		cr.UserToDeviceDistance(ref bx, ref by);
		double pw = Math.Max(1, Math.Sqrt(ax * ax + ay * ay)), ph = Math.Max(1, Math.Sqrt(bx * bx + by * by));
		double shrink = Math.Min(1, Math.Sqrt(MaxTilePixels / (pw * ph)));
		int iw = Math.Max(1, (int)Math.Ceiling(pw * shrink));
		int ih = Math.Max(1, (int)Math.Ceiling(ph * shrink));

		var matrix = Matrix.Identity;
		matrix.Scale(cellW / iw, cellH / ih);
		matrix.Append(cellToUser);
		if (!matrix.HasInverse)
		{
			SetTransparentSource(cr);
			return;
		}
		matrix.Invert();

		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, iw, ih);
		using (var tc = new Cairo.Context(surface))
			PaintTiles(tc, map, cols, rows, iw / cellW, ih / cellH, draw);

		using var pattern = new Cairo.SurfacePattern(surface);
		pattern.Matrix = matrix.ToCairo();
		pattern.Extend = TileMode == TileMode.None ? Cairo.Extend.None : Cairo.Extend.Repeat;
		pattern.Filter = Cairo.Filter.Bilinear;
		cr.SetSource(pattern);
	}

	/// <summary>RelativeTransform lifted from the bounding-box unit space into absolute space.</summary>
	Matrix RelativeToAbsolute(Rect box)
	{
		var relative = RelativeTransform?.Value ?? Matrix.Identity;
		if (relative.IsIdentity)
			return Matrix.Identity;
		var m = Matrix.Identity;
		m.Translate(-box.X, -box.Y);
		m.Scale(1 / box.Width, 1 / box.Height);
		m.Append(relative);
		m.Scale(box.Width, box.Height);
		m.Translate(box.X, box.Y);
		return m;
	}

	void PaintTiles(Cairo.Context tc, TileMapping map, int cols, int rows, double kx, double ky,
		Action<Cairo.Context> draw)
	{
		double opacity = Opacity;
		bool group = opacity < 1;
		if (group)
			tc.PushGroup();
		tc.Scale(kx, ky);
		double tw = map.Viewport.Width, th = map.Viewport.Height;
		for (int col = 0; col < cols; col++)
		{
			for (int row = 0; row < rows; row++)
			{
				tc.Save();
				tc.Translate(col * tw, row * th);
				if (col == 1) { tc.Translate(tw, 0); tc.Scale(-1, 1); }
				if (row == 1) { tc.Translate(0, th); tc.Scale(1, -1); }
				tc.Rectangle(0, 0, tw, th);
				tc.Clip();
				tc.Translate(map.OffsetX, map.OffsetY);
				tc.Scale(map.ScaleX, map.ScaleY);
				tc.Translate(-map.Viewbox.X, -map.Viewbox.Y);
				draw(tc);
				tc.Restore();
			}
		}
		if (group)
		{
			tc.PopGroupToSource();
			tc.PaintWithAlpha(opacity);
		}
	}
}
