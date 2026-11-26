using System;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests.WindowSizing;

/// <summary>An unsized Manual window opens at the OS-default-like size, not at its content's size.</summary>
public class WindowDefaultSizeGtkTests : WpfGtkTestBase
{
	static double ExpectedWidth => Math.Floor(SystemParameters.WorkArea.Width * 3 / 4);

	static double ExpectedHeight => Math.Floor(SystemParameters.WorkArea.Height * 3 / 4);

	static (int W, int H) Open(WpfWindow win)
	{
		win.Show();
		GtkPump.Until(() => win.Widget.IsMapped && WindowExtent.Of(win).H > 0);
		GtkPump.For(300);
		return WindowExtent.Of(win);
	}

	static Border Tiny() => new Border { Width = 40, Height = 30 };

	[Fact]
	public void An_unsized_window_opens_at_the_default_size() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { Content = Tiny() };
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, ExpectedWidth - 1, ExpectedWidth + 1);
			Assert.InRange(h, ExpectedHeight - 1, ExpectedHeight + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void Explicit_Width_and_Height_are_unchanged() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { Width = 420, Height = 310, Content = Tiny() };
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 419, 421);
			Assert.InRange(h, 309, 311);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void A_mixed_window_gets_its_explicit_width_and_the_default_height() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { Width = 420, Content = Tiny() };
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 419, 421);
			Assert.InRange(h, ExpectedHeight - 1, ExpectedHeight + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void SizeToContent_WidthAndHeight_still_fits_a_fixed_Border() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow
		{
			SizeToContent = SizeToContent.WidthAndHeight,
			Content = new Border { Width = 300, Height = 200 },
		};
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 295, 420);
			Assert.InRange(h, 195, 320);
			Assert.True(w < ExpectedWidth && h < ExpectedHeight);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void SizeToContent_Width_keeps_the_default_height() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { SizeToContent = SizeToContent.Width, Content = new Border { Width = 300, Height = 20 } };
		try
		{
			var (w, h) = Open(win);
			Assert.InRange(w, 295, 420);
			Assert.InRange(h, ExpectedHeight - 1, ExpectedHeight + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void MaxWidth_below_the_default_clamps_it() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var win = new WpfWindow { MaxWidth = 500, Content = Tiny() };
		try
		{
			var (w, _) = Open(win);
			Assert.InRange(w, 499, 501);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void MinHeight_above_the_default_raises_it() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		double min = ExpectedHeight + 50;
		var win = new WpfWindow { MinHeight = min, Content = Tiny() };
		try
		{
			var (_, h) = Open(win);
			Assert.InRange(h, min - 1, min + 1);
		}
		finally { win.Close(); }
	});

	[Fact]
	public void DefaultExtent_is_three_quarters_of_the_work_area_floored() => Run(() =>
	{
		Assert.Equal(1440, WpfWindow.DefaultExtent(1920));
		Assert.Equal(810, WpfWindow.DefaultExtent(1080));
		Assert.Equal(768, WpfWindow.DefaultExtent(1025));
	});
}
