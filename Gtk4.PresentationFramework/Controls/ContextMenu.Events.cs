namespace System.Windows.Controls;

public partial class ContextMenu
{
	FrameworkElement? _target;

	/// <summary>Bubble event raised when the menu opened.</summary>
	public static readonly RoutedEvent OpenedEvent = EventManager.RegisterRoutedEvent(
		"Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ContextMenu));

	/// <summary>Bubble event raised when the menu closed.</summary>
	public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent(
		"Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ContextMenu));

	/// <summary>Occurs when the menu opened.</summary>
	public event RoutedEventHandler Opened
	{
		add => AddHandler(OpenedEvent, value);
		remove => RemoveHandler(OpenedEvent, value);
	}

	/// <summary>Occurs when the menu closed.</summary>
	public event RoutedEventHandler Closed
	{
		add => AddHandler(ClosedEvent, value);
		remove => RemoveHandler(ClosedEvent, value);
	}

	/// <summary>Opens the menu on <paramref name="target"/> (the element that owns the context menu).</summary>
	internal void OpenFor(FrameworkElement target)
	{
		_target = target;
		// A popover needs a rooted parent; without a window only the open state is recorded.
		if (target.Widget.Root != null)
			Open(target.Widget);
		else
			MarkOpenWithoutPopup();
		RaiseEvent(new RoutedEventArgs(OpenedEvent));
	}

	void MarkOpenWithoutPopup()
	{
		_updating = true;
		try { SetValue(IsOpenProperty, true); }
		finally { _updating = false; }
	}

	void OnClosedCore()
	{
		RaiseEvent(new RoutedEventArgs(ClosedEvent));
		var target = _target;
		_target = null;
		target?.RaiseEvent(new ContextMenuEventArgs(target, opening: false));
	}
}
