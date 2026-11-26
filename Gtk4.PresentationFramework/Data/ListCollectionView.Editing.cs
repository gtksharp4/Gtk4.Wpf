using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace System.Windows.Data;

/// <remarks>
/// The new-item placeholder row is not produced: <see cref="NewItemPlaceholderPosition"/> is stored and
/// has no effect on the items the view yields.
/// </remarks>
public partial class ListCollectionView
{
	object? _newItem;
	object? _editItem;
	NewItemPlaceholderPosition _placeholder;

	/// <inheritdoc/>
	public NewItemPlaceholderPosition NewItemPlaceholderPosition
	{
		get => _placeholder;
		set
		{
			if (value < NewItemPlaceholderPosition.None || value > NewItemPlaceholderPosition.AtEnd)
				throw new InvalidEnumArgumentException(nameof(value), (int)value, typeof(NewItemPlaceholderPosition));
			if (value == _placeholder)
				return;
			if (IsAddingNew || IsEditingItem)
				throw new InvalidOperationException("The placeholder position cannot change during an add or edit.");
			_placeholder = value;
		}
	}

	/// <inheritdoc/>
	public bool IsAddingNew => _newItem != null;

	/// <inheritdoc/>
	public object? CurrentAddItem => _newItem;

	/// <inheritdoc/>
	public bool IsEditingItem => _editItem != null;

	/// <inheritdoc/>
	public object? CurrentEditItem => _editItem;

	/// <inheritdoc/>
	public bool CanAddNew => !IsEditingItem && !InternalList.IsFixedSize && ItemType() != null;

	/// <inheritdoc/>
	public bool CanRemove => !IsEditingItem && !IsAddingNew && !InternalList.IsFixedSize;

	/// <inheritdoc/>
	public bool CanCancelEdit => _editItem is IEditableObject;

	object IEditableCollectionView.CurrentAddItem => _newItem!;

	object IEditableCollectionView.CurrentEditItem => _editItem!;

	// The element type, when it can be built with a public parameterless constructor.
	Type? ItemType()
	{
		var list = InternalList;
		var type = list.GetType().GetInterfaces()
			.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(System.Collections.Generic.IEnumerable<>))
			.Select(i => i.GetGenericArguments()[0])
			.FirstOrDefault(t => t != typeof(object));
		if (type == null && list.Count > 0 && list[0] != null)
			type = list[0]!.GetType();
		bool buildable = type != null && !type.IsAbstract && !type.IsInterface
			&& (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null);
		return buildable ? type : null;
	}

	/// <inheritdoc/>
	public object AddNew()
	{
		if (IsEditingItem)
			CommitEdit();
		CommitNew();
		var type = CanAddNew ? ItemType() : null;
		if (type == null)
			throw new InvalidOperationException("A new item cannot be added to this view.");
		var item = Activator.CreateInstance(type)!;
		(item as IEditableObject)?.BeginEdit();
		_newItem = item;
		InternalList.Add(item);
		RefreshAfterChange();
		MoveCurrentTo(item);
		return item;
	}

	/// <inheritdoc/>
	public void CommitNew()
	{
		if (IsEditingItem)
			throw new InvalidOperationException("CommitNew is not allowed while an item is being edited.");
		if (_newItem == null)
			return;
		var item = _newItem;
		_newItem = null;
		(item as IEditableObject)?.EndEdit();
		RefreshAfterChange();
	}

	/// <inheritdoc/>
	public void CancelNew()
	{
		if (IsEditingItem)
			throw new InvalidOperationException("CancelNew is not allowed while an item is being edited.");
		if (_newItem == null)
			return;
		var item = _newItem;
		_newItem = null;
		(item as IEditableObject)?.CancelEdit();
		InternalList.Remove(item);
		RefreshAfterChange();
	}

	/// <inheritdoc/>
	public void EditItem(object item)
	{
		if (IsAddingNew)
		{
			if (Equals(item, _newItem))
				return;
			CommitNew();
		}
		CommitEdit();
		_editItem = item;
		(item as IEditableObject)?.BeginEdit();
	}

	/// <inheritdoc/>
	public void CommitEdit()
	{
		if (IsAddingNew)
			throw new InvalidOperationException("CommitEdit is not allowed while a new item is being added.");
		if (_editItem == null)
			return;
		var item = _editItem;
		_editItem = null;
		(item as IEditableObject)?.EndEdit();
		RefreshAfterChange();
	}

	/// <inheritdoc/>
	public void CancelEdit()
	{
		if (IsAddingNew)
			throw new InvalidOperationException("CancelEdit is not allowed while a new item is being added.");
		if (_editItem == null)
			return;
		if (!CanCancelEdit)
			throw new InvalidOperationException("The item being edited does not implement IEditableObject.");
		var item = (IEditableObject)_editItem;
		_editItem = null;
		item.CancelEdit();
	}

	/// <inheritdoc/>
	public void RemoveAt(int index)
	{
		if (IsEditingItem || IsAddingNew)
			throw new InvalidOperationException("RemoveAt is not allowed while an item is being added or edited.");
		var item = GetItemAt(index);
		InternalList.Remove(item);
		RefreshAfterChange();
	}

	/// <inheritdoc/>
	public void Remove(object item)
	{
		int index = IndexOf(item);
		if (index >= 0)
			RemoveAt(index);
	}

	// A source that raises CollectionChanged already refreshed the view.
	void RefreshAfterChange()
	{
		if (!(InternalList is INotifyCollectionChanged))
			Refresh();
	}
}
