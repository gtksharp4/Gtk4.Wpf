using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>Detached VisualBrush sources: no extra window, no activation change, release and re-layout.</summary>
public class VisualBrushHostTests : WpfGtkTestBase
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
		Assert.True(GtkPump.Until(() => win.Widget.IsMapped), "window never mapped");
		GtkPump.For(300);
		return win;
	}

	static int VisibleToplevels() => Gtk.Window.ListToplevels().Count(w => w.IsVisible);

	// Closed windows of earlier tests stay listed, and on Xvfb one of them can still report itself active.
	static IntPtr[] OtherActiveToplevels(WpfWindow win) => Gtk.Window.ListToplevels()
		.Where(w => w.Handle != win.Widget.Handle && w is Gtk.Window { IsActive: true })
		.Select(w => w.Handle)
		.ToArray();

	static VisualBrush Corner(UIElement source) => new VisualBrush(source)
	{
		Stretch = Stretch.None, AlignmentX = AlignmentX.Left, AlignmentY = AlignmentY.Top,
	};

	[Fact]
	public void A_detached_source_never_steals_activation_or_adds_a_toplevel() => Run(() =>
	{
		var win = Open();
		try
		{
			// Without a window manager (Xvfb) a shown window is not always activated, and on some Linux runner runs no
			// number of requests makes it so. Where it is active it must stay active; everywhere, no other toplevel (the
			// source's host included) may become active while the brush paints.
			bool active = GtkPump.Until(() =>
			{
				if (!win.IsActive)
					win.Activate();
				return win.IsActive;
			}, 6000);
			Assert.True(active || OperatingSystem.IsLinux(), "the window never became active");
			int toplevels = VisibleToplevels();
			var activeBefore = OtherActiveToplevels(win);
			var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
			var rect = new Rectangle { Fill = Corner(source) };
			Sample(rect, 10, 10);
			GtkPump.For(600);
			Sample(rect, 10, 10);
			source.Background = Brushes.Blue;
			GtkPump.Until(() => { var q = Sample(rect, 10, 10); return q.B > 200 && q.R < 50; });
			var p = Sample(rect, 10, 10);
			Assert.True(p.B > 200 && p.R < 50, p.ToString());
			if (active)
				Assert.True(win.IsActive, "the window lost activation");
			Assert.Empty(OtherActiveToplevels(win).Except(activeBefore));
			Assert.Equal(toplevels, VisibleToplevels());
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_brush_painted_inside_its_own_source_settles() => Run(() =>
	{
		var win = Open();
		try
		{
			var source = new StackPanel { Width = 100, Height = 100, Background = Brushes.White };
			var brush = new VisualBrush(source);
			source.Children.Add(new Rectangle { Width = 50, Height = 50, Fill = brush });
			((StackPanel)win.Content!).Children.Add(source);
			GtkState.Require(() => source.IsWidgetCreated && source.Widget.IsMapped && source.Widget.Width > 0,
				"source allocated", source);
			GtkPump.For(1200);
			int changed = 0;
			brush.Changed += (_, _) => changed++;
			GtkPump.For(1500);
			Assert.InRange(changed, 0, 2);
		}
		finally { win.Close(); }
	});

	[MethodImpl(MethodImplOptions.NoInlining)]
	static (WeakReference Brush, Border Source) PaintAndForget()
	{
		var source = new Border { Width = 30, Height = 30, Background = Brushes.Red };
		var brush = Corner(source);
		var rect = new Rectangle { Fill = brush };
		Sample(rect, 5, 5);
		GtkPump.For(400);
		return (new WeakReference(brush), source);
	}

	[Fact]
	public void The_host_is_released_when_the_last_brush_is_gone() => Run(() =>
	{
		var win = Open();
		try
		{
			var (weak, source) = PaintAndForget();
			Assert.True(DetachedSourceHost.IsHosted(source.Widget), "the source is not hosted");
			// Callbacks still queued on a stalled loop can hold the brush, so collect between iterations until
			// it goes; a real leak still fails below.
			GtkPump.Until(() =>
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				return !weak.IsAlive;
			});
			DetachedSourceHost.Sweep();
			Assert.False(weak.IsAlive, "the brush is still referenced");
			Assert.False(DetachedSourceHost.IsHosted(source.Widget), "the source is still hosted");
			Assert.Null(source.Widget.Parent);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Retargeting_the_brush_releases_the_old_source_and_hosts_the_new_one() => Run(() =>
	{
		var win = Open();
		try
		{
			var first = new Border { Width = 30, Height = 30, Background = Brushes.Red };
			var brush = Corner(first);
			var rect = new Rectangle { Fill = brush };
			Sample(rect, 5, 5);
			GtkPump.Until(() => DetachedSourceHost.IsHosted(first.Widget));
			Assert.True(DetachedSourceHost.IsHosted(first.Widget));
			var second = new Border { Width = 30, Height = 30, Background = Brushes.Blue };
			brush.Visual = second;
			Sample(rect, 5, 5);
			GtkPump.Until(() => DetachedSourceHost.IsHosted(second.Widget) && !DetachedSourceHost.IsHosted(first.Widget)
				&& Sample(rect, 5, 5).B > 200);
			Assert.True(DetachedSourceHost.IsHosted(second.Widget));
			Assert.False(DetachedSourceHost.IsHosted(first.Widget));
			var p = Sample(rect, 5, 5);
			Assert.True(p.B > 200 && p.R < 50, p.ToString());
			Assert.Null(first.Widget.Parent);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Two_brushes_sharing_a_source_both_keep_it_hosted() => Run(() =>
	{
		var win = Open();
		try
		{
			var source = new Border { Width = 30, Height = 30, Background = Brushes.Red };
			var a = Corner(source);
			var b = (VisualBrush)a.Clone();
			var ra = new Rectangle { Fill = a };
			var rb = new Rectangle { Fill = b };
			Sample(ra, 5, 5);
			Sample(rb, 5, 5);
			GtkPump.For(400);
			a.Visual = null;
			GtkPump.For(200);
			var p = Sample(rb, 5, 5);
			Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p.ToString());
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_detached_source_is_laid_out_again_after_it_changes_size() => Run(() =>
	{
		var win = Open();
		try
		{
			var source = new Border { Width = 40, Height = 40, Background = Brushes.Red };
			var rect = new Rectangle { Fill = Corner(source) };
			Sample(rect, 10, 10);
			GtkPump.For(500);
			Assert.Equal(0, Sample(rect, 70, 10).A);
			source.Width = 80;
			GtkPump.Until(() => { var q = Sample(rect, 70, 10); return q.R > 200 && q.B < 50 && q.A > 200; });
			var p = Sample(rect, 70, 10);
			Assert.True(p.R > 200 && p.B < 50 && p.A > 200, p.ToString());
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Setting_the_Visual_does_not_create_the_source_widget() => Run(() =>
	{
		var source = new Border { Width = 40, Height = 40 };
		var brush = new VisualBrush { Visual = source };
		Assert.False(source.HasBackingWidget, "the widget was created before the first paint");
	});
}
