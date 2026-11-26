using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;
using WpfLine = System.Windows.Shapes.Line;
using WpfPath = System.Windows.Shapes.Path;
using WpfPolygon = System.Windows.Shapes.Polygon;
using WpfPolyline = System.Windows.Shapes.Polyline;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>Changing a measure-affecting input re-measures: the size changes on the next Measure.</summary>
public class ShapeInvalidationTests : WpfGtkTestBase
{
	static readonly Size Big = new Size(1000, 1000);

	static double Width(FrameworkElement e)
	{
		e.Measure(Big);
		return e.DesiredSize.Width;
	}

	[Fact]
	public void Late_Data_assignment_changes_the_size() => Run(() =>
	{
		var path = new WpfPath { Stroke = Brushes.Black, StrokeThickness = 4 };
		Assert.Equal(0, Width(path));
		path.Data = new EllipseGeometry(new Point(50, 50), 25, 25);
		Assert.Equal(77, Width(path));
	});

	[Fact]
	public void Mutating_the_assigned_geometry_changes_the_size() => Run(() =>
	{
		var ellipse = new EllipseGeometry(new Point(50, 50), 25, 25);
		var path = new WpfPath { Stroke = Brushes.Black, StrokeThickness = 4, Data = ellipse };
		Assert.Equal(77, Width(path));
		ellipse.RadiusX = 40;
		Assert.Equal(92, Width(path));
		ellipse.Center = new Point(60, 50);
		Assert.Equal(102, Width(path));
	});

	[Fact]
	public void Replacing_the_geometry_stops_listening_to_the_old_one() => Run(() =>
	{
		var first = new EllipseGeometry(new Point(50, 50), 25, 25);
		var path = new WpfPath { Data = first, Stroke = Brushes.Black, StrokeThickness = 4 };
		path.Data = new EllipseGeometry(new Point(10, 10), 5, 5);
		Assert.Equal(17, Width(path));
		first.RadiusX = 90;
		Assert.Equal(17, Width(path));
	});

	[Fact]
	public void Adding_a_child_to_a_GeometryGroup_changes_the_size() => Run(() =>
	{
		var group = new GeometryGroup();
		group.Children.Add(new RectangleGeometry(new Rect(0, 0, 10, 10)));
		var path = new WpfPath { Data = group, Stroke = Brushes.Black, StrokeThickness = 2 };
		Assert.Equal(11, Width(path), 1);
		group.Children.Add(new RectangleGeometry(new Rect(40, 0, 10, 10)));
		Assert.Equal(51, Width(path), 1);
	});

	[Fact]
	public void Changing_a_GeometryGroup_child_changes_the_size() => Run(() =>
	{
		var child = new RectangleGeometry(new Rect(0, 0, 10, 10));
		var group = new GeometryGroup();
		group.Children.Add(child);
		var path = new WpfPath { Data = group, Stroke = Brushes.Black, StrokeThickness = 2 };
		Assert.Equal(11, Width(path), 1);
		child.Rect = new Rect(0, 0, 30, 10);
		Assert.Equal(31, Width(path), 1);
	});

	[Fact]
	public void GeometryGroup_child_transform_moves_the_bounds() => Run(() =>
	{
		var group = new GeometryGroup();
		group.Children.Add(new RectangleGeometry(new Rect(0, 0, 10, 10)) { Transform = new TranslateTransform(20, 0) });
		group.Children.Add(new RectangleGeometry(new Rect(0, 0, 1, 1)));
		var path = new WpfPath { Data = group, Stroke = Brushes.Black, StrokeThickness = 2 };
		Assert.Equal(31, Width(path), 1);
	});

	[Fact]
	public void Late_Points_assignment_and_point_mutation_change_the_size() => Run(() =>
	{
		var polygon = new WpfPolygon();
		Assert.Equal(0, Width(polygon));
		polygon.Points = new PointCollection { new Point(0, 0), new Point(30, 10) };
		Assert.Equal(30, Width(polygon));
		polygon.Points.Add(new Point(60, 20));
		Assert.Equal(60, Width(polygon));
		polygon.Points[2] = new Point(45, 20);
		Assert.Equal(45, Width(polygon));
		polygon.Points.Clear();
		Assert.Equal(0, Width(polygon));
	});

	[Fact]
	public void Default_points_collection_is_mutable_and_notifies() => Run(() =>
	{
		var polyline = new WpfPolyline();
		polyline.Points.Add(new Point(0, 0));
		polyline.Points.Add(new Point(25, 5));
		Assert.Equal(25, Width(polyline));
	});

	[Fact]
	public void Stroke_thickness_cap_and_stretch_changes_re_measure() => Run(() =>
	{
		var line = new WpfLine { X2 = 20, Stroke = Brushes.Black, StrokeThickness = 2 };
		Assert.Equal(20, Width(line));
		line.StrokeThickness = 8;
		Assert.Equal(20, Width(line));
		line.StrokeStartLineCap = PenLineCap.Square;
		line.StrokeEndLineCap = PenLineCap.Square;
		Assert.Equal(24, Width(line));
		line.Stretch = Stretch.Fill;
		Assert.Equal(1000, Width(line));
	});

	[Fact]
	public void Line_endpoint_change_re_measures() => Run(() =>
	{
		var line = new WpfLine { X1 = 1, Y1 = 1, X2 = 50, Y2 = 50, Stroke = Brushes.Black };
		line.Measure(Big);
		line.X2 = 90;
		Assert.True(Width(line) >= 90);
	});

	[Theory]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void Line_endpoints_reject_non_finite_values(double value) => Run(() =>
	{
		var line = new WpfLine();
		Assert.Throws<ArgumentException>(() => line.X1 = value);
		Assert.Throws<ArgumentException>(() => line.Y2 = value);
	});

	[Fact]
	public void Frozen_point_collection_rejects_mutation_and_clones_its_points() => Run(() =>
	{
		var points = new PointCollection { new Point(1, 2) };
		var clone = (PointCollection)points.Clone();
		points.Freeze();
		Assert.Throws<InvalidOperationException>(() => points.Add(new Point(3, 4)));
		Assert.Single(clone);
		clone.Add(new Point(5, 6));
		Assert.Equal(2, clone.Count);
	});

	[Fact]
	public void Point_collection_raises_Changed_on_every_mutation() => Run(() =>
	{
		var points = new PointCollection();
		int n = 0;
		points.Changed += (_, _) => n++;
		points.Add(new Point(1, 1));
		points.Insert(0, new Point(2, 2));
		points[0] = new Point(3, 3);
		points.Remove(new Point(3, 3));
		points.RemoveAt(0);
		points.Clear();
		Assert.Equal(6, n);
	});
}
