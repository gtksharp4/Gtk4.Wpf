namespace System.Windows.Controls.Primitives;

/// <summary>Base class for text editing controls (<see cref="TextBox"/>, <see cref="RichTextBox"/>).</summary>
public abstract class TextBoxBase : Control
{
	/// <summary>Identifies the <see cref="SelectionChanged"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent SelectionChangedEvent = EventManager.RegisterRoutedEvent(
		nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TextBoxBase));

	/// <summary>Occurs when the selection changes.</summary>
	public event RoutedEventHandler SelectionChanged
	{
		add => AddHandler(SelectionChangedEvent, value);
		remove => RemoveHandler(SelectionChangedEvent, value);
	}

	/// <summary>Raises <see cref="SelectionChangedEvent"/>.</summary>
	internal void RaiseSelectionChangedEvent() => RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, this));
}
