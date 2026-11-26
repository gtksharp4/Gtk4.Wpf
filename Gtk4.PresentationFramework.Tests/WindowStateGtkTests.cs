using System.Windows;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// GTK smoke tests for window state features: <see cref="WpfWindow.Activate"/>,
/// <see cref="WpfWindow.WindowState"/> round-trip after realize,
/// <see cref="WpfWindow.Topmost"/>, <see cref="WpfWindow.ShowInTaskbar"/>,
/// and <see cref="WpfWindow.StateChanged"/>.
/// </summary>
public class WindowStateGtkTests : WpfGtkTestBase
{
	// ── Activate() ───────────────────────────────────────────────────────────

	[Fact]
	public void Activate_on_realized_window_does_not_throw() => Run(() =>
	{
		var win = new WpfWindow { Title = "ActivateTest" };
		_ = win.Widget; // materialise native window
		win.Activate(); // calls Present() — must not throw
	});

	// ── WindowState after realize ─────────────────────────────────────────────

	[Fact]
	public void WindowState_Maximized_does_not_throw_on_realized_window() => Run(() =>
	{
		var win = new WpfWindow { Title = "MaxTest" };
		_ = win.Widget;
		win.WindowState = WindowState.Maximized;
		Assert.Equal(WindowState.Maximized, win.WindowState);
	});

	[Fact]
	public void WindowState_Normal_after_Maximized_does_not_throw() => Run(() =>
	{
		var win = new WpfWindow { Title = "NormalTest" };
		_ = win.Widget;
		win.WindowState = WindowState.Maximized;
		win.WindowState = WindowState.Normal;
		Assert.Equal(WindowState.Normal, win.WindowState);
	});

	// ── StateChanged from DP path ─────────────────────────────────────────────

	[Fact]
	public void StateChanged_raised_when_WindowState_set_on_realized_window() => Run(() =>
	{
		var win = new WpfWindow { Title = "StateChangedTest" };
		_ = win.Widget;

		int count = 0;
		win.StateChanged += (_, _) => count++;

		win.WindowState = WindowState.Maximized;
		Assert.True(count >= 1);
	});

	// ── Topmost and ShowInTaskbar pre-realize values survive realize ───────────

	[Fact]
	public void Topmost_and_ShowInTaskbar_set_before_realize_survive_realize() => Run(() =>
	{
		var win = new WpfWindow
		{
			Title = "TopmostTest",
			Topmost = true,
			ShowInTaskbar = false,
		};
		_ = win.Widget; // realize
		Assert.True(win.Topmost);
		Assert.False(win.ShowInTaskbar);
	});

	// ── WM→DP notification wiring doesn't crash ───────────────────────────────

	[Fact]
	public void Notify_wiring_does_not_throw_on_realize() => Run(() =>
	{
		// WireStateNotifications is called from ApplyChromeToWindow (CreateWidget).
		// If the GLib notify subscription fails it throws at realize time.
		var win = new WpfWindow { Title = "NotifyWireTest" };
		_ = win.Widget; // must not throw
	});
}
