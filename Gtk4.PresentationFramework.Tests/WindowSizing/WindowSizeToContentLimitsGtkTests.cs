using System;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.WindowSizing;

/// <summary>MinWidth/MaxWidth/MinHeight/MaxHeight clamp an axis that SizeToContent sizes, as in WPF.</summary>
public class WindowSizeToContentLimitsGtkTests : WpfGtkTestBase
{
	static (int W, int H) Open(WpfWindow win)
	{
		win.Show();
		GtkPump.Until(() => win.Widget.IsMapped && WindowExtent.Of(win).H > 0);
		GtkPump.For(300);
		return WindowExtent.Of(win);
	}

	[Fact]
	public void MaxWidth_caps_a_SizeToContent_width_below_the_content() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.WidthAndHeight,
			MaxWidth = 260,
			Content = new Border { Width = 500, Height = 120 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 259, 261);
			Assert.InRange(h, 119, 220);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void MaxHeight_caps_a_SizeToContent_height_below_the_content() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.WidthAndHeight,
			MaxHeight = 150,
			Content = new Border { Width = 200, Height = 600 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(h, 149, 151);
			Assert.InRange(w, 199, 420);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void MaxHeight_on_SizeToContent_Height_keeps_the_default_width() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.Height,
			MaxHeight = 150,
			Content = new Border { Width = 200, Height = 600 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(h, 149, 151);
			double expected = WpfWindow.DefaultExtent(SystemParameters.WorkArea.Width);
			Assert.InRange(w, expected - 1, expected + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void MinWidth_raises_a_SizeToContent_width_above_the_content() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.WidthAndHeight,
			MinWidth = 480,
			Content = new Border { Width = 100, Height = 80 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 479, 481);
			Assert.InRange(h, 79, 180);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void MinHeight_raises_a_SizeToContent_height_above_the_content() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.WidthAndHeight,
			MinHeight = 400,
			Content = new Border { Width = 100, Height = 80 },
		};
		try
		{
			var (_, h) = Open(win);
			Assert.InRange(h, 399, 401);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void The_limits_apply_to_the_SizeToContent_axis_only() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.Width,
			Height = 300,
			MaxWidth = 240,
			Content = new Border { Width = 600, Height = 50 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 239, 241);
			Assert.InRange(h, 299, 301);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Content_inside_the_limits_keeps_its_natural_size() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.WidthAndHeight,
			MinWidth = 100,
			MaxWidth = 900,
			MinHeight = 50,
			MaxHeight = 700,
			Content = new Border { Width = 300, Height = 200 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 299, 420);
			Assert.InRange(h, 199, 320);
		}
		finally { win.Close(); }
	});
}
