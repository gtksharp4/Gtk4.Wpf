namespace System.Windows;

/// <summary>
/// Routed-event declarations for keyboard, mouse, and text input.
/// Focus DPs and focus helpers live in <c>UIElement.Input.cs</c>.
/// GTK controller wiring lives in <c>UIElement.GtkInput.cs</c>.
/// </summary>
public abstract partial class UIElement
{
	// ── Focus routed events ──────────────────────────────────────────────────

	/// <summary>Raised when keyboard focus enters this element.</summary>
	public static readonly RoutedEvent GotFocusEvent =
		EventManager.RegisterRoutedEvent(
			"GotFocus", RoutingStrategy.Bubble,
			typeof(RoutedEventHandler), typeof(UIElement));

	/// <summary>Raised when keyboard focus leaves this element.</summary>
	public static readonly RoutedEvent LostFocusEvent =
		EventManager.RegisterRoutedEvent(
			"LostFocus", RoutingStrategy.Bubble,
			typeof(RoutedEventHandler), typeof(UIElement));

	/// <summary>Occurs when keyboard focus enters this element.</summary>
	public event RoutedEventHandler? GotFocus
	{
		add    => AddHandler(GotFocusEvent, value!);
		remove => RemoveHandler(GotFocusEvent, value!);
	}

	/// <summary>Occurs when keyboard focus leaves this element.</summary>
	public event RoutedEventHandler? LostFocus
	{
		add    => AddHandler(LostFocusEvent, value!);
		remove => RemoveHandler(LostFocusEvent, value!);
	}

	// ── Keyboard routed events ───────────────────────────────────────────────

	/// <summary>Tunnel PreviewKeyDown.</summary>
	public static readonly RoutedEvent PreviewKeyDownEvent =
		EventManager.RegisterRoutedEvent("PreviewKeyDown", RoutingStrategy.Tunnel,
			typeof(System.Windows.Input.KeyEventHandler), typeof(UIElement));

	/// <summary>Bubble KeyDown.</summary>
	public static readonly RoutedEvent KeyDownEvent =
		EventManager.RegisterRoutedEvent("KeyDown", RoutingStrategy.Bubble,
			typeof(System.Windows.Input.KeyEventHandler), typeof(UIElement));

	/// <summary>Tunnel PreviewKeyUp.</summary>
	public static readonly RoutedEvent PreviewKeyUpEvent =
		EventManager.RegisterRoutedEvent("PreviewKeyUp", RoutingStrategy.Tunnel,
			typeof(System.Windows.Input.KeyEventHandler), typeof(UIElement));

	/// <summary>Bubble KeyUp.</summary>
	public static readonly RoutedEvent KeyUpEvent =
		EventManager.RegisterRoutedEvent("KeyUp", RoutingStrategy.Bubble,
			typeof(System.Windows.Input.KeyEventHandler), typeof(UIElement));

	/// <summary>Occurs when a key is pressed.</summary>
	public event System.Windows.Input.KeyEventHandler? KeyDown
	{
		add    => AddHandler(KeyDownEvent, value!);
		remove => RemoveHandler(KeyDownEvent, value!);
	}

	/// <summary>Occurs when a key is released.</summary>
	public event System.Windows.Input.KeyEventHandler? KeyUp
	{
		add    => AddHandler(KeyUpEvent, value!);
		remove => RemoveHandler(KeyUpEvent, value!);
	}

	// ── Mouse routed events ──────────────────────────────────────────────────

	/// <summary>Tunnel PreviewMouseDown.</summary>
	public static readonly RoutedEvent PreviewMouseDownEvent =
		EventManager.RegisterRoutedEvent("PreviewMouseDown", RoutingStrategy.Tunnel,
			typeof(System.Windows.Input.MouseButtonEventHandler), typeof(UIElement));

	/// <summary>Bubble MouseDown.</summary>
	public static readonly RoutedEvent MouseDownEvent =
		EventManager.RegisterRoutedEvent("MouseDown", RoutingStrategy.Bubble,
			typeof(System.Windows.Input.MouseButtonEventHandler), typeof(UIElement));

	/// <summary>Tunnel PreviewMouseUp.</summary>
	public static readonly RoutedEvent PreviewMouseUpEvent =
		EventManager.RegisterRoutedEvent("PreviewMouseUp", RoutingStrategy.Tunnel,
			typeof(System.Windows.Input.MouseButtonEventHandler), typeof(UIElement));

	/// <summary>Bubble MouseUp.</summary>
	public static readonly RoutedEvent MouseUpEvent =
		EventManager.RegisterRoutedEvent("MouseUp", RoutingStrategy.Bubble,
			typeof(System.Windows.Input.MouseButtonEventHandler), typeof(UIElement));

	/// <summary>Bubble MouseMove.</summary>
	public static readonly RoutedEvent MouseMoveEvent =
		EventManager.RegisterRoutedEvent("MouseMove", RoutingStrategy.Bubble,
			typeof(System.Windows.Input.MouseEventHandler), typeof(UIElement));

	/// <summary>Direct MouseEnter.</summary>
	public static readonly RoutedEvent MouseEnterEvent =
		EventManager.RegisterRoutedEvent("MouseEnter", RoutingStrategy.Direct,
			typeof(System.Windows.Input.MouseEventHandler), typeof(UIElement));

	/// <summary>Direct MouseLeave.</summary>
	public static readonly RoutedEvent MouseLeaveEvent =
		EventManager.RegisterRoutedEvent("MouseLeave", RoutingStrategy.Direct,
			typeof(System.Windows.Input.MouseEventHandler), typeof(UIElement));

	/// <summary>Occurs when a mouse button is pressed over this element.</summary>
	public event System.Windows.Input.MouseButtonEventHandler? MouseDown
	{
		add    => AddHandler(MouseDownEvent, value!);
		remove => RemoveHandler(MouseDownEvent, value!);
	}

	/// <summary>Occurs when a mouse button is released over this element.</summary>
	public event System.Windows.Input.MouseButtonEventHandler? MouseUp
	{
		add    => AddHandler(MouseUpEvent, value!);
		remove => RemoveHandler(MouseUpEvent, value!);
	}

	/// <summary>Occurs when the pointer moves over this element.</summary>
	public event System.Windows.Input.MouseEventHandler? MouseMove
	{
		add    => AddHandler(MouseMoveEvent, value!);
		remove => RemoveHandler(MouseMoveEvent, value!);
	}

	/// <summary>Occurs when the pointer enters this element.</summary>
	public event System.Windows.Input.MouseEventHandler? MouseEnter
	{
		add    => AddHandler(MouseEnterEvent, value!);
		remove => RemoveHandler(MouseEnterEvent, value!);
	}

	/// <summary>Occurs when the pointer leaves this element.</summary>
	public event System.Windows.Input.MouseEventHandler? MouseLeave
	{
		add    => AddHandler(MouseLeaveEvent, value!);
		remove => RemoveHandler(MouseLeaveEvent, value!);
	}

	// ── Text-input routed events ─────────────────────────────────────────────

	/// <summary>Tunnel PreviewTextInput.</summary>
	public static readonly RoutedEvent PreviewTextInputEvent =
		EventManager.RegisterRoutedEvent("PreviewTextInput", RoutingStrategy.Tunnel,
			typeof(System.Windows.Input.TextCompositionEventHandler), typeof(UIElement));

	/// <summary>Bubble TextInput.</summary>
	public static readonly RoutedEvent TextInputEvent =
		EventManager.RegisterRoutedEvent("TextInput", RoutingStrategy.Bubble,
			typeof(System.Windows.Input.TextCompositionEventHandler), typeof(UIElement));

	/// <summary>Occurs when text is generated by a key stroke or IME.</summary>
	public event System.Windows.Input.TextCompositionEventHandler? TextInput
	{
		add    => AddHandler(TextInputEvent, value!);
		remove => RemoveHandler(TextInputEvent, value!);
	}
}