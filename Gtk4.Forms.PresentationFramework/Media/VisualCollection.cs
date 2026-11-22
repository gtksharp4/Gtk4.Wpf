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
			_owner.ReleaseVisualChild(old);
			_items[index] = value;
			try { _owner.AdoptVisualChild(value); }
			catch
			{
				_items[index] = old;
				_owner.AdoptVisualChild(old);
				throw;
			}
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
		try { _owner.AdoptVisualChild(item); }
		catch
		{
			_items.RemoveAt(_items.Count - 1);
			throw;
		}
	}

	/// <inheritdoc/>
	public void Insert(int index, Visual item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		_items.Insert(index, item);
		try { _owner.AdoptVisualChild(item); }
		catch
		{
			_items.RemoveAt(index);
			throw;
		}
	}

	/// <inheritdoc/>
	public bool Remove(Visual item)
	{
		if (item == null) return false;
		bool removed = _items.Remove(item);
		if (removed) _owner.ReleaseVisualChild(item);
		return removed;
	}

	/// <inheritdoc/>
	public void RemoveAt(int index)
	{
		var item = _items[index];
		_items.RemoveAt(index);
		_owner.ReleaseVisualChild(item);
	}

	/// <inheritdoc/>
	public void Clear()
	{
		var old = _items.ToArray();
		_items.Clear();
		foreach (var item in old) _owner.ReleaseVisualChild(item);
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
