using System;
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
		bool _busy;
		bool _done;
		object? _value;

		internal DeferredValue(Func<object?> factory) => _factory = factory;

		internal object? Realize(object key)
		{
			if (_done) return _value;
			if (_busy) throw new InvalidOperationException($"Circular reference to resource '{key}'.");
			_busy = true;
			try
			{
				_value = _factory();
				_done = true;
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
	/// that throws stays unrealised and is retried on the next lookup.
	/// </summary>
	public void SetDeferred(object key, Func<object?> factory)
	{
		if (key == null) throw new ArgumentNullException(nameof(key));
		if (factory == null) throw new ArgumentNullException(nameof(factory));
		_inner[key] = new DeferredValue(factory);
		_resourceChangedHandlers.Raise(key, DependencyProperty.UnsetValue);
	}

	/// <summary>Copies the local entries and merged dictionaries of <paramref name="source"/>, keeping deferred values deferred.</summary>
	public void CopyEntriesFrom(ResourceDictionary source)
	{
		if (source == null) throw new ArgumentNullException(nameof(source));
		foreach (var pair in source._inner.ToList())
		{
			_inner[pair.Key] = pair.Value;
			_resourceChangedHandlers.Raise(pair.Key, pair.Value is DeferredValue ? DependencyProperty.UnsetValue : pair.Value);
		}
		foreach (var merged in source.MergedDictionaries) MergedDictionaries.Add(merged);
	}

	object? Realize(object key, object? value)
	{
		if (value is not DeferredValue deferred) return value;
		var realised = deferred.Realize(key);
		if (_inner.TryGetValue(key, out var current) && ReferenceEquals(current, deferred)) _inner[key] = realised;
		return realised;
	}

	void RealizeAll()
	{
		foreach (var pair in _inner.Where(p => p.Value is DeferredValue).ToList()) Realize(pair.Key, pair.Value);
	}
}
