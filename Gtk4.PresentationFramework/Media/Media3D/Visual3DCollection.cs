using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

/// <summary>The child visuals of a <see cref="ModelVisual3D"/> or a Viewport3D.</summary>
public sealed class Visual3DCollection : IList<Visual3D>, IList
{
	readonly List<Visual3D> _items = new List<Visual3D>();
	readonly IVisual3DOwner _owner;

	internal Visual3DCollection(IVisual3DOwner owner) => _owner = owner;

	/// <summary>The number of children.</summary>
	public int Count => _items.Count;

	bool ICollection<Visual3D>.IsReadOnly => false;

	bool IList.IsReadOnly => false;

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

	/// <summary>Gets or sets the child at <paramref name="index"/>.</summary>
	public Visual3D this[int index]
	{
		get => _items[index];
		set
		{
			var old = _items[index];
			if (ReferenceEquals(old, value)) return;
			Attach(value);
			old.Owner = null;
			_items[index] = value;
			_owner.Invalidate3D();
		}
	}

	object? IList.this[int index]
	{
		get => _items[index];
		set => this[index] = (Visual3D)value!;
	}

	void Attach(Visual3D child)
	{
		if (child == null) throw new ArgumentNullException(nameof(child));
		if (child.Owner != null)
			throw new ArgumentException("The Visual3D already has a parent.", nameof(child));
		if (ReferenceEquals(child, _owner))
			throw new ArgumentException("A Visual3D cannot be its own child.", nameof(child));
		child.Owner = _owner;
	}

	/// <summary>Adds a child.</summary>
	public void Add(Visual3D value)
	{
		Attach(value);
		_items.Add(value);
		_owner.Invalidate3D();
	}

	/// <summary>Removes all children.</summary>
	public void Clear()
	{
		foreach (var item in _items) item.Owner = null;
		_items.Clear();
		_owner.Invalidate3D();
	}

	/// <summary>Whether <paramref name="value"/> is a child.</summary>
	public bool Contains(Visual3D value) => _items.Contains(value);

	/// <summary>The index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(Visual3D value) => _items.IndexOf(value);

	/// <summary>Inserts a child.</summary>
	public void Insert(int index, Visual3D value)
	{
		Attach(value);
		_items.Insert(index, value);
		_owner.Invalidate3D();
	}

	/// <summary>Removes a child.</summary>
	public bool Remove(Visual3D value)
	{
		if (!_items.Remove(value)) return false;
		value.Owner = null;
		_owner.Invalidate3D();
		return true;
	}

	/// <summary>Removes the child at <paramref name="index"/>.</summary>
	public void RemoveAt(int index)
	{
		_items[index].Owner = null;
		_items.RemoveAt(index);
		_owner.Invalidate3D();
	}

	/// <summary>Copies the children to an array.</summary>
	public void CopyTo(Visual3D[] array, int index) => _items.CopyTo(array, index);

	/// <summary>Returns an enumerator over the children.</summary>
	public List<Visual3D>.Enumerator GetEnumerator() => _items.GetEnumerator();

	IEnumerator<Visual3D> IEnumerable<Visual3D>.GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

	int IList.Add(object? value)
	{
		Add((Visual3D)value!);
		return _items.Count - 1;
	}

	bool IList.Contains(object? value) => value is Visual3D v && _items.Contains(v);

	int IList.IndexOf(object? value) => value is Visual3D v ? _items.IndexOf(v) : -1;

	void IList.Insert(int index, object? value) => Insert(index, (Visual3D)value!);

	void IList.Remove(object? value)
	{
		if (value is Visual3D v) Remove(v);
	}

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
}
