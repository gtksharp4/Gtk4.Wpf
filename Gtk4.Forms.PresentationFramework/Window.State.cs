using System;

namespace System.Windows;

public partial class Window
{
	// ── Reentrancy guard (shared with Window.Chrome.cs ApplyWindowState) ─────
	//
	// Set to true while DP→native apply is in progress; suppresses the WM→DP
	// callback so a programmatic Maximize() does not echo back as a DP write.
	internal bool _updatingWindowState;

	// ── StateChanged event ────────────────────────────────────────────────────

	/// <summary>
	/// Raised when <see cref="WindowState"/> changes — either because the
	/// application set the DP or because the window manager changed the state.
	/// </summary>
	public event EventHandler? StateChanged;

	// ── Activated / Deactivated events ────────────────────────────────────────

	/// <summary>
	/// Raised when the window receives input focus (becomes the active window).
	/// Wired to GTK <c>notify::is-active</c> when the native window is realized.
	/// </summary>
	public event EventHandler? Activated;

	/// <summary>
	/// Raised when the window loses input focus.
	/// Wired to GTK <c>notify::is-active</c> when the native window is realized.
	/// </summary>
	public event EventHandler? Deactivated;

	// ── Topmost DP ───────────────────────────────────────────────────────────

	/// <summary>Gets or sets whether the window stays on top of other windows.</summary>
	/// <remarks>
	/// GTK4 limitation: GTK4 has no portable always-on-top API. The DP value is
	/// stored and round-trips correctly but has no native effect on most platforms.
	/// </remarks>
	public static readonly DependencyProperty TopmostProperty =
		DependencyProperty.Register(
			nameof(Topmost),
			typeof(bool),
			typeof(Window),
			new PropertyMetadata(false));

	/// <summary>
	/// Gets or sets whether the window is displayed above all other non-topmost
	/// windows.
	/// </summary>
	/// <remarks>
	/// GTK4 limitation: no portable always-on-top API exists; the value is stored
	/// but not applied to the native window.
	/// </remarks>
	public bool Topmost
	{
		get => (bool)(GetValue(TopmostProperty) ?? false);
		set => SetValue(TopmostProperty, value);
	}

	// ── ShowInTaskbar DP ─────────────────────────────────────────────────────

	/// <summary>Gets or sets whether the window appears in the taskbar.</summary>
	/// <remarks>
	/// GTK4 limitation: GTK4 has no taskbar-hint API equivalent. The DP value is
	/// stored and round-trips correctly but has no native effect.
	/// </remarks>
	public static readonly DependencyProperty ShowInTaskbarProperty =
		DependencyProperty.Register(
			nameof(ShowInTaskbar),
			typeof(bool),
			typeof(Window),
			new PropertyMetadata(true));

	/// <summary>
	/// Gets or sets whether the window shows in the taskbar.
	/// </summary>
	/// <remarks>
	/// GTK4 limitation: no taskbar-hint API; the value is stored but not applied
	/// to the native window.
	/// </remarks>
	public bool ShowInTaskbar
	{
		get => (bool)(GetValue(ShowInTaskbarProperty) ?? true);
		set => SetValue(ShowInTaskbarProperty, value);
	}

	// ── Activate() ──────────────────────────────────────────────────────────

	/// <summary>
	/// Attempts to bring the window to the foreground and activate it.
	/// Calls <see cref="Gtk.Window.Present()"/> when the native window exists;
	/// no-op otherwise (before the window is shown).
	/// </summary>
	public void Activate()
	{
		_window?.Present();
	}

	// ── WM→DP round-trip wiring ───────────────────────────────────────────────

	/// <summary>
	/// Called from <see cref="CreateWidget"/> to subscribe to the GTK
	/// <c>notify::maximized</c> and <c>notify::suspended</c> (iconified/minimized)
	/// property-change notifications so the window manager's state changes are
	/// reflected back into the <see cref="WindowState"/> DP.
	/// Also wires <c>notify::is-active</c> for <see cref="Activated"/> /
	/// <see cref="Deactivated"/>.
	/// </summary>
	/// <remarks>
	/// The notification handlers are not explicitly unsubscribed: they live for the
	/// window's lifetime and the backing <see cref="Gtk.Window"/> is disposed with the
	/// <see cref="Window"/>, so there is no per-event churn or leak to clean up.
	/// </remarks>
	internal void WireStateNotifications(Gtk.Window win)
	{
		win.AddNotification("maximized", OnGtkMaximizedChanged);
		win.AddNotification("suspended", OnGtkSuspendedChanged);
		win.AddNotification("is-active", OnGtkIsActiveChanged);
	}

	/// <summary>Handles GTK <c>notify::maximized</c> → resolves and syncs the DP.</summary>
	/// <remarks>Exposed <c>internal</c> for unit testing the WM→DP sync seam.</remarks>
	internal void OnGtkMaximizedChanged(object o, GLib.NotifyArgs args) => SyncFromGtkFlags((Gtk.Window)o);

	/// <summary>Handles GTK <c>notify::suspended</c> (iconified) → resolves and syncs the DP.</summary>
	/// <remarks>Exposed <c>internal</c> for unit testing the WM→DP sync seam.</remarks>
	internal void OnGtkSuspendedChanged(object o, GLib.NotifyArgs args) => SyncFromGtkFlags((Gtk.Window)o);

	// Reads BOTH GTK state flags and syncs the resolved WPF state. Reading both
	// (rather than one per handler) applies a stable precedence so a maximized window
	// that is then minimized does not flip back to Maximized on the next maximized notify.
	void SyncFromGtkFlags(Gtk.Window gtkWin)
	{
		if (_updatingWindowState || _window == null) return;
		SyncWindowStateFromGtk(ResolveGtkState(gtkWin.Suspended, gtkWin.Maximized));
	}

	/// <summary>
	/// Resolves the single WPF <see cref="WindowState"/> from the two independent GTK
	/// flags. A window can be both maximized and suspended (iconified) in GTK; WPF's
	/// enum must pick one, so minimized (suspended) takes precedence over maximized.
	/// </summary>
	internal static WindowState ResolveGtkState(bool suspended, bool maximized) =>
		suspended ? WindowState.Minimized
		: maximized ? WindowState.Maximized
		: WindowState.Normal;

	/// <summary>
	/// Updates the <see cref="WindowState"/> DP from a WM-originated change and
	/// raises <see cref="StateChanged"/>. The <see cref="_updatingWindowState"/>
	/// flag is NOT set here — the guard is only needed in the DP→native direction.
	/// Setting the DP here triggers <see cref="Window.Chrome.cs"/>'s
	/// <c>OnWindowStateChanged</c>, which is guarded to no-op when the flag is set.
	/// So we set the flag in that callback, not here, to prevent the echo.
	/// </summary>
	/// <remarks>Exposed <c>internal</c> so the WM→DP seam can be unit-tested headlessly.</remarks>
	internal void SyncWindowStateFromGtk(WindowState newState)
	{
		if ((WindowState)(GetValue(WindowStateProperty) ?? WindowState.Normal) == newState) return;
		// Mark that this DP write originates from GTK so OnWindowStateChanged
		// skips re-applying to GTK (avoid echo: GTK→DP→GTK).
		_updatingWindowState = true;
		try
		{
			SetValue(WindowStateProperty, newState);
		}
		finally
		{
			_updatingWindowState = false;
		}
		StateChanged?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Raises <see cref="StateChanged"/> when the DP is changed programmatically
	/// (DP→native direction). Called from <see cref="Window.Chrome.cs"/>'s
	/// <c>OnWindowStateChanged</c> after the native state is applied.
	/// </summary>
	internal void RaiseStateChanged()
	{
		StateChanged?.Invoke(this, EventArgs.Empty);
	}
}
