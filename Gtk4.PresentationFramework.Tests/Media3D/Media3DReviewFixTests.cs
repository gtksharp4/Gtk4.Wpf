using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

/// <summary>Review fixes: null camera, normals, lazy collections, invalidation wiring and TransformBounds.</summary>
public class Media3DReviewFixTests : WpfGtkTestBase
{
	static PerspectiveCamera Above() =>
		new PerspectiveCamera(new Point3D(0, 0, 5), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 90);

	static MeshGeometry3D Triangle(Vector3D? normal)
	{
		var mesh = new MeshGeometry3D();
		mesh.Positions.Add(new Point3D(-1, -1, 0));
		mesh.Positions.Add(new Point3D(1, -1, 0));
		mesh.Positions.Add(new Point3D(0, 1, 0));
		foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(i);
		if (normal is Vector3D n)
			for (int i = 0; i < 3; i++) mesh.Normals.Add(n);
		return mesh;
	}

	static ModelVisual3D Lit(MeshGeometry3D mesh, Light light, Material? back = null)
	{
		var group = new Model3DGroup();
		group.Children.Add(light);
		group.Children.Add(new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.White)) { BackMaterial = back });
		return new ModelVisual3D { Content = group };
	}

	[Fact]
	public void A_viewport_without_a_camera_paints_nothing_and_does_not_throw() => Run(() =>
	{
		var viewport = new Viewport3D { Camera = null! };
		viewport.Children.Add(Lit(Triangle(null), new AmbientLight()));
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 10, 10);
		using var cr = new Cairo.Context(surface);
		viewport.PaintTo(cr, 10, 10);
		Assert.Empty(SceneRenderer.Build(null, viewport.Children, 10, 10));
		surface.Flush();
		Assert.All(surface.Data, b => Assert.Equal((byte)0, b));
	});

	[Fact]
	public void Authored_normals_pointing_away_from_the_light_stay_dark_on_a_front_face()
	{
		var down = new Vector3D(0, 0, -1);
		var away = Lit(Triangle(new Vector3D(0, 0, -1)), new DirectionalLight(Colors.White, down));
		var item = Assert.Single(SceneRenderer.Build(Above(), new[] { away }, 100, 100));
		Assert.True(item.Polygon.IsFront);
		Assert.Equal(0.0, item.Layers[0].LightR);

		var toward = Lit(Triangle(new Vector3D(0, 0, 1)), new DirectionalLight(Colors.White, down));
		item = Assert.Single(SceneRenderer.Build(Above(), new[] { toward }, 100, 100));
		Assert.Equal(1.0, item.Layers[0].LightR, 6);
	}

	[Fact]
	public void A_back_face_is_lit_with_the_normal_turned_towards_the_viewer()
	{
		var mesh = new MeshGeometry3D();
		mesh.Positions.Add(new Point3D(-1, -1, 0));
		mesh.Positions.Add(new Point3D(0, 1, 0));
		mesh.Positions.Add(new Point3D(1, -1, 0));
		foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(i);
		var visual = Lit(mesh, new DirectionalLight(Colors.White, new Vector3D(0, 0, 1)),
			new DiffuseMaterial(Brushes.White));
		var item = Assert.Single(SceneRenderer.Build(Above(), new[] { visual }, 100, 100));
		Assert.False(item.Polygon.IsFront);
		// The light shines towards +Z, away from the camera, so a face turned to the viewer is dark.
		Assert.Equal(0.0, item.Layers[0].LightR);

		visual = Lit(mesh, new DirectionalLight(Colors.White, new Vector3D(0, 0, -1)),
			new DiffuseMaterial(Brushes.White));
		item = Assert.Single(SceneRenderer.Build(Above(), new[] { visual }, 100, 100));
		Assert.Equal(1.0, item.Layers[0].LightR, 6);
	}

	[Fact]
	public void Normals_transform_by_the_inverse_transpose_of_the_world_matrix()
	{
		var visual = Lit(Triangle(new Vector3D(1, 1, 0)), new AmbientLight());
		visual.Transform = new ScaleTransform3D(1, 4, 1);
		var scene = new SceneCollector();
		scene.Collect(new[] { visual }, Matrix3D.Identity);
		var n = Assert.Single(scene.Triangles).N0;
		var expected = new Vector3D(1, 0.25, 0);
		expected.Normalize();
		Assert.Equal(expected.X, n.X, 6);
		Assert.Equal(expected.Y, n.Y, 6);
		Assert.Equal(0.0, n.Z, 6);
	}

	[Fact]
	public void A_non_uniform_scale_lights_a_slanted_normal_by_the_inverse_transpose()
	{
		double Intensity(Transform3D? scale)
		{
			var mesh = new MeshGeometry3D();
			mesh.Positions.Add(new Point3D(-1, 0, 1));
			mesh.Positions.Add(new Point3D(1, 0, 1));
			mesh.Positions.Add(new Point3D(0, 0, -1));
			foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(i);
			foreach (int i in new[] { 0, 1, 2 }) mesh.Normals.Add(new Vector3D(1, 1, 0));
			var visual = Lit(mesh, new DirectionalLight(Colors.White, new Vector3D(-1, 0, 0)));
			if (scale != null) visual.Transform = scale;
			var camera = new PerspectiveCamera(
				new Point3D(0, 5, 0), new Vector3D(0, -1, 0), new Vector3D(0, 0, -1), 90);
			return Assert.Single(SceneRenderer.Build(camera, new[] { visual }, 100, 100)).Layers[0].LightR;
		}

		Assert.Equal(Math.Sqrt(0.5), Intensity(null), 6);
		// Normal (1,1,0) scaled by (1,4,1) is (1,0.25,0) under the inverse transpose, (1,4,0) under the world matrix.
		Assert.Equal(1 / Math.Sqrt(1.0625), Intensity(new ScaleTransform3D(1, 4, 1)), 6);
	}

	[Fact]
	public void Reading_a_default_collection_raises_no_Changed_and_sets_no_local_value()
	{
		var mesh = new MeshGeometry3D();
		int changes = 0;
		mesh.Changed += (s, e) => changes++;
		Assert.Empty(mesh.Positions);
		Assert.Empty(mesh.Normals);
		Assert.Empty(mesh.TriangleIndices);
		Assert.Equal(0, changes);
		Assert.Equal(DependencyProperty.UnsetValue, mesh.ReadLocalValue(MeshGeometry3D.PositionsProperty));

		mesh.Positions.Add(new Point3D(1, 2, 3));
		Assert.Equal(1, changes);
		Assert.Same(mesh.Positions, mesh.ReadLocalValue(MeshGeometry3D.PositionsProperty));
		mesh.Positions.Add(new Point3D(4, 5, 6));
		Assert.Equal(2, changes);
	}

	[Fact]
	public void Reading_Model3DGroup_Children_raises_no_Changed_until_a_child_is_added()
	{
		var group = new Model3DGroup();
		int changes = 0;
		group.Changed += (s, e) => changes++;
		Assert.Empty(group.Children);
		Assert.Equal(0, changes);
		Assert.Equal(DependencyProperty.UnsetValue, group.ReadLocalValue(Model3DGroup.ChildrenProperty));
		group.Children.Add(new AmbientLight());
		Assert.Equal(1, changes);
		Assert.Same(group.Children, group.ReadLocalValue(Model3DGroup.ChildrenProperty));
	}

	[Fact]
	public void TransformBounds_is_public_and_moves_the_box()
	{
		var bounds = new TranslateTransform3D(1, 2, 3).TransformBounds(new Rect3D(0, 0, 0, 1, 1, 1));
		Assert.Equal(new Rect3D(1, 2, 3, 1, 1, 1), bounds);
		Assert.True(typeof(Transform3D).GetMethod("TransformBounds")!.IsPublic);
	}

	[Fact]
	public void Nested_changes_and_the_camera_invalidate_the_viewport_until_the_child_is_removed() => Run(() =>
	{
		var brush = new SolidColorBrush(Colors.Red);
		var rotation = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
		var mesh = Triangle(null);
		var group = new Model3DGroup();
		group.Children.Add(new GeometryModel3D(mesh, new DiffuseMaterial(brush)));
		var visual = new ModelVisual3D { Content = group, Transform = new RotateTransform3D(rotation) };
		var viewport = new Viewport3D { Camera = Above() };
		viewport.Children.Add(visual);

		void Expect(Action mutate, string what)
		{
			int before = viewport.InvalidateCount;
			mutate();
			Assert.True(viewport.InvalidateCount > before, what);
		}

		Expect(() => brush.Color = Colors.Blue, "material brush colour");
		Expect(() => rotation.Angle = 45, "rotation angle");
		Expect(() => mesh.Positions.Add(new Point3D(0, 0, 1)), "mesh positions");
		Expect(() => ((PerspectiveCamera)viewport.Camera).Position = new Point3D(0, 0, 9), "camera position");

		viewport.Children.Remove(visual);
		int count = viewport.InvalidateCount;
		brush.Color = Colors.Green;
		rotation.Angle = 90;
		mesh.Positions.Add(new Point3D(0, 0, 2));
		Assert.Equal(count, viewport.InvalidateCount);
	});
}
