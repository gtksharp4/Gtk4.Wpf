using System.Collections;
using System.Collections.Generic;

// D3: The source-typed overload has been removed; the primary constructor in ComboBox.cs
// now accepts object so both ComboBox and ListBox set a correct non-null source.
// This partial adds multi-select members (AddedItems / RemovedItems) and a
// multi-item constructor used by ListBox in multi-selection mode.

namespace System.Windows.Controls;

public sealed partial class SelectionChangedEventArgs
{
	/// <summary>
	/// Items added to the selection in this change.
	/// For single-selection events this contains the new item (if any).
	/// </summary>
	public IList AddedItems { get; }

	/// <summary>
	/// Items removed from the selection in this change.
	/// For single-selection events this contains the old item (if any).
	/// </summary>
	public IList RemovedItems { get; }

	/// <summary>
	/// Multi-select constructor: explicit added/removed lists.
	/// <see cref="OldValue"/> is the first removed item (or <c>null</c>);
	/// <see cref="NewValue"/> is the first added item (or <c>null</c>).
	/// </summary>
	internal SelectionChangedEventArgs(
		object source,
		IList addedItems,
		IList removedItems)
		: this(
			source,
			removedItems.Count > 0 ? removedItems[0] : null,
			addedItems.Count > 0 ? addedItems[0] : null)
	{
		// Override the lists set by the delegated single-item ctor.
		AddedItems = addedItems;
		RemovedItems = removedItems;
	}
}
