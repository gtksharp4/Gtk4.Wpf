using System;
using System.Windows;
using Xunit;
using WpfWindow = System.Windows.Window;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no-GTK) tests for <c>Topmost</c>, <c>ShowInTaskbar</c>, <c>Activate()</c>,
/// <c>Activated</c>/<c>Deactivated</c>/<c>StateChanged</c> events, and the
/// WM→DP round-trip seam exposed by <c>OnGtkMaximizedChanged</c> /
/// <c>OnGtkSuspendedChanged</c>.
/// </summary>
public class WindowStateTests
{
	// ── Topmost DP ───────────────────────────────────────────────────────────

	[Fact]
	public void Topmost_default_is_false()
	{
		var win = new WpfWindow();
		Assert.False(win.Topmost);
	}

	[Fact]
	public void Topmost_dp_default_metadata_is_false()
	{
		var meta = WpfWindow.TopmostProperty.GetMetadata(typeof(WpfWindow));
		Assert.Equal(false, meta.DefaultValue);
	}

	[Fact]
	public void Topmost_set_before_native_window_does_not_throw()
	{
		var win = new WpfWindow();
		win.Topmost = true;
		Assert.True(win.Topmost);
	}

	[Fact]
	public void Topmost_roundtrip()
	{
		var win = new WpfWindow();
		win.Topmost = true;
		Assert.True(win.Topmost);
		win.Topmost = false;
		Assert.False(win.Topmost);
	}

	// ── ShowInTaskbar DP ─────────────────────────────────────────────────────

	[Fact]
	public void ShowInTaskbar_default_is_true()
	{
		var win = new WpfWindow();
		Assert.True(win.ShowInTaskbar);
	}

	[Fact]
	public void ShowInTaskbar_dp_default_metadata_is_true()
	{
		var meta = WpfWindow.ShowInTaskbarProperty.GetMetadata(typeof(WpfWindow));
		Assert.Equal(true, meta.DefaultValue);
	}

	[Fact]
	public void ShowInTaskbar_set_before_native_window_does_not_throw()
	{
		var win = new WpfWindow();
		win.ShowInTaskbar = false;
		Assert.False(win.ShowInTaskbar);
	}

	[Fact]
	public void ShowInTaskbar_roundtrip()
	{
		var win = new WpfWindow();
		win.ShowInTaskbar = false;
		Assert.False(win.ShowInTaskbar);
		win.ShowInTaskbar = true;
		Assert.True(win.ShowInTaskbar);
	}

	// ── Activate() headless ──────────────────────────────────────────────────

	[Fact]
	public void Activate_headless_does_not_throw()
	{
		var win = new WpfWindow();
		// No native window — should be a no-op, not an exception.
		win.Activate();
	}

	// ── StateChanged event ────────────────────────────────────────────────────

	[Fact]
	public void StateChanged_not_raised_when_dp_unchanged_before_native_window()
	{
		var win = new WpfWindow();
		int count = 0;
		win.StateChanged += (_, _) => count++;
		// No native window — DP write triggers OnWindowStateChanged which skips
		// apply (no _window) but does NOT raise StateChanged from Chrome path.
		win.WindowState = WindowState.Normal; // same as default — no change callback
		Assert.Equal(0, count);
	}

	// ── WM→DP round-trip seam (OnGtkMaximizedChanged / OnGtkSuspendedChanged) ──

	[Fact]
	public void SyncWindowStateFromGtk_updates_dp_and_raises_StateChanged_once()
	{
		var win = new WpfWindow();
		Assert.Equal(WindowState.Normal, win.WindowState);

		int stateChangedCount = 0;
		win.StateChanged += (_, _) => stateChangedCount++;

		// Drive the real WM→DP seam directly (internal, InternalsVisibleTo).
		win.SyncWindowStateFromGtk(WindowState.Maximized);

		Assert.Equal(WindowState.Maximized, win.WindowState);
		Assert.Equal(1, stateChangedCount);           // raised exactly once
		Assert.False(win._updatingWindowState);        // finally released the guard
	}

	[Fact]
	public void SyncWindowStateFromGtk_is_no_op_when_state_unchanged()
	{
		var win = new WpfWindow();           // default Normal
		int stateChangedCount = 0;
		win.StateChanged += (_, _) => stateChangedCount++;

		win.SyncWindowStateFromGtk(WindowState.Normal);

		Assert.Equal(WindowState.Normal, win.WindowState);
		Assert.Equal(0, stateChangedCount);  // unchanged → no event
	}

	[Theory]
	[InlineData(false, false, WindowState.Normal)]
	[InlineData(false, true,  WindowState.Maximized)]
	[InlineData(true,  false, WindowState.Minimized)]
	[InlineData(true,  true,  WindowState.Minimized)]   // suspended wins over maximized
	public void ResolveGtkState_applies_minimized_precedence(bool suspended, bool maximized, WindowState expected)
	{
		Assert.Equal(expected, WpfWindow.ResolveGtkState(suspended, maximized));
	}

	// ── Pre-realize persistence ───────────────────────────────────────────────

	[Fact]
	public void All_new_props_set_before_realize_persist()
	{
		var win = new WpfWindow
		{
			Topmost = true,
			ShowInTaskbar = false,
		};
		Assert.True(win.Topmost);
		Assert.False(win.ShowInTaskbar);
	}
}
