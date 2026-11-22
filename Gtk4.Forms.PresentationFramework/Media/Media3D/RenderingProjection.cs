using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>A projected vertex: pixel position, normalised depth and interpolated texture coordinate.</summary>
struct ScreenVertex
{
	internal double X, Y, Depth, U, V;
}

/// <summary>A clipped, projected triangle.</summary>
sealed class ScreenPolygon
{
	internal readonly List<ScreenVertex> Vertices = new List<ScreenVertex>(4);

	/// <summary>True when the triangle is counter-clockwise as seen by the camera.</summary>
	internal bool IsFront;

	/// <summary>Mean normalised depth; larger is farther.</summary>
	internal double Depth;
}

/// <summary>Projection of world triangles to pixels: near-plane clipping, perspective divide and winding.</summary>
static class SceneProjection
{
	struct Clip
	{
		internal double X, Y, Z, W, U, V;
	}

	/// <summary>
	/// Projects a world-space triangle through <paramref name="viewProjection"/> to a
	/// <paramref name="width"/> x <paramref name="height"/> viewport; null when it is fully clipped or degenerate.
	/// </summary>
	internal static ScreenPolygon? Project(
		Point3D p0, Point3D p1, Point3D p2, Point uv0, Point uv1, Point uv2,
		Matrix3D viewProjection, double width, double height)
	{
		var input = new[] { ToClip(p0, uv0, viewProjection), ToClip(p1, uv1, viewProjection),
			ToClip(p2, uv2, viewProjection) };
		if (input[0].Z > input[0].W && input[1].Z > input[1].W && input[2].Z > input[2].W) return null;
		var clipped = ClipNear(input);
		if (clipped.Count < 3) return null;

		var polygon = new ScreenPolygon();
		double depthSum = 0;
		foreach (var c in clipped)
		{
			if (c.W <= 1e-12) return null;
			double nx = c.X / c.W, ny = c.Y / c.W, nz = c.Z / c.W;
			polygon.Vertices.Add(new ScreenVertex
			{
				X = (nx * 0.5 + 0.5) * width,
				Y = (1 - (ny * 0.5 + 0.5)) * height,
				Depth = nz,
				U = c.U,
				V = c.V,
			});
			depthSum += nz;
		}
		polygon.Depth = depthSum / clipped.Count;
		double area = SignedArea(polygon.Vertices);
		if (area == 0) return null;
		// Screen y points down, so a counter-clockwise triangle has a negative area here.
		polygon.IsFront = area < 0;
		return polygon;
	}

	static double SignedArea(List<ScreenVertex> v)
	{
		double area = 0;
		for (int i = 0; i < v.Count; i++)
		{
			var a = v[i];
			var b = v[(i + 1) % v.Count];
			area += a.X * b.Y - b.X * a.Y;
		}
		return area / 2;
	}

	static Clip ToClip(Point3D p, Point uv, Matrix3D m)
	{
		var h = m.Transform(new Point4D(p.X, p.Y, p.Z, 1));
		return new Clip { X = h.X, Y = h.Y, Z = h.Z, W = h.W, U = uv.X, V = uv.Y };
	}

	static List<Clip> ClipNear(Clip[] input)
	{
		var output = new List<Clip>(4);
		for (int i = 0; i < input.Length; i++)
		{
			var a = input[i];
			var b = input[(i + 1) % input.Length];
			bool aIn = a.Z >= 0, bIn = b.Z >= 0;
			if (aIn) output.Add(a);
			if (aIn == bIn) continue;
			double t = a.Z / (a.Z - b.Z);
			output.Add(new Clip
			{
				X = a.X + (b.X - a.X) * t, Y = a.Y + (b.Y - a.Y) * t, Z = 0,
				W = a.W + (b.W - a.W) * t, U = a.U + (b.U - a.U) * t, V = a.V + (b.V - a.V) * t,
			});
		}
		return output;
	}

	/// <summary>The unit normal of a triangle (counter-clockwise front), or zero when degenerate.</summary>
	internal static Vector3D FaceNormal(Point3D p0, Point3D p1, Point3D p2)
	{
		var n = Vector3D.CrossProduct(p1 - p0, p2 - p0);
		if (n.LengthSquared > 0) n.Normalize();
		return n;
	}
}
