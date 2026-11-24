using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>A VisualBrush of a live element, rendered from its GTK widget.</summary>
public class VisualBrushLiveTests : WpfGtkTestBase
{
	static (byte R, byte G, byte B, byte A) PixelAt(Cairo.ImageSurface s, int x, int y)
	{
		s.Flush();
		byte[] d = s.Data;
		int i = y * s.Stride + x * 4;
		return (d[i + 2], d[i + 1], d[i], d[i + 3]);
	}

	static Grid TwoHalves()
	{
		var g = new Grid { Width = 100, Height = 100 };
		g.ColumnDefinitions.Add(new ColumnDefinition());
		g.ColumnDefinitions.Add(new ColumnDefinition());
		var red = new Border { Background = Brushes.Red };
		var blue = new Border { Background = Brushes.Blue };
		Grid.SetColumn(blue, 1);
		g.Children.Add(red);
		g.Children.Add(blue);
		return g;
	}

	[Fact]
	public void A_shown_element_paints_red_left_and_blue_right() => Run(() =>
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
			var rect = new Rectangle { Fill = new VisualBrush(source) };
			using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
			using (var cr = new Cairo.Context(surface))
				rect.PaintTo(cr, 100, 100);
			var left = PixelAt(surface, 20, 50);
			var right = PixelAt(surface, 80, 50);
			Assert.True(left.R > 200 && left.B < 50, left.ToString());
			Assert.True(right.B > 200 && right.R < 50, right.ToString());
		}
		finally { win.Close(); }
	});

	static (byte R, byte G, byte B, byte A) Sample(Rectangle rect, int x, int y)
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using (var cr = new Cairo.Context(surface))
			rect.PaintTo(cr, 100, 100);
		return PixelAt(surface, x, y);
	}

	[Fact]
	public void Changing_the_source_background_raises_Changed_and_repaints() => Run(() =>
	{
		var source = new Border { Width = 100, Height = 100, Background = Brushes.Red };
		var host = new StackPanel();
		host.Children.Add(source);
		var win = new WpfWindow { Content = host, Width = 300, Height = 300 };
		try
		{
			win.Show();
			GtkPump.For(300);
			var brush = new VisualBrush(source);
			var rect = new Rectangle { Fill = brush };
			Assert.True(Sample(rect, 50, 50).R > 200);
			bool changed = false;
			brush.Changed += (_, _) => changed = true;
			source.Background = Brushes.Blue;
			Assert.True(GtkPump.Until(() => changed, 2000));
			var p = Sample(rect, 50, 50);
			Assert.True(p.B > 200 && p.R < 50, p.ToString());
		}
		finally { win.Close(); }
	});

	[Fact]
	public void An_unparented_element_is_laid_out_and_painted() => Run(() =>
	{
		var win = new WpfWindow { Content = new StackPanel(), Width = 300, Height = 300 };
		win.Show();
		GtkPump.For(300);
		try
		{
			var source = new Border { Width = 60, Height = 60, Background = Brushes.Red };
			var brush = new VisualBrush(source);
			var rect = new Rectangle { Fill = brush };
			bool changed = false;
			brush.Changed += (_, _) => changed = true;
			Sample(rect, 50, 50);
			Assert.True(GtkPump.Until(() => changed, 3000));
			GtkPump.For(800);
			var p = Sample(rect, 50, 50);
			Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p.ToString());
		}
		finally { win.Close(); }
	});
}
