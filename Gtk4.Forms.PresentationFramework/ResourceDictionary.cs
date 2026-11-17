using System;
using System.Collections;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// Minimal WPF-compatible resource dictionary. Stores keyed resources and supports
/// merged-dictionary composition (Phase 0 — merge is additive only).
/// </summary>
public class ResourceDictionary : IDictionary<object, object?>
{
	readonly Dictionary<object, object?> _inner = new Dictionary<object, object?>();

	/// <summary>
	/// Fired when an entry is added, replaced, removed (<paramref name="newValue"/> is
	/// <see cref="DependencyProperty.UnsetValue"/>), or the dictionary is cleared
	/// (<paramref name="key"/> is <c>null</c>).  Live <c>{DynamicResource}</c> references
	/// subscribe here to re-resolve when their key changes.
	/// </summary>
	internal event Action<object?, object?>? ResourceChanged;

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
			if (_inner.TryGetValue(key, out var v)) return v;
			for (int i = MergedDictionaries.Count - 1; i >= 0; i--)
				if (MergedDictionaries[i].TryGetValue(key, out v)) return v;
			return null;
		}
		set
		{
			_inner[key] = value;
			ResourceChanged?.Invoke(key, value);
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
		if (_inner.TryGetValue(key, out value)) return true;
		for (int i = MergedDictionaries.Count - 1; i >= 0; i--)
			if (MergedDictionaries[i].TryGetValue(key, out value)) return true;
		return false;
	}

	/// <inheritdoc/>
	public void Add(object key, object? value)
	{
		_inner.Add(key, value);
		ResourceChanged?.Invoke(key, value);
	}

	/// <inheritdoc/>
	public bool Remove(object key)
	{
		bool removed = _inner.Remove(key);
		if (removed)
			ResourceChanged?.Invoke(key, DependencyProperty.UnsetValue);
		return removed;
	}

	/// <inheritdoc/>
	public void Clear()
	{
		_inner.Clear();
		ResourceChanged?.Invoke(null, DependencyProperty.UnsetValue);
	}

	/// <inheritdoc/>
	public int Count => _inner.Count;

	/// <inheritdoc/>
	public bool IsReadOnly => false;

	/// <inheritdoc/>
	public ICollection<object> Keys => _inner.Keys;

	/// <inheritdoc/>
	public ICollection<object?> Values => _inner.Values;

	/// <inheritdoc/>
	public void Add(KeyValuePair<object, object?> item)
	{
		_inner.Add(item.Key, item.Value);
		ResourceChanged?.Invoke(item.Key, item.Value);
	}

	/// <inheritdoc/>
	public bool Contains(KeyValuePair<object, object?> item) =>
		((ICollection<KeyValuePair<object, object?>>)_inner).Contains(item);

	/// <inheritdoc/>
	public void CopyTo(KeyValuePair<object, object?>[] array, int arrayIndex) =>
		((ICollection<KeyValuePair<object, object?>>)_inner).CopyTo(array, arrayIndex);

	/// <inheritdoc/>
	public bool Remove(KeyValuePair<object, object?> item)
	{
		bool removed = ((ICollection<KeyValuePair<object, object?>>)_inner).Remove(item);
		if (removed)
			ResourceChanged?.Invoke(item.Key, DependencyProperty.UnsetValue);
		return removed;
	}

	/// <inheritdoc/>
	public IEnumerator<KeyValuePair<object, object?>> GetEnumerator() => _inner.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => _inner.GetEnumerator();
}