using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>Viewbox selection, device-resolution tiles, brush Opacity and the Backdrop containers' stability.</summary>
public class VisualBrushAcceptanceTests : WpfGtkTestBase
{
	static (byte R, byte G, byte B, byte A) At(Cairo.ImageSurface s, int x, int y)
	{
		s.Flush();
		var d = s.Data;
		int i = y * s.Stride + x * 4;
		return (d[i + 2], d[i + 1], d[i], d[i + 3]);
	}

	static Grid TwoHalves()
	{
		var g = new Grid { Width = 100, Height = 100 };
		g.ColumnDefinitions.Add(new ColumnDefinition());
		g.ColumnDefinitions.Add(new ColumnDefinition());
		var blue = new Border { Background = Brushes.Blue };
		Grid.SetColumn(blue, 1);
		g.Children.Add(new Border { Background = Brushes.Red });
		g.Children.Add(blue);
		return g;
	}

	static void WithShownSource(System.Action<Grid, WpfWindow> body)
	{
		var source = TwoHalves();
		var host = new StackPanel();
		host.Children.Add(source);
		var win = new WpfWindow { Content = host, Width = 300, Height = 300 };
		try
		{
			win.Show();
			GtkPump.For(300);
			source.UpdateLayout();
			body(source, win);
		}
		finally { win.Close(); }
	}

	static Cairo.ImageSurface Paint(Rectangle r, int size = 100)
	{
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, size, size);
		using var cr = new Cairo.Context(surface);
		r.PaintTo(cr, size, size);
		return surface;
	}

	[Fact]
	public void A_relative_Viewbox_selects_only_the_left_half() => Run(() => WithShownSource((source, _) =>
	{
		var brush = new VisualBrush(source) { Viewbox = new Rect(0, 0, 0.5, 1) };
		using var s = Paint(new Rectangle { Fill = brush });
		foreach (int x in new[] { 5, 50, 95 })
		{
			var p = At(s, x, 50);
			Assert.True(p.R > 200 && p.B < 50, p.ToString());
		}
	}));

	[Fact]
	public void An_absolute_Viewbox_selects_only_the_right_half() => Run(() => WithShownSource((source, _) =>
	{
		var brush = new VisualBrush(source)
		{
			ViewboxUnits = BrushMappingMode.Absolute, Viewbox = new Rect(50, 0, 50, 100),
		};
		using var s = Paint(new Rectangle { Fill = brush });
		foreach (int x in new[] { 5, 50, 95 })
		{
			var p = At(s, x, 50);
			Assert.True(p.B > 200 && p.R < 50, p.ToString());
		}
	}));

	[Fact]
	public void A_VisualBrush_inside_a_scaled_DrawingBrush_keeps_a_sharp_edge() => Run(() => WithShownSource((source, _) =>
	{
		var drawing = new GeometryDrawing
		{
			Brush = new VisualBrush(source), Geometry = new RectangleGeometry(new Rect(0, 0, 1, 1)),
		};
		using var s = Paint(new Rectangle { Fill = new DrawingBrush(drawing) });
		var left = At(s, 40, 50);
		var right = At(s, 60, 50);
		Assert.True(left.R > 200 && left.B < 50, left.ToString());
		Assert.True(right.B > 200 && right.R < 50, right.ToString());
	}));

	[Fact]
	public void Brush_Opacity_half_of_red_over_white_is_pink() => Run(() => WithShownSource((source, _) =>
	{
		var brush = new VisualBrush(source) { Opacity = 0.5, Viewbox = new Rect(0, 0, 0.5, 1) };
		using var s = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using (var cr = new Cairo.Context(s))
		{
			cr.SetSourceRGB(1, 1, 1);
			cr.Paint();
			new Rectangle { Fill = brush }.PaintTo(cr, 100, 100);
		}
		var p = At(s, 50, 50);
		Assert.InRange(p.R, 250, 255);
		Assert.InRange(p.G, 118, 138);
		Assert.InRange(p.B, 118, 138);
	}));

	[Fact]
	public void Unparented_source_paints_no_black_and_survives_a_repaint() => Run(() =>
	{
		var brush = new VisualBrush(new Border { Width = 30, Height = 30, Background = Brushes.Red });
		using var first = Paint(new Rectangle { Fill = brush });
		var p = At(first, 50, 50);
		Assert.False(p.A == 255 && p.R < 10 && p.G < 10 && p.B < 10, p.ToString());
	});

	[Fact]
	public void TryGetLiveSurface_returns_the_source_pixels() => Run(() => WithShownSource((source, _) =>
	{
		var brush = new VisualBrush(source);
		Assert.True(brush.TryGetLiveSurface(out var surface, out var bounds));
		Assert.Equal(new Rect(0, 0, 100, 100), bounds);
		var p = At(surface, 10, 50);
		Assert.True(p.R > 200 && p.B < 50, p.ToString());
	}));

	[Fact]
	public void A_Backdrop_container_keeps_CSS_name_selectors_working() => Run(() =>
	{
		var panel = new StackPanel { Width = 50, Height = 50, Background = Brushes.Green };
		var win = new WpfWindow { Content = panel, Width = 200, Height = 200 };
		try
		{
			win.Show();
			GtkPump.For(200);
			Assert.False(ElementCssStyle.IsUnnamed(panel.Widget.Name), panel.Widget.Name);
			Assert.True(ElementCssStyle.IsUnnamed("GtkBackdropBox"));
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Repainting_many_masked_and_tiled_containers_across_window_open_and_close_is_stable() => Run(() =>
	{
		var mask = new LinearGradientBrush(Colors.Black, Colors.Transparent, 90);
		for (int round = 0; round < 6; round++)
		{
			var stack = new StackPanel();
			for (int i = 0; i < 25; i++)
			{
				var tile = new DrawingBrush(new GeometryDrawing
				{
					Brush = Brushes.Red, Geometry = new RectangleGeometry(new Rect(0, 0, 5, 5)),
				});
				stack.Children.Add(new Canvas { Width = 40, Height = 8, Background = tile, OpacityMask = mask });
				stack.Children.Add(new Border { Width = 40, Height = 8, Background = tile, OpacityMask = mask });
			}
			var win = new WpfWindow { Content = stack, Width = 200, Height = 600 };
			win.Show();
			GtkPump.For(60);
			stack.Children[0].InvalidateVisual();
			GtkPump.For(60);
			win.Close();
			GtkPump.For(30);
		}
	});
}
