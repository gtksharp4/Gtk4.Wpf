using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media3D;

/// <summary>
/// Real-GTK: textured triangles far smaller than a pixel (a dying particle) and triangles at or behind the
/// near plane must paint a scene GSK can replay (it reports "out of memory" when cairo rejects a pattern).
/// </summary>
public class Viewport3DDegenerateGtkTests : WpfGtkTestBase
{
	const int W = 200, H = 150;

	sealed class CountingBrush : ImageBrush
	{
		internal int Applied;

		internal override void ApplyToCairo(Cairo.Context cr, double width, double height)
		{
			Applied++;
			base.ApplyToCairo(cr, width, height);
		}
	}

	static ImageBrush Texture()
	{
		var disc = new System.Windows.Shapes.Ellipse { Width = 32, Height = 32, Fill = Brushes.Red };
		disc.Measure(new Size(32, 32));
		disc.Arrange(new Rect(0, 0, 32, 32));
		var bmp = new RenderTargetBitmap(32, 32, 96, 96, PixelFormats.Pbgra32);
		bmp.Render(disc);
		bmp.Freeze();
		return new ImageBrush(bmp);
	}

	/// <summary>One textured quad of the given edge at the origin under the demo's orthographic camera.</summary>
	static Viewport3D Quad(double size, Camera camera, double z = 0, Brush? texture = null)
	{
		var mesh = new MeshGeometry3D();
		mesh.Positions.Add(new Point3D(0, 0, z));
		mesh.Positions.Add(new Point3D(0, size, z));
		mesh.Positions.Add(new Point3D(size, size, z));
		mesh.Positions.Add(new Point3D(size, 0, z));
		mesh.TextureCoordinates.Add(new Point(0, 0));
		mesh.TextureCoordinates.Add(new Point(0, 1));
		mesh.TextureCoordinates.Add(new Point(1, 1));
		mesh.TextureCoordinates.Add(new Point(1, 0));
		foreach (int i in new[] { 0, 2, 1, 0, 3, 2 }) mesh.TriangleIndices.Add(i);
		var group = new Model3DGroup();
		group.Children.Add(new AmbientLight());
		group.Children.Add(new GeometryModel3D(mesh, new DiffuseMaterial(texture ?? Texture())));
		var viewport = new Viewport3D { Camera = camera };
		viewport.Children.Add(new ModelVisual3D { Content = group });
		return viewport;
	}

	static OrthographicCamera Ortho() =>
		new OrthographicCamera(new Point3D(0, 0, 32), new Vector3D(0, 0, -32), new Vector3D(0, 1, 0), 128);

	static Graphene.Rect NewRect(float w, float h)
	{
		var raw = System.Runtime.InteropServices.Marshal.AllocHGlobal(16);
		System.Runtime.InteropServices.Marshal.Copy(new[] { 0f, 0f, w, h }, 0, raw, 4);
		return new Graphene.Rect(raw);
	}

	/// <summary>Paints through a GTK snapshot and replays the render node, as the compositor does.</summary>
	static Cairo.Status Replay(Viewport3D viewport)
	{
		var snapshot = new Gtk.Snapshot();
		var bounds = NewRect(W, H);
		using (var cr = snapshot.AppendCairo(bounds)) viewport.PaintTo(cr, W, H);
		var node = snapshot.FreeToNode();
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, W, H);
		using var target = new Cairo.Context(surface);
		node.Draw(target);
		return target.Status;
	}

	[Theory]
	[InlineData(1e-3)]
	[InlineData(1e-5)]
	[InlineData(1e-7)]
	[InlineData(1e-12)]
	[InlineData(3e-6)]
	[InlineData(1e-4)]
	[InlineData(1e-2)]
	public void A_sub_pixel_textured_quad_replays_without_a_cairo_error(double size) => Run(() =>
		Assert.Equal(Cairo.Status.Success, Replay(Quad(size, Ortho()))));

	static Cairo.Rectangle Ink(Viewport3D viewport, out Cairo.Status status)
	{
		using var rec = new Cairo.RecordingSurface(Cairo.Content.ColorAlpha, NewExtents());
		using var cr = new Cairo.Context(rec);
		viewport.PaintTo(cr, W, H);
		status = cr.Status;
		return rec.InkExtents;
	}

	static Cairo.Rectangle NewExtents() => new Cairo.Rectangle(-10000, -10000, 20000, 20000);

	[Fact]
	public void A_texture_scaled_below_a_hundredth_of_a_pixel_is_never_applied() => Run(() =>
	{
		var brush = new CountingBrush { ImageSource = ((ImageBrush)Texture()).ImageSource };
		Ink(Quad(0.0003, Ortho(), texture: brush), out var status);
		Assert.Equal(Cairo.Status.Success, status);
		Assert.Equal(0, brush.Applied);
	});

	[Fact]
	public void A_normal_textured_quad_still_applies_its_texture() => Run(() =>
	{
		var brush = new CountingBrush { ImageSource = ((ImageBrush)Texture()).ImageSource };
		var ink = Ink(Quad(8, Ortho(), texture: brush), out var status);
		Assert.Equal(Cairo.Status.Success, status);
		Assert.InRange(ink.Width, 8, 20);
		Assert.True(brush.Applied > 0);
	});

	static PerspectiveCamera Perspective() =>
		new PerspectiveCamera(new Point3D(0, 0, 5), new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 90);

	[Fact]
	public void A_quad_behind_the_camera_paints_nothing() => Run(() =>
	{
		var ink = Ink(Quad(2, Perspective(), z: 10), out var status);
		Assert.Equal(Cairo.Status.Success, status);
		Assert.Equal(0, ink.Width);
	});

	[Fact]
	public void A_quad_crossing_the_near_plane_stays_inside_the_viewport() => Run(() =>
	{
		var camera = Perspective();
		camera.NearPlaneDistance = 1;
		var mesh = new MeshGeometry3D();
		mesh.Positions.Add(new Point3D(-1, -1, 0));
		mesh.Positions.Add(new Point3D(1, -1, 0));
		mesh.Positions.Add(new Point3D(0, 1, 40));
		foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(i);
		var viewport = new Viewport3D { Camera = camera };
		viewport.Children.Add(new ModelVisual3D
		{
			Content = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.Red)),
		});
		var ink = Ink(viewport, out var status);
		Assert.Equal(Cairo.Status.Success, status);
		Assert.True(ink.X >= -1 && ink.Y >= -1 && ink.X + ink.Width <= W + 1 && ink.Y + ink.Height <= H + 1,
			$"ink {ink.X},{ink.Y} {ink.Width}x{ink.Height} leaves the {W}x{H} viewport");
	});

	[Theory]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	public void A_non_finite_vertex_paints_nothing_and_leaves_the_context_healthy(double bad) => Run(() =>
	{
		var mesh = new MeshGeometry3D();
		mesh.Positions.Add(new Point3D(-1, -1, 0));
		mesh.Positions.Add(new Point3D(1, -1, 0));
		mesh.Positions.Add(new Point3D(bad, 1, 0));
		foreach (int i in new[] { 0, 1, 2 }) mesh.TriangleIndices.Add(i);
		var viewport = new Viewport3D { Camera = Perspective() };
		viewport.Children.Add(new ModelVisual3D
		{
			Content = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.Red)),
		});
		var ink = Ink(viewport, out var status);
		Assert.Equal(Cairo.Status.Success, status);
		Assert.Equal(0, ink.Width);
	});
}
