using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

public partial class Model3DCollection : IList<Model3D>, IList
{
	FreezableListCore<Model3D>? _core;

	FreezableListCore<Model3D> List =>
		_core ??= new FreezableListCore<Model3D>(() => IsFrozen, ThrowIfFrozen, OnChanged);

	/// <summary>The number of items.</summary>
	public int Count => List.Count;

	/// <summary>Gets or sets the item at <paramref name="index"/>.</summary>
	public Model3D this[int index]
	{
		get => List.Get(index);
		set => List.Set(index, value);
	}

	bool ICollection<Model3D>.IsReadOnly => IsFrozen;

	bool IList.IsReadOnly => IsFrozen;

	bool IList.IsFixedSize => IsFrozen;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)List.Items).SyncRoot;

	object? IList.this[int index]
	{
		get => List.Get(index);
		set => List.Set(index, (Model3D)value!);
	}

	/// <summary>Adds an item.</summary>
	public void Add(Model3D value) => List.Add(value);

	/// <summary>Removes all items.</summary>
	public void Clear() => List.Clear();

	/// <summary>Whether the list contains <paramref name="value"/>.</summary>
	public bool Contains(Model3D value) => List.Contains(value);

	/// <summary>The index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(Model3D value) => List.IndexOf(value);

	/// <summary>Inserts an item.</summary>
	public void Insert(int index, Model3D value) => List.Insert(index, value);

	/// <summary>Removes the first occurrence of <paramref name="value"/>.</summary>
	public bool Remove(Model3D value) => List.Remove(value);

	/// <summary>Removes the item at <paramref name="index"/>.</summary>
	public void RemoveAt(int index) => List.RemoveAt(index);

	/// <summary>Copies the items to an array.</summary>
	public void CopyTo(Model3D[] array, int index) => List.CopyTo(array, index);

	/// <summary>Returns an enumerator over the items.</summary>
	public List<Model3D>.Enumerator GetEnumerator() => List.GetEnumerator();

	IEnumerator<Model3D> IEnumerable<Model3D>.GetEnumerator() => List.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

	int IList.Add(object? value)
	{
		Add((Model3D)value!);
		return List.Count - 1;
	}

	bool IList.Contains(object? value) => value is Model3D t && List.Contains(t);

	int IList.IndexOf(object? value) => value is Model3D t ? List.IndexOf(t) : -1;

	void IList.Insert(int index, object? value) => Insert(index, (Model3D)value!);

	void IList.Remove(object? value)
	{
		if (value is Model3D t) Remove(t);
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
		_core?.CloneInto(((Model3DCollection)destination).List, currentValueOnly);
	}
}
