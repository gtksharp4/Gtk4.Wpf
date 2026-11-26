using System;

namespace System.Windows;

public partial class Window
{
	/// <summary>
	/// Handles GTK <c>notify::is-active</c>. Raises <see cref="Activated"/> or
	/// <see cref="Deactivated"/> depending on the current GTK active state.
	/// </summary>
	void OnGtkIsActiveChanged(object o, GLib.NotifyArgs args)
	{
		if (_window == null) return;
		NotifyActivation(_window.IsActive);
	}

	/// <summary>
	/// Updates <see cref="IsActive"/>, then raises Activated/Deactivated and informs the
	/// <see cref="Application"/>; also the test seam. Only a real transition does anything, as in WPF.
	/// </summary>
	internal void NotifyActivation(bool active)
	{
		if (active == (bool)GetValue(IsActiveProperty)!) return;
		SetValue(IsActivePropertyKey, active);
		if (active)
			Activated?.Invoke(this, EventArgs.Empty);
		else
			Deactivated?.Invoke(this, EventArgs.Empty);
		_application?.OnWindowActivationChanged(this, active);
	}
}
