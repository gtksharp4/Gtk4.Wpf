using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace System.Windows.Data;

/// <summary>
/// A list of entries, in order: each entry is a <see cref="CollectionContainer"/> or a single item, and the list
/// members (<see cref="Count"/>, the indexer, <see cref="IndexOf"/>, <see cref="Insert"/>) work on those entries,
/// as in WPF. The flattened items (a container expanded to the items of its
/// <see cref="CollectionContainer.Collection"/>) are what a view over the list shows: it refreshes when an entry
/// is added or removed and when a container's collection or its content changes.
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

	/// <summary>Raised when the flattened items may have changed: an entry change or a container change.</summary>
	internal event EventHandler? FlattenedChanged;

	/// <summary>The items with each container expanded, in order.</summary>
	internal IEnumerable<object?> Flatten()
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

	/// <summary>Gets the number of entries; a <see cref="CollectionContainer"/> counts as one.</summary>
	public int Count => _entries.Count;

	/// <inheritdoc/>
	public bool IsFixedSize => false;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <inheritdoc/>
	public bool IsSynchronized => false;

	/// <inheritdoc/>
	public object SyncRoot => this;

	/// <summary>Gets or sets the entry at <paramref name="index"/>; setting raises a Replace change.</summary>
	public object? this[int index]
	{
		get => _entries[index];
		set
		{
			var old = _entries[index];
			_entries[index] = value;
			Detach(old);
			Attach(value);
			Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, old, index));
		}
	}

	/// <summary>Adds an entry: a <see cref="CollectionContainer"/> or a single item.</summary>
	/// <returns>The index of the entry.</returns>
	public int Add(object? value)
	{
		int index = _entries.Count;
		_entries.Add(value);
		Attach(value);
		Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
		return index;
	}

	/// <summary>Inserts an entry at the entry index <paramref name="index"/>.</summary>
	public void Insert(int index, object? value)
	{
		_entries.Insert(index, value);
		Attach(value);
		Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
	}

	/// <summary>Removes the first entry that equals <paramref name="value"/>.</summary>
	public void Remove(object? value)
	{
		int i = _entries.IndexOf(value);
		if (i >= 0) RemoveAt(i);
	}

	/// <summary>Removes the entry at <paramref name="index"/>; a container's items go with it.</summary>
	public void RemoveAt(int index)
	{
		var entry = _entries[index];
		_entries.RemoveAt(index);
		Detach(entry);
		Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, entry, index));
	}

	/// <summary>Removes every entry.</summary>
	public void Clear()
	{
		foreach (var entry in _entries) Detach(entry);
		_entries.Clear();
		Raise(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	/// <summary>Returns whether the entries contain <paramref name="value"/>.</summary>
	public bool Contains(object? value) => _entries.Contains(value);

	/// <summary>Returns the entry index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(object? value) => _entries.IndexOf(value);

	/// <inheritdoc/>
	public void CopyTo(Array array, int index)
	{
		foreach (var entry in _entries) array.SetValue(entry, index++);
	}

	/// <inheritdoc/>
	public IEnumerator GetEnumerator() => _entries.ToArray().GetEnumerator();

	int IList.Add(object? value) => Add(value);

	void IList.Remove(object? value) => Remove(value);

	void Attach(object? entry)
	{
		if (entry is CollectionContainer c) c.ContentChanged += OnContainerChanged;
	}

	void Detach(object? entry)
	{
		if (entry is CollectionContainer c) c.ContentChanged -= OnContainerChanged;
	}

	void OnContainerChanged(object? sender, EventArgs e) => FlattenedChanged?.Invoke(this, EventArgs.Empty);

	void Raise(NotifyCollectionChangedEventArgs args)
	{
		CollectionChanged?.Invoke(this, args);
		FlattenedChanged?.Invoke(this, EventArgs.Empty);
	}
}

/// <summary>What a view over a <see cref="CompositeCollection"/> enumerates: the flattened items.</summary>
sealed class CompositeFlatSource : IEnumerable, INotifyCollectionChanged
{
	readonly CompositeCollection _composite;

	internal CompositeFlatSource(CompositeCollection composite)
	{
		_composite = composite;
		composite.FlattenedChanged += (_, _) => CollectionChanged?.Invoke(this,
			new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public event NotifyCollectionChangedEventHandler? CollectionChanged;

	public IEnumerator GetEnumerator() => _composite.Flatten().GetEnumerator();
}
