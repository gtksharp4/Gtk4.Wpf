using System;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.WindowSizing;

/// <summary>A Manual window with NaN Width/Height keeps its default size and clips larger content (as in WPF).</summary>
public class WindowNanClipGtkTests : WpfGtkTestBase
{
	static double DefaultW => Math.Floor(SystemParameters.WorkArea.Width * 3 / 4);

	static double DefaultH => Math.Floor(SystemParameters.WorkArea.Height * 3 / 4);

	static (int W, int H) Open(WpfWindow win)
	{
		win.Show();
		GtkPump.Until(() => win.Widget.IsMapped && WindowExtent.Of(win).H > 0);
		GtkPump.For(300);
		return WindowExtent.Of(win);
	}

	[Fact]
	public void Content_larger_than_the_default_size_does_not_grow_a_NaN_size_window() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { Content = new Border { Width = 6000, Height = 5000 } };
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, DefaultW - 1, DefaultW + 1);
			Assert.InRange(h, DefaultH - 1, DefaultH + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_NaN_axis_that_SizeToContent_sizes_still_follows_the_content() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.Width,
			Height = 300,
			Content = new Border { Width = 500, Height = 100 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 499, 520);
			Assert.InRange(h, 299, 301);
		}
		finally { win.Close(); }
	});

	// GTK's set_default_size does not resize a mapped window on every backend (the Win32 one ignores it), so the test
	// checks what allows a user resize: the window is neither pinned to a size nor held above a small minimum by
	// its content.
	[Fact]
	public void A_clipped_NaN_size_window_keeps_no_size_pin_and_a_small_minimum() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { Content = new Border { Width = 6000, Height = 5000 } };
		try
		{
			Open(win);
			var gtkWin = (Gtk.Window)win.Widget;
			gtkWin.GetSizeRequest(out int reqW, out int reqH);
			Assert.Equal((-1, -1), (reqW, reqH));
			gtkWin.Measure(Gtk.Orientation.Horizontal, -1, out int minW, out _, out _, out _);
			gtkWin.Measure(Gtk.Orientation.Vertical, -1, out int minH, out _, out _, out _);
			Assert.InRange(minW, 0, 600);
			Assert.InRange(minH, 0, 400);
		}
		finally { win.Close(); }
	});

	// WPF measures the NaN height at the default while SizeToContent=Width follows the content's width.
	[Fact]
	public void SizeToContent_Width_with_a_NaN_height_follows_the_width_and_clips_the_height() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.Width,
			Content = new Border { Width = 500, Height = 5000 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 499, 520);
			Assert.InRange(h, DefaultH - 1, DefaultH + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void The_window_clip_host_never_scrolls_for_ScrollIntoView_or_wheel_range() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var list = new ListBox();
		for (int i = 0; i < 200; i++) list.Items.Add("Item " + i);
		var win = new WpfWindow { Content = new StackPanel { Children = { list } } };
		try
		{
			Open(win);
			var host = Assert.IsType<WpfWindow.ClipHost>(((Gtk.Window)win.Widget).Child);
			list.ScrollIntoView("Item 199");
			GtkPump.For(300);
			Assert.Equal(0, host.Vadjustment.Value);
			host.Vadjustment.Value = 500; // what a wheel or focus scroll would do
			Assert.Equal(0, host.Vadjustment.Value);
			Assert.Equal(0, host.Hadjustment.Value);
		}
		finally { win.Close(); }
	});
}
