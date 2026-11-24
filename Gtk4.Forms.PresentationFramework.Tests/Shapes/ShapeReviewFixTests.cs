using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfLine = System.Windows.Shapes.Line;
using WpfPath = System.Windows.Shapes.Path;
using WpfPolygon = System.Windows.Shapes.Polygon;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.Shapes;

/// <summary>Round 69 review fixes: natural size is not a minimum, fill rule, alignment, change events.</summary>
public class ShapeReviewFixTests : WpfGtkTestBase
{
	static readonly Size Big = new Size(1000, 1000);

	[Fact]
	public void Stretch_None_line_wider_than_the_window_is_clipped_and_siblings_keep_space() => Run(() =>
	{
		var line = new WpfLine { X2 = 400, Y2 = 10, Stroke = Brushes.Black, VerticalAlignment = VerticalAlignment.Top };
		var sibling = new Border { Background = Brushes.Red, Width = 100 };
		var grid = new Grid();
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		grid.Children.Add(line);
		grid.Children.Add(sibling);
		Grid.SetColumn(sibling, 1);
		var window = new WpfWindow { Width = 300, Height = 200, Content = grid };
		try
		{
			window.Show();
			GtkState.Require(() => sibling.ActualWidth > 0, "layout", sibling);
			GtkPump.For(150);
			Assert.InRange(window.ActualWidth, 0, 300.5);
			Assert.InRange(sibling.ActualWidth, 99.5, 300);
			Assert.InRange(line.ActualWidth, 0, 200.5);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void Explicit_size_wins_over_a_bigger_geometry() => Run(() =>
	{
		var path = new WpfPath
		{
			Width = 10, Height = 10, Fill = Brushes.Red, Data = new RectangleGeometry(new Rect(0, 0, 20, 20))
		};
		var window = new WpfWindow { Width = 300, Height = 200, Content = new StackPanel { Children = { path } } };
		try
		{
			window.Show();
			GtkState.Require(() => path.ActualWidth > 0, "layout", path);
			GtkPump.For(150);
			Assert.Equal(10, path.ActualWidth, 0.5);
			Assert.Equal(10, path.ActualHeight, 0.5);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void EvenOdd_group_with_a_Transform_keeps_its_hole() => Run(() =>
	{
		var group = new GeometryGroup { FillRule = FillRule.EvenOdd, Transform = new TranslateTransform(1, 0) };
		group.Children.Add(new EllipseGeometry(new Point(50, 50), 40, 40));
		group.Children.Add(new EllipseGeometry(new Point(50, 50), 20, 20));
		var path = new WpfPath { Data = group, Fill = Brushes.Red };
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 120, 120);
		using (var cr = new Cairo.Context(surface)) path.PaintTo(cr, 120, 120);
		surface.Flush();
		byte[] d = surface.Data;
		int Alpha(int x, int y) => d[y * surface.Stride + x * 4 + 3];
		Assert.Equal(0, Alpha(51, 50));
		Assert.Equal(255, Alpha(51 + 30, 50));
	});

	static WpfPolygon Triangle(VerticalAlignment alignment) => new WpfPolygon
	{
		Stretch = Stretch.Uniform, VerticalAlignment = alignment,
		Points = new PointCollection { new Point(0, 0), new Point(10, 10), new Point(0, 10) }
	};

	// Managed Measure/Arrange only; the real GTK path is ShapeSlotTests.
	[Theory]
	[InlineData(VerticalAlignment.Stretch, 25)]
	[InlineData(VerticalAlignment.Center, 0)]
	[InlineData(VerticalAlignment.Top, 0)]
	[InlineData(VerticalAlignment.Bottom, 0)]
	public void Managed_layout_uniform_shape_is_aligned_inside_its_slot(VerticalAlignment alignment, double offsetY) => Run(() =>
	{
		var p = Triangle(alignment);
		p.Measure(new Size(50, 100));
		p.Arrange(new Rect(0, 0, 50, 100));
		Assert.Equal(5, p.StretchMatrix.M11);
		Assert.Equal(5, p.StretchMatrix.M22);
		Assert.Equal(offsetY, p.StretchMatrix.OffsetY, 0.001);
		Assert.Equal(0, p.StretchMatrix.OffsetX, 0.001);
	});

	[Fact]
	public void Adding_a_figure_or_segment_after_assignment_re_measures() => Run(() =>
	{
		var geometry = new PathGeometry();
		var path = new WpfPath { Data = geometry, Fill = Brushes.Black };
		path.Measure(Big);
		Assert.Equal(0, path.DesiredSize.Width);
		var figure = new PathFigure { StartPoint = new Point(0, 0) };
		geometry.Figures.Add(figure);
		figure.Segments.Add(new LineSegment(new Point(30, 10), true));
		path.Measure(Big);
		Assert.Equal(30, path.DesiredSize.Width, 1);
		figure.Segments.Add(new LineSegment(new Point(60, 10), true));
		path.Measure(Big);
		Assert.Equal(60, path.DesiredSize.Width, 1);
		((LineSegment)figure.Segments[1]).Point = new Point(80, 10);
		path.Measure(Big);
		Assert.Equal(80, path.DesiredSize.Width, 1);
	});

	[Fact]
	public void Reading_Points_creates_no_local_value_and_a_Style_setter_wins() => Run(() =>
	{
		var polygon = new WpfPolygon();
		Assert.Empty(polygon.Points);
		Assert.Equal(DependencyProperty.UnsetValue, polygon.ReadLocalValue(WpfPolygon.PointsProperty));
		var style = new Style(typeof(WpfPolygon));
		style.Setters.Add(new Setter(WpfPolygon.PointsProperty,
			new PointCollection { new Point(0, 0), new Point(40, 10) }));
		polygon.Style = style;
		Assert.Equal(2, polygon.Points.Count);
		polygon.Measure(Big);
		Assert.Equal(40, polygon.DesiredSize.Width);
	});

	[Fact]
	public void Natural_layout_callbacks_survive_garbage_collection() => Run(() =>
	{
		var line = new WpfLine { X2 = 80, Y2 = 10, Stroke = Brushes.Black };
		var window = new WpfWindow { Width = 300, Height = 200, Content = new StackPanel { Children = { line } } };
		try
		{
			window.Show();
			GtkState.Require(() => line.ActualWidth > 0, "layout", line);
			for (int i = 0; i < 3; i++)
			{
				System.GC.Collect();
				System.GC.WaitForPendingFinalizers();
				line.X2 = 90 + i * 10;
				GtkPump.For(100);
			}
			Assert.True(line.ActualWidth >= 99);
		}
		finally { window.Close(); }
	});
}
