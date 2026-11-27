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

	[Fact]
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
			Assert.Equal(200, sibling.ActualWidth, 1);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void Stretch_None_line_in_a_100px_StackPanel_does_not_widen_it_beside_a_sibling() => Run(() =>
	{
		var line = new WpfLine { X2 = 400, Y2 = 10, Stroke = Brushes.Black, VerticalAlignment = VerticalAlignment.Top };
		var first = new StackPanel { Width = 100 };
		first.Children.Add(line);
		var second = new Border { Width = 100, Background = Brushes.Red };
		var panel = new StackPanel { Orientation = Orientation.Horizontal };
		panel.Children.Add(first);
		panel.Children.Add(second);
		var window = new WpfWindow { Width = 300, Height = 200, Content = panel };
		try
		{
			window.Show();
			GtkState.Require(() => second.ActualWidth > 0, "layout", second);
			GtkPump.For(150);
			Assert.InRange(first.ActualWidth, 99, 101);
			Assert.InRange(second.ActualWidth, 99, 101);
		}
		finally { window.Close(); }
	});

	[Fact]
	public void A_line_capped_by_a_100px_panel_grows_when_the_panel_widens() => Run(() =>
	{
		var line = new WpfLine { X2 = 400, Y2 = 10, Stroke = Brushes.Black, HorizontalAlignment = HorizontalAlignment.Left };
		var panel = new StackPanel { Width = 100, HorizontalAlignment = HorizontalAlignment.Left };
		panel.Children.Add(line);
		var window = new WpfWindow { Width = 600, Height = 200, Content = panel };
		try
		{
			window.Show();
			GtkState.Require(() => line.ActualWidth > 0, "layout", line);
			panel.Width = 500;
			Assert.True(GtkPump.Until(() => line.ActualWidth >= 399), $"line stayed {line.ActualWidth}");
		}
		finally { window.Close(); }
	});
}
