using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.Lifecycle;

/// <summary>
/// Real-GTK tests for the live size path: ActualWidth is the widget's border box (not GTK's
/// content box), a managed UpdateLayout on a shown window does not fight the GTK allocation, and
/// a Content swap on a loaded window re-raises ContentRendered.
/// </summary>
public class WindowLifecycleGtkSizeTests : WpfGtkTestBase
{
	static (WpfWindow win, Button child) ShowWindow(string title)
	{
		var child = new Button { Content = "Hello" };
		var win = new WpfWindow { Title = title, Width = 320, Height = 200, Content = child };
		win.Show();
		return (win, child);
	}

	[Fact]
	public void ActualWidth_is_the_border_box_including_CSS_padding_and_border() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, child) = ShowWindow("LifecycleBorderBox");
		try
		{
			GtkState.Require(() => child.ActualWidth > 0 && win.ActualWidth > 0, "non-zero ActualWidth", win, child);
			GtkPump.For(100);

			string State() => GtkState.Describe(win, child);
			if (!child.Widget.ComputeBounds(child.Widget, out var bounds))
				Assert.Fail("ComputeBounds failed. " + State());
			Assert.Equal(bounds.Width, child.ActualWidth);
			Assert.Equal(bounds.Height, child.ActualHeight);
			// A themed Gtk.Button has CSS padding + border: gtk_widget_get_width (the content box)
			// is narrower than what is painted, and WPF's ActualWidth includes the element's border.
			if (child.ActualWidth <= child.Widget.Width)
				Assert.Fail($"ActualWidth {child.ActualWidth} <= content-box width {child.Widget.Width}. {State()}");
			if (child.ActualHeight <= child.Widget.Height)
				Assert.Fail($"ActualHeight {child.ActualHeight} <= content-box height {child.Widget.Height}. {State()}");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void UpdateLayout_on_a_shown_window_raises_no_SizeChanged() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, child) = ShowWindow("LifecycleUpdateLayout");
		try
		{
			GtkState.Require(() => child.ActualWidth > 0 && win.ActualWidth > 0, "non-zero ActualWidth", win, child);
			GtkPump.For(100);
			int changes = 0;
			win.SizeChanged += (_, _) => changes++;
			child.SizeChanged += (_, _) => changes++;
			var before = (win.ActualWidth, child.ActualWidth);

			win.InvalidateMeasure();
			win.UpdateLayout();
			child.InvalidateMeasure();
			child.UpdateLayout();
			GtkPump.For(100);

			Assert.Equal(0, changes);
			Assert.Equal(before, (win.ActualWidth, child.ActualWidth));
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Content_swap_on_a_loaded_window_raises_ContentRendered_again() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var (win, _) = ShowWindow("LifecycleContentRenderedAgain");
		int rendered = 0;
		win.ContentRendered += (_, _) => rendered++;
		try
		{
			Assert.True(GtkPump.Until(() => rendered == 1));
			GtkPump.For(100);
			Assert.Equal(1, rendered);

			win.Content = new Border();

			Assert.True(GtkPump.Until(() => rendered == 2), $"rendered={rendered}");
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Window_without_Content_raises_no_ContentRendered() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { Title = "LifecycleNoContent", Width = 200, Height = 120 };
		int rendered = 0;
		win.ContentRendered += (_, _) => rendered++;
		win.Show();
		try
		{
			GtkState.Require(() => win.IsLoaded, "window Loaded", win);
			GtkPump.For(150);
			Assert.Equal(0, rendered);
		}
		finally { win.Close(); }
	});
}
