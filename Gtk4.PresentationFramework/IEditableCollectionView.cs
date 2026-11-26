namespace System.ComponentModel;

/// <summary>Where the placeholder row for a new item is shown.</summary>
public enum NewItemPlaceholderPosition
{
	/// <summary>No placeholder.</summary>
	None,

	/// <summary>Before the items.</summary>
	AtBeginning,

	/// <summary>After the items.</summary>
	AtEnd,
}

/// <summary>A collection view that can add, edit and remove items transactionally.</summary>
public interface IEditableCollectionView
{
	/// <summary>Where the new-item placeholder is shown.</summary>
	NewItemPlaceholderPosition NewItemPlaceholderPosition { get; set; }

	/// <summary>Whether <see cref="AddNew"/> can create an item.</summary>
	bool CanAddNew { get; }

	/// <summary>Adds a new item and starts editing it.</summary>
	object AddNew();

	/// <summary>Accepts the item created by <see cref="AddNew"/>.</summary>
	void CommitNew();

	/// <summary>Discards the item created by <see cref="AddNew"/>.</summary>
	void CancelNew();

	/// <summary>Whether a new item is being added.</summary>
	bool IsAddingNew { get; }

	/// <summary>The item being added, or null.</summary>
	object CurrentAddItem { get; }

	/// <summary>Whether items can be removed.</summary>
	bool CanRemove { get; }

	/// <summary>Removes the item at <paramref name="index"/> of the view.</summary>
	void RemoveAt(int index);

	/// <summary>Removes <paramref name="item"/>.</summary>
	void Remove(object item);

	/// <summary>Whether <see cref="CancelEdit"/> can undo the current edit.</summary>
	bool CanCancelEdit { get; }

	/// <summary>Starts editing <paramref name="item"/>.</summary>
	void EditItem(object item);

	/// <summary>Accepts the current edit.</summary>
	void CommitEdit();

	/// <summary>Discards the current edit.</summary>
	void CancelEdit();

	/// <summary>Whether an item is being edited.</summary>
	bool IsEditingItem { get; }

	/// <summary>The item being edited, or null.</summary>
	object CurrentEditItem { get; }
}
