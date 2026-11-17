using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// An ordered collection of <see cref="Visual"/> children owned by a
/// <see cref="Visual"/> parent.
/// Mirrors <c>System.Windows.Media.VisualCollection</c>.
/// </summary>
public sealed class VisualCollection : IList<Visual>
{
	readonly Visual _owner;
	readonly List<Visual> _items = new();

	/// <summary>Creates a collection owned by <paramref name="owner"/>.</summary>
	/// <exception cref="ArgumentNullException"/>
	public VisualCollection(Visual owner)
	{
		_owner = owner ?? throw new ArgumentNullException(nameof(owner));
	}

	// ── IList<Visual> ────────────────────────────────────────────────────────────

	/// <inheritdoc/>
	public Visual this[int index]
	{
		get => _items[index];
		set
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			var old = _items[index];
			old.VisualParent = null;
			_items[index] = value;
			value.VisualParent = _owner;
		}
	}

	/// <inheritdoc/>
	public int Count => _items.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <inheritdoc/>
	public void Add(Visual item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		_items.Add(item);
		item.VisualParent = _owner;
	}

	/// <inheritdoc/>
	public void Insert(int index, Visual item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		_items.Insert(index, item);
		item.VisualParent = _owner;
	}

	/// <inheritdoc/>
	public bool Remove(Visual item)
	{
		if (item == null) return false;
		bool removed = _items.Remove(item);
		if (removed) item.VisualParent = null;
		return removed;
	}

	/// <inheritdoc/>
	public void RemoveAt(int index)
	{
		var item = _items[index];
		_items.RemoveAt(index);
		item.VisualParent = null;
	}

	/// <inheritdoc/>
	public void Clear()
	{
		foreach (var item in _items) item.VisualParent = null;
		_items.Clear();
	}

	/// <inheritdoc/>
	public bool Contains(Visual item) => _items.Contains(item);

	/// <inheritdoc/>
	public int IndexOf(Visual item) => _items.IndexOf(item);

	/// <inheritdoc/>
	public void CopyTo(Visual[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	/// <inheritdoc/>
	public IEnumerator<Visual> GetEnumerator() => _items.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}
