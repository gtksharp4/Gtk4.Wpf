using System.Collections.Generic;
using System.Windows.Shapes;

namespace System.Windows.Media.Media3D;

/// <summary>A light placed in world space.</summary>
sealed class SceneLight
{
	internal Light Source = null!;
	internal Point3D Position;
	internal Vector3D Direction;
}

/// <summary>One mesh triangle in world space.</summary>
struct SceneTriangle
{
	internal Point3D P0, P1, P2;
	internal Vector3D N0, N1, N2;
	internal bool HasNormals;
	internal Point T0, T1, T2;
	internal bool HasTexture;
	internal Material? Front, Back;
}

/// <summary>Walks a Visual3D tree and gathers the world-space triangles and lights.</summary>
sealed class SceneCollector
{
	internal readonly List<SceneTriangle> Triangles = new List<SceneTriangle>();
	internal readonly List<SceneLight> Lights = new List<SceneLight>();

	internal void Collect(IEnumerable<Visual3D> visuals, Matrix3D parent)
	{
		foreach (var visual in visuals)
		{
			var world = Local(visual.Transform) * parent;
			if (visual is ModelVisual3D mv)
			{
				if (mv.Content != null) CollectModel(mv.Content, world);
				Collect(mv.Children, world);
			}
		}
	}

	static Matrix3D Local(Transform3D? t) => t == null ? Matrix3D.Identity : t.Value;

	void CollectModel(Model3D model, Matrix3D parent)
	{
		var world = Local(model.Transform) * parent;
		switch (model)
		{
			case Model3DGroup group:
				foreach (var child in Media3DProps.Peek<Model3DCollection>(group, Model3DGroup.ChildrenProperty))
					CollectModel(child, world);
				break;
			case Light light:
				AddLight(light, world);
				break;
			case GeometryModel3D gm when gm.Geometry is MeshGeometry3D mesh:
				AddMesh(gm, mesh, world);
				break;
		}
	}

	void AddLight(Light light, Matrix3D world)
	{
		var l = new SceneLight { Source = light };
		switch (light)
		{
			case DirectionalLight d:
				l.Direction = world.Transform(d.Direction);
				break;
			case SpotLight s:
				l.Position = world.Transform(s.Position);
				l.Direction = world.Transform(s.Direction);
				break;
			case PointLightBase p:
				l.Position = world.Transform(p.Position);
				break;
		}
		Lights.Add(l);
	}

	void AddMesh(GeometryModel3D model, MeshGeometry3D mesh, Matrix3D world)
	{
		var positions = Media3DProps.Peek<Point3DCollection>(mesh, MeshGeometry3D.PositionsProperty);
		var indices = Media3DProps.Peek<Int32Collection>(mesh, MeshGeometry3D.TriangleIndicesProperty);
		var normals = Media3DProps.Peek<Vector3DCollection>(mesh, MeshGeometry3D.NormalsProperty);
		var texture = (PointCollection?)mesh.GetValue(MeshGeometry3D.TextureCoordinatesProperty);
		bool useNormals = normals.Count == positions.Count;
		var inverse = world;
		if (inverse.HasInverse) inverse.Invert();
		bool useTexture = texture != null && texture.Count == positions.Count;
		int count = indices.Count > 0 ? indices.Count : positions.Count;
		for (int i = 0; i + 2 < count; i += 3)
		{
			int a = indices.Count > 0 ? indices[i] : i;
			int b = indices.Count > 0 ? indices[i + 1] : i + 1;
			int c = indices.Count > 0 ? indices[i + 2] : i + 2;
			if (a < 0 || b < 0 || c < 0 || a >= positions.Count || b >= positions.Count || c >= positions.Count)
				continue;
			var tri = new SceneTriangle
			{
				P0 = world.Transform(positions[a]),
				P1 = world.Transform(positions[b]),
				P2 = world.Transform(positions[c]),
				Front = model.Material,
				Back = model.BackMaterial,
				HasNormals = useNormals,
				HasTexture = useTexture,
			};
			if (useNormals)
			{
				tri.N0 = NormalOf(inverse, normals[a]);
				tri.N1 = NormalOf(inverse, normals[b]);
				tri.N2 = NormalOf(inverse, normals[c]);
			}
			if (useTexture)
			{
				tri.T0 = texture![a];
				tri.T1 = texture[b];
				tri.T2 = texture[c];
			}
			Triangles.Add(tri);
		}
	}

	/// <summary>Transforms a normal by the inverse transpose of the world matrix and normalises it.</summary>
	static Vector3D NormalOf(Matrix3D inverse, Vector3D n)
	{
		var result = new Vector3D(
			n.X * inverse.M11 + n.Y * inverse.M12 + n.Z * inverse.M13,
			n.X * inverse.M21 + n.Y * inverse.M22 + n.Z * inverse.M23,
			n.X * inverse.M31 + n.Y * inverse.M32 + n.Z * inverse.M33);
		if (result.LengthSquared > 0) result.Normalize();
		return result;
	}
}
