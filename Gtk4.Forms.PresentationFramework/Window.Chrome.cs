namespace System.Windows;

public partial class Window
{
	// ── WindowState ───────────────────────────────────────────────────────────

	/// <summary>Gets or sets the display state of the window.</summary>
	public static readonly DependencyProperty WindowStateProperty =
		DependencyProperty.Register(
			nameof(WindowState),
			typeof(WindowState),
			typeof(Window),
			new PropertyMetadata(WindowState.Normal, OnWindowStateChanged));

	static void OnWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var w = (Window)d;
		// Skip when the DP write originated from the WM→DP sync path (Window.State.cs)
		// to avoid a GTK→DP→GTK echo.
		if (w._updatingWindowState) return;
		if (w._window != null)
		{
			w._updatingWindowState = true;
			try { ApplyWindowState(w._window, (WindowState)e.NewValue!); }
			finally { w._updatingWindowState = false; }
			w.RaiseStateChanged();
		}
	}

	/// <summary>
	/// Gets or sets the display state of the window (Normal, Minimized, Maximized).
	/// </summary>
	/// <remarks>
	/// Two-way: setting this property drives the native GTK window; window-manager
	/// changes (the user clicking maximize/minimize) are reflected back via
	/// <c>notify::maximized</c> / <c>notify::suspended</c> and raise
	/// <see cref="StateChanged"/>.
	/// </remarks>
	public WindowState WindowState
	{
		get => (WindowState)(GetValue(WindowStateProperty) ?? WindowState.Normal);
		set => SetValue(WindowStateProperty, value);
	}

	static void ApplyWindowState(Gtk.Window win, WindowState state)
	{
		switch (state)
		{
			case WindowState.Minimized:
				win.Minimize();
				break;
			case WindowState.Maximized:
				win.Maximize();
				break;
			default: // Normal
				win.Unmaximize();
				win.Unminimize();
				break;
		}
	}

	// ── WindowStartupLocation ─────────────────────────────────────────────────

	/// <summary>Gets or sets where the window is positioned when first shown.</summary>
	public static readonly DependencyProperty WindowStartupLocationProperty =
		DependencyProperty.Register(
			nameof(WindowStartupLocation),
			typeof(WindowStartupLocation),
			typeof(Window),
			new PropertyMetadata(WindowStartupLocation.Manual));

	/// <summary>
	/// Gets or sets the startup location of the window.
	/// </summary>
	/// <remarks>
	/// GTK4 limitation: GTK4 removed <c>gtk_window_move()</c>; programmatic window
	/// positioning is not available. <see cref="WindowStartupLocation.CenterScreen"/> and
	/// <see cref="WindowStartupLocation.CenterOwner"/> are stored and respected for the
	/// WPF API surface but cannot be applied to the native window. The OS/compositor
	/// determines the actual placement.
	/// </remarks>
	public WindowStartupLocation WindowStartupLocation
	{
		get => (WindowStartupLocation)(GetValue(WindowStartupLocationProperty) ?? WindowStartupLocation.Manual);
		set => SetValue(WindowStartupLocationProperty, value);
	}

	/// <summary>
	/// Called from <see cref="Show"/> before <c>Present()</c>.
	/// GTK4 does not expose a window-move API, so all modes are no-ops at the native level.
	/// The method exists so the DP value is consumed at the correct point and future
	/// implementations can fill in platform-specific positioning.
	/// </summary>
	internal void ApplyStartupLocation(Gtk.Window win)
	{
		// GTK4 removed gtk_window_move() — CenterScreen/CenterOwner cannot be implemented.
		// The switch is intentionally empty; the value is stored in the DP for round-trip fidelity.
		ApplyTransparencyOnShow();
		_ = win;
		_ = WindowStartupLocation;
	}

	// ── Shared chrome application ─────────────────────────────────────────────

	/// <summary>
	/// Applies all stored chrome DP values to <paramref name="win"/> immediately after it
	/// is created. Called from <see cref="CreateWidget"/> once <c>_window</c> is set.
	/// </summary>
	internal void ApplyChromeToWindow(Gtk.Window win)
	{
		ApplyWindowState(win, WindowState);
		ApplyShellChrome();
		ApplyResizeMode(win, _headerBar, ResizeMode);
		// SizeToContent is applied via ApplyExplicitSize; no separate call needed here.
		// WindowStartupLocation is applied via ApplyStartupLocation in Show().
		// Wire WM→DP state round-trip and Activated/Deactivated (Window.State.cs).
		WireStateNotifications(win);
		// SourceInitialized / Loaded / ContentRendered / live size sync (Window.Lifecycle.cs).
		WireLifecycleSignals(win);
		WireDragMove(win);
		ApplyShellChrome();
	}
}
