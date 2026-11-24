using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.Shapes;

/// <summary>The exact element tree of the WPF-Samples Graphics/ShapesGallery app.</summary>
sealed class ShapesGalleryTree
{
	public WpfWindow Window = null!;
	public Border Border = null!;
	public Grid Grid = null!;
	public Rectangle Rect = null!;
	public Ellipse Ellipse = null!;
	public Line Line = null!;
	public System.Windows.Shapes.Path Path = null!;
	public Polygon Polygon = null!;
	public Polyline Polyline = null!;

	public static ShapesGalleryTree Build()
	{
		var t = new ShapesGalleryTree();
		t.Border = new Border
		{
			BorderBrush = Brushes.Black, BorderThickness = new Thickness(2), Width = 400, Height = 600,
			Padding = new Thickness(15), Background = Brushes.White
		};
		t.Grid = new Grid { Margin = new Thickness(15) };
		t.Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(125) });
		t.Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
		for (int i = 0; i < 7; i++) t.Grid.RowDefinitions.Add(new RowDefinition());
		t.Rect = new Rectangle
		{
			Stroke = Brushes.Black, Fill = Brushes.SkyBlue, Height = 50, Width = 50,
			HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center
		};
		t.Ellipse = new Ellipse
		{
			Stroke = Brushes.Black, Fill = Brushes.DarkBlue, Width = 50, Height = 75,
			HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center
		};
		t.Line = new Line
		{
			Stroke = Brushes.LightSteelBlue, X1 = 1, X2 = 50, Y1 = 1, Y2 = 50, StrokeThickness = 2,
			HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center
		};
		t.Path = new System.Windows.Shapes.Path
		{
			Stroke = Brushes.Black, Fill = Brushes.MediumSlateBlue, StrokeThickness = 4,
			HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center
		};
		t.Path.Data = new EllipseGeometry { Center = new Point(50, 50), RadiusX = 25, RadiusY = 25 };
		t.Polygon = new Polygon
		{
			Stroke = Brushes.Black, Fill = Brushes.LightSeaGreen, StrokeThickness = 2,
			HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center
		};
		t.Polygon.Points = new PointCollection { new Point(1, 50), new Point(10, 80), new Point(50, 50) };
		t.Polyline = new Polyline { Stroke = Brushes.SlateGray, StrokeThickness = 2, FillRule = FillRule.EvenOdd };
		t.Polyline.Points = new PointCollection { new Point(1, 50), new Point(10, 80), new Point(20, 40) };
		Shape[] shapes = { t.Rect, t.Ellipse, t.Line, t.Path, t.Polygon, t.Polyline };
		for (int i = 0; i < shapes.Length; i++)
		{
			t.Grid.Children.Add(shapes[i]);
			Grid.SetRow(shapes[i], i + 1);
			Grid.SetColumn(shapes[i], 0);
		}
		t.Border.Child = t.Grid;
		t.Window = new WpfWindow { Content = t.Border, Background = Brushes.LightSlateGray, Title = "Shapes Sample" };
		return t;
	}

	public void Show()
	{
		Window.Show();
		GtkState.Require(() => Polyline.ActualHeight > 0, "layout", Polyline);
		GtkPump.For(150);
		// The GTK grid does the real layout; the managed contract is measured with the grid cell constraint.
		var cell = new System.Windows.Size(125, 536.0 / 7);
		foreach (var shape in new Shape[] { Rect, Ellipse, Line, Path, Polygon, Polyline }) shape.Measure(cell);
	}
}
