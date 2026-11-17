using System;

namespace System.Windows;

/// <summary>Standard handler delegate for routed events.</summary>
public delegate void RoutedEventHandler(object sender, RoutedEventArgs e);

/// <summary>
/// Base event-argument class for routed events.  Subclass to carry additional data.
/// </summary>
public class RoutedEventArgs : EventArgs
{
	/// <summary>The <see cref="RoutedEvent"/> identifier.</summary>
	public RoutedEvent RoutedEvent { get; set; }

	/// <summary>
	/// The object that originally raised the event.  Defaults to the element on which
	/// <see cref="RoutedEventHost.RaiseEvent"/> is called.
	/// </summary>
	public object? Source { get; set; }

	/// <summary>
	/// When set to <c>true</c> by a handler, suppresses further routing (Bubble/Tunnel).
	/// </summary>
	public bool Handled { get; set; }

	/// <summary>Creates an instance; <see cref="RoutedEvent"/> must be set before raising.</summary>
	public RoutedEventArgs() { RoutedEvent = null!; }

	/// <summary>Creates an instance with a pre-set <see cref="RoutedEvent"/>.</summary>
	public RoutedEventArgs(RoutedEvent routedEvent)
	{
		RoutedEvent = routedEvent ?? throw new ArgumentNullException(nameof(routedEvent));
	}
}