using System;
using System.Collections.Generic;

namespace System.Windows;

public partial class Application
{
	readonly HashSet<Window> _activeWindows = new HashSet<Window>();
	bool _appActive;
	bool _deactivatePending;

	/// <summary>Raised by <see cref="OnStartup"/> before the StartupUri window is created.</summary>
	public event StartupEventHandler? Startup;

	/// <summary>Raised when one of the application's windows becomes active while none was.</summary>
	public event EventHandler? Activated;

	/// <summary>Raised when the application's last active window deactivates.</summary>
	public event EventHandler? Deactivated;

	/// <summary>
	/// Store-only: GTK reports session end through a registered <c>GtkApplication</c>, which this
	/// layer does not use, so the event is never raised.
	/// </summary>
	public event SessionEndingCancelEventHandler? SessionEnding;

	/// <summary>Raises <see cref="Startup"/>. A subclass that skips the base call suppresses the event.</summary>
	protected virtual void OnStartup(StartupEventArgs e) => Startup?.Invoke(this, e);

	/// <summary>Raises <see cref="Activated"/>.</summary>
	protected virtual void OnActivated(EventArgs e) => Activated?.Invoke(this, e);

	/// <summary>Raises <see cref="Deactivated"/>.</summary>
	protected virtual void OnDeactivated(EventArgs e) => Deactivated?.Invoke(this, e);

	/// <summary>Raises <see cref="SessionEnding"/> (never called by the GTK layer).</summary>
	protected virtual void OnSessionEnding(SessionEndingCancelEventArgs e) => SessionEnding?.Invoke(this, e);

	// Window.Activated/Deactivated feed this. The app activates with its first active window; the
	// deactivation is deferred because the window manager normally deactivates the old toplevel
	// before activating the next one, and WPF raises neither event when focus stays in the app.
	internal void OnWindowActivationChanged(Window window, bool active)
	{
		if (active)
		{
			if (!_windows.Contains(window)) return;
			_deactivatePending = false;
			_activeWindows.Add(window);
			if (!_appActive)
			{
				_appActive = true;
				OnActivated(EventArgs.Empty);
			}
		}
		else
			DeactivateWindow(window);
	}

	void DeactivateWindow(Window window)
	{
		if (!_activeWindows.Remove(window) || _activeWindows.Count > 0 || !_appActive || _deactivatePending)
			return;
		_deactivatePending = true;
		Dispatcher.BeginInvoke(new Action(() =>
		{
			if (!_deactivatePending || _activeWindows.Count > 0) return;
			_deactivatePending = false;
			_appActive = false;
			OnDeactivated(EventArgs.Empty);
		}));
	}
}
