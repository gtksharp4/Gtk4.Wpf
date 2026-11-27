using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace System.Windows.Data;

/// <summary>
/// A list built from several collections and single items, in order. A <see cref="CollectionContainer"/> entry
/// contributes the items of its <see cref="CollectionContainer.Collection"/>; any other entry is one item.
/// Enumeration, <see cref="Count"/> and the indexer see the flattened items. A view over it refreshes when an
/// entry is added or removed and when a container's collection or its content changes.
/// </summary>
public class CompositeCollection : IList, INotifyCollectionChanged
{
	readonly List<object?> _entries = new List<object?>();

	/// <summary>Creates an empty composite collection.</summary>
	public CompositeCollection()
	{
	}

	/// <summary>Creates a composite collection with room for <paramref name="capacity"/> entries.</summary>
	public CompositeCollection(int capacity) => _entries.Capacity = capacity;

	/// <inheritdoc/>
	public event NotifyCollectionChangedEventHandler? CollectionChanged;

	IEnumerable<object?> Flatten()
	{
		foreach (var entry in _entries.ToArray())
		{
			if (entry is CollectionContainer container)
			{
				if (container.Collection == null) continue;
				foreach (var item in container.Collection) yield return item;
			}
			else yield return entry;
		}
	}

	/// <summary>Gets the number of items, with each container expanded.</summary>
	public int Count => Flatten().Count();

	/// <inheritdoc/>
	public bool IsFixedSize => false;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <inheritdoc/>
	public bool IsSynchronized => false;

	/// <inheritdoc/>
	public object SyncRoot => this;

	/// <summary>Gets the flattened item at <paramref name="index"/>. The composite cannot be assigned through.</summary>
	/// <exception cref="NotSupportedException">The setter is used.</exception>
	public object? this[int index]
	{
		get => Flatten().ElementAt(index);
		set => throw new NotSupportedException("A CompositeCollection item cannot be replaced by index.");
	}

	/// <summary>Adds an entry: a <see cref="CollectionContainer"/> or a single item.</summary>
	/// <returns>The flattened position of the entry's first item.</returns>
	public int Add(object? value)
	{
		int position = Count;
		_entries.Add(value);
		Attach(value);
		RaiseReset();
		return position;
	}

	/// <summary>Inserts an entry at the flattened <paramref name="index"/>, which must fall on an entry boundary.</summary>
	public void Insert(int index, object? value)
	{
		_entries.Insert(EntryIndexAt(index), value);
		Attach(value);
		RaiseReset();
	}

	/// <summary>Removes the first entry that equals <paramref name="value"/>.</summary>
	public void Remove(object? value)
	{
		int i = _entries.IndexOf(value);
		if (i < 0) return;
		_entries.RemoveAt(i);
		Detach(value);
		RaiseReset();
	}

	/// <summary>Removes the entry at the flattened <paramref name="index"/>; a container's items go with it.</summary>
	public void RemoveAt(int index)
	{
		int i = EntryIndexAt(index);
		if (i >= _entries.Count) throw new ArgumentOutOfRangeException(nameof(index));
		var entry = _entries[i];
		_entries.RemoveAt(i);
		Detach(entry);
		RaiseReset();
	}

	/// <summary>Removes every entry.</summary>
	public void Clear()
	{
		foreach (var entry in _entries) Detach(entry);
		_entries.Clear();
		RaiseReset();
	}

	/// <summary>Returns whether the flattened items contain <paramref name="value"/>.</summary>
	public bool Contains(object? value) => IndexOf(value) >= 0;

	/// <summary>Returns the flattened index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(object? value)
	{
		int i = 0;
		foreach (var item in Flatten())
		{
			if (Equals(item, value)) return i;
			i++;
		}
		return -1;
	}

	/// <inheritdoc/>
	public void CopyTo(Array array, int index)
	{
		foreach (var item in Flatten()) array.SetValue(item, index++);
	}

	/// <inheritdoc/>
	public IEnumerator GetEnumerator() => Flatten().GetEnumerator();

	int IList.Add(object? value) => Add(value);

	void IList.Remove(object? value) => Remove(value);

	// The entry that starts at flattened position index; a position inside a container's run is not a boundary.
	int EntryIndexAt(int index)
	{
		if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
		int position = 0;
		for (int i = 0; i < _entries.Count; i++)
		{
			if (position == index) return i;
			position += _entries[i] is CollectionContainer c ? c.ItemCount : 1;
			if (position > index) throw new NotSupportedException("The index falls inside a CollectionContainer.");
		}
		if (position == index) return _entries.Count;
		throw new ArgumentOutOfRangeException(nameof(index));
	}

	void Attach(object? entry)
	{
		if (entry is CollectionContainer c) c.ContentChanged += OnContainerChanged;
	}

	void Detach(object? entry)
	{
		if (entry is CollectionContainer c) c.ContentChanged -= OnContainerChanged;
	}

	void OnContainerChanged(object? sender, EventArgs e) => RaiseReset();

	void RaiseReset()
		=> CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
}
