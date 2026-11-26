using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows.Controls;

/// <summary>
/// Ordered collection of <see cref="UIElement"/> children owned by a
/// <see cref="Panel"/>. Adding or removing elements calls back into the owner panel
/// so the GTK widget tree stays in sync.
/// </summary>
public sealed class UIElementCollection : IList<UIElement>
{
	readonly List<UIElement> _items = new List<UIElement>();
	readonly Action<UIElement> _added;
	readonly Action<UIElement> _removed;

	internal UIElementCollection(Panel owner) : this(owner.OnChildAdded, owner.OnChildRemoved) { }

	/// <summary>Creates a collection for a non-panel owner (for example <c>FixedPage</c>).</summary>
	internal UIElementCollection(Action<UIElement> added, Action<UIElement> removed)
	{
		_added = added;
		_removed = removed;
	}

	/// <inheritdoc/>
	public UIElement this[int index]
	{
		get => _items[index];
		set
		{
			var old = _items[index];
			_items[index] = value;
			_removed(old);
			_added(value);
		}
	}

	/// <inheritdoc/>
	public int Count => _items.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <inheritdoc/>
	public void Add(UIElement item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		_items.Add(item);
		_added(item);
	}

	/// <inheritdoc/>
	public void Insert(int index, UIElement item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		_items.Insert(index, item);
		_added(item);
	}

	/// <inheritdoc/>
	public bool Remove(UIElement item)
	{
		var removed = _items.Remove(item);
		if (removed) _removed(item);
		return removed;
	}

	/// <inheritdoc/>
	public void RemoveAt(int index)
	{
		var item = _items[index];
		_items.RemoveAt(index);
		_removed(item);
	}

	/// <inheritdoc/>
	public void Clear()
	{
		foreach (var item in _items.ToArray())
			_removed(item);
		_items.Clear();
	}

	/// <inheritdoc/>
	public bool Contains(UIElement item) => _items.Contains(item);

	/// <inheritdoc/>
	public int IndexOf(UIElement item) => _items.IndexOf(item);

	/// <inheritdoc/>
	public void CopyTo(UIElement[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

	/// <inheritdoc/>
	public IEnumerator<UIElement> GetEnumerator() => _items.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
}