using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Media.Animation;

public partial class DoubleKeyFrameCollection : IList<DoubleKeyFrame>, IList
{
	FreezableListCore<DoubleKeyFrame>? _core;

	FreezableListCore<DoubleKeyFrame> List =>
		_core ??= new FreezableListCore<DoubleKeyFrame>(() => IsFrozen, ThrowIfFrozen, OnChanged);

	/// <summary>The number of items.</summary>
	public int Count => List.Count;

	/// <summary>Gets or sets the item at <paramref name="index"/>.</summary>
	public DoubleKeyFrame this[int index]
	{
		get => List.Get(index);
		set => List.Set(index, value);
	}

	bool ICollection<DoubleKeyFrame>.IsReadOnly => IsFrozen;

	bool IList.IsReadOnly => IsFrozen;

	bool IList.IsFixedSize => IsFrozen;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)List.Items).SyncRoot;

	object? IList.this[int index]
	{
		get => List.Get(index);
		set => List.Set(index, (DoubleKeyFrame)value!);
	}

	/// <summary>Adds an item.</summary>
	public void Add(DoubleKeyFrame value) => List.Add(value);

	/// <summary>Removes all items.</summary>
	public void Clear() => List.Clear();

	/// <summary>Whether the list contains <paramref name="value"/>.</summary>
	public bool Contains(DoubleKeyFrame value) => List.Contains(value);

	/// <summary>The index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(DoubleKeyFrame value) => List.IndexOf(value);

	/// <summary>Inserts an item.</summary>
	public void Insert(int index, DoubleKeyFrame value) => List.Insert(index, value);

	/// <summary>Removes the first occurrence of <paramref name="value"/>.</summary>
	public bool Remove(DoubleKeyFrame value) => List.Remove(value);

	/// <summary>Removes the item at <paramref name="index"/>.</summary>
	public void RemoveAt(int index) => List.RemoveAt(index);

	/// <summary>Copies the items to an array.</summary>
	public void CopyTo(DoubleKeyFrame[] array, int index) => List.CopyTo(array, index);

	/// <summary>Returns an enumerator over the items.</summary>
	public List<DoubleKeyFrame>.Enumerator GetEnumerator() => List.GetEnumerator();

	IEnumerator<DoubleKeyFrame> IEnumerable<DoubleKeyFrame>.GetEnumerator() => List.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => List.GetEnumerator();

	int IList.Add(object? value)
	{
		Add((DoubleKeyFrame)value!);
		return List.Count - 1;
	}

	bool IList.Contains(object? value) => value is DoubleKeyFrame t && List.Contains(t);

	int IList.IndexOf(object? value) => value is DoubleKeyFrame t ? List.IndexOf(t) : -1;

	void IList.Insert(int index, object? value) => Insert(index, (DoubleKeyFrame)value!);

	void IList.Remove(object? value)
	{
		if (value is DoubleKeyFrame t) Remove(t);
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
		_core?.CloneInto(((DoubleKeyFrameCollection)destination).List, currentValueOnly);
	}
}
