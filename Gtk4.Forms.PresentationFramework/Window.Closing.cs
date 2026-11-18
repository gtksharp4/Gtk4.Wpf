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
}
