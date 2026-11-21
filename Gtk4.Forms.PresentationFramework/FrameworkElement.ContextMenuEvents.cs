using System.Windows.Controls;

namespace System.Windows;

/// <summary>
/// ContextMenuOpening/Closing. A right-button release raises ContextMenuOpening; if no handler sets
/// <c>Handled</c>, the nearest element up the tree with a <see cref="ContextMenu"/> opens it (so a handler may
/// assign <c>ContextMenu</c> first). GTK has already closed the popover when ContextMenuClosing is raised, so
/// handling that event cannot keep the menu open.
/// </summary>
public abstract partial class FrameworkElement
{
	/// <summary>Bubble event raised when a context menu is about to open for this element.</summary>
	public static readonly RoutedEvent ContextMenuOpeningEvent = EventManager.RegisterRoutedEvent(
		"ContextMenuOpening", RoutingStrategy.Bubble, typeof(ContextMenuEventHandler), typeof(FrameworkElement));

	/// <summary>Bubble event raised when a context menu for this element closed.</summary>
	public static readonly RoutedEvent ContextMenuClosingEvent = EventManager.RegisterRoutedEvent(
		"ContextMenuClosing", RoutingStrategy.Bubble, typeof(ContextMenuEventHandler), typeof(FrameworkElement));

	/// <summary>Occurs when a context menu is about to open.</summary>
	public event ContextMenuEventHandler ContextMenuOpening
	{
		add => AddHandler(ContextMenuOpeningEvent, value);
		remove => RemoveHandler(ContextMenuOpeningEvent, value);
	}

	/// <summary>Occurs when a context menu closed.</summary>
	public event ContextMenuEventHandler ContextMenuClosing
	{
		add => AddHandler(ContextMenuClosingEvent, value);
		remove => RemoveHandler(ContextMenuClosingEvent, value);
	}

	/// <summary>Called when a context menu is about to open (class handler).</summary>
	protected virtual void OnContextMenuOpening(ContextMenuEventArgs e) { }

	/// <summary>Called when a context menu closed (class handler).</summary>
	protected virtual void OnContextMenuClosing(ContextMenuEventArgs e) { }

	static void RegisterContextMenuClassHandlers()
	{
		EventManager.RegisterClassHandler(typeof(FrameworkElement), ContextMenuOpeningEvent,
			new ContextMenuEventHandler((s, e) => ((FrameworkElement)s).OnContextMenuOpening(e)));
		EventManager.RegisterClassHandler(typeof(FrameworkElement), ContextMenuClosingEvent,
			new ContextMenuEventHandler((s, e) => ((FrameworkElement)s).OnContextMenuClosing(e)));
	}

	static FrameworkElement()
	{
		RegisterContextMenuClassHandlers();
	}

	internal override void OnContextMenuRequested(Point position)
	{
		var args = new ContextMenuEventArgs(this, opening: true, position.X, position.Y);
		RaiseEvent(args);
		if (args.Handled) return;
		for (UIElement? e = this; e != null; e = e.LogicalParent)
		{
			if (e is FrameworkElement { ContextMenu: { } menu } fe)
			{
				menu.OpenFor(fe);
				return;
			}
		}
	}
}
