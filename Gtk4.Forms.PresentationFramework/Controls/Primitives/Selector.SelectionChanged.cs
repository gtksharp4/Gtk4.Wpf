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

	List<object?>? _pendingAdded;
	List<object?>? _pendingRemoved;

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
	/// Raises <see cref="SelectionChanged"/> now, or, while an <c>InitializeComponent</c> is running, once when
	/// the outermost initialization ends (WPF defers selection events during <c>BeginInit</c>/<c>EndInit</c>).
	/// </summary>
	internal void RaiseSelectionChanged(IList added, IList removed)
	{
		if (!InitScope.Active)
		{
			OnSelectionChanged(new SelectionChangedEventArgs(SelectionChangedEvent, removed, added) { Source = this });
			return;
		}
		if (_pendingAdded == null)
		{
			_pendingAdded = new List<object?>();
			_pendingRemoved = new List<object?>();
			InitScope.Defer(FlushSelectionChanged);
		}
		Merge(removed, _pendingAdded, _pendingRemoved!);
		Merge(added, _pendingRemoved!, _pendingAdded);
	}

	// An item added then removed (or the reverse) while deferred nets out.
	static void Merge(IList items, List<object?> opposite, List<object?> same)
	{
		foreach (var item in items)
		{
			if (!opposite.Remove(item)) same.Add(item);
		}
	}

	void FlushSelectionChanged()
	{
		var added = _pendingAdded!;
		var removed = _pendingRemoved!;
		_pendingAdded = _pendingRemoved = null;
		if (added.Count == 0 && removed.Count == 0) return;
		OnSelectionChanged(new SelectionChangedEventArgs(SelectionChangedEvent, removed, added) { Source = this });
	}
}
