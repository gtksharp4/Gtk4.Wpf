using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfLine = System.Windows.Shapes.Line;
using WpfPolygon = System.Windows.Shapes.Polygon;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Shapes;

/// <summary>Round 69 lane I: shape slots in a real GTK window.</summary>
public class ShapeSlotTests : WpfGtkTestBase
{
	[Fact]
	public void Stretch_Uniform_polygon_with_Stretch_alignment_is_centred_in_a_Border() => Run(() =>
	{
		var polygon = new WpfPolygon
		{
			Stretch = Stretch.Uniform, Fill = Brushes.Red,
			Points = new PointCollection { new Point(0, 0), new Point(10, 10), new Point(0, 10) }
		};
		var window = new WpfWindow
		{
			Width = 300, Height = 200,
			Content = new Border
			{
				Width = 50, Height = 100, HorizontalAlignment = HorizontalAlignment.Left, Child = polygon
			}
		};
		try
		{
			window.Show();
			GtkState.Require(() => polygon.ActualWidth > 0, "layout", polygon);
			GtkPump.For(150);
			// WPF: the shape's RenderSize is the stretched 50x50 and ArrangeCore applies the 25 px offset, which this
			// layer folds into the stretch matrix because the GTK allocation is the whole 50x100 slot.
			var m = polygon.StretchMatrix;
			Assert.Equal(m.M11, m.M22, 0.001);
			Assert.InRange(polygon.ActualWidth, 40, 50);
			Assert.Equal(polygon.ActualWidth, polygon.ActualHeight, 0.5);
			Assert.True(m.OffsetY > 10);
		}
		finally { window.Close(); }
	});

	[Fact(Skip = "known gap: fixed Grid tracks do not cap children; see plan doc")]
	public void Stretch_None_line_in_a_fixed_100px_column_is_clipped_and_the_star_column_keeps_space() => Run(() =>
	{
		var line = new WpfLine { X2 = 400, Y2 = 10, Stroke = Brushes.Black, VerticalAlignment = VerticalAlignment.Top };
		var sibling = new Border { Background = Brushes.Red };
		var grid = new Grid();
		grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
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
			// WPF: ArrangeCore keeps the unclipped desired size (about 400) and clips to the 100 px column.
			Assert.InRange(line.ActualWidth, 399, 402);
			Assert.Equal(200, sibling.ActualWidth, 1);
		}
		finally { window.Close(); }
	});
}
