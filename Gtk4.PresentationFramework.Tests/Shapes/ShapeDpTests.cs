using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;
using WpfPath = System.Windows.Shapes.Path;
using WpfPolygon = System.Windows.Shapes.Polygon;
using WpfPolyline = System.Windows.Shapes.Polyline;
using WpfRectangle = System.Windows.Shapes.Rectangle;
using WpfEllipse = System.Windows.Shapes.Ellipse;
using WpfLine = System.Windows.Shapes.Line;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>
/// Pure (no GTK) tests: DP defaults, CLR wrapper round-trips for shapes.
/// </summary>
public class ShapeDpTests
{
	// ── Shape base DP defaults ────────────────────────────────────────────────

	[Fact]
	public void Rectangle_Fill_default_is_null()
	{
		var r = new WpfRectangle();
		Assert.Null(r.Fill);
	}

	[Fact]
	public void Rectangle_Stroke_default_is_null()
	{
		var r = new WpfRectangle();
		Assert.Null(r.Stroke);
	}

	[Fact]
	public void Rectangle_StrokeThickness_default_is_one()
	{
		var r = new WpfRectangle();
		Assert.Equal(1.0, r.StrokeThickness);
	}

	[Fact]
	public void Rectangle_Stretch_default_is_Fill()
	{
		var r = new WpfRectangle();
		Assert.Equal(Stretch.Fill, r.Stretch);
	}

	// ── Rectangle specific ────────────────────────────────────────────────────

	[Fact]
	public void Rectangle_RadiusX_default_is_zero()
	{
		var r = new WpfRectangle();
		Assert.Equal(0.0, r.RadiusX);
	}

	[Fact]
	public void Rectangle_RadiusY_default_is_zero()
	{
		var r = new WpfRectangle();
		Assert.Equal(0.0, r.RadiusY);
	}

	[Fact]
	public void Rectangle_RadiusX_roundtrip()
	{
		var r = new WpfRectangle();
		r.RadiusX = 10;
		Assert.Equal(10.0, r.RadiusX);
	}

	[Fact]
	public void Rectangle_Fill_roundtrip()
	{
		var r = new WpfRectangle();
		var brush = Brushes.Red;
		r.Fill = brush;
		Assert.Same(brush, r.Fill);
	}

	[Fact]
	public void Rectangle_Stroke_roundtrip()
	{
		var r = new WpfRectangle();
		var brush = Brushes.Blue;
		r.Stroke = brush;
		Assert.Same(brush, r.Stroke);
	}

	// ── Ellipse ──────────────────────────────────────────────────────────────

	[Fact]
	public void Ellipse_Fill_default_is_null()
	{
		var e = new WpfEllipse();
		Assert.Null(e.Fill);
	}

	[Fact]
	public void Ellipse_StrokeThickness_default_is_one()
	{
		var e = new WpfEllipse();
		Assert.Equal(1.0, e.StrokeThickness);
	}

	[Fact]
	public void Ellipse_Fill_roundtrip()
	{
		var e = new WpfEllipse();
		e.Fill = Brushes.Green;
		Assert.Equal(Colors.Green, ((SolidColorBrush)e.Fill).Color);
	}

	// ── Line ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Line_X1_default_is_zero()
	{
		var l = new WpfLine();
		Assert.Equal(0.0, l.X1);
	}

	[Fact]
	public void Line_coordinates_roundtrip()
	{
		var l = new WpfLine { X1 = 10, Y1 = 20, X2 = 100, Y2 = 200 };
		Assert.Equal(10.0, l.X1);
		Assert.Equal(20.0, l.Y1);
		Assert.Equal(100.0, l.X2);
		Assert.Equal(200.0, l.Y2);
	}

	// ── Polygon ──────────────────────────────────────────────────────────────

	[Fact]
	public void Polygon_Points_default_is_empty()
	{
		var p = new WpfPolygon();
		Assert.NotNull(p.Points);
		Assert.Empty(p.Points);
	}

	[Fact]
	public void Polygon_Points_roundtrip()
	{
		var p = new WpfPolygon();
		p.Points.Add(new System.Windows.Point(0, 0));
		p.Points.Add(new System.Windows.Point(50, 0));
		p.Points.Add(new System.Windows.Point(25, 50));
		Assert.Equal(3, p.Points.Count);
		Assert.Equal(50.0, p.Points[1].X);
	}

	[Fact]
	public void Polyline_Points_roundtrip()
	{
		var p = new WpfPolyline();
		var col = new PointCollection
		{
			new System.Windows.Point(0, 0),
			new System.Windows.Point(100, 0)
		};
		p.Points = col;
		Assert.Same(col, p.Points);
		Assert.Equal(2, p.Points.Count);
	}

	// ── Path ─────────────────────────────────────────────────────────────────

	[Fact]
	public void Path_Data_default_is_null()
	{
		var path = new WpfPath();
		Assert.Null(path.Data);
	}

	[Fact]
	public void Path_Data_holds_rectangle_geometry()
	{
		var path = new WpfPath();
		var geom = new RectangleGeometry(new System.Windows.Rect(0, 0, 100, 50));
		path.Data = geom;
		Assert.Same(geom, path.Data);
	}

	[Fact]
	public void Path_Data_holds_ellipse_geometry()
	{
		var path = new WpfPath();
		var geom = new EllipseGeometry(new System.Windows.Point(50, 50), 40, 30);
		path.Data = geom;
		Assert.IsType<EllipseGeometry>(path.Data);
	}

	[Fact]
	public void Path_Data_holds_path_geometry()
	{
		var path = new WpfPath();
		var pg = new PathGeometry();
		path.Data = pg;
		Assert.IsType<PathGeometry>(path.Data);
	}
}