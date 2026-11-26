using System.ComponentModel;

namespace System.Windows;

/// <summary>Why the user session is ending.</summary>
public enum ReasonSessionEnding
{
	/// <summary>The user is logging off.</summary>
	Logoff,

	/// <summary>The system is shutting down.</summary>
	Shutdown,
}

/// <summary>
/// Arguments for <see cref="Application.SessionEnding"/>. GTK 4 reports session end only
/// through a registered <c>GtkApplication</c>, which this layer does not use, so the event is
/// store-only and never raised.
/// </summary>
public class SessionEndingCancelEventArgs : CancelEventArgs
{
	internal SessionEndingCancelEventArgs(ReasonSessionEnding reason)
	{
		ReasonSessionEnding = reason;
	}

	/// <summary>Why the session is ending.</summary>
	public ReasonSessionEnding ReasonSessionEnding { get; }
}

/// <summary>Handles <see cref="Application.SessionEnding"/>.</summary>
public delegate void SessionEndingCancelEventHandler(object sender, SessionEndingCancelEventArgs e);
