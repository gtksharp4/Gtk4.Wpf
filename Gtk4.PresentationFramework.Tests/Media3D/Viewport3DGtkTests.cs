using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media3D;

/// <summary>The Viewport3D draws through its Gtk.DrawingArea on the software renderer.</summary>
public class Viewport3DGtkTests : WpfGtkTestBase
{
	const int Size = 100;

	static (byte B, byte G, byte R, byte A) PixelAt(Cairo.ImageSurface surface, int x, int y)
	{
		surface.Flush();
		byte[] data = surface.Data;
		int i = y * surface.Stride + x * 4;
		return (data[i], data[i + 1], data[i + 2], data[i + 3]);
	}

	static Viewport3D RedTriangleViewport(Light light)
	{
		var mesh = new MeshGeometry3D();
		mesh.Positions.Add(new Point3D(-1, -1, 0));
		mesh.Positions.Add(new Point3D(1, -1, 0));
		mesh.Positions.Add(new Point3D(0, 1, 0));
		foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(i);
		var model = new Model3DGroup();
		model.Children.Add(light);
		model.Children.Add(new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.Red)));
		var viewport = new Viewport3D
		{
			Camera = new PerspectiveCamera(new Point3D(0, 0, 5), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 90),
		};
		viewport.Children.Add(new ModelVisual3D { Content = model });
		return viewport;
	}

	static Cairo.ImageSurface Paint(Viewport3D viewport)
	{
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, Size, Size);
		using var cr = new Cairo.Context(surface);
		viewport.PaintTo(cr, Size, Size);
		return surface;
	}

	[Fact]
	public void A_lit_red_triangle_paints_red_inside_and_nothing_outside() => Run(() =>
	{
		var viewport = RedTriangleViewport(new AmbientLight());
		Assert.IsType<Gtk.DrawingArea>(viewport.Widget);
		using var surface = Paint(viewport);

		var inside = PixelAt(surface, Size / 2, Size / 2 + 2);
		Assert.True(inside.R >= 250, $"expected red, got rgb({inside.R},{inside.G},{inside.B})");
		Assert.True(inside.G <= 5 && inside.B <= 5);
		Assert.Equal((byte)255, inside.A);
		Assert.Equal((byte)0, PixelAt(surface, 2, 2).A);
	});

	[Fact]
	public void Without_a_light_the_triangle_is_black_and_a_dimmer_light_darkens_it() => Run(() =>
	{
		using (var unlit = Paint(RedTriangleViewport(new AmbientLight(Colors.Black))))
		{
			var p = PixelAt(unlit, Size / 2, Size / 2 + 2);
			Assert.Equal((byte)0, p.R);
		}
		using var dim = Paint(RedTriangleViewport(new AmbientLight(Color.FromRgb(128, 128, 128))));
		var d = PixelAt(dim, Size / 2, Size / 2 + 2);
		Assert.InRange(d.R, 120, 135);
	});

	[Fact]
	public void Changing_the_scene_after_the_first_paint_is_visible_in_the_next_paint() => Run(() =>
	{
		var viewport = RedTriangleViewport(new AmbientLight());
		using (var first = Paint(viewport))
			Assert.True(PixelAt(first, Size / 2, Size / 2 + 2).R >= 250);
		var visual = (ModelVisual3D)viewport.Children[0];
		visual.Transform = new TranslateTransform3D(100, 0, 0);
		using var second = Paint(viewport);
		Assert.Equal((byte)0, PixelAt(second, Size / 2, Size / 2 + 2).A);
	});
}
