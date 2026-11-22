using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Minimal WPF-compatible resource dictionary. Stores keyed resources and supports
/// merged-dictionary composition (Phase 0 — merge is additive only).
/// </summary>
public partial class ResourceDictionary : IDictionary<object, object?>
{
	readonly Dictionary<object, object?> _inner = new Dictionary<object, object?>();

	/// <summary>
	/// Weak-reference subscriber list backing <see cref="ResourceChanged"/>.
	/// </summary>
	readonly WeakResourceChangedHandlers _resourceChangedHandlers = new();

	/// <summary>
	/// Fired when an entry is added, replaced, removed (<paramref name="newValue"/> is
	/// <see cref="DependencyProperty.UnsetValue"/>), or the dictionary is cleared
	/// (<paramref name="key"/> is <c>null</c>).  Live <c>{DynamicResource}</c> references
	/// subscribe here to re-resolve when their key changes.
	/// Subscribers are held as <em>weak</em> references, so a collected element is
	/// automatically skipped; the dictionary does not pin it alive.
	/// </summary>
	internal event Action<object?, object?> ResourceChanged
	{
		add => _resourceChangedHandlers.Add(value);
		remove => _resourceChangedHandlers.Remove(value);
	}

	/// <summary>
	/// Merged resource dictionaries. Resources in merged dictionaries are consulted
	/// after the local dictionary (last merged wins for duplicates).
	/// </summary>
	public IList<ResourceDictionary> MergedDictionaries { get; } =
		new List<ResourceDictionary>();

	/// <inheritdoc/>
	public object? this[object key]
	{
		get
		{
			return TryGetValue(key, out var v) ? v : null;
		}
		set
		{
			_inner[key] = value;
			_resourceChangedHandlers.Raise(key, value);
		}
	}

	/// <inheritdoc/>
	public bool ContainsKey(object key)
	{
		if (_inner.ContainsKey(key)) return true;
		foreach (var merged in MergedDictionaries)
			if (merged.ContainsKey(key)) return true;
		return false;
	}

	/// <inheritdoc/>
	public bool TryGetValue(object key, out object? value)
	{
		if (_inner.TryGetValue(key, out value))
		{
			value = Realize(key, value);
			return true;
		}
		for (int i = MergedDictionaries.Count - 1; i >= 0; i--)
			if (MergedDictionaries[i].TryGetValue(key, out value)) return true;
		return false;
	}

	/// <inheritdoc/>
	public void Add(object key, object? value)
	{
		_inner.Add(key, value);
		_resourceChangedHandlers.Raise(key, value);
	}

	/// <inheritdoc/>
	public bool Remove(object key)
	{
		bool removed = _inner.Remove(key);
		if (removed)
			_resourceChangedHandlers.Raise(key, DependencyProperty.UnsetValue);
		return removed;
	}

	/// <inheritdoc/>
	public void Clear()
	{
		_inner.Clear();
		_resourceChangedHandlers.Raise(null, DependencyProperty.UnsetValue);
	}

	/// <inheritdoc/>
	public int Count => _inner.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <inheritdoc/>
	public ICollection<object> Keys => _inner.Keys;

	/// <inheritdoc/>
	public ICollection<object?> Values
	{
		get
		{
			RealizeAll();
			return _inner.Values;
		}
	}

	/// <inheritdoc/>
	public void Add(KeyValuePair<object, object?> item)
	{
		_inner.Add(item.Key, item.Value);
		_resourceChangedHandlers.Raise(item.Key, item.Value);
	}

	/// <inheritdoc/>
	public bool Contains(KeyValuePair<object, object?> item)
	{
		RealizeAll();
		return ((ICollection<KeyValuePair<object, object?>>)_inner).Contains(item);
	}

	/// <inheritdoc/>
	public void CopyTo(KeyValuePair<object, object?>[] array, int arrayIndex)
	{
		RealizeAll();
		((ICollection<KeyValuePair<object, object?>>)_inner).CopyTo(array, arrayIndex);
	}

	/// <inheritdoc/>
	public bool Remove(KeyValuePair<object, object?> item)
	{
		bool removed = ((ICollection<KeyValuePair<object, object?>>)_inner).Remove(item);
		if (removed)
			_resourceChangedHandlers.Raise(item.Key, DependencyProperty.UnsetValue);
		return removed;
	}

	/// <inheritdoc/>
	public IEnumerator<KeyValuePair<object, object?>> GetEnumerator()
	{
		RealizeAll();
		return _inner.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}