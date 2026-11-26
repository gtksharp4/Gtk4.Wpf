using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>A brush painted inside its own source settles however it got there (local, Style, nested).</summary>
public class VisualBrushSelfRefTests : WpfGtkTestBase
{
	static void Settles(Func<VisualBrush, Rectangle, Brush> apply)
	{
		var text = new TextBlock { Text = "first", FontSize = 20 };
		var source = new StackPanel { Width = 150, Height = 150, Background = Brushes.White };
		var brush = new VisualBrush(source);
		source.Children.Add(text);
		var rect = new Rectangle { Width = 80, Height = 80 };
		var fill = apply(brush, rect);
		if (fill != null) rect.Fill = fill;
		source.Children.Add(rect);
		var win = new WpfWindow { Width = 250, Height = 250, Content = source };
		win.Show();
		GtkPump.For(1500);
		try
		{
			int changed = 0;
			brush.Changed += (_, _) => changed++;
			GtkPump.For(3000);
			Assert.True(changed <= 3, "idle Changed " + changed);
			int before = changed;
			text.Text = "second";
			GtkPump.For(1500);
			int after = changed - before;
			Assert.InRange(after, 1, 4);
			before = changed;
			GtkPump.For(2000);
			Assert.True(changed - before <= 2, "settled Changed " + (changed - before));
		}
		finally { win.Close(); }
	}

	[Fact]
	public void A_self_containing_brush_applied_through_a_Style_settles() => Run(() =>
		Settles((brush, rect) =>
		{
			var style = new Style(typeof(Rectangle));
			style.Setters.Add(new Setter(Shape.FillProperty, brush));
			rect.Style = style;
			return null!;
		}));

	[Fact]
	public void A_self_containing_brush_nested_in_a_DrawingBrush_settles() => Run(() =>
		Settles((brush, rect) => new DrawingBrush(new GeometryDrawing(
			brush, null, new RectangleGeometry(new Rect(0, 0, 10, 10))))));

	[Fact]
	public void A_self_containing_brush_set_locally_settles() => Run(() =>
		Settles((brush, rect) => brush));
}
