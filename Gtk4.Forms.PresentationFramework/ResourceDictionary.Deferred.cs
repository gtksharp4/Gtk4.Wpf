using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

namespace System.Windows;

public partial class ResourceDictionary
{
	// A keyed value built on first lookup, as WPF defers dictionary values. Shared between dictionaries
	// that copy it, so every holder sees the same realised instance.
	sealed class DeferredValue
	{
		readonly Func<object?> _factory;
		readonly bool _shared;
		bool _busy;
		bool _done;
		object? _value;

		internal DeferredValue(Func<object?> factory, bool shared)
		{
			_factory = factory;
			_shared = shared;
		}

		internal bool Shared => _shared;

		internal object? Realize(object key)
		{
			if (_shared && _done) return _value;
			if (_busy) throw new InvalidOperationException($"Circular reference to resource '{key}'.");
			_busy = true;
			try
			{
				_value = _factory();
				_done = _shared;
				return _value;
			}
			finally
			{
				_busy = false;
			}
		}
	}

	/// <summary>
	/// Adds a value that is built by <paramref name="factory"/> on first lookup (generated code uses it for values
	/// holding <c>{StaticResource}</c> references, which WPF resolves only when the value is realised). A factory
	/// that throws stays unrealised and is retried on the next lookup. With <paramref name="shared"/> false
	/// (<c>x:Shared="False"</c>) every lookup builds a new value.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void SetDeferred(object key, Func<object?> factory, bool shared = true)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (factory == null) throw new ArgumentNullException(nameof(factory));
		_inner[key] = new DeferredValue(factory, shared);
		_resourceChangedHandlers.Raise(key, DependencyProperty.UnsetValue);
	}

	/// <summary>
	/// Copies the local entries and merged dictionaries of <paramref name="source"/>, keeping deferred values deferred.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public void CopyEntriesFrom(ResourceDictionary source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		foreach (var pair in source._inner.ToList())
		{
			_inner[pair.Key] = pair.Value;
			_resourceChangedHandlers.Raise(
				pair.Key, pair.Value is DeferredValue ? DependencyProperty.UnsetValue : pair.Value);
		}
		foreach (var merged in source.MergedDictionaries) MergedDictionaries.Add(merged);
	}

	object? Realize(object key, object? value)
	{
		if (value is not DeferredValue deferred) return value;
		var realised = deferred.Realize(key);
		if (deferred.Shared && _inner.TryGetValue(key, out var current) && ReferenceEquals(current, deferred))
			_inner[key] = realised;
		return realised;
	}

	// The entries with every deferred value realised (an unshared value is built anew for the snapshot).
	List<KeyValuePair<object, object?>> Snapshot()
	{
		var list = new List<KeyValuePair<object, object?>>();
		foreach (var pair in _inner.ToList())
			list.Add(pair.Value is DeferredValue
				? new KeyValuePair<object, object?>(pair.Key, Realize(pair.Key, pair.Value))
				: pair);
		return list;
	}
}
