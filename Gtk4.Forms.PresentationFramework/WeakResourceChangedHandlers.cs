using System;
using System.Collections.Generic;

namespace System.Windows;

/// <summary>
/// A weak-reference list of <c>Action&lt;object?, object?&gt;</c> delegates used by
/// <see cref="ResourceDictionary.ResourceChanged"/>.  Storing delegates as weak
/// references means a live dictionary does not pin subscriber objects; a collected
/// element is simply skipped and pruned on the next raise or subscribe call.
/// </summary>
internal sealed class WeakResourceChangedHandlers
{
	// Each slot holds a WeakReference<Action<object?,object?>> so the GC can
	// reclaim the target (the FrameworkElement's bound method) when nothing else
	// keeps the element alive.
	readonly List<WeakReference<Action<object?, object?>>> _handlers = new();

	/// <summary>
	/// Adds <paramref name="handler"/> to the list unless it is already present.
	/// Dead entries found during the scan are pruned.
	/// </summary>
	internal void Add(Action<object?, object?> handler)
	{
		// Prune dead entries and check for existing subscription in one pass.
		for (int i = _handlers.Count - 1; i >= 0; i--)
		{
			if (!_handlers[i].TryGetTarget(out var existing))
			{
				_handlers.RemoveAt(i);
			}
			else if (ReferenceEquals(existing, handler))
			{
				return;  // already subscribed
			}
		}

		_handlers.Add(new WeakReference<Action<object?, object?>>(handler));
	}

	/// <summary>
	/// Removes the first entry whose target is reference-equal to
	/// <paramref name="handler"/>.  Dead entries found during the scan are also pruned.
	/// </summary>
	internal void Remove(Action<object?, object?> handler)
	{
		for (int i = _handlers.Count - 1; i >= 0; i--)
		{
			if (!_handlers[i].TryGetTarget(out var existing))
			{
				_handlers.RemoveAt(i);
			}
			else if (ReferenceEquals(existing, handler))
			{
				_handlers.RemoveAt(i);
				return;
			}
		}
	}

	/// <summary>
	/// Invokes all live handlers with <paramref name="key"/> and
	/// <paramref name="newValue"/>.  Dead entries are pruned during the raise.
	/// </summary>
	internal void Raise(object? key, object? newValue)
	{
		if (_handlers.Count == 0) return;

		// Snapshot count so a handler that mutates the list does not confuse iteration.
		int count = _handlers.Count;
		for (int i = count - 1; i >= 0; i--)
		{
			if (i >= _handlers.Count) continue;  // shrunk by earlier pruning
			if (_handlers[i].TryGetTarget(out var handler))
			{
				handler(key, newValue);
			}
			else
			{
				_handlers.RemoveAt(i);
			}
		}
	}

	/// <summary>Returns <c>true</c> when no live handlers remain.</summary>
	internal bool IsEmpty
	{
		get
		{
			for (int i = _handlers.Count - 1; i >= 0; i--)
			{
				if (_handlers[i].TryGetTarget(out _))
					return false;
				_handlers.RemoveAt(i);
			}
			return true;
		}
	}
}
