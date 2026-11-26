using System.Windows;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for window chrome DPs: asserts GTK property mappings when a
/// native <see cref="Gtk.Window"/> is available, and that chrome set before
/// <see cref="WpfWindow.Widget"/> is created is applied at materialisation.
/// </summary>
public class WindowChromeGtkTests : WpfGtkTestBase
{
	// ── WindowState GTK mapping ───────────────────────────────────────────────

	[Fact]
	public void WindowState_Normal_applied_to_gtk_window() => Run(() =>
	{
		var win = new WpfWindow { Title = "StateNormal" };
		_ = win.Widget;
		win.WindowState = WindowState.Normal;
		Assert.False(((Gtk.Window)win.Widget).Maximized);
	});

	[Fact]
	public void WindowState_Maximized_applied_to_gtk_window() => Run(() =>
	{
		var win = new WpfWindow { Title = "StateMax" };
		_ = win.Widget;
		win.WindowState = WindowState.Maximized;
		Assert.True(((Gtk.Window)win.Widget).Maximized);
	});

	// ── ResizeMode GTK mapping ────────────────────────────────────────────────

	[Fact]
	public void ResizeMode_NoResize_sets_gtk_not_resizable() => Run(() =>
	{
		var win = new WpfWindow { Title = "NoResize" };
		_ = win.Widget;
		win.ResizeMode = ResizeMode.NoResize;
		Assert.False(((Gtk.Window)win.Widget).Resizable);
	});

	[Fact]
	public void ResizeMode_CanResize_sets_gtk_resizable() => Run(() =>
	{
		var win = new WpfWindow { Title = "CanResize" };
		_ = win.Widget;
		win.ResizeMode = ResizeMode.CanResize;
		Assert.True(((Gtk.Window)win.Widget).Resizable);
	});

	[Fact]
	public void ResizeMode_CanMinimize_sets_gtk_not_resizable() => Run(() =>
	{
		var win = new WpfWindow { Title = "CanMinimize" };
		_ = win.Widget;
		win.ResizeMode = ResizeMode.CanMinimize;
		Assert.False(((Gtk.Window)win.Widget).Resizable);
	});

	// ── WindowStyle GTK mapping ───────────────────────────────────────────────

	[Fact]
	public void WindowStyle_None_sets_gtk_not_decorated() => Run(() =>
	{
		var win = new WpfWindow { Title = "StyleNone" };
		_ = win.Widget;
		win.WindowStyle = WindowStyle.None;
		Assert.False(((Gtk.Window)win.Widget).Decorated);
	});

	[Fact]
	public void WindowStyle_SingleBorderWindow_sets_gtk_decorated() => Run(() =>
	{
		var win = new WpfWindow { Title = "StyleSingle" };
		_ = win.Widget;
		win.WindowStyle = WindowStyle.SingleBorderWindow;
		Assert.True(((Gtk.Window)win.Widget).Decorated);
	});

	[Fact]
	public void WindowStyle_ThreeDBorderWindow_sets_gtk_decorated() => Run(() =>
	{
		var win = new WpfWindow { Title = "Style3D" };
		_ = win.Widget;
		win.WindowStyle = WindowStyle.ThreeDBorderWindow;
		Assert.True(((Gtk.Window)win.Widget).Decorated);
	});

	// ── Deferral: chrome set before CreateWidget applied at materialisation ───

	[Fact]
	public void Chrome_set_before_CreateWidget_is_applied_at_realize() => Run(() =>
	{
		// Set chrome DPs before the backing GTK window is created.
		var win = new WpfWindow
		{
			Title = "DeferredChrome",
			WindowStyle = WindowStyle.None,
			ResizeMode = ResizeMode.NoResize,
			WindowState = WindowState.Maximized,
		};

		// Materialising Widget triggers CreateWidget → ApplyChromeToWindow.
		var gtkWin = (Gtk.Window)win.Widget;

		Assert.False(gtkWin.Decorated);
		Assert.False(gtkWin.Resizable);
		Assert.True(gtkWin.Maximized);
	});
}
