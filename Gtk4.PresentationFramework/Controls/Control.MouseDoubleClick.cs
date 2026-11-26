using System.Windows.Input;

namespace System.Windows.Controls;

/// <summary>
/// <c>PreviewMouseDoubleClick</c>/<c>MouseDoubleClick</c>. As in WPF they are raised by Control
/// class handlers on (Preview)MouseLeftButtonDown and (Preview)MouseRightButtonDown when
/// <see cref="MouseButtonEventArgs.ClickCount"/> is 2 (GTK <c>GestureClick</c> n_press), as a new
/// args object; handling it marks the original button event handled.
/// </summary>
public abstract partial class Control
{
	static Control()
	{
		var handler = new MouseButtonEventHandler(HandleDoubleClick);
		EventManager.RegisterClassHandler(typeof(Control), PreviewMouseLeftButtonDownEvent, handler, true);
		EventManager.RegisterClassHandler(typeof(Control), MouseLeftButtonDownEvent, handler, true);
		EventManager.RegisterClassHandler(typeof(Control), PreviewMouseRightButtonDownEvent, handler, true);
		EventManager.RegisterClassHandler(typeof(Control), MouseRightButtonDownEvent, handler, true);
	}

	/// <summary>Identifies the <see cref="PreviewMouseDoubleClick"/> routed event (Direct).</summary>
	public static readonly RoutedEvent PreviewMouseDoubleClickEvent =
		EventManager.RegisterRoutedEvent("PreviewMouseDoubleClick", RoutingStrategy.Direct,
			typeof(MouseButtonEventHandler), typeof(Control));

	/// <summary>Identifies the <see cref="MouseDoubleClick"/> routed event (Direct).</summary>
	public static readonly RoutedEvent MouseDoubleClickEvent =
		EventManager.RegisterRoutedEvent("MouseDoubleClick", RoutingStrategy.Direct,
			typeof(MouseButtonEventHandler), typeof(Control));

	/// <summary>Occurs when a mouse button is clicked two or more times (tunnel phase).</summary>
	public event MouseButtonEventHandler? PreviewMouseDoubleClick
	{
		add    => AddHandler(PreviewMouseDoubleClickEvent, value!);
		remove => RemoveHandler(PreviewMouseDoubleClickEvent, value!);
	}

	/// <summary>Occurs when a mouse button is clicked two or more times.</summary>
	public event MouseButtonEventHandler? MouseDoubleClick
	{
		add    => AddHandler(MouseDoubleClickEvent, value!);
		remove => RemoveHandler(MouseDoubleClickEvent, value!);
	}

	/// <summary>Raises <see cref="PreviewMouseDoubleClick"/>.</summary>
	protected virtual void OnPreviewMouseDoubleClick(MouseButtonEventArgs e) => RaiseEvent(e);

	/// <summary>Raises <see cref="MouseDoubleClick"/>.</summary>
	protected virtual void OnMouseDoubleClick(MouseButtonEventArgs e) => RaiseEvent(e);

	static void HandleDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.ClickCount != 2 || sender is not Control ctrl) return;

		bool preview = ReferenceEquals(e.RoutedEvent, PreviewMouseLeftButtonDownEvent)
			|| ReferenceEquals(e.RoutedEvent, PreviewMouseRightButtonDownEvent);
		var doubleClick = new MouseButtonEventArgs(
			preview ? PreviewMouseDoubleClickEvent : MouseDoubleClickEvent,
			e.GetPosition(null), e.ChangedButton, e.ClickCount,
			e.LeftButton, e.RightButton, e.Modifiers)
		{
			PositionSource = e.PositionSource,
			Source = e.Source,
		};

		if (preview)
			ctrl.OnPreviewMouseDoubleClick(doubleClick);
		else
			ctrl.OnMouseDoubleClick(doubleClick);

		if (doubleClick.Handled) e.Handled = true;
	}
}
