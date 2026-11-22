using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media;

/// <summary>
/// Shared list implementation of the 3D freezable collections: a change raises <c>Changed</c>, freezing makes
/// the list read-only, and items that are freezables forward their own <c>Changed</c> to the list.
/// </summary>
public abstract class FreezableList<T> : Freezable, IList<T>, IList
{
	static readonly bool _holdsFreezables = typeof(Freezable).IsAssignableFrom(typeof(T));

	readonly List<T> _items = new List<T>();
	FreezableChildWatcher? _watcher;

	internal FreezableList() { }

	/// <summary>The number of items.</summary>
	public int Count => _items.Count;

	/// <summary>Gets or sets the item at <paramref name="index"/>.</summary>
	public T this[int index]
	{
		get => _items[index];
		set
		{
			ThrowIfFrozen();
			_items[index] = value;
			Modified();
		}
	}

	bool ICollection<T>.IsReadOnly => IsFrozen;

	bool IList.IsReadOnly => IsFrozen;

	bool IList.IsFixedSize => IsFrozen;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

	object? IList.this[int index]
	{
		get => _items[index];
		set => this[index] = (T)value!;
	}

	/// <summary>Adds an item.</summary>
	public void Add(T value)
	{
		ThrowIfFrozen();
		_items.Add(value);
		Modified();
	}

	/// <summary>Removes all items.</summary>
	public void Clear()
	{
		ThrowIfFrozen();
		_items.Clear();
		Modified();
	}

	/// <summary>Whether the list contains <paramref name="value"/>.</summary>
	public bool Contains(T value) => _items.Contains(value);

	/// <summary>The index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(T value) => _items.IndexOf(value);

	/// <summary>Inserts an item.</summary>
	public void Insert(int index, T value)
	{
		ThrowIfFrozen();
		_items.Insert(index, value);
		Modified();
	}

	/// <summary>Removes the first occurrence of <paramref name="value"/>.</summary>
	public bool Remove(T value)
	{
		ThrowIfFrozen();
		bool removed = _items.Remove(value);
		if (removed) Modified();
		return removed;
	}

	/// <summary>Removes the item at <paramref name="index"/>.</summary>
	public void RemoveAt(int index)
	{
		ThrowIfFrozen();
		_items.RemoveAt(index);
		Modified();
	}

	/// <summary>Copies the items to an array.</summary>
	public void CopyTo(T[] array, int index) => _items.CopyTo(array, index);

	/// <summary>Returns an enumerator over the items.</summary>
	public List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

	int IList.Add(object? value)
	{
		Add((T)value!);
		return _items.Count - 1;
	}

	bool IList.Contains(object? value) => value is T t && _items.Contains(t);

	int IList.IndexOf(object? value) => value is T t ? _items.IndexOf(t) : -1;

	void IList.Insert(int index, object? value) => Insert(index, (T)value!);

	void IList.Remove(object? value)
	{
		if (value is T t) Remove(t);
	}

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

	void Modified()
	{
		if (_holdsFreezables)
		{
			_watcher ??= new FreezableChildWatcher(() => { if (!IsFrozen) OnChanged(); });
			_watcher.Reset(_items.OfType<Freezable>());
		}
		OnChanged();
	}

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		if (!_holdsFreezables) return;
		foreach (var item in _items)
			(item as Freezable)?.Freeze();
	}

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var target = (FreezableList<T>)destination;
		foreach (var item in _items)
		{
			T copy = item is Freezable f
				? (T)(object)(currentValueOnly ? f.CloneCurrentValue() : f.Clone())
				: item;
			target.Add(copy);
		}
	}

	internal string Format(IFormatProvider? provider, Func<T, string> format)
	{
		var sb = new System.Text.StringBuilder();
		for (int i = 0; i < _items.Count; i++)
		{
			if (i > 0) sb.Append(' ');
			sb.Append(format(_items[i]));
		}
		return sb.ToString();
	}
}
