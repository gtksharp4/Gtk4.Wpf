using System.Windows;
using System.Windows.Controls;
using Gtk4.Forms.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.Forms.PresentationFramework.Tests.WindowSizing;

/// <summary>A window around a ScrollViewer opens at the default size; a ScrollViewer never grows the window.</summary>
public class ScrollViewerWindowSizeGtkTests : WpfGtkTestBase
{
	static (ScrollViewer sv, WpfWindow win) Show(WpfWindow win, ScrollViewer sv)
	{
		win.Content = sv;
		win.Show();
		GtkPump.For(300);
		return (sv, win);
	}

	[Fact]
	public void SizeToContent_with_an_explicit_Height_keeps_the_ScrollViewer_at_that_height() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow { SizeToContent = SizeToContent.WidthAndHeight },
			new ScrollViewer { Height = 200, Content = new Border { Width = 300, Height = 2000 } });
		try
		{
			Assert.InRange(sv.Widget.Height, 199, 201);
			Assert.True(win.Widget.Height <= SystemParameters.WorkArea.Height, $"{win.Widget.Height}");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void SizeToContent_with_a_MaxHeight_window_stays_within_the_MaxHeight() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow { SizeToContent = SizeToContent.WidthAndHeight, MaxHeight = 500 },
			new ScrollViewer { Content = new Border { Width = 300, Height = 3000 } });
		try
		{
			Assert.True(win.Widget.Height <= 500, $"{win.Widget.Height}");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void An_unsized_window_around_tall_content_opens_at_the_default_and_scrolls() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow(), new ScrollViewer { Content = new Border { Width = 200, Height = 2000 } });
		try
		{
			double expected = System.Math.Floor(SystemParameters.WorkArea.Height * 3 / 4);
			Assert.InRange(win.Widget.Height, expected - 1, expected + 1);
			var adj = ((Gtk.ScrolledWindow)sv.Widget).Vadjustment;
			Assert.True(adj.Upper > adj.PageSize, $"upper {adj.Upper} page {adj.PageSize}");
			Assert.True(sv.Widget.Height < 2000);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void An_explicit_size_window_is_unchanged_by_a_larger_ScrollViewer_content() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow { Width = 400, Height = 300 },
			new ScrollViewer { Content = new Border { Width = 2000, Height = 2000 } });
		try
		{
			Assert.InRange(win.Widget.Width, 399, 401);
			Assert.InRange(win.Widget.Height, 299, 301);
			Assert.True(sv.Widget.Width <= 400 && sv.Widget.Height <= 300);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void The_WPF_defaults_reach_the_GTK_scrolled_window() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow { Width = 400, Height = 300 },
			new ScrollViewer { Content = new TextBlock { Text = "x" } });
		try
		{
			var sw = (Gtk.ScrolledWindow)sv.Widget;
			Assert.Equal(Gtk.PolicyType.External, sw.HscrollbarPolicy); // no bar, no minimum-width pin
			Assert.Equal(Gtk.PolicyType.Always, sw.VscrollbarPolicy);
		}
		finally { win.Close(); }
	});
}
