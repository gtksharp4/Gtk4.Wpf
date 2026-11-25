using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Shapes;

/// <summary>
/// An ordered list of <see cref="System.Windows.Point"/> values used by
/// <see cref="Polygon"/> and <see cref="Polyline"/>. Every mutation raises <see cref="Freezable.Changed"/>;
/// a frozen collection rejects mutation. Mirrors <c>System.Windows.Media.PointCollection</c>.
/// </summary>
public class PointCollection : Freezable, IList<System.Windows.Point>, IList
{
	readonly List<System.Windows.Point> _items;

	/// <summary>Creates an empty collection.</summary>
	public PointCollection() => _items = new List<System.Windows.Point>();

	/// <summary>Creates an empty collection with room for <paramref name="capacity"/> points.</summary>
	public PointCollection(int capacity)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
		_items = new List<System.Windows.Point>(capacity);
	}

	/// <summary>Creates a collection pre-populated with <paramref name="points"/>.</summary>
	public PointCollection(IEnumerable<System.Windows.Point> points)
	{
		if (points == null) throw new ArgumentNullException(nameof(points));
		_items = new List<System.Windows.Point>(points);
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new PointCollection();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly) =>
		((PointCollection)destination)._items.AddRange(_items);

	/// <summary>Number of points.</summary>
	public int Count => _items.Count;

	/// <summary>Gets or sets the point at <paramref name="index"/>.</summary>
	public System.Windows.Point this[int index]
	{
		get => _items[index];
		set
		{
			ThrowIfFrozen();
			_items[index] = value;
			OnChanged();
		}
	}

	/// <summary>Appends <paramref name="value"/>.</summary>
	public void Add(System.Windows.Point value)
	{
		ThrowIfFrozen();
		_items.Add(value);
		OnChanged();
	}

	/// <summary>Inserts <paramref name="value"/> at <paramref name="index"/>.</summary>
	public void Insert(int index, System.Windows.Point value)
	{
		ThrowIfFrozen();
		_items.Insert(index, value);
		OnChanged();
	}

	/// <summary>Removes the first occurrence of <paramref name="value"/>.</summary>
	public bool Remove(System.Windows.Point value)
	{
		ThrowIfFrozen();
		bool removed = _items.Remove(value);
		if (removed) OnChanged();
		return removed;
	}

	/// <summary>Removes the point at <paramref name="index"/>.</summary>
	public void RemoveAt(int index)
	{
		ThrowIfFrozen();
		_items.RemoveAt(index);
		OnChanged();
	}

	/// <summary>Removes every point.</summary>
	public void Clear()
	{
		ThrowIfFrozen();
		_items.Clear();
		OnChanged();
	}

	/// <summary>Whether <paramref name="value"/> is in the collection.</summary>
	public bool Contains(System.Windows.Point value) => _items.Contains(value);

	/// <summary>Index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(System.Windows.Point value) => _items.IndexOf(value);

	/// <summary>Copies the points to <paramref name="array"/>.</summary>
	public void CopyTo(System.Windows.Point[] array, int index) => _items.CopyTo(array, index);

	/// <summary>Enumerates the points.</summary>
	public List<System.Windows.Point>.Enumerator GetEnumerator() => _items.GetEnumerator();

	IEnumerator<System.Windows.Point> IEnumerable<System.Windows.Point>.GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

	bool ICollection<System.Windows.Point>.IsReadOnly => IsFrozen;

	bool IList.IsReadOnly => IsFrozen;

	bool IList.IsFixedSize => IsFrozen;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

	object? IList.this[int index]
	{
		get => _items[index];
		set => this[index] = (System.Windows.Point)value!;
	}

	int IList.Add(object? value)
	{
		Add((System.Windows.Point)value!);
		return _items.Count - 1;
	}

	bool IList.Contains(object? value) => value is System.Windows.Point p && _items.Contains(p);

	int IList.IndexOf(object? value) => value is System.Windows.Point p ? _items.IndexOf(p) : -1;

	void IList.Insert(int index, object? value) => Insert(index, (System.Windows.Point)value!);

	void IList.Remove(object? value)
	{
		if (value is System.Windows.Point p) Remove(p);
	}

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
}
