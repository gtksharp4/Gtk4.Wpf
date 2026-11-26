using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Animation;

namespace System.Windows;

/// <summary>
/// An ordered <see cref="TextDecoration"/> list that is freezable: once frozen, every mutation throws
/// <see cref="InvalidOperationException"/>. Mirrors WPF <c>TextDecorationCollection</c>.
/// </summary>
public sealed class TextDecorationCollection : Animatable, IList<TextDecoration>, IList
{
	readonly List<TextDecoration> _items;

	/// <summary>Creates an empty collection.</summary>
	public TextDecorationCollection() => _items = new List<TextDecoration>();

	/// <summary>Creates an empty collection with room for <paramref name="capacity"/> items.</summary>
	public TextDecorationCollection(int capacity) => _items = new List<TextDecoration>(capacity);

	/// <summary>Creates a collection holding <paramref name="collection"/>.</summary>
	public TextDecorationCollection(IEnumerable<TextDecoration> collection)
	{
		if (collection == null)
			throw new ArgumentNullException(nameof(collection));
		_items = new List<TextDecoration>(collection);
	}

	/// <summary>Number of decorations.</summary>
	public int Count => _items.Count;

	/// <summary>The decoration at <paramref name="index"/>.</summary>
	public TextDecoration this[int index]
	{
		get => _items[index];
		set
		{
			ThrowIfFrozen();
			_items[index] = value ?? throw new ArgumentNullException(nameof(value));
			OnChanged();
		}
	}

	/// <summary>Adds <paramref name="value"/>.</summary>
	public void Add(TextDecoration value)
	{
		ThrowIfFrozen();
		_items.Add(value ?? throw new ArgumentNullException(nameof(value)));
		OnChanged();
	}

	/// <summary>Inserts <paramref name="value"/> at <paramref name="index"/>.</summary>
	public void Insert(int index, TextDecoration value)
	{
		ThrowIfFrozen();
		_items.Insert(index, value ?? throw new ArgumentNullException(nameof(value)));
		OnChanged();
	}

	/// <summary>Removes <paramref name="value"/>; returns whether it was present.</summary>
	public bool Remove(TextDecoration value)
	{
		ThrowIfFrozen();
		bool removed = _items.Remove(value);
		if (removed)
			OnChanged();
		return removed;
	}

	/// <summary>Removes the decoration at <paramref name="index"/>.</summary>
	public void RemoveAt(int index)
	{
		ThrowIfFrozen();
		_items.RemoveAt(index);
		OnChanged();
	}

	/// <summary>Removes all decorations.</summary>
	public void Clear()
	{
		ThrowIfFrozen();
		_items.Clear();
		OnChanged();
	}

	/// <summary>Whether <paramref name="value"/> is in the collection.</summary>
	public bool Contains(TextDecoration value) => _items.Contains(value);

	/// <summary>Index of <paramref name="value"/>, or -1.</summary>
	public int IndexOf(TextDecoration value) => _items.IndexOf(value);

	/// <summary>Copies the decorations into <paramref name="array"/>.</summary>
	public void CopyTo(TextDecoration[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	/// <summary>Returns an enumerator over the decorations.</summary>
	public List<TextDecoration>.Enumerator GetEnumerator() => _items.GetEnumerator();

	/// <summary>Creates an unfrozen deep copy.</summary>
	public new TextDecorationCollection Clone() => (TextDecorationCollection)base.Clone();

	/// <summary>Creates an unfrozen deep copy using current values.</summary>
	public new TextDecorationCollection CloneCurrentValue() => (TextDecorationCollection)base.CloneCurrentValue();

	/// <inheritdoc/>
	public override bool CanFreeze => _items.TrueForAll(d => d.CanFreeze) && base.CanFreeze;

	/// <inheritdoc/>
	protected override void OnFreeze()
	{
		foreach (var item in _items)
			item.Freeze();
		base.OnFreeze();
	}

	/// <inheritdoc/>
	protected override Freezable CreateInstanceCore() => new TextDecorationCollection();

	/// <inheritdoc/>
	protected override void CloneOverride(Freezable destination, bool currentValueOnly)
	{
		base.CloneOverride(destination, currentValueOnly);
		var clone = (TextDecorationCollection)destination;
		foreach (var item in _items)
			clone._items.Add(item.Clone());
	}

	IEnumerator<TextDecoration> IEnumerable<TextDecoration>.GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

	bool ICollection<TextDecoration>.IsReadOnly => IsFrozen;

	bool IList.IsReadOnly => IsFrozen;

	bool IList.IsFixedSize => IsFrozen;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => ((ICollection)_items).SyncRoot;

	object? IList.this[int index]
	{
		get => this[index];
		set => this[index] = (TextDecoration)value!;
	}

	int IList.Add(object? value)
	{
		Add((TextDecoration)value!);
		return Count - 1;
	}

	void IList.Insert(int index, object? value) => Insert(index, (TextDecoration)value!);

	void IList.Remove(object? value) => Remove((TextDecoration)value!);

	bool IList.Contains(object? value) => value is TextDecoration d && Contains(d);

	int IList.IndexOf(object? value) => value is TextDecoration d ? IndexOf(d) : -1;

	void ICollection.CopyTo(Array array, int index) => ((ICollection)_items).CopyTo(array, index);
}
