using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Documents;

/// <summary>
/// The storage behind the Table collections: a list whose members get the owner as logical parent. Adding an
/// element that already has a parent throws <see cref="InvalidOperationException"/>, as in WPF.
/// </summary>
sealed class ParentedList<T> : IList<T>, IList
	where T : FrameworkContentElement
{
	readonly List<T> _items = new List<T>();
	readonly DependencyObject _owner;

	internal ParentedList(DependencyObject owner) => _owner = owner;

	public int Count => _items.Count;

	public bool IsReadOnly => false;

	public T this[int index]
	{
		get => _items[index];
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			var old = _items[index];
			if (ReferenceEquals(old, value)) return;
			ContentParenting.Adopt(_owner, value);
			ContentParenting.Release(_owner, old);
			_items[index] = value;
			ContentParenting.Changed(_owner);
		}
	}

	public void Add(T item) => Insert(_items.Count, item);

	public void Insert(int index, T item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		if (index < 0 || index > _items.Count) throw new ArgumentOutOfRangeException(nameof(index));
		ContentParenting.Adopt(_owner, item);
		_items.Insert(index, item);
		ContentParenting.Changed(_owner);
	}

	public void RemoveAt(int index)
	{
		var item = _items[index];
		_items.RemoveAt(index);
		ContentParenting.Release(_owner, item);
		ContentParenting.Changed(_owner);
	}

	public bool Remove(T item)
	{
		int index = _items.IndexOf(item);
		if (index < 0) return false;
		RemoveAt(index);
		return true;
	}

	public void AddRange(IEnumerable range)
	{
		if (range == null) throw new ArgumentNullException(nameof(range));
		foreach (var item in range) Add(Cast(item));
	}

	public void RemoveRange(int index, int count)
	{
		if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
		if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
		if (_items.Count - index < count)
			throw new ArgumentException("index and count do not denote a valid range.");
		var old = _items.GetRange(index, count);
		_items.RemoveRange(index, count);
		foreach (var item in old) ContentParenting.Release(_owner, item);
		ContentParenting.Changed(_owner);
	}

	public int Capacity
	{
		get => _items.Capacity;
		set => _items.Capacity = value;
	}

	public void TrimToSize() => _items.Capacity = _items.Count;

	public void Clear()
	{
		var old = _items.ToArray();
		_items.Clear();
		foreach (var item in old) ContentParenting.Release(_owner, item);
		ContentParenting.Changed(_owner);
	}

	public bool Contains(T item) => _items.Contains(item);

	public int IndexOf(T item) => _items.IndexOf(item);

	public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	public List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

	IEnumerator<T> IEnumerable<T>.GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

	object? IList.this[int index]
	{
		get => _items[index];
		set => this[index] = Cast(value);
	}

	int IList.Add(object? value)
	{
		Add(Cast(value));
		return _items.Count - 1;
	}

	bool IList.Contains(object? value) => value is T item && _items.Contains(item);

	int IList.IndexOf(object? value) => value is T item ? _items.IndexOf(item) : -1;

	void IList.Insert(int index, object? value) => Insert(index, Cast(value));

	void IList.Remove(object? value)
	{
		if (value is T item) Remove(item);
	}

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);

	static T Cast(object? value)
	{
		if (value == null) throw new ArgumentNullException(nameof(value));
		if (value is T item) return item;
		throw new ArgumentException($"The value must be a {typeof(T).Name}.", nameof(value));
	}
}
