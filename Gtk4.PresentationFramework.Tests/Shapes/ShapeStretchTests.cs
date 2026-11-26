using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;
using WpfPolygon = System.Windows.Shapes.Polygon;
using WpfRectangle = System.Windows.Shapes.Rectangle;
using WpfEllipse = System.Windows.Shapes.Ellipse;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>Stretch metrics and the Shape/Rectangle/Ellipse measure and arrange contract.</summary>
public class ShapeStretchTests : WpfGtkTestBase
{
	// ShapeElements StretchExample: Polygon Points="0,0 10,10 0,10" in a 50x100 slot.
	static WpfPolygon Example(Stretch stretch, double thickness = 0)
	{
		var polygon = new WpfPolygon
		{
			Stretch = stretch, Stroke = thickness > 0 ? Brushes.Black : null, StrokeThickness = thickness
		};
		polygon.Points = new PointCollection { new Point(0, 0), new Point(10, 10), new Point(0, 10) };
		return polygon;
	}

	static readonly Size Slot = new Size(50, 100);

	static void Layout(UIElement e)
	{
		e.Measure(Slot);
		e.Arrange(new Rect(0, 0, 50, 100));
	}

	[Fact]
	public void None_measures_the_natural_size_and_keeps_an_identity_matrix() => Run(() =>
	{
		var p = Example(Stretch.None);
		Layout(p);
		Assert.Equal(new Size(10, 10), p.DesiredSize);
		Assert.True(p.StretchMatrix.IsIdentity);
	});

	[Fact]
	public void Fill_scales_each_axis_to_the_slot() => Run(() =>
	{
		var p = Example(Stretch.Fill);
		Layout(p);
		Assert.Equal(new Size(50, 100), p.DesiredSize);
		Assert.Equal(5, p.StretchMatrix.M11);
		Assert.Equal(10, p.StretchMatrix.M22);
	});

	[Fact]
	public void Uniform_uses_the_smaller_scale() => Run(() =>
	{
		var p = Example(Stretch.Uniform);
		Layout(p);
		Assert.Equal(new Size(50, 50), p.DesiredSize);
		Assert.Equal(5, p.StretchMatrix.M11);
		Assert.Equal(5, p.StretchMatrix.M22);
	});

	[Fact]
	public void UniformToFill_uses_the_larger_scale() => Run(() =>
	{
		var p = Example(Stretch.UniformToFill);
		Layout(p);
		Assert.Equal(new Size(100, 100), p.DesiredSize);
		Assert.Equal(10, p.StretchMatrix.M11);
		Assert.Equal(10, p.StretchMatrix.M22);
	});

	[Fact]
	public void UniformToFill_with_one_infinite_side_uses_the_other() => Run(() =>
	{
		var p = Example(Stretch.UniformToFill);
		p.Measure(new Size(40, double.PositiveInfinity));
		Assert.Equal(new Size(40, 40), p.DesiredSize);
	});

	[Fact]
	public void UniformToFill_with_both_infinite_uses_the_natural_size() => Run(() =>
	{
		var p = Example(Stretch.UniformToFill);
		p.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		Assert.Equal(new Size(10, 10), p.DesiredSize);
	});

	[Fact]
	public void Stroke_is_subtracted_before_scaling_and_the_matrix_translates_by_half_of_it() => Run(() =>
	{
		var p = Example(Stretch.Fill, thickness: 10);
		Layout(p);
		Assert.Equal(4, p.StretchMatrix.M11);
		Assert.Equal(9, p.StretchMatrix.M22);
		Assert.Equal(5, p.StretchMatrix.OffsetX);
		Assert.Equal(new Size(50, 100), p.DesiredSize);
	});

	[Fact]
	public void Geometry_offset_from_the_origin_is_removed_by_the_translate() => Run(() =>
	{
		var polygon = new WpfPolygon { Stretch = Stretch.Fill };
		polygon.Points = new PointCollection { new Point(10, 20), new Point(20, 30) };
		polygon.Measure(new Size(10, 10));
		polygon.Arrange(new Rect(0, 0, 10, 10));
		Assert.Equal(-10, polygon.StretchMatrix.OffsetX);
		Assert.Equal(-20, polygon.StretchMatrix.OffsetY);
	});

	[Fact]
	public void Zero_width_geometry_does_not_scale_that_axis() => Run(() =>
	{
		var polygon = new WpfPolygon { Stretch = Stretch.Uniform };
		polygon.Points = new PointCollection { new Point(5, 0), new Point(5, 10) };
		polygon.Measure(new Size(40, 40));
		polygon.Arrange(new Rect(0, 0, 40, 40));
		Assert.Equal(1, polygon.StretchMatrix.M11);
		Assert.Equal(4, polygon.StretchMatrix.M22);
	});

	[Fact]
	public void No_points_measure_zero() => Run(() =>
	{
		var polygon = new WpfPolygon { Stroke = Brushes.Black, StrokeThickness = 4 };
		polygon.Measure(Slot);
		Assert.Equal(new Size(0, 0), polygon.DesiredSize);
	});

	[Fact]
	public void Single_point_polygon_still_measures_from_its_bounds() => Run(() =>
	{
		var polygon = new WpfPolygon { Stroke = Brushes.Black, StrokeThickness = 2 };
		polygon.Points = new PointCollection { new Point(20, 30) };
		polygon.Measure(Slot);
		Assert.True(polygon.DesiredSize.Width >= 20 && polygon.DesiredSize.Height >= 30);
	});

	[Fact]
	public void Stroke_thickness_is_zero_without_a_stroke_or_for_nan() => Run(() =>
	{
		Assert.Equal(0, new WpfPolygon { StrokeThickness = 5 }.GetStrokeThickness());
		Assert.Equal(0, new WpfPolygon { Stroke = Brushes.Black, StrokeThickness = double.NaN }.GetStrokeThickness());
		Assert.Equal(3, new WpfPolygon { Stroke = Brushes.Black, StrokeThickness = -3 }.GetStrokeThickness());
	});

	[Fact]
	public void Rectangle_and_ellipse_default_to_Fill_and_measure_the_stroke() => Run(() =>
	{
		var r = new WpfRectangle { Stroke = Brushes.Black, StrokeThickness = 4 };
		var e = new WpfEllipse { Stroke = Brushes.Black, StrokeThickness = 4 };
		Assert.Equal(Stretch.Fill, r.Stretch);
		Assert.Equal(Stretch.Fill, e.Stretch);
		r.Measure(Slot);
		e.Measure(Slot);
		Assert.Equal(new Size(4, 4), r.DesiredSize);
		Assert.Equal(new Size(4, 4), e.DesiredSize);
	});

	[Fact]
	public void Rectangle_without_stroke_measures_zero() => Run(() =>
	{
		var r = new WpfRectangle();
		r.Measure(Slot);
		Assert.Equal(new Size(0, 0), r.DesiredSize);
	});

	[Theory]
	[InlineData(50, 100, 100, 100)]
	[InlineData(50, double.PositiveInfinity, 50, 50)]
	[InlineData(double.PositiveInfinity, double.PositiveInfinity, 0, 0)]
	public void Rectangle_UniformToFill_measure_follows_the_constraint(double w, double h, double ew, double eh) =>
		Run(() =>
		{
			var r = new WpfRectangle { Stretch = Stretch.UniformToFill };
			r.Measure(new Size(w, h));
			Assert.Equal(new Size(ew, eh), r.DesiredSize);
		});

	[Fact]
	public void Rectangle_outline_rect_is_inset_by_half_the_stroke() => Run(() =>
	{
		var r = new WpfRectangle { Stroke = Brushes.Black, StrokeThickness = 4 };
		Assert.Equal(new Rect(2, 2, 46, 96), r.GetRectLikeRect(50, 100));
		r.Stretch = Stretch.Uniform;
		Assert.Equal(new Rect(2, 2, 46, 46), r.GetRectLikeRect(50, 100));
		r.Stretch = Stretch.UniformToFill;
		Assert.Equal(new Rect(2, 2, 96, 96), r.GetRectLikeRect(50, 100));
		r.Stretch = Stretch.None;
		Assert.Equal(new Rect(2, 2, 0, 0), r.GetRectLikeRect(50, 100));
	});
}
