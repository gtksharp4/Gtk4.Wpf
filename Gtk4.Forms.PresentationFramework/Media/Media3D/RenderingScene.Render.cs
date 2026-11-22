using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>
/// The software renderer of a Viewport3D. Hidden surfaces are removed with a back-to-front depth sort of whole
/// triangles (painter's algorithm), not WPF's per-pixel z-buffer; intersecting triangles can therefore overlap
/// incorrectly. Lighting is flat per triangle.
/// </summary>
static class SceneRenderer
{
	/// <summary>Builds the depth-sorted paint list for a viewport of the given pixel size.</summary>
	internal static List<PaintItem> Build(Camera? camera, IEnumerable<Visual3D> children, double width, double height)
	{
		var items = new List<PaintItem>();
		if (camera == null || width <= 0 || height <= 0) return items;
		var view = camera.GetViewMatrix();
		var viewProjection = view * camera.GetProjectionMatrix(width / height);
		var eye = Matrix3D.Identity;
		var eyePoint = new Point3D(0, 0, 0);
		if (view.HasInverse)
		{
			eye = view;
			eye.Invert();
			eyePoint = eye.Transform(eyePoint);
		}

		var scene = new SceneCollector();
		scene.Collect(children, Matrix3D.Identity);
		foreach (var tri in scene.Triangles)
		{
			var polygon = SceneProjection.Project(tri.P0, tri.P1, tri.P2, tri.T0, tri.T1, tri.T2,
				viewProjection, width, height);
			if (polygon == null) continue;
			var material = polygon.IsFront ? tri.Front : tri.Back;
			if (material == null) continue;

			var item = new PaintItem { Polygon = polygon };
			var center = new Point3D((tri.P0.X + tri.P1.X + tri.P2.X) / 3, (tri.P0.Y + tri.P1.Y + tri.P2.Y) / 3,
				(tri.P0.Z + tri.P1.Z + tri.P2.Z) / 3);
			var normal = Normal(tri);
			if (!polygon.IsFront) normal = -normal;
			SceneLighting.Shade(material, center, normal, eyePoint, scene.Lights, item.Layers);
			items.Add(item);
		}
		items.Sort((a, b) => b.Polygon.Depth.CompareTo(a.Polygon.Depth));
		return items;
	}

	static Vector3D Normal(SceneTriangle tri)
	{
		var n = tri.HasNormals ? tri.N0 + tri.N1 + tri.N2 : default;
		if (n.LengthSquared == 0) return SceneProjection.FaceNormal(tri.P0, tri.P1, tri.P2);
		n.Normalize();
		return n;
	}

	/// <summary>Paints a scene into <paramref name="cr"/>.</summary>
	internal static void Render(
		Cairo.Context cr, Camera? camera, IEnumerable<Visual3D> children, int width, int height) =>
		ScenePainter.Paint(cr, Build(camera, children, width, height));
}
