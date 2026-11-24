using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>Tile-brush backgrounds and OpacityMask on native containers, read back from the GTK snapshot.</summary>
public class BackdropPaintTests : WpfGtkTestBase
{
	static DrawingBrush Solid(Color c) => new DrawingBrush(new GeometryDrawing
	{
		Brush = new SolidColorBrush(c), Geometry = new RectangleGeometry(new Rect(0, 0, 10, 10)),
	});

	// Renders the widget with GTK's own snapshot path into a Cairo surface.
	static Cairo.ImageSurface Render(Gtk.Widget widget)
	{
		int w = widget.Width, h = widget.Height;
		using var paintable = new Gtk.WidgetPaintable(widget);
		using var snapshot = new Gtk.Snapshot();
		paintable.Snapshot(snapshot, w, h);
		using var node = snapshot.FreeToNode();
		var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
		using var cr = new Cairo.Context(surface);
		node.Draw(cr);
		return surface;
	}

	static (byte R, byte G, byte B, byte A) At(Cairo.ImageSurface s, int x, int y)
	{
		s.Flush();
		var d = s.Data;
		int i = y * s.Stride + x * 4;
		return (d[i + 2], d[i + 1], d[i], d[i + 3]);
	}

	static T Shown<T>(T panel, System.Action<Cairo.ImageSurface> check) where T : FrameworkElement
	{
		var win = new WpfWindow { Content = panel, Width = 200, Height = 200 };
		try
		{
			win.Show();
			GtkPump.For(400);
			using var s = Render(panel.Widget);
			check(s);
		}
		finally { win.Close(); }
		return panel;
	}

	[Fact]
	public void A_StackPanel_with_a_tiled_DrawingBrush_paints_it() => Run(() =>
		Shown(new StackPanel { Width = 100, Height = 100, Background = Solid(Colors.Red) }, s =>
		{
			var p = At(s, 50, 50);
			Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p.ToString());
		}));

	[Fact]
	public void A_Grid_background_paints_behind_its_children() => Run(() =>
	{
		var grid = new Grid { Width = 100, Height = 100, Background = Solid(Colors.Blue) };
		grid.Children.Add(new Border { Width = 20, Height = 20, Background = Brushes.Red, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top });
		Shown(grid, s =>
		{
			var bg = At(s, 80, 80);
			Assert.True(bg.B > 200 && bg.R < 50, bg.ToString());
			var child = At(s, 5, 5);
			Assert.True(child.R > 200 && child.B < 50, child.ToString());
		});
	});

	[Fact]
	public void A_Canvas_background_paints() => Run(() =>
		Shown(new Canvas { Width = 100, Height = 100, Background = Solid(Colors.Red) }, s =>
		{
			var p = At(s, 50, 50);
			Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p.ToString());
		}));

	[Fact]
	public void A_Border_with_a_VisualBrush_background_paints() => Run(() =>
	{
		var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
		Shown(new Border { Width = 100, Height = 100, Background = new VisualBrush(source) }, s =>
		{
			var p = At(s, 50, 50);
			Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p.ToString());
		});
	});

	[Fact]
	public void A_Canvas_OpacityMask_fades_the_content_towards_transparent() => Run(() =>
	{
		var mask = new LinearGradientBrush(Colors.Black, Colors.Transparent, 90);
		Shown(new Canvas { Width = 100, Height = 100, Background = Solid(Colors.Red), OpacityMask = mask }, s =>
		{
			var top = At(s, 50, 3);
			var bottom = At(s, 50, 96);
			Assert.True(top.A > 200, top.ToString());
			Assert.True(bottom.A < 60, bottom.ToString());
		});
	});
}
