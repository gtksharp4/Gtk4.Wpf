using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>A projected triangle ready to paint.</summary>
sealed class PaintItem
{
	internal ScreenPolygon Polygon = null!;
	internal List<ShadedLayer> Layers = new List<ShadedLayer>();
}

/// <summary>Paints shaded, projected triangles with Cairo.</summary>
static partial class ScenePainter
{
	internal static void Paint(Cairo.Context cr, List<PaintItem> items)
	{
		using var textures = new TextureCache();
		foreach (var item in items)
			foreach (var layer in item.Layers)
				PaintLayer(cr, item.Polygon, layer, textures);
	}

	static double Clamp(double v) => v < 0 ? 0 : v > 1 ? 1 : v;

	static void PaintLayer(Cairo.Context cr, ScreenPolygon polygon, ShadedLayer layer, TextureCache textures)
	{
		if (layer.A <= 0) return;
		cr.Save();
		cr.Operator = layer.Additive ? Cairo.Operator.Add : Cairo.Operator.Over;
		if (layer.TextureBrush != null)
		{
			// A brush with no paint source (missing image, empty visual) is transparent, as in WPF.
			PaintTexture(cr, polygon, layer, textures);
			cr.Restore();
			return;
		}
		double r = Clamp(layer.R * layer.LightR), g = Clamp(layer.G * layer.LightG), b = Clamp(layer.B * layer.LightB);
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
}
