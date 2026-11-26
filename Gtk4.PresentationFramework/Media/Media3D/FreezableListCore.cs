using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace System.Windows.Media;

/// <summary>
/// List logic shared by the freezable collections (composed, not inherited): a change raises the owner's
/// <c>Changed</c>, freezing makes the list read-only, and items that are freezables forward their own
/// <c>Changed</c> to the owner.
/// </summary>
sealed class FreezableListCore<T>
{
	static readonly bool _holdsFreezables = typeof(Freezable).IsAssignableFrom(typeof(T));

	readonly List<T> _items = new List<T>();
	readonly Func<bool> _isFrozen;
	readonly Action _throwIfFrozen;
	readonly Action _changed;
	FreezableChildWatcher? _watcher;

	internal FreezableListCore(Func<bool> isFrozen, Action throwIfFrozen, Action changed)
	{
		_isFrozen = isFrozen;
		_throwIfFrozen = throwIfFrozen;
		_changed = changed;
	}

	internal int Count => _items.Count;

	internal IList Items => _items;

	internal T Get(int index) => _items[index];

	internal void Set(int index, T value)
	{
		_throwIfFrozen();
		_items[index] = value;
		Modified();
	}

	internal void Add(T value)
	{
		_throwIfFrozen();
		_items.Add(value);
		Modified();
	}

	internal void Clear()
	{
		_throwIfFrozen();
		_items.Clear();
		Modified();
	}

	internal bool Contains(T value) => _items.Contains(value);

	internal int IndexOf(T value) => _items.IndexOf(value);

	internal void Insert(int index, T value)
	{
		_throwIfFrozen();
		_items.Insert(index, value);
		Modified();
	}

	internal bool Remove(T value)
	{
		_throwIfFrozen();
		bool removed = _items.Remove(value);
		if (removed) Modified();
		return removed;
	}

	internal void RemoveAt(int index)
	{
		_throwIfFrozen();
		_items.RemoveAt(index);
		Modified();
	}

	internal void CopyTo(T[] array, int index) => _items.CopyTo(array, index);

	internal List<T>.Enumerator GetEnumerator() => _items.GetEnumerator();

	void Modified()
	{
		if (_holdsFreezables)
		{
			_watcher ??= new FreezableChildWatcher(() => { if (!_isFrozen()) _changed(); });
			_watcher.Reset(_items.OfType<Freezable>());
		}
		_changed();
	}

	internal bool CanFreezeItems() => !_holdsFreezables || Freezable.CanFreezeAll(_items);

	internal void FreezeItems()
	{
		if (!_holdsFreezables) return;
		foreach (var item in _items)
			(item as Freezable)?.Freeze();
	}

	/// <summary>Appends copies of the items to <paramref name="target"/>.</summary>
	internal void CloneInto(FreezableListCore<T> target, bool currentValueOnly)
	{
		foreach (var item in _items)
		{
			T copy = item is Freezable f
				? (T)(object)(currentValueOnly ? f.CloneCurrentValue() : f.Clone())
				: item;
			target.Add(copy);
		}
	}

	internal string Format(IFormatProvider? provider, Func<T, string> format)
	{
		var sb = new System.Text.StringBuilder();
		for (int i = 0; i < _items.Count; i++)
		{
			if (i > 0) sb.Append(' ');
			sb.Append(format(_items[i]));
		}
		return sb.ToString();
	}
}
