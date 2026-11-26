namespace System.Windows.Controls;

/// <summary>Handler for <see cref="FrameworkElement.ContextMenuOpeningEvent"/> and its closing counterpart.</summary>
public delegate void ContextMenuEventHandler(object sender, ContextMenuEventArgs e);

/// <summary>Data for the ContextMenuOpening and ContextMenuClosing events.</summary>
public sealed class ContextMenuEventArgs : RoutedEventArgs
{
	internal ContextMenuEventArgs(object source, bool opening, double left = -1, double top = -1)
		: base(opening ? FrameworkElement.ContextMenuOpeningEvent : FrameworkElement.ContextMenuClosingEvent)
	{
		Source = source;
		CursorLeft = left;
		CursorTop = top;
	}

	/// <summary>
	/// The horizontal position of the pointer relative to the element, or -1 when the menu was
	/// requested without a pointer.
	/// </summary>
	public double CursorLeft { get; }

	/// <summary>The vertical position of the pointer relative to the element, or -1 without a pointer.</summary>
	public double CursorTop { get; }
}
