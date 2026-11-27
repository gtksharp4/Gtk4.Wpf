using System;
using System.Collections;

namespace System.Windows.Documents;

/// <summary>
/// The non-generic <see cref="IList"/> view of the text collections (WPF TextElementCollection is an IList).
/// Insert and the indexed setter are not supported.
/// </summary>
public sealed partial class InlineCollection : IList
{
	bool IList.IsReadOnly => false;

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

	object? IList.this[int index]
	{
		get => _items[index];
		set => throw new NotSupportedException("Replace by Remove and Add: the collection has no indexed setter.");
	}

	int IList.Add(object? value)
	{
		Add(Cast(value));
		return _items.Count - 1;
	}

	bool IList.Contains(object? value) => value is Inline item && _items.Contains(item);

	int IList.IndexOf(object? value) => value is Inline item ? _items.IndexOf(item) : -1;

	void IList.Insert(int index, object? value) =>
		throw new NotSupportedException("Only appending is supported; use Add.");

	void IList.Remove(object? value)
	{
		if (value is Inline item) Remove(item);
	}

	void IList.RemoveAt(int index) => Remove(_items[index]);

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

	static Inline Cast(object? value) => value as Inline
		?? throw new ArgumentException("The value must be a Inline.", nameof(value));
}

public sealed partial class BlockCollection : IList
{
	bool IList.IsReadOnly => false;

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

	object? IList.this[int index]
	{
		get => _items[index];
		set => throw new NotSupportedException("Replace by Remove and Add: the collection has no indexed setter.");
	}

	int IList.Add(object? value)
	{
		Add(Cast(value));
		return _items.Count - 1;
	}

	bool IList.Contains(object? value) => value is Block item && _items.Contains(item);

	int IList.IndexOf(object? value) => value is Block item ? _items.IndexOf(item) : -1;

	void IList.Insert(int index, object? value) =>
		throw new NotSupportedException("Only appending is supported; use Add.");

	void IList.Remove(object? value)
	{
		if (value is Block item) Remove(item);
	}

	void IList.RemoveAt(int index) => Remove(_items[index]);

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

	static Block Cast(object? value) => value as Block
		?? throw new ArgumentException("The value must be a Block.", nameof(value));
}

public sealed partial class ListItemCollection : IList
{
	bool IList.IsReadOnly => false;

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

	object? IList.this[int index]
	{
		get => _items[index];
		set => throw new NotSupportedException("Replace by Remove and Add: the collection has no indexed setter.");
	}

	int IList.Add(object? value)
	{
		Add(Cast(value));
		return _items.Count - 1;
	}

	bool IList.Contains(object? value) => value is ListItem item && _items.Contains(item);

	int IList.IndexOf(object? value) => value is ListItem item ? _items.IndexOf(item) : -1;

	void IList.Insert(int index, object? value) =>
		throw new NotSupportedException("Only appending is supported; use Add.");

	void IList.Remove(object? value)
	{
		if (value is ListItem item) Remove(item);
	}

	void IList.RemoveAt(int index) => Remove(_items[index]);

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

	static ListItem Cast(object? value) => value as ListItem
		?? throw new ArgumentException("The value must be a ListItem.", nameof(value));
}
