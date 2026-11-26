using System.Windows.Input;

namespace System.Windows.Input;

public static partial class Keyboard
{
	/// <summary>The single keyboard device.</summary>
	public static KeyboardDevice PrimaryDevice { get; } = new GtkKeyboardDevice();

	/// <summary>Tunnel event raised before an element gets keyboard focus; handling it cancels the move.</summary>
	public static readonly RoutedEvent PreviewGotKeyboardFocusEvent = EventManager.RegisterRoutedEvent(
		"PreviewGotKeyboardFocus", RoutingStrategy.Tunnel, typeof(KeyboardFocusChangedEventHandler), typeof(Keyboard));

	/// <summary>Bubble event raised after an element got keyboard focus.</summary>
	public static readonly RoutedEvent GotKeyboardFocusEvent = EventManager.RegisterRoutedEvent(
		"GotKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Keyboard));

	/// <summary>Tunnel event raised before an element loses keyboard focus; handling it cancels the move.</summary>
	public static readonly RoutedEvent PreviewLostKeyboardFocusEvent = EventManager.RegisterRoutedEvent(
		"PreviewLostKeyboardFocus", RoutingStrategy.Tunnel, typeof(KeyboardFocusChangedEventHandler), typeof(Keyboard));

	/// <summary>Bubble event raised after an element lost keyboard focus.</summary>
	public static readonly RoutedEvent LostKeyboardFocusEvent = EventManager.RegisterRoutedEvent(
		"LostKeyboardFocus", RoutingStrategy.Bubble, typeof(KeyboardFocusChangedEventHandler), typeof(Keyboard));

	/// <summary>Removes keyboard focus from the focused element.</summary>
	public static void ClearFocus() => (FocusedElement as UIElement)?.SetFocusedInternal(false);

	/// <summary>Adds a PreviewGotKeyboardFocus handler to <paramref name="element"/>.</summary>
	public static void AddPreviewGotKeyboardFocusHandler(
		DependencyObject element, KeyboardFocusChangedEventHandler handler) =>
		Handle(element, PreviewGotKeyboardFocusEvent, handler, true);

	/// <summary>Removes a PreviewGotKeyboardFocus handler from <paramref name="element"/>.</summary>
	public static void RemovePreviewGotKeyboardFocusHandler(
		DependencyObject element, KeyboardFocusChangedEventHandler handler) =>
		Handle(element, PreviewGotKeyboardFocusEvent, handler, false);

	/// <summary>Adds a GotKeyboardFocus handler to <paramref name="element"/>.</summary>
	public static void AddGotKeyboardFocusHandler(
		DependencyObject element, KeyboardFocusChangedEventHandler handler) =>
		Handle(element, GotKeyboardFocusEvent, handler, true);

	/// <summary>Removes a GotKeyboardFocus handler from <paramref name="element"/>.</summary>
	public static void RemoveGotKeyboardFocusHandler(
		DependencyObject element, KeyboardFocusChangedEventHandler handler) =>
		Handle(element, GotKeyboardFocusEvent, handler, false);

	/// <summary>Adds a PreviewLostKeyboardFocus handler to <paramref name="element"/>.</summary>
	public static void AddPreviewLostKeyboardFocusHandler(
		DependencyObject element, KeyboardFocusChangedEventHandler handler) =>
		Handle(element, PreviewLostKeyboardFocusEvent, handler, true);

	/// <summary>Removes a PreviewLostKeyboardFocus handler from <paramref name="element"/>.</summary>
	public static void RemovePreviewLostKeyboardFocusHandler(
		DependencyObject element, KeyboardFocusChangedEventHandler handler) =>
		Handle(element, PreviewLostKeyboardFocusEvent, handler, false);

	/// <summary>Adds a LostKeyboardFocus handler to <paramref name="element"/>.</summary>
	public static void AddLostKeyboardFocusHandler(
		DependencyObject element, KeyboardFocusChangedEventHandler handler) =>
		Handle(element, LostKeyboardFocusEvent, handler, true);

	/// <summary>Removes a LostKeyboardFocus handler from <paramref name="element"/>.</summary>
	public static void RemoveLostKeyboardFocusHandler(
		DependencyObject element, KeyboardFocusChangedEventHandler handler) =>
		Handle(element, LostKeyboardFocusEvent, handler, false);

	static void Handle(DependencyObject element, RoutedEvent routedEvent, Delegate handler, bool add)
	{
		if (element == null) throw new ArgumentNullException(nameof(element));
		if (handler == null) throw new ArgumentNullException(nameof(handler));
		if (element is not UIElement ui)
			throw new ArgumentException("The element must be a UIElement.", nameof(element));
		if (add) ui.AddHandler(routedEvent, handler);
		else ui.RemoveHandler(routedEvent, handler);
	}
}
