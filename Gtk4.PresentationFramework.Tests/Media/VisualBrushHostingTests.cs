using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>Which window hosts a detached source, windowless painting, and a brush that stays live.</summary>
public class VisualBrushHostingTests : WpfGtkTestBase
{
	static (byte R, byte G, byte B, byte A) Sample(Rectangle rect, int x, int y)
	{
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, 100, 100);
		using (var cr = new Cairo.Context(surface))
			rect.PaintTo(cr, 100, 100);
		surface.Flush();
		var d = surface.Data;
		int i = y * surface.Stride + x * 4;
		return (d[i + 2], d[i + 1], d[i], d[i + 3]);
	}

	static WpfWindow Open()
	{
		var win = new WpfWindow { Content = new StackPanel(), Width = 300, Height = 300 };
		win.Show();
		GtkPump.For(300);
		return win;
	}

	static VisualBrush Corner(UIElement source) => new VisualBrush(source)
	{
		Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top,
	};

	static bool IsBlue((byte R, byte G, byte B, byte A) p) => p.B > 200 && p.R < 50 && p.A > 200;

	[Fact]
	public void The_source_is_hosted_in_the_window_that_paints_the_brush() => Run(() =>
	{
		var a = Open();
		var b = Open();
		try
		{
			var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
			var rect = new Rectangle { Width = 50, Height = 50, Fill = Corner(source) };
			((StackPanel)a.Content!).Children.Add(rect);
			Assert.True(GtkPump.Until(() => source.HasBackingWidget && source.Widget.Root == a.Widget, 3000),
				"the source is not hosted in the window that paints it");
			b.Close();
			GtkPump.For(300);
			source.Background = Brushes.Blue;
			Assert.True(GtkPump.Until(() => IsBlue(Sample(rect, 10, 10)), 3000), "the change never reached the brush");
		}
		finally { a.Close(); }
	});

	[Fact]
	public void A_source_is_hosted_again_when_its_window_closes() => Run(() =>
	{
		var a = Open();
		var b = Open();
		try
		{
			var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
			var rect = new Rectangle { Fill = Corner(source) };
			Sample(rect, 10, 10);
			GtkPump.For(500);
			b.Close();
			GtkPump.For(300);
			source.Background = Brushes.Blue;
			Sample(rect, 10, 10);
			Assert.True(GtkPump.Until(() => IsBlue(Sample(rect, 10, 10)), 3000));
		}
		finally { a.Close(); }
	});

	[Fact]
	public void A_settled_brush_is_hosted_again_when_its_host_window_closes() => Run(() =>
	{
		var a = Open();
		var b = Open();
		try
		{
			var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
			var brush = Corner(source);
			var inA = new Rectangle { Width = 50, Height = 50, Fill = brush };
			var inB = new Rectangle { Width = 50, Height = 50, Fill = brush };
			((StackPanel)a.Content!).Children.Add(inA);
			((StackPanel)b.Content!).Children.Add(inB);
			Assert.True(GtkPump.Until(() => source.HasBackingWidget && source.Widget.Root != null, 3000));
			GtkPump.For(1500);
			var host = source.Widget.Root == a.Widget ? a : b;
			var kept = host == a ? inB : inA;
			host.Close();
			GtkPump.For(300);
			source.Background = Brushes.Blue;
			Assert.True(GtkPump.Until(() => IsBlue(Sample(kept, 10, 10)), 3000), "the settled brush froze");
		}
		finally { a.Close(); b.Close(); }
	});

	static (byte R, byte G, byte B, byte A) OnScreen(Gtk.Widget widget, int x, int y)
	{
		int w = Math.Max(1, widget.Width), h = Math.Max(1, widget.Height);
		var snapshot = new Gtk.Snapshot();
		new Gtk.WidgetPaintable(widget).Snapshot(snapshot, w, h);
		using var surface = new Cairo.ImageSurface(Cairo.Format.Argb32, w, h);
		var node = snapshot.FreeToNode();
		if (node != null)
			using (var cr = new Cairo.Context(surface))
				node.Draw(cr);
		surface.Flush();
		var d = surface.Data;
		int i = y * surface.Stride + x * 4;
		return (d[i + 2], d[i + 1], d[i], d[i + 3]);
	}

	// The shape is the window content, so no panel records the painting window and the source falls back
	// to the newest window (B); after it settles, B closes and a change must still reach A.
	[Fact]
	public void A_settled_brush_whose_fallback_host_closes_follows_a_change() => Run(() =>
	{
		var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
		var rect = new Rectangle { Width = 60, Height = 60, Fill = Corner(source) };
		var a = new WpfWindow { Width = 200, Height = 200 };
		a.Show();
		GtkPump.For(500);
		var b = Open();
		try
		{
			a.Content = rect;
			GtkPump.For(1500);
			b.Close();
			GtkPump.For(800);
			source.Background = Brushes.Blue;
			Assert.True(GtkPump.Until(() => IsBlue(OnScreen(rect.Widget, 10, 10)), 3000), "the settled brush froze");
		}
		finally { a.Close(); }
	});

	[Fact]
	public void A_detached_source_paints_with_no_window_at_all() => Run(() =>
	{
		var source = new Border { Width = 60, Height = 60, Background = Brushes.Red };
		var rect = new Rectangle { Fill = new VisualBrush(source) };
		var p = Sample(rect, 50, 50);
		Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p + " mapped=" + source.Widget.IsMapped + " vis=" + source.Widget.Visible + " w=" + source.Widget.Width + " rs=" + source.RenderSize);
	});

	[Fact]
	public void A_self_containing_brush_still_follows_a_change_after_it_settles() => Run(() =>
	{
		var win = Open();
		try
		{
			var text = new TextBlock { Text = "one" };
			var panel = new StackPanel { Width = 120, Height = 80, Background = Brushes.White };
			var brush = new VisualBrush(panel);
			panel.Children.Add(text);
			panel.Children.Add(new Rectangle { Width = 40, Height = 20, Fill = brush });
			((StackPanel)win.Content!).Children.Add(panel);
			GtkPump.For(1500);
			int changed = 0;
			brush.Changed += (_, _) => changed++;
			text.Text = "two and a longer text";
			Assert.True(GtkPump.Until(() => changed > 0, 3000), "the brush never noticed the change");
			GtkPump.For(1500);
			int before = changed;
			text.Text = "three";
			Assert.True(GtkPump.Until(() => changed > before, 3000), "the brush missed a change after settling");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void No_op_redraws_do_not_hide_a_real_change() => Run(() =>
	{
		var win = Open();
		try
		{
			var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
			var rect = new Rectangle { Fill = Corner(source) };
			Sample(rect, 10, 10);
			GtkPump.For(800);
			for (int i = 0; i < 3; i++) source.Widget.QueueDraw();
			source.Background = Brushes.Blue;
			Assert.True(GtkPump.Until(() => IsBlue(Sample(rect, 10, 10)), 3000), "the change was dropped");
		}
		finally { win.Close(); }
	});
}
