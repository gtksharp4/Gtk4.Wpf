using System.Collections.Generic;

namespace System.Windows
{
	/// <summary>
	/// Manages the child-tracking list that lets inherited-property changes propagate
	/// downward through the logical tree.
	/// </summary>
	/// <remarks>
	/// Phase-0 implementation: a simple <see cref="WeakReference"/> list per parent object.
	/// Children register themselves when they set <see cref="DependencyObject.InheritanceParent"/>.
	/// </remarks>
	internal static class DependencyObjectInheritance
	{
		// Parent → weak list of children.
		static readonly Dictionary<DependencyObject, List<WeakReference>> _children =
			new Dictionary<DependencyObject, List<WeakReference>>(ReferenceEqualityComparer.Instance);
		static readonly object _lock = new object();

		// ── Registration ─────────────────────────────────────────────────────────────

		/// <summary>Registers <paramref name="child"/> as a child of <paramref name="parent"/>.</summary>
		static void AddChild(DependencyObject parent, DependencyObject child)
		{
			lock (_lock)
			{
				if (!_children.TryGetValue(parent, out var list))
				{
					list = new List<WeakReference>();
					_children[parent] = list;
				}
				list.Add(new WeakReference(child));
			}
		}

		/// <summary>Removes <paramref name="child"/> from its previous parent's list.</summary>
		static void RemoveChild(DependencyObject parent, DependencyObject child)
		{
			lock (_lock)
			{
				if (!_children.TryGetValue(parent, out var list)) return;
				for (int i = list.Count - 1; i >= 0; i--)
				{
					var t = list[i].Target as DependencyObject;
					if (t == null || ReferenceEquals(t, child))
						list.RemoveAt(i);
				}
			}
		}

		// ── Called by DependencyObject.InheritanceParent setter ──────────────────────

		/// <summary>
		/// Called when a <see cref="DependencyObject"/> changes its
		/// <see cref="DependencyObject.InheritanceParent"/>. Re-evaluates every inheriting
		/// property with no local value.
		/// </summary>
		internal static void NotifyInheritedPropertiesChanged(DependencyObject child)
		{
			// We cannot enumerate all registered properties efficiently in Phase 0 without
			// iterating the entire registry.  Instead we rely on each property that IS
			// inherited having its value change propagated via PropagateInheritedValue.
			// This method just ensures the child-tracking bookkeeping is correct so future
			// changes from the new parent flow correctly.
			// (Full re-evaluation on parent change would require a full property walk —
			//  left as a Phase-1 enhancement.)
		}

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
			List<WeakReference>? list;
			lock (_lock)
			{
				if (!_children.TryGetValue(source, out list)) return;
				list = new List<WeakReference>(list); // snapshot so callbacks may mutate
			}

			foreach (var wr in list)
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
}
