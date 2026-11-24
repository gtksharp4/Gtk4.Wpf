namespace System.Windows.Media.Media3D;

static partial class ScenePainter
{
	const double TextureSize = TextureCache.Size;

	/// <summary>
	/// Maps the cached brush texture through the triangle's texture coordinates with an affine matrix (not
	/// perspective-correct) and darkens by the light level.
	/// </summary>
	static void PaintTexture(Cairo.Context cr, ScreenPolygon polygon, ShadedLayer layer, TextureCache textures)
	{
		if (!TryAffine(polygon, out var matrix)) return;
		var pattern = textures.Get(layer.TextureBrush!);
		matrix.Invert();
		pattern.Matrix = matrix;
		cr.Save();
		cr.SetSource(pattern);
		Path(cr, polygon);
		if (layer.A >= 1)
		{
			cr.Fill();
		}
		else
		{
			cr.Clip();
			cr.PaintWithAlpha(Clamp(layer.A));
		}
		cr.Restore();
		double light = Clamp((layer.LightR + layer.LightG + layer.LightB) / 3);
		if (light < 1)
		{
			// The mask keeps transparent texels transparent: a texel's colour is scaled, its alpha is not.
			cr.Save();
			Path(cr, polygon);
			cr.Clip();
			cr.SetSourceRGBA(0, 0, 0, (1 - light) * Clamp(layer.A));
			cr.Mask(pattern);
			cr.Restore();
		}
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
