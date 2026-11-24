namespace System.Windows.Media.Media3D;

static partial class ScenePainter
{
	const double TextureSize = TextureCache.Size;

	/// <summary>
	/// Smallest texel-to-pixel area scale painted. Below it a texel shrinks below about 1e-4 px (the 256x256 texture
	/// covers under a thousandth of a pixel); cairo cannot represent the inverse pattern matrix (pixman uses 16.16
	/// fixed point) and the compositor fails the frame with "out of memory". The 1e-8 threshold is empirical,
	/// measured on ParticlesDemo. Such a triangle gets the solid colour unless it covers under a pixel.
	/// </summary>
	const double MinTextureScale = 1e-8;

	/// <summary>
	/// Maps the cached brush texture through the triangle's texture coordinates with an affine matrix (not
	/// perspective-correct) and darkens by the light level. False when the caller should paint the solid colour.
	/// </summary>
	static bool PaintTexture(Cairo.Context cr, ScreenPolygon polygon, ShadedLayer layer, TextureCache textures)
	{
		if (!TryAffine(polygon, out var matrix)) return false;
		if (Math.Abs(matrix.Xx * matrix.Yy - matrix.Xy * matrix.Yx) < MinTextureScale)
			return ScreenArea(polygon) < 1;
		var pattern = textures.Get(layer.TextureBrush!);
		matrix.Invert();
		pattern.Matrix = matrix;
		double light = Clamp((layer.LightR + layer.LightG + layer.LightB) / 3);
		var op = cr.Operator;
		cr.Save();
		Path(cr, polygon);
		cr.Clip();
		// Darken the texel before compositing: Atop scales its colour and keeps its alpha.
		cr.PushGroup();
		cr.SetSource(pattern);
		cr.Paint();
		if (light < 1)
		{
			cr.Operator = Cairo.Operator.Atop;
			cr.SetSourceRGBA(0, 0, 0, 1 - light);
			cr.Paint();
		}
		cr.PopGroupToSource();
		cr.Operator = op;
		cr.PaintWithAlpha(Clamp(layer.A));
		cr.Restore();
		return true;
	}

	// Shoelace area in px²: a degenerate texture on a visible triangle falls back to solid colour (like WPF).
	static double ScreenArea(ScreenPolygon polygon)
	{
		var v = polygon.Vertices;
		double sum = 0;
		for (int i = 0; i < v.Count; i++)
		{
			var a = v[i];
			var b = v[(i + 1) % v.Count];
			sum += a.X * b.Y - b.X * a.Y;
		}
		return Math.Abs(sum) / 2;
	}

	/// <summary>The affine matrix taking texture pixels (u, v scaled to 256) to screen pixels.</summary>
	static bool TryAffine(ScreenPolygon polygon, out Cairo.Matrix matrix)
	{
		var v = polygon.Vertices;
		for (int i = 1; i + 1 < v.Count; i++)
			if (Solve(v[0], v[i], v[i + 1], out matrix)) return true;
		matrix = default!;
		return false;
	}

	static bool Solve(ScreenVertex a, ScreenVertex b, ScreenVertex c, out Cairo.Matrix matrix)
	{
		double u0 = a.U * TextureSize, v0 = a.V * TextureSize;
		double du1 = b.U * TextureSize - u0, dv1 = b.V * TextureSize - v0;
		double du2 = c.U * TextureSize - u0, dv2 = c.V * TextureSize - v0;
		double det = du1 * dv2 - du2 * dv1;
		matrix = default!;
		if (Math.Abs(det) < 1e-9) return false;
		double dx1 = b.X - a.X, dx2 = c.X - a.X, dy1 = b.Y - a.Y, dy2 = c.Y - a.Y;
		double xu = (dx1 * dv2 - dx2 * dv1) / det, xv = (du1 * dx2 - du2 * dx1) / det;
		double yu = (dy1 * dv2 - dy2 * dv1) / det, yv = (du1 * dy2 - du2 * dy1) / det;
		matrix = new Cairo.Matrix(xu, yu, xv, yv, a.X - xu * u0 - xv * v0, a.Y - yu * u0 - yv * v0);
		return true;
	}
}
