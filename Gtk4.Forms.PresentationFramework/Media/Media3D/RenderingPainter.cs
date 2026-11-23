using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>A projected triangle ready to paint.</summary>
sealed class PaintItem
{
	internal ScreenPolygon Polygon = null!;
	internal List<ShadedLayer> Layers = new List<ShadedLayer>();
}

/// <summary>Paints shaded, projected triangles with Cairo.</summary>
static class ScenePainter
{
	const double TextureSize = 256;

	/// <summary>
	/// Smallest texel-to-pixel area scale painted. Below it the whole texture covers a hundredth of a pixel; cairo
	/// cannot represent the inverse pattern matrix (pixman uses 16.16 fixed point) and the compositor fails the
	/// frame with "out of memory". WPF draws nothing for such a triangle.
	/// </summary>
	const double MinTextureScale = 1e-8;

	internal static void Paint(Cairo.Context cr, List<PaintItem> items)
	{
		foreach (var item in items)
			foreach (var layer in item.Layers)
				PaintLayer(cr, item.Polygon, layer);
	}

	static double Clamp(double v) => v < 0 ? 0 : v > 1 ? 1 : v;

	static void PaintLayer(Cairo.Context cr, ScreenPolygon polygon, ShadedLayer layer)
	{
		if (layer.A <= 0) return;
		cr.Save();
		cr.Operator = layer.Additive ? Cairo.Operator.Add : Cairo.Operator.Over;
		if (layer.TextureBrush != null && PaintTexture(cr, polygon, layer))
		{
			cr.Restore();
			return;
		}
		double r = Clamp(layer.R * layer.LightR), g = Clamp(layer.G * layer.LightG), b = Clamp(layer.B * layer.LightB);
		if (layer.TextureBrush != null)
		{
			var c = layer.TextureBrush.ToGdkRgba();
			r = Clamp(c.Red * layer.LightR);
			g = Clamp(c.Green * layer.LightG);
			b = Clamp(c.Blue * layer.LightB);
		}
		Path(cr, polygon);
		cr.SetSourceRGBA(r, g, b, layer.A);
		if (layer.A >= 1)
		{
			// A hairline of the same colour hides the seams between adjacent triangles.
			cr.FillPreserve();
			cr.LineWidth = 1;
			cr.Stroke();
		}
		else
		{
			cr.Fill();
		}
		cr.Restore();
	}

	static void Path(Cairo.Context cr, ScreenPolygon polygon)
	{
		cr.NewPath();
		for (int i = 0; i < polygon.Vertices.Count; i++)
		{
			var v = polygon.Vertices[i];
			if (i == 0) cr.MoveTo(v.X, v.Y);
			else cr.LineTo(v.X, v.Y);
		}
		cr.ClosePath();
	}

	/// <summary>
	/// Maps the brush through the triangle's texture coordinates with an affine matrix (not perspective-correct),
	/// clips to the triangle and darkens by the light level.
	/// </summary>
	static bool PaintTexture(Cairo.Context cr, ScreenPolygon polygon, ShadedLayer layer)
	{
		if (!TryAffine(polygon, out var matrix)) return false;
		if (Math.Abs(matrix.Xx * matrix.Yy - matrix.Xy * matrix.Yx) < MinTextureScale) return true;
		Path(cr, polygon);
		cr.Clip();
		cr.Save();
		cr.Transform(matrix);
		layer.TextureBrush!.ApplyToCairo(cr, TextureSize, TextureSize);
		cr.PaintWithAlpha(Clamp(layer.A));
		cr.Restore();
		double light = Clamp((layer.LightR + layer.LightG + layer.LightB) / 3);
		if (light < 1)
		{
			Path(cr, polygon);
			cr.SetSourceRGBA(0, 0, 0, 1 - light);
			cr.Fill();
		}
		return true;
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
