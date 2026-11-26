using System.Windows.Input;

namespace System.Windows;

/// <summary>
/// Button-specific mouse events (<c>MouseLeftButtonDown</c> etc.). As in WPF they are
/// <see cref="RoutingStrategy.Direct"/> and are produced by "cracking" the generic
/// (Preview)MouseDown/Up: a class handler on each element of the generic route re-raises the
/// specific event on that element with the same args, so handling the specific event marks the
/// generic one handled. Middle and X buttons have no specific events.
/// </summary>
public abstract partial class UIElement
{
	static UIElement()
	{
		InstallAppearanceHook();
		RegisterKeyboardFocusClassHandlers();
		RegisterMouseVirtualHandlers();
		EventManager.RegisterClassHandler(typeof(UIElement), PreviewMouseDownEvent,
			new MouseButtonEventHandler(OnPreviewMouseDownThunk), true);
		EventManager.RegisterClassHandler(typeof(UIElement), MouseDownEvent,
			new MouseButtonEventHandler(OnMouseDownThunk), true);
		EventManager.RegisterClassHandler(typeof(UIElement), PreviewMouseUpEvent,
			new MouseButtonEventHandler(OnPreviewMouseUpThunk), true);
		EventManager.RegisterClassHandler(typeof(UIElement), MouseUpEvent,
			new MouseButtonEventHandler(OnMouseUpThunk), true);
	}

	static RoutedEvent RegisterButtonEvent(string name) =>
		EventManager.RegisterRoutedEvent(name, RoutingStrategy.Direct,
			typeof(MouseButtonEventHandler), typeof(UIElement));

	/// <summary>Direct PreviewMouseLeftButtonDown.</summary>
	public static readonly RoutedEvent PreviewMouseLeftButtonDownEvent = RegisterButtonEvent("PreviewMouseLeftButtonDown");
	/// <summary>Direct MouseLeftButtonDown.</summary>
	public static readonly RoutedEvent MouseLeftButtonDownEvent = RegisterButtonEvent("MouseLeftButtonDown");
	/// <summary>Direct PreviewMouseLeftButtonUp.</summary>
	public static readonly RoutedEvent PreviewMouseLeftButtonUpEvent = RegisterButtonEvent("PreviewMouseLeftButtonUp");
	/// <summary>Direct MouseLeftButtonUp.</summary>
	public static readonly RoutedEvent MouseLeftButtonUpEvent = RegisterButtonEvent("MouseLeftButtonUp");
	/// <summary>Direct PreviewMouseRightButtonDown.</summary>
	public static readonly RoutedEvent PreviewMouseRightButtonDownEvent = RegisterButtonEvent("PreviewMouseRightButtonDown");
	/// <summary>Direct MouseRightButtonDown.</summary>
	public static readonly RoutedEvent MouseRightButtonDownEvent = RegisterButtonEvent("MouseRightButtonDown");
	/// <summary>Direct PreviewMouseRightButtonUp.</summary>
	public static readonly RoutedEvent PreviewMouseRightButtonUpEvent = RegisterButtonEvent("PreviewMouseRightButtonUp");
	/// <summary>Direct MouseRightButtonUp.</summary>
	public static readonly RoutedEvent MouseRightButtonUpEvent = RegisterButtonEvent("MouseRightButtonUp");

	/// <summary>Occurs when the left button is pressed (tunnel phase of MouseDown).</summary>
	public event MouseButtonEventHandler? PreviewMouseLeftButtonDown
	{
		add    => AddHandler(PreviewMouseLeftButtonDownEvent, value!);
		remove => RemoveHandler(PreviewMouseLeftButtonDownEvent, value!);
	}

	/// <summary>Occurs when the left button is pressed over this element.</summary>
	public event MouseButtonEventHandler? MouseLeftButtonDown
	{
		add    => AddHandler(MouseLeftButtonDownEvent, value!);
		remove => RemoveHandler(MouseLeftButtonDownEvent, value!);
	}

	/// <summary>Occurs when the left button is released (tunnel phase of MouseUp).</summary>
	public event MouseButtonEventHandler? PreviewMouseLeftButtonUp
	{
		add    => AddHandler(PreviewMouseLeftButtonUpEvent, value!);
		remove => RemoveHandler(PreviewMouseLeftButtonUpEvent, value!);
	}

	/// <summary>Occurs when the left button is released over this element.</summary>
	public event MouseButtonEventHandler? MouseLeftButtonUp
	{
		add    => AddHandler(MouseLeftButtonUpEvent, value!);
		remove => RemoveHandler(MouseLeftButtonUpEvent, value!);
	}

	/// <summary>Occurs when the right button is pressed (tunnel phase of MouseDown).</summary>
	public event MouseButtonEventHandler? PreviewMouseRightButtonDown
	{
		add    => AddHandler(PreviewMouseRightButtonDownEvent, value!);
		remove => RemoveHandler(PreviewMouseRightButtonDownEvent, value!);
	}

	/// <summary>Occurs when the right button is pressed over this element.</summary>
	public event MouseButtonEventHandler? MouseRightButtonDown
	{
		add    => AddHandler(MouseRightButtonDownEvent, value!);
		remove => RemoveHandler(MouseRightButtonDownEvent, value!);
	}

	/// <summary>Occurs when the right button is released (tunnel phase of MouseUp).</summary>
	public event MouseButtonEventHandler? PreviewMouseRightButtonUp
	{
		add    => AddHandler(PreviewMouseRightButtonUpEvent, value!);
		remove => RemoveHandler(PreviewMouseRightButtonUpEvent, value!);
	}

	/// <summary>Occurs when the right button is released over this element.</summary>
	public event MouseButtonEventHandler? MouseRightButtonUp
	{
		add    => AddHandler(MouseRightButtonUpEvent, value!);
		remove => RemoveHandler(MouseRightButtonUpEvent, value!);
	}

	// ── Cracking (WPF UIElement.CrackMouseButtonEventAndReRaiseEvent) ──────────────

	static void OnPreviewMouseDownThunk(object sender, MouseButtonEventArgs e) =>
		CrackMouseButtonEvent(sender, e, PreviewMouseLeftButtonDownEvent, PreviewMouseRightButtonDownEvent);

	static void OnMouseDownThunk(object sender, MouseButtonEventArgs e) =>
		CrackMouseButtonEvent(sender, e, MouseLeftButtonDownEvent, MouseRightButtonDownEvent);

	static void OnPreviewMouseUpThunk(object sender, MouseButtonEventArgs e) =>
		CrackMouseButtonEvent(sender, e, PreviewMouseLeftButtonUpEvent, PreviewMouseRightButtonUpEvent);

	static void OnMouseUpThunk(object sender, MouseButtonEventArgs e) =>
		CrackMouseButtonEvent(sender, e, MouseLeftButtonUpEvent, MouseRightButtonUpEvent);

	/// <summary>
	/// Re-raises <paramref name="e"/> as the left/right specific event, Direct on
	/// <paramref name="sender"/>, then restores the original identity. As in WPF the sub-event is
	/// always raised and carries the handledness along (already-handled args reach only
	/// <c>handledEventsToo</c> handlers); handling it leaves the generic args handled.
	/// </summary>
	static void CrackMouseButtonEvent(object sender, MouseButtonEventArgs e, RoutedEvent left, RoutedEvent right)
	{
		RoutedEvent? specific = e.ChangedButton switch
		{
			MouseButton.Left  => left,
			MouseButton.Right => right,
			_                 => null,
		};
		if (specific == null || sender is not DependencyObject element) return;

		var original = e.RoutedEvent;
		e.RoutedEvent = specific;
		try { RoutedEventHost.RaiseEvent(element, e); }
		finally { e.RoutedEvent = original; }
	}
}
