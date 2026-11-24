using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Shapes;

/// <summary>
/// The six ShapesGallery shapes painted at the size GTK allocated them in a shown window; the pixels show
/// where each outline and fill lands (geometry coordinates relative to the shape origin).
/// </summary>
public class ShapesGalleryPixelTests : WpfGtkTestBase
{
	sealed class Painted : System.IDisposable
	{
		readonly Cairo.ImageSurface _surface;

		public Painted(Shape shape)
		{
			int w = (int)System.Math.Ceiling(shape.ActualWidth), h = (int)System.Math.Ceiling(shape.ActualHeight);
			_surface = new Cairo.ImageSurface(Cairo.Format.Argb32, System.Math.Max(w, 1), System.Math.Max(h, 1));
			using var cr = new Cairo.Context(_surface);
			shape.PaintTo(cr, w, h);
		}

		public (byte R, byte G, byte B, byte A) At(int x, int y)
		{
			_surface.Flush();
			byte[] d = _surface.Data;
			int i = y * _surface.Stride + x * 4;
			return (d[i + 2], d[i + 1], d[i], d[i + 3]);
		}

		public void Dispose() => _surface.Dispose();
	}

	static void AssertColor(Color expected, (byte R, byte G, byte B, byte A) actual, string what, int tolerance = 12)
	{
		Assert.True(actual.A > 200, $"{what}: transparent {actual}");
		Assert.InRange(actual.R, expected.R - tolerance, expected.R + tolerance);
		Assert.InRange(actual.G, expected.G - tolerance, expected.G + tolerance);
		Assert.InRange(actual.B, expected.B - tolerance, expected.B + tolerance);
	}

	static void AssertEmpty((byte R, byte G, byte B, byte A) actual, string what) =>
		Assert.True(actual.A < 20, $"{what}: painted {actual}");

	static void WithGallery(System.Action<ShapesGalleryTree> body)
	{
		var t = ShapesGalleryTree.Build();
		try
		{
			t.Show();
			body(t);
		}
		finally { t.Window.Close(); }
	}

	[Fact]
	public void Line_draws_light_steel_blue_on_its_diagonal() => Run(() => WithGallery(t =>
	{
		using var p = new Painted(t.Line);
		AssertColor(Colors.LightSteelBlue, p.At(25, 25), "diagonal");
		AssertColor(Colors.LightSteelBlue, p.At(10, 10), "diagonal start");
		AssertEmpty(p.At(40, 10), "off the diagonal");
	}));

	[Fact]
	public void Path_fills_medium_slate_blue_with_a_black_stroke_at_the_geometry_coordinates() => Run(() =>
		WithGallery(t =>
		{
			using var p = new Painted(t.Path);
			AssertColor(Colors.MediumSlateBlue, p.At(50, 50), "centre fill");
			AssertColor(Colors.Black, p.At(50, 25), "top of the ring");
			AssertColor(Colors.Black, p.At(25, 50), "left of the ring");
			AssertEmpty(p.At(10, 10), "outside the circle");
		}));

	[Fact]
	public void Polygon_fills_light_sea_green_with_a_black_edge() => Run(() => WithGallery(t =>
	{
		using var p = new Painted(t.Polygon);
		AssertColor(Colors.LightSeaGreen, p.At(20, 58), "interior");
		AssertColor(Colors.Black, p.At(30, 50), "top edge");
		AssertEmpty(p.At(40, 75), "below the triangle");
	}));

	[Fact]
	public void Polyline_draws_slate_gray_on_its_first_segment() => Run(() => WithGallery(t =>
	{
		using var p = new Painted(t.Polyline);
		// Segment (1,50)-(10,80): midpoint (5.5, 65).
		AssertColor(Colors.SlateGray, p.At(5, 65), "first segment", 30);
		AssertEmpty(p.At(40, 65), "away from the segments");
	}));

	[Fact]
	public void Rectangle_and_ellipse_draw_their_fill_and_a_fully_visible_inset_stroke() => Run(() =>
		WithGallery(t =>
		{
			using var r = new Painted(t.Rect);
			AssertColor(Colors.Black, r.At(0, 25), "left stroke fully visible");
			AssertColor(Colors.Black, r.At(25, 0), "top stroke fully visible");
			AssertColor(Colors.SkyBlue, r.At(25, 25), "fill");
			using var e = new Painted(t.Ellipse);
			AssertColor(Colors.DarkBlue, e.At(25, 37), "ellipse fill");
			AssertColor(Colors.Black, e.At(25, 0), "ellipse top stroke");
		}));

	[Fact]
	public void Stretch_Fill_polygon_covers_the_slot_and_the_stroke_is_not_scaled() => Run(() =>
	{
		var polygon = new Polygon { Fill = Brushes.Red, Stretch = Stretch.Fill };
		polygon.Points = new PointCollection { new System.Windows.Point(0, 0), new System.Windows.Point(10, 10),
			new System.Windows.Point(0, 10) };
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 50, 100);
		using (var cr = new Cairo.Context(surface)) polygon.PaintTo(cr, 50, 100);
		surface.Flush();
		byte[] d = surface.Data;
		// Triangle (0,0)-(50,100)-(0,100): (10,80) is inside, (45,20) is outside.
		Assert.Equal(255, d[80 * surface.Stride + 10 * 4 + 2]);
		Assert.Equal(0, d[20 * surface.Stride + 45 * 4 + 3]);
		surface.Dispose();
	});
}
