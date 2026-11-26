using System;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>Measured layout of the six ShapesGallery shapes (see ShapesGalleryTree).</summary>
public class ShapesGalleryLayoutTests : WpfGtkTestBase
{
	// Cell height: window content 600 - 2*2 border - 2*15 padding - 2*15 margin = 536, over 7 star rows.
	static void AssertSize(double w, double h, System.Windows.Size s, double eps = 0.01)
	{
		Assert.InRange(s.Width, w - eps, w + eps);
		Assert.InRange(s.Height, h - eps, h + eps);
	}

	[Fact]
	public void Rectangle_and_ellipse_keep_their_explicit_sizes() => Run(() =>
	{
		var t = ShapesGalleryTree.Build();
		try
		{
			t.Show();
			AssertSize(50, 50, t.Rect.DesiredSize);
			AssertSize(50, 75, t.Ellipse.DesiredSize);
			Assert.Equal(50, t.Rect.ActualWidth);
			Assert.Equal(75, t.Ellipse.ActualHeight);
		}
		finally { t.Window.Close(); }
	});

	[Fact]
	public void Line_measures_to_its_pen_inflated_far_corner() => Run(() =>
	{
		var t = ShapesGalleryTree.Build();
		try
		{
			t.Show();
			// Flat caps: the stroke rectangle's far corner is 50 + (thickness/2)*sin45 = 50 + sqrt(2)/2.
			double expected = 50 + Math.Sqrt(2) / 2;
			AssertSize(expected, expected, t.Line.DesiredSize);
			Assert.InRange(t.Line.ActualWidth, expected - 0.01, Math.Ceiling(expected));
		}
		finally { t.Window.Close(); }
	});

	[Fact]
	public void Path_with_late_ellipse_data_measures_77_by_77() => Run(() =>
	{
		var t = ShapesGalleryTree.Build();
		try
		{
			t.Show();
			AssertSize(77, 77, t.Path.DesiredSize);
			Assert.InRange(t.Path.ActualWidth, 77, 78);
		}
		finally { t.Window.Close(); }
	});

	[Fact]
	public void Polygon_measures_to_its_pen_inflated_far_corner() => Run(() =>
	{
		var t = ShapesGalleryTree.Build();
		try
		{
			t.Show();
			var (right, bottom) = ShapeExpectations.PolygonFarCorner(
				new[] { (1.0, 50.0), (10.0, 80.0), (50.0, 50.0) }, 2, 10);
			AssertSize(right, bottom, t.Polygon.DesiredSize, 0.02);
		}
		finally { t.Window.Close(); }
	});

	[Fact]
	public void Polyline_measures_to_its_far_corner_and_is_allocated() => Run(() =>
	{
		var t = ShapesGalleryTree.Build();
		try
		{
			t.Show();
			// Open polyline: one miter at (10,80), hw 1 / sin(half-angle 15.3 deg) = 3.78 below the vertex.
			Assert.InRange(t.Polyline.DesiredSize.Width, 20.9, 21.05);
			Assert.InRange(t.Polyline.DesiredSize.Height, 83.7, 83.85);
			Assert.True(t.Polyline.ActualWidth >= 20 && t.Polyline.ActualHeight >= 20);
		}
		finally { t.Window.Close(); }
	});
}
