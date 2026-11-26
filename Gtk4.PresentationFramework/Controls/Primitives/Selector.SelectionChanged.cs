using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls.Primitives;

public abstract partial class Selector
{
	/// <summary>Identifies the <see cref="SelectionChanged"/> routed event (Bubble).</summary>
	public static readonly RoutedEvent SelectionChangedEvent =
		EventManager.RegisterRoutedEvent(
			nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(Selector));

	/// <summary>Occurs when the selection changes.</summary>
	public event SelectionChangedEventHandler SelectionChanged
	{
		add => AddHandler(SelectionChangedEvent, value);
		remove => RemoveHandler(SelectionChangedEvent, value);
	}

	// The root that was being initialised when this selector was created (null outside InitializeComponent).
	readonly FrameworkElement? _initOwner = InitScope.Current;

	/// <summary>Raises <see cref="SelectionChanged"/>.</summary>
	protected virtual void OnSelectionChanged(SelectionChangedEventArgs e) => RaiseEvent(e);

	/// <summary>Reports a change of a single selected item.</summary>
	internal void NotifySelectionChanged(object? oldItem, object? newItem)
	{
		var removed = new List<object?>();
		var added = new List<object?>();
		if (oldItem != null) removed.Add(oldItem);
		if (newItem != null) added.Add(newItem);
		RaiseSelectionChanged(added, removed);
	}

	/// <summary>
	/// Raises <see cref="SelectionChanged"/> now, or, while this selector's initialisation is pending, one event
	/// per change at the earliest of its own <c>EndInit</c> or its owning root's <c>EndInit</c>.
	/// </summary>
	/// <remarks>
	/// WPF raises it synchronously and only holds back the items of the selector's own BeginInit/EndInit.
	/// Divergence: the generated <c>InitializeComponent</c> brackets the root, not each selector, so selectors created
	/// inside that root are held with the root.
	/// </remarks>
	internal void RaiseSelectionChanged(IList added, IList removed)
	{
		if (added.Count == 0 && removed.Count == 0) return; // WPF never raises an empty change
		OnAutomationSelectionChanged();
		var args = new SelectionChangedEventArgs(SelectionChangedEvent, removed, added) { Source = this };
		FrameworkElement? holder = InitHolder();
		if (holder == null) Deliver(args, added, removed);
		else InitScope.Defer(holder, () => Deliver(args, added, removed));
	}

	void Deliver(SelectionChangedEventArgs args, IList added, IList removed)
	{
		OnSelectionChanged(args);
		OnAutomationSelectionEvents(added, removed);
	}

	// The open init scope holding this selector back: its own, the root that created it, else (a selector built
	// outside any scope) the nearest logical ancestor that is initialising.
	FrameworkElement? InitHolder()
	{
		if (!InitScope.Active) return null;
		if (InitScope.IsOpen(this)) return this;
		if (_initOwner != null) return InitScope.IsOpen(_initOwner) ? _initOwner : null;
		for (UIElement? p = LogicalParent; p != null; p = p.LogicalParent)
			if (p is FrameworkElement fe && InitScope.IsOpen(fe)) return fe;
		return null;
	}
}
