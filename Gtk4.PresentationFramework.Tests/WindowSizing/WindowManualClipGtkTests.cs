using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.WindowSizing;

/// <summary>A Manual-size window keeps its size when the content is larger (WPF clips; GTK would grow).</summary>
public class WindowManualClipGtkTests : WpfGtkTestBase
{
	static (int W, int H) Open(WpfWindow win)
	{
		win.Show();
		GtkPump.Until(() => win.Widget.IsMapped && WindowExtent.Of(win).H > 0);
		GtkPump.For(300);
		return WindowExtent.Of(win);
	}

	[Fact]
	public void Fixed_size_content_larger_than_the_window_does_not_grow_it() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { Width = 400, Height = 300, Content = new Border { Width = 900, Height = 700 } };
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 399, 401);
			Assert.InRange(h, 299, 301);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Height_bound_to_the_window_does_not_grow_the_window_on_every_layout() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var panel = new StackPanel { Height = 264 };
		var win = new WpfWindow { Width = 400, Height = 300, Padding = new Thickness(0, 0, 0, 20), Content = panel };
		win.SizeChanged += (_, _) => panel.Height = win.ActualHeight - 36;
		try
		{
			var (_, h) = Open(win);
			GtkPump.For(500);
			Assert.InRange(WindowExtent.Of(win).H, 299, 301);
		}
		finally { win.Close(); }
	});
}
