using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>Represents the method that handles a <c>SelectionChanged</c> routed event.</summary>
public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);

/// <summary>Event data for <see cref="Primitives.Selector.SelectionChanged"/>.</summary>
public class SelectionChangedEventArgs : RoutedEventArgs
{
	/// <summary>Initializes a new instance with the items removed from and added to the selection.</summary>
	/// <exception cref="ArgumentNullException">A list is <see langword="null"/>.</exception>
	public SelectionChangedEventArgs(RoutedEvent id, IList removedItems, IList addedItems)
	{
		RoutedEvent = id ?? throw new ArgumentNullException(nameof(id));
		RemovedItems = removedItems ?? throw new ArgumentNullException(nameof(removedItems));
		AddedItems = addedItems ?? throw new ArgumentNullException(nameof(addedItems));
	}

	/// <summary>Items added to the selection.</summary>
	public IList AddedItems { get; }

	/// <summary>Items removed from the selection.</summary>
	public IList RemovedItems { get; }

	/// <summary>The item that replaced <see cref="RemovedItems"/>[0] (single-item changes only).</summary>
	internal object? OldValue => RemovedItems.Count > 0 ? RemovedItems[0] : null;

	/// <summary>The first added item.</summary>
	internal object? NewValue => AddedItems.Count > 0 ? AddedItems[0] : null;

	/// <summary>Event data for a change of a single selected item.</summary>
	internal static SelectionChangedEventArgs ForItems(RoutedEvent id, object source, object? oldItem, object? newItem)
	{
		var removed = new List<object?>();
		var added = new List<object?>();
		if (oldItem != null) removed.Add(oldItem);
		if (newItem != null) added.Add(newItem);
		return new SelectionChangedEventArgs(id, removed, added) { Source = source };
	}
}
