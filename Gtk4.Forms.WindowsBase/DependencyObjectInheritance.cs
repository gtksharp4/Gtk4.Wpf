using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows;

/// <summary>
/// Manages the child-tracking list that lets inherited-property changes propagate
/// downward through the logical tree.
/// </summary>
/// <remarks>
/// Uses <see cref="ConditionalWeakTable{TKey,TValue}"/> so entries are collected
/// when the parent <see cref="DependencyObject"/> is GC'd (no memory leak).
/// </remarks>
internal static partial class DependencyObjectInheritance
{
	// Parent → weak list of children.
	// ConditionalWeakTable holds the parent weakly; the value (ChildList) is
	// collected together with the parent.
	static readonly ConditionalWeakTable<DependencyObject, ChildList> _children =
		new ConditionalWeakTable<DependencyObject, ChildList>();
	static readonly object _lock = new object();

	/// <summary>Mutable weak-reference list of children, one per parent.</summary>
	sealed class ChildList
	{
		internal readonly List<WeakReference> Items = new List<WeakReference>();
	}

	// ── Registration ─────────────────────────────────────────────────────────────

	/// <summary>Registers <paramref name="child"/> as a child of <paramref name="parent"/>.</summary>
	static void AddChild(DependencyObject parent, DependencyObject child)
	{
		lock (_lock)
		{
			var list = _children.GetOrCreateValue(parent);
			list.Items.Add(new WeakReference(child));
		}
	}

	/// <summary>Removes <paramref name="child"/> from its previous parent's list.</summary>
	static void RemoveChild(DependencyObject parent, DependencyObject child)
	{
		lock (_lock)
		{
			if (!_children.TryGetValue(parent, out var list)) return;
			for (int i = list.Items.Count - 1; i >= 0; i--)
			{
				var t = list.Items[i].Target as DependencyObject;
				if (t == null || ReferenceEquals(t, child))
					list.Items.RemoveAt(i);
			}
		}
	}

	// ── Called by DependencyObject.InheritanceParent setter ──────────────────────
	// The per-parent-change walk is now inlined in DependencyObject.InheritanceParent
	// (it must snapshot effective values *before* the parent field is updated).
	// Track() below handles bookkeeping only.

	/// <summary>
	/// Registers a parent→child edge when <paramref name="child"/> adopts
	/// <paramref name="newParent"/> as its <see cref="DependencyObject.InheritanceParent"/>.
	/// </summary>
	internal static void Track(
		DependencyObject child,
		DependencyObject? oldParent,
		DependencyObject? newParent)
	{
		if (oldParent != null) RemoveChild(oldParent, child);
		if (newParent != null) AddChild(newParent, child);
	}

	// ── Called when a property value changes on a parent ─────────────────────────

	/// <summary>
	/// Propagates an inherited-property change from <paramref name="source"/> to all
	/// registered children that have no local value for <paramref name="dp"/>.
	/// </summary>
	/// <param name="oldValue">
	/// The previously effective value on <paramref name="source"/> (before the change).
	/// Children that have no local value will transition from this old value to
	/// <paramref name="newValue"/>.
	/// </param>
	internal static void PropagateInheritedValue(
		DependencyProperty dp,
		DependencyObject source,
		object? oldValue,
		object? newValue)
	{
		List<WeakReference>? snapshot;
		lock (_lock)
		{
			if (!_children.TryGetValue(source, out var list)) return;
			snapshot = new List<WeakReference>(list.Items); // snapshot so callbacks may mutate
		}

		foreach (var wr in snapshot)
		{
			if (wr.Target is DependencyObject child)
				child.NotifyInheritedValueChanged(dp, oldValue, newValue);
		}
	}
}

/// <summary>
/// Reference-equality comparer used as dictionary key comparer for
/// <see cref="DependencyObject"/> instances.
/// </summary>
internal sealed class ReferenceEqualityComparer : IEqualityComparer<DependencyObject>
{
	internal static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
	bool IEqualityComparer<DependencyObject>.Equals(DependencyObject? x, DependencyObject? y) =>
		ReferenceEquals(x, y);
	int IEqualityComparer<DependencyObject>.GetHashCode(DependencyObject obj) =>
		System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
}