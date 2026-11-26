using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

public partial class Vector3DCollection : IList<Vector3D>, IList
{
	FreezableListCore<Vector3D>? _core;

	FreezableListCore<Vector3D> List =>
		_core ??= new FreezableListCore<Vector3D>(() => IsFrozen, ThrowIfFrozen, OnChanged);

	/// <summary>The number of items.</summary>
	public int Count => List.Count;

	/// <summary>Gets or sets the item at <paramref name="index"/>.</summary>
	public Vector3D this[int index]
	{
		get => List.Get(index);
		set => List.Set(index, value);
	}

	bool ICollection<Vector3D>.IsReadOnly => IsFrozen;

	bool IList.IsReadOnly => IsFrozen;

	bool IList.IsFixedSize => IsFrozen;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)List.Items).SyncRoot;

	object? IList.this[int index]
	{
		get => List.Get(index);
		set => List.Set(index, (Vector3D)value!);
	}

	/// <summary>Adds an item.</summary>
	public void Add(Vector3D value) => List.Add(value);

	/// <summary>Removes all items.</summary>
	public void Clear() => List.Clear();

	/// <summary>Whether the list contains <paramref name="value"/>.</summary>
	public bool Contains(Vector3D value) => List.Contains(value);

	/// <summary>The index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(Vector3D value) => List.IndexOf(value);

	/// <summary>Inserts an item.</summary>
	public void Insert(int index, Vector3D value) => List.Insert(index, value);

	/// <summary>Removes the first occurrence of <paramref name="value"/>.</summary>
	public bool Remove(Vector3D value) => List.Remove(value);

	/// <summary>Removes the item at <paramref name="index"/>.</summary>
	public void RemoveAt(int index) => List.RemoveAt(index);

	/// <summary>Copies the items to an array.</summary>
	public void CopyTo(Vector3D[] array, int index) => List.CopyTo(array, index);

	/// <summary>Returns an enumerator over the items.</summary>
	public List<Vector3D>.Enumerator GetEnumerator() => List.GetEnumerator();

	IEnumerator<Vector3D> IEnumerable<Vector3D>.GetEnumerator() => List.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

	int IList.Add(object? value)
	{
		Add((Vector3D)value!);
		return List.Count - 1;
	}

	bool IList.Contains(object? value) => value is Vector3D t && List.Contains(t);

	int IList.IndexOf(object? value) => value is Vector3D t ? List.IndexOf(t) : -1;

	void IList.Insert(int index, object? value) => Insert(index, (Vector3D)value!);

	void IList.Remove(object? value)
	{
		if (value is Vector3D t) Remove(t);
	}

	void ICollection.CopyTo(Array array, int index) => ((ICollection)List.Items).CopyTo(array, index);

	/// <inheritdoc/>
	public override bool CanFreeze => base.CanFreeze && (_core?.CanFreezeItems() ?? true);

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		base.OnFreeze();
		_core?.FreezeItems();
	}

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		_core?.CloneInto(((Vector3DCollection)destination).List, currentValueOnly);
	}
}
