using System.ComponentModel;

namespace System.Windows;

public partial class Window
{
	// ── Closing event ─────────────────────────────────────────────────────────

	/// <summary>
	/// Raised before the window closes. A handler may set
	/// <see cref="CancelEventArgs.Cancel"/> to <see langword="true"/> to veto the close.
	/// If not canceled, <see cref="Closed"/> follows immediately after.
	/// </summary>
	public event CancelEventHandler? Closing;

	// ── Re-entrancy guard ─────────────────────────────────────────────────────

	bool _closingInProgress;

	// ── Internal seam ─────────────────────────────────────────────────────────

	/// <summary>
	/// Runs the full closing protocol: raises <see cref="Closing"/>, and if not
	/// canceled, calls <see cref="CloseCore"/>. Returns <see langword="true"/> when
	/// the close proceeded, <see langword="false"/> when a handler vetoed it.
	/// </summary>
	/// <remarks>
	/// Used by <see cref="Close()"/>, <see cref="Close(bool?)"/>, and the GTK
	/// <c>CloseRequest</c> signal handler. The internal visibility allows unit tests
	/// to drive the closing protocol without a real window manager.
	/// </remarks>
	internal bool RequestClose()
	{
		// Guard: ignore re-entrant calls (e.g. a Closing handler that calls Close()).
		if (_closingInProgress)
			return false;

		// Already fully closed — treat as a no-op (Closed fires at most once).
		if (_closed)
			return false;

		_closingInProgress = true;
		try
		{
			var args = new CancelEventArgs();
			Closing?.Invoke(this, args);
			if (_closed) return true; // a handler already shut the app down

			if (args.Cancel)
				return false;

			CloseCore(keepResult: true);
			return true;
		}
		finally
		{
			_closingInProgress = false;
		}
	}

	/// <summary>
	/// Maps the <see cref="RequestClose"/> outcome to the GTK <c>close-request</c> return
	/// value: GTK4 treats <see langword="true"/> as "stop the default close", so a vetoed
	/// close (<paramref name="proceeded"/> = <see langword="false"/>) returns
	/// <see langword="true"/> to suppress GTK's destroy, and an allowed close returns
	/// <see langword="false"/>.
	/// </summary>
	internal static bool MapCloseRetVal(bool proceeded) => !proceeded;

	/// <summary>
	/// Closes the window and unblocks <see cref="ShowDialog"/> with
	/// <paramref name="dialogResult"/>. Must be called on the GTK thread.
	/// A <see cref="Closing"/> handler may veto the close; in that case
	/// <see cref="DialogResult"/> is restored to its previous value.
	/// </summary>
	public void Close(bool? dialogResult)
	{
		bool? previous = _dialogResult;
		_dialogResult = dialogResult;
		if (!RequestClose())
			_dialogResult = previous; // veto: undo the tentative set
	}

	/// <summary>
	/// Common close implementation. Sets the flag and hides the backing window.
	/// <paramref name="keepResult"/> is always true from public callers — the parameter
	/// exists so the window-manager handler can be routed here without resetting DialogResult.
	/// </summary>
	void CloseCore(bool keepResult)
	{
		_ = keepResult; // reserved for future use
		_dialogClosed = true;
		((Gtk.Window)Widget).Hide();
		SetShown(false);

		// Fire Closed once, after the window is hidden (WPF order). Drives app shutdown.
		if (!_closed)
		{
			Visibility = Visibility.Hidden; // so re-setting Visible after close is a change and throws
			_closed = true;
			Closed?.Invoke(this, EventArgs.Empty);
			LifecycleBroadcaster.BroadcastUnloaded(this); // WPF: Unloaded follows Closed
			_application?.OnWindowClosed(this);
		}
	}

	/// <summary>
	/// Closes for <see cref="Application.Shutdown(int)"/>: raises <see cref="Closing"/> (as WPF does)
	/// but ignores its Cancel.
	/// </summary>
	internal void CloseForShutdown()
	{
		if (_closed) return;
		if (!_closingInProgress)
		{
			_closingInProgress = true;
			try { Closing?.Invoke(this, new CancelEventArgs()); }
			finally { _closingInProgress = false; }
		}
		if (!_closed)
			CloseCore(keepResult: true);
	}

	/// <summary>
	/// Handles the GTK close-request signal (window-manager X button / Alt+F4).
	/// Runs the <see cref="Closing"/> protocol. Returning <see langword="true"/> from
	/// this handler tells GTK to suppress its default destroy — used when a handler
	/// vetoes the close.
	/// </summary>
	void OnCloseRequest(object sender, Gtk.CloseRequestArgs e)
	{
		bool proceeded = RequestClose();
		// GTK4: returning true stops the default close; false lets GTK destroy the window.
		e.RetVal = MapCloseRetVal(proceeded);
	}
}
