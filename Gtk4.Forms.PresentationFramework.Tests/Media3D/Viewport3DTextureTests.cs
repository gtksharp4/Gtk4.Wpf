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

	[Theory]
	[InlineData(true, true)]
	[InlineData(false, true)]
	[InlineData(true, false)]
	[InlineData(false, false)]
	public void A_texture_with_no_pixels_stays_transparent_under_dim_or_no_light(bool missingFile, bool ambient)
		=> Run(() =>
	{
		var brush = missingFile
			? BrushOf(System.IO.Path.Combine(System.AppContext.BaseDirectory, "does-not-exist.png"))
			: new ImageBrush();
		var quad = Quad(new DiffuseMaterial(brush));
		var scene = ambient ? Lit(quad, Color.FromRgb(128, 128, 128)) : new Model3DGroup { Children = { quad } };
		using var surface = Paint(Viewport(scene));
		Assert.True(IsBackdrop(PixelAt(surface, 100, 100)), $"centre was {PixelAt(surface, 100, 100)}");
	});

	[Fact]
	public void Transparent_texels_stay_transparent_under_dim_light_and_opaque_ones_are_dimmed() => Run(() =>
	{
		var brush = BrushOf(HalfTransparentPng());
		var scene = Lit(Quad(new DiffuseMaterial(brush)), Color.FromRgb(128, 128, 128));
		using var surface = Paint(Viewport(scene));
		Assert.True(IsBackdrop(PixelAt(surface, 85, 100)), $"transparent half was {PixelAt(surface, 85, 100)}");
		var opaque = PixelAt(surface, 115, 100);
		Assert.InRange(opaque.R, 120, 135);
		Assert.True(opaque.G < 10 && opaque.B < 10, $"opaque half was {opaque}");
	});

	[Fact]
	public void A_half_opacity_texture_keeps_half_the_backdrop_under_dim_light() => Run(() =>
	{
		var brush = BrushOf(TwoColourPng());
		brush.Opacity = 0.5;
		var scene = Lit(Quad(new DiffuseMaterial(brush)), Color.FromRgb(128, 128, 128));
		using var surface = Paint(Viewport(scene));
		var top = PixelAt(surface, 100, 80);
		// 0.5 * backdrop + 0.5 * red * 0.5 light; the old mask-on-top darkened the backdrop share to 0.375.
		Assert.InRange(top.G, 120, 135);
		Assert.InRange(top.R, 58, 70);
	});

	[Fact]
	public void A_shared_brush_maps_texels_by_texture_coordinate_across_many_triangles() => Run(() =>
	{
		var brush = BrushOf(TwoColourPng());
		var mesh = new MeshGeometry3D();
		const int strips = 10;
		for (int s = 0; s <= strips; s++)
		{
			double u = (double)s / strips;
			mesh.Positions.Add(new Point3D(-1 + 2 * u, -1, 0));
			mesh.Positions.Add(new Point3D(-1 + 2 * u, 1, 0));
			mesh.TextureCoordinates.Add(new System.Windows.Point(u, 1));
			mesh.TextureCoordinates.Add(new System.Windows.Point(u, 0));
		}
		for (int s = 0; s < strips; s++)
		{
			int a = s * 2;
			foreach (int i in new[] { a, a + 2, a + 1, a + 2, a + 3, a + 1 }) mesh.TriangleIndices.Add(i);
		}
		using var mine = Paint(Viewport(Lit(new GeometryModel3D(mesh, new DiffuseMaterial(brush)))));
		using var reference = Paint(Viewport(Lit(Quad(new DiffuseMaterial(brush)))));
		foreach (int x in new[] { 83, 90, 97, 103, 117 })
		{
			var top = PixelAt(mine, x, 85);
			var bottom = PixelAt(mine, x, 115);
			Assert.True(top.R > 200 && top.B < 60, $"top at {x}: {top}");
			Assert.True(bottom.B > 200 && bottom.R < 60, $"bottom at {x}: {bottom}");
			AssertClose(PixelAt(reference, x, 85), top);
			AssertClose(PixelAt(reference, x, 115), bottom);
		}
	});

	static void AssertClose((byte R, byte G, byte B, byte A) expected, (byte R, byte G, byte B, byte A) actual)
	{
		Assert.InRange(actual.R, expected.R - 8, expected.R + 8);
		Assert.InRange(actual.G, expected.G - 8, expected.G + 8);
		Assert.InRange(actual.B, expected.B - 8, expected.B + 8);
	}
}
