using System;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.WindowSizing;

/// <summary>
/// WPF measures a SizeToContent window's content with an infinite constraint, so a ScrollViewer reports its
/// content's size and the window grows to it (up to Max and the work area); GTK's natural size for a scrolled
/// window is not the content's, so the window is sized from the managed Measure.
/// </summary>
public class WindowSizeToContentScrollViewerGtkTests : WpfGtkTestBase
{
	static (ScrollViewer sv, WpfWindow win) Show(WpfWindow win, ScrollViewer sv)
	{
		win.Content = sv;
		win.Show();
		GtkPump.Until(() => sv.Widget.IsMapped && sv.Widget.Height > 0);
		GtkPump.For(300);
		return (sv, win);
	}

	[Fact]
	public void WidthAndHeight_around_a_ScrollViewer_fits_the_content() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow { SizeToContent = SizeToContent.WidthAndHeight },
			new ScrollViewer { Content = new Border { Width = 520, Height = 400 } });
		try
		{
			Assert.True(sv.Widget.Height >= 399, $"viewer height {sv.Widget.Height}");
			Assert.True(sv.Widget.Width >= 519, $"viewer width {sv.Widget.Width}");
			Assert.True(WindowExtent.Of(win).H < SystemParameters.WorkArea.Height);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Height_around_a_ScrollViewer_keeps_the_default_width() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow { SizeToContent = SizeToContent.Height },
			new ScrollViewer { Content = new Border { Width = 300, Height = 380 } });
		try
		{
			double expected = WpfWindow.DefaultExtent(SystemParameters.WorkArea.Width);
			Assert.InRange(WindowExtent.Of(win).W, expected - 1, expected + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Height_around_a_ScrollViewer_keeps_the_explicit_width() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow { SizeToContent = SizeToContent.Height, Width = 640 },
			new ScrollViewer { Content = new Border { Width = 300, Height = 380 } });
		try
		{
			Assert.True(sv.Widget.Height >= 379, $"viewer height {sv.Widget.Height}");
			Assert.InRange(WindowExtent.Of(win).W, 639, 641);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Content_taller_than_MaxHeight_scrolls_inside_the_capped_window() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (sv, win) = Show(new WpfWindow { SizeToContent = SizeToContent.Height, MaxHeight = 420 },
			new ScrollViewer { Content = new Border { Width = 300, Height = 3000 } });
		try
		{
			Assert.InRange(WindowExtent.Of(win).H, 300, 421);
			var adj = ((Gtk.ScrolledWindow)sv.Widget).Vadjustment;
			Assert.True(adj.Upper > adj.PageSize);
		}
		finally { win.Close(); }
	});
}
