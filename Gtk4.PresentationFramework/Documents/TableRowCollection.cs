using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>The rows of a <see cref="TableRowGroup"/>.</summary>
public sealed class TableRowCollection : IList<TableRow>, IList
{
	readonly ParentedList<TableRow> _items;

	internal TableRowCollection(TableRowGroup owner) => _items = new ParentedList<TableRow>(owner);

	/// <summary>Gets the number of elements.</summary>
	public int Count => _items.Count;

	/// <summary>Always <see langword="false"/>.</summary>
	public bool IsReadOnly => false;

	/// <summary>Gets or sets the element at <paramref name="index"/>.</summary>
	public TableRow this[int index]
	{
		get => _items[index];
		set => _items[index] = value;
	}

	/// <summary>Appends <paramref name="item"/> and makes the owner its logical parent.</summary>
	/// <exception cref="InvalidOperationException">The item already has a parent.</exception>
	public void Add(TableRow item) => _items.Add(item);

	/// <summary>Inserts <paramref name="item"/> at <paramref name="index"/>.</summary>
	public void Insert(int index, TableRow item) => _items.Insert(index, item);

	/// <summary>Removes the element at <paramref name="index"/> and clears its parent.</summary>
	public void RemoveAt(int index) => _items.RemoveAt(index);

	/// <summary>Removes <paramref name="item"/>; returns whether it was present.</summary>
	public bool Remove(TableRow item) => _items.Remove(item);

	/// <summary>Removes every element and clears their parents.</summary>
	public void Clear() => _items.Clear();

	/// <summary>Whether <paramref name="item"/> is in the collection.</summary>
	public bool Contains(TableRow item) => _items.Contains(item);

	/// <summary>The index of <paramref name="item"/>, or -1.</summary>
	public int IndexOf(TableRow item) => _items.IndexOf(item);

	/// <summary>Copies the elements to <paramref name="array"/>.</summary>
	public void CopyTo(TableRow[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	/// <summary>Enumerates the elements.</summary>
	public IEnumerator<TableRow> GetEnumerator() => ((IEnumerable<TableRow>)_items).GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_items).GetEnumerator();

	bool IList.IsFixedSize => false;

	/// <summary>Always <see langword="false"/>.</summary>
	public bool IsSynchronized => false;

	/// <summary>The object to lock on.</summary>
	public object SyncRoot => ((ICollection)_items).SyncRoot;

	/// <summary>Appends every element of <paramref name="range"/>.</summary>
	public void AddRange(IEnumerable range) => _items.AddRange(range);

	/// <summary>Removes <paramref name="count"/> elements starting at <paramref name="index"/>.</summary>
	public void RemoveRange(int index, int count) => _items.RemoveRange(index, count);

	/// <summary>The number of elements the collection can hold without resizing.</summary>
	public int Capacity
	{
		get => _items.Capacity;
		set => _items.Capacity = value;
	}

	/// <summary>Trims unused capacity.</summary>
	public void TrimToSize() => _items.TrimToSize();

	/// <summary>Copies the elements to <paramref name="array"/>.</summary>
	public void CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

	object? IList.this[int index]
	{
		get => ((IList)_items)[index];
		set => ((IList)_items)[index] = value;
	}

	int IList.Add(object? value) => ((IList)_items).Add(value);

	bool IList.Contains(object? value) => ((IList)_items).Contains(value);

	int IList.IndexOf(object? value) => ((IList)_items).IndexOf(value);

	void IList.Insert(int index, object? value) => ((IList)_items).Insert(index, value);

	void IList.Remove(object? value) => ((IList)_items).Remove(value);
}
