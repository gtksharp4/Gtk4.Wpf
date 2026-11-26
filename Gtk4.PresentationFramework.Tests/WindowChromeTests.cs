using System;
using System.Windows;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no-GTK) tests for window chrome DPs: defaults, metadata, round-trip get/set,
/// and that setting before any native window exists does not throw.
/// </summary>
public class WindowChromeTests
{
	// ── WindowState ───────────────────────────────────────────────────────────

	[Fact]
	public void WindowState_default_is_Normal()
	{
		var win = new WpfWindow();
		Assert.Equal(WindowState.Normal, win.WindowState);
	}

	[Fact]
	public void WindowState_dp_default_metadata_is_Normal()
	{
		var meta = WpfWindow.WindowStateProperty.GetMetadata(typeof(WpfWindow));
		Assert.Equal(WindowState.Normal, meta.DefaultValue);
	}

	[Fact]
	public void WindowState_set_before_native_window_does_not_throw()
	{
		var win = new WpfWindow();
		win.WindowState = WindowState.Maximized;
		Assert.Equal(WindowState.Maximized, win.WindowState);
	}

	[Fact]
	public void WindowState_roundtrip_all_values()
	{
		var win = new WpfWindow();
		foreach (WindowState s in Enum.GetValues(typeof(WindowState)))
		{
			win.WindowState = s;
			Assert.Equal(s, win.WindowState);
		}
	}

	// ── WindowStartupLocation ─────────────────────────────────────────────────

	[Fact]
	public void WindowStartupLocation_default_is_Manual()
	{
		var win = new WpfWindow();
		Assert.Equal(WindowStartupLocation.Manual, win.WindowStartupLocation);
	}

	[Fact]
	public void WindowStartupLocation_dp_default_metadata_is_Manual()
	{
		var meta = WpfWindow.WindowStartupLocationProperty.GetMetadata(typeof(WpfWindow));
		Assert.Equal(WindowStartupLocation.Manual, meta.DefaultValue);
	}

	[Fact]
	public void WindowStartupLocation_set_before_native_window_does_not_throw()
	{
		var win = new WpfWindow();
		win.WindowStartupLocation = WindowStartupLocation.CenterScreen;
		Assert.Equal(WindowStartupLocation.CenterScreen, win.WindowStartupLocation);
	}

	[Fact]
	public void WindowStartupLocation_roundtrip_all_values()
	{
		var win = new WpfWindow();
		foreach (WindowStartupLocation loc in Enum.GetValues(typeof(WindowStartupLocation)))
		{
			win.WindowStartupLocation = loc;
			Assert.Equal(loc, win.WindowStartupLocation);
		}
	}

	// ── ResizeMode ────────────────────────────────────────────────────────────

	[Fact]
	public void ResizeMode_default_is_CanResize()
	{
		var win = new WpfWindow();
		Assert.Equal(ResizeMode.CanResize, win.ResizeMode);
	}

	[Fact]
	public void ResizeMode_dp_default_metadata_is_CanResize()
	{
		var meta = WpfWindow.ResizeModeProperty.GetMetadata(typeof(WpfWindow));
		Assert.Equal(ResizeMode.CanResize, meta.DefaultValue);
	}

	[Fact]
	public void ResizeMode_set_before_native_window_does_not_throw()
	{
		var win = new WpfWindow();
		win.ResizeMode = ResizeMode.NoResize;
		Assert.Equal(ResizeMode.NoResize, win.ResizeMode);
	}

	[Fact]
	public void ResizeMode_roundtrip_all_values()
	{
		var win = new WpfWindow();
		foreach (ResizeMode m in Enum.GetValues(typeof(ResizeMode)))
		{
			win.ResizeMode = m;
			Assert.Equal(m, win.ResizeMode);
		}
	}

	// ── WindowStyle ───────────────────────────────────────────────────────────

	[Fact]
	public void WindowStyle_default_is_SingleBorderWindow()
	{
		var win = new WpfWindow();
		Assert.Equal(WindowStyle.SingleBorderWindow, win.WindowStyle);
	}

	[Fact]
	public void WindowStyle_dp_default_metadata_is_SingleBorderWindow()
	{
		var meta = WpfWindow.WindowStyleProperty.GetMetadata(typeof(WpfWindow));
		Assert.Equal(WindowStyle.SingleBorderWindow, meta.DefaultValue);
	}

	[Fact]
	public void WindowStyle_set_before_native_window_does_not_throw()
	{
		var win = new WpfWindow();
		win.WindowStyle = WindowStyle.None;
		Assert.Equal(WindowStyle.None, win.WindowStyle);
	}

	[Fact]
	public void WindowStyle_roundtrip_all_values()
	{
		var win = new WpfWindow();
		foreach (WindowStyle s in Enum.GetValues(typeof(WindowStyle)))
		{
			win.WindowStyle = s;
			Assert.Equal(s, win.WindowStyle);
		}
	}

	// ── SizeToContent ─────────────────────────────────────────────────────────

	[Fact]
	public void SizeToContent_default_is_Manual()
	{
		var win = new WpfWindow();
		Assert.Equal(SizeToContent.Manual, win.SizeToContent);
	}

	[Fact]
	public void SizeToContent_dp_default_metadata_is_Manual()
	{
		var meta = WpfWindow.SizeToContentProperty.GetMetadata(typeof(WpfWindow));
		Assert.Equal(SizeToContent.Manual, meta.DefaultValue);
	}

	[Fact]
	public void SizeToContent_set_before_native_window_does_not_throw()
	{
		var win = new WpfWindow();
		win.SizeToContent = SizeToContent.WidthAndHeight;
		Assert.Equal(SizeToContent.WidthAndHeight, win.SizeToContent);
	}

	[Fact]
	public void SizeToContent_roundtrip_all_values()
	{
		var win = new WpfWindow();
		foreach (SizeToContent stc in Enum.GetValues(typeof(SizeToContent)))
		{
			win.SizeToContent = stc;
			Assert.Equal(stc, win.SizeToContent);
		}
	}
}
