using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Xunit;
using static Gtk4.Forms.PresentationFramework.Tests.Media3D.TexturedSceneHelpers;

namespace Gtk4.Forms.PresentationFramework.Tests.Media3D;

/// <summary>Textured materials on the software Viewport3D, read back as pixels.</summary>
public class Viewport3DTextureTests : WpfGtkTestBase
{
	static GeometryModel3D Quad(Material material, Material? back = null) =>
		new GeometryModel3D(Plane(), material) { BackMaterial = back };

	static void AssertBackdropEverywhere(Cairo.ImageSurface s)
	{
		foreach (var (x, y) in new[] { (100, 100), (70, 70), (130, 130), (10, 10) })
			Assert.True(IsBackdrop(PixelAt(s, x, y)), $"({x},{y}) was painted: {PixelAt(s, x, y)}");
	}

	[Theory]
	[InlineData("emissive-null")]
	[InlineData("diffuse-null")]
	[InlineData("emissive-missing")]
	[InlineData("diffuse-missing")]
	public void A_brush_with_no_paint_source_leaves_the_backdrop_untouched(string kind) => Run(() =>
	{
		var brush = kind.EndsWith("null")
			? new ImageBrush()
			: BrushOf(System.IO.Path.Combine(System.AppContext.BaseDirectory, "does-not-exist.png"));
		Material material = kind.StartsWith("emissive") ? new EmissiveMaterial(brush) : new DiffuseMaterial(brush);
		using var surface = Paint(Viewport(Lit(Quad(material))));
		AssertBackdropEverywhere(surface);
	});

	[Fact]
	public void An_emissive_texture_lands_inside_the_projected_quad_with_v0_at_the_top() => Run(() =>
	{
		var material = new MaterialGroup();
		material.Children.Add(new EmissiveMaterial(BrushOf(TwoColourPng())));
		using var surface = Paint(Viewport(Lit(Quad(material))));
		// FOV 60 at distance 5: the quad spans about 0.35 of the viewport height around the centre.
		var top = PixelAt(surface, 100, 80);
		var bottom = PixelAt(surface, 100, 120);
		Assert.True(top.R > 200 && top.B < 60, $"top {top}");
		Assert.True(bottom.B > 200 && bottom.R < 60, $"bottom {bottom}");
		Assert.True(IsBackdrop(PixelAt(surface, 100, 20)));
		Assert.True(IsBackdrop(PixelAt(surface, 20, 100)));
	});

	[Fact]
	public void A_BackMaterial_texture_shows_on_the_back_face_and_is_absent_without_one() => Run(() =>
	{
		var eye = new Point3D(0, 0, -5);
		var look = new Vector3D(0, 0, 1);
		var back = new EmissiveMaterial(BrushOf(TwoColourPng()));
		using (var with = Paint(Viewport(Lit(Quad(new DiffuseMaterial(Brushes.Gray), back)), eye, look)))
		{
			var top = PixelAt(with, 100, 80);
			Assert.True(top.R > 200 && top.B < 60, $"top {top}");
		}
		using var without = Paint(Viewport(Lit(Quad(new DiffuseMaterial(Brushes.Gray))), eye, look));
		AssertBackdropEverywhere(without);
	});

	[Fact]
	public void Diffuse_texture_brightness_follows_the_ambient_light() => Run(() =>
	{
		var brush = BrushOf(TwoColourPng());
		using var full = Paint(Viewport(Lit(Quad(new DiffuseMaterial(brush)), Colors.White)));
		using var half = Paint(Viewport(Lit(Quad(new DiffuseMaterial(brush)), Color.FromRgb(128, 128, 128))));
		Assert.True(PixelAt(full, 100, 80).R >= 250);
		Assert.InRange(PixelAt(half, 100, 80).R, 120, 135);
	});

	[Fact]
	public void A_brush_shared_by_every_triangle_is_rasterised_once_per_paint() => Run(() =>
	{
		var brush = BrushOf(TwoColourPng());
		var mesh = new MeshGeometry3D();
		for (int t = 0; t < 40; t++)
		{
			double x = -1 + t * 0.05;
			mesh.Positions.Add(new Point3D(x, -0.5, 0));
			mesh.Positions.Add(new Point3D(x + 0.04, -0.5, 0));
			mesh.Positions.Add(new Point3D(x, 0.5, 0));
			mesh.TextureCoordinates.Add(new System.Windows.Point(0, 1));
			mesh.TextureCoordinates.Add(new System.Windows.Point(1, 1));
			mesh.TextureCoordinates.Add(new System.Windows.Point(0, 0));
			foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(t * 3 + i);
		}
		var viewport = Viewport(Lit(new GeometryModel3D(mesh, new DiffuseMaterial(brush))));
		int before = TextureCache.RasterizeCount;
		using var surface = Paint(viewport);
		Assert.Equal(1, TextureCache.RasterizeCount - before);
		Assert.False(IsBackdrop(PixelAt(surface, 100, 100)));
	});
}
