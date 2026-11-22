using System.Collections.Generic;

namespace System.Windows.Media;

/// <summary>
/// Forwards the <see cref="Freezable.Changed"/> of child freezables to their owner, so animating a nested
/// object (an <c>EllipseGeometry</c> inside a <c>GeometryDrawing</c> inside a brush) repaints the owner chain.
/// </summary>
sealed class FreezableChildWatcher
{
	readonly EventHandler _forward;
	readonly Dictionary<object, Freezable> _byKey = new Dictionary<object, Freezable>();
	readonly List<Freezable> _collection = new List<Freezable>();

	/// <summary>Creates a watcher that calls <paramref name="raise"/> when a watched child changes.</summary>
	internal FreezableChildWatcher(Action raise) => _forward = (s, e) => raise();

	/// <summary>Watches <paramref name="child"/> as the value of <paramref name="key"/>, replacing the old one.</summary>
	internal void Set(object key, object? child)
	{
		if (_byKey.TryGetValue(key, out var old))
		{
			old.Changed -= _forward;
			_byKey.Remove(key);
		}
		if (child is Freezable { IsFrozen: false } freezable)
		{
			freezable.Changed += _forward;
			_byKey[key] = freezable;
		}
	}

	/// <summary>Watches exactly the unfrozen members of <paramref name="children"/>.</summary>
	internal void Reset(System.Collections.Generic.IEnumerable<Freezable> children)
	{
		foreach (var old in _collection) old.Changed -= _forward;
		_collection.Clear();
		foreach (var child in children)
		{
			if (child.IsFrozen) continue;
			child.Changed += _forward;
			_collection.Add(child);
		}
	}
}
