using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

/// <summary>Projection, winding, clipping, lighting and depth order of the software renderer (no GTK).</summary>
public class SceneRendererTests
{
	static PerspectiveCamera CameraAtZ(double z = 5) =>
		new PerspectiveCamera(new Point3D(0, 0, z), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 90);

	static MeshGeometry3D Triangle(Point3D a, Point3D b, Point3D c)
	{
		var mesh = new MeshGeometry3D();
		mesh.Positions.Add(a);
		mesh.Positions.Add(b);
		mesh.Positions.Add(c);
		foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(i);
		return mesh;
	}

	static ModelVisual3D Scene(Model3D model) => new ModelVisual3D { Content = model };

	static ModelVisual3D Lit(GeometryModel3D geometry, params Light[] lights)
	{
		var group = new Model3DGroup();
		foreach (var light in lights) group.Children.Add(light);
		group.Children.Add(geometry);
		return Scene(group);
	}

	static System.Collections.Generic.List<PaintItem> Build(Camera camera, ModelVisual3D visual) =>
		SceneRenderer.Build(camera, new[] { visual }, 100, 100);

	static GeometryModel3D Front(MeshGeometry3D mesh, Brush brush) =>
		new GeometryModel3D(mesh, new DiffuseMaterial(brush));

	static readonly Point3D A = new Point3D(-1, -1, 0), B = new Point3D(1, -1, 0), C = new Point3D(0, 1, 0);

	[Fact]
	public void A_counter_clockwise_triangle_facing_the_camera_is_a_front_face_centred_in_the_viewport()
	{
		var items = Build(CameraAtZ(), Lit(Front(Triangle(A, B, C), Brushes.Red), new AmbientLight()));
		var item = Assert.Single(items);
		Assert.True(item.Polygon.IsFront);
		Assert.Equal(3, item.Polygon.Vertices.Count);
		Assert.Equal(50.0, item.Polygon.Vertices.Average(v => v.X), 6);
		Assert.True(item.Polygon.Vertices.All(v => v.Y >= 0 && v.Y <= 100));
	}

	[Fact]
	public void Reversed_winding_is_a_back_face_that_uses_BackMaterial_or_is_skipped()
	{
		var reversed = Triangle(A, C, B);
		var model = Front(reversed, Brushes.Red);
		Assert.Empty(Build(CameraAtZ(), Lit(model, new AmbientLight())));

		model.BackMaterial = new DiffuseMaterial(Brushes.Blue);
		var item = Assert.Single(Build(CameraAtZ(), Lit(model, new AmbientLight())));
		Assert.False(item.Polygon.IsFront);
		Assert.Equal(0.0, item.Layers[0].R);
		Assert.Equal(1.0, item.Layers[0].B);
	}

	[Fact]
	public void Triangles_behind_the_camera_are_dropped_and_straddling_ones_are_clipped_at_the_near_plane()
	{
		var behind = Triangle(new Point3D(-1, -1, 10), new Point3D(1, -1, 10), new Point3D(0, 1, 10));
		Assert.Empty(Build(CameraAtZ(), Lit(Front(behind, Brushes.Red), new AmbientLight())));

		var straddling = Triangle(new Point3D(-1, -1, 0), new Point3D(1, -1, 0), new Point3D(0, 1, 20));
		var model = Front(straddling, Brushes.Red);
		model.BackMaterial = new DiffuseMaterial(Brushes.Blue);
		var item = Assert.Single(Build(CameraAtZ(), Lit(model, new AmbientLight())));
		Assert.Equal(4, item.Polygon.Vertices.Count);
		Assert.All(item.Polygon.Vertices, v => Assert.True(!double.IsNaN(v.X) && !double.IsInfinity(v.X)));
	}

	[Fact]
	public void The_horizontal_field_of_view_scales_both_axes_with_the_viewport_width()
	{
		var wide = SceneRenderer.Build(CameraAtZ(), new[] { Lit(Front(Triangle(A, B, C), Brushes.Red),
			new AmbientLight()) }, 200, 100);
		var square = Build(CameraAtZ(), Lit(Front(Triangle(A, B, C), Brushes.Red), new AmbientLight()));
		double wideHeight = wide[0].Polygon.Vertices.Max(v => v.Y) - wide[0].Polygon.Vertices.Min(v => v.Y);
		double squareHeight = square[0].Polygon.Vertices.Max(v => v.Y) - square[0].Polygon.Vertices.Min(v => v.Y);
		Assert.Equal(2 * squareHeight, wideHeight, 6);
		double wideWidth = wide[0].Polygon.Vertices.Max(v => v.X) - wide[0].Polygon.Vertices.Min(v => v.X);
		double squareWidth = square[0].Polygon.Vertices.Max(v => v.X) - square[0].Polygon.Vertices.Min(v => v.X);
		Assert.Equal(2 * squareWidth, wideWidth, 6);
	}

	[Fact]
	public void Triangles_are_painted_back_to_front()
	{
		var far = Front(Triangle(new Point3D(-1, -1, -3), new Point3D(1, -1, -3), new Point3D(0, 1, -3)), Brushes.Blue);
		var near = Front(Triangle(A, B, C), Brushes.Red);
		var group = new Model3DGroup();
		group.Children.Add(new AmbientLight());
		group.Children.Add(near);
		group.Children.Add(far);
		var items = Build(CameraAtZ(), Scene(group));
		Assert.Equal(2, items.Count);
		Assert.Equal(1.0, items[0].Layers[0].B);
		Assert.Equal(1.0, items[1].Layers[0].R);
	}

	[Fact]
	public void Ambient_and_directional_lights_scale_the_diffuse_colour()
	{
		var model = Front(Triangle(A, B, C), Brushes.White);
		var half = Build(CameraAtZ(), Lit(model, new AmbientLight(Color.FromRgb(128, 128, 128))));
		Assert.Equal(128 / 255.0, half[0].Layers[0].LightR, 6);

		var facing = Build(CameraAtZ(), Lit(model, new DirectionalLight(Colors.White, new Vector3D(0, 0, -1))));
		Assert.Equal(1.0, facing[0].Layers[0].LightR, 6);
		var away = Build(CameraAtZ(), Lit(model, new DirectionalLight(Colors.White, new Vector3D(0, 0, 1))));
		Assert.Equal(0.0, away[0].Layers[0].LightR, 6);
		var unlit = Build(CameraAtZ(), Lit(model));
		Assert.Equal(0.0, unlit[0].Layers[0].LightR);
	}

	[Fact]
	public void Point_light_attenuation_and_range_and_spot_cone_limit_the_light()
	{
		var model = Front(Triangle(A, B, C), Brushes.White);
		var near = new PointLight(Colors.White, new Point3D(0, 0, 1)) { LinearAttenuation = 1 };
		double lit = Build(CameraAtZ(), Lit(model, near))[0].Layers[0].LightR;
		Assert.True(lit > 0 && lit < 1);
		var outOfRange = new PointLight(Colors.White, new Point3D(0, 0, 1)) { Range = 0.5 };
		Assert.Equal(0.0, Build(CameraAtZ(), Lit(model, outOfRange))[0].Layers[0].LightR);

		var onAxis = new SpotLight(Colors.White, new Point3D(0, 0, 3), new Vector3D(0, 0, -1), 30, 10);
		Assert.True(Build(CameraAtZ(), Lit(model, onAxis))[0].Layers[0].LightR > 0.9);
		var offAxis = new SpotLight(Colors.White, new Point3D(0, 0, 3), new Vector3D(1, 0, 0), 30, 10);
		Assert.Equal(0.0, Build(CameraAtZ(), Lit(model, offAxis))[0].Layers[0].LightR);
	}

	[Fact]
	public void Emissive_ignores_lights_and_specular_adds_a_highlight()
	{
		var emissive = new GeometryModel3D(Triangle(A, B, C), new EmissiveMaterial(Brushes.Green));
		var layer = Build(CameraAtZ(), Lit(emissive))[0].Layers[0];
		Assert.Equal(1.0, layer.LightG);
		Assert.False(layer.Additive);

		var group = new MaterialGroup();
		group.Children.Add(new DiffuseMaterial(Brushes.Red));
		group.Children.Add(new SpecularMaterial(Brushes.White, 10));
		var shiny = new GeometryModel3D(Triangle(A, B, C), group);
		var layers = Build(CameraAtZ(), Lit(shiny, new DirectionalLight(Colors.White, new Vector3D(0, 0, -1))))[0].Layers;
		Assert.Equal(2, layers.Count);
		Assert.True(layers[1].Additive);
		Assert.True(layers[1].LightR > 0.9);
	}

	[Fact]
	public void Transforms_compose_along_the_visual_and_model_tree()
	{
		var model = Front(Triangle(A, B, C), Brushes.Red);
		model.Transform = new TranslateTransform3D(10, 0, 0);
		var visual = Lit(model, new AmbientLight());
		visual.Transform = new TranslateTransform3D(-10, 0, 0);
		var item = Assert.Single(Build(CameraAtZ(), visual));
		Assert.Equal(50.0, item.Polygon.Vertices.Average(v => v.X), 6);

		visual.Transform = new TranslateTransform3D(-30, 0, 0);
		var moved = Build(CameraAtZ(), visual);
		Assert.True(moved.Count == 0 || moved[0].Polygon.Vertices.Average(v => v.X) < 0);
	}

	[Fact]
	public void Camera_transform_and_orthographic_projection_are_applied()
	{
		var model = Front(Triangle(A, B, C), Brushes.Red);
		var visual = Lit(model, new AmbientLight());
		var camera = CameraAtZ();
		camera.Transform = new TranslateTransform3D(1, 0, 0);
		var shifted = Build(camera, visual)[0].Polygon.Vertices.Average(v => v.X);
		Assert.True(shifted < 50.0);
		var ortho = new OrthographicCamera(new Point3D(0, 0, 5), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 4);
		var vertices = Build(ortho, visual)[0].Polygon.Vertices;
		Assert.Equal(50.0, vertices.Max(v => v.X) - vertices.Min(v => v.X), 6);
	}
}
