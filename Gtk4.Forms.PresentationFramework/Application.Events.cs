using System;
using System.Collections.Generic;

namespace System.Windows;

public partial class Application
{
	readonly HashSet<Window> _activeWindows = new HashSet<Window>();

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

	// Window.Activated/Deactivated feed this: the app activates with its first active window
	// and deactivates when the last one goes inactive.
	internal void OnWindowActivationChanged(Window window, bool active)
	{
		if (active)
		{
			bool wasActive = _activeWindows.Count > 0;
			if (_activeWindows.Add(window) && !wasActive)
				OnActivated(EventArgs.Empty);
		}
		else if (_activeWindows.Remove(window) && _activeWindows.Count == 0)
		{
			OnDeactivated(EventArgs.Empty);
		}
	}
}
