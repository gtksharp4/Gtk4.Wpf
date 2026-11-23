using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows;

/// <summary>Downward edges of the context chain: inheritance children plus the freezables a holder holds.</summary>
internal static partial class DependencyObjectInheritance
{
	static readonly ConditionalWeakTable<DependencyObject, ChildList> _contextChildren =
		new ConditionalWeakTable<DependencyObject, ChildList>();

	/// <summary>Registers the context edge <paramref name="holder"/> to <paramref name="child"/>.</summary>
	internal static void TrackContext(DependencyObject child, DependencyObject? oldHolder, DependencyObject? newHolder)
	{
		lock (_lock)
		{
			if (oldHolder != null && _contextChildren.TryGetValue(oldHolder, out var old))
				old.Items.RemoveAll(w => !(w.Target is DependencyObject t) || ReferenceEquals(t, child));
			if (newHolder != null)
				_contextChildren.GetOrCreateValue(newHolder).Items.Add(new WeakReference(child));
		}
	}

	[ThreadStatic]
	static HashSet<DependencyObject>? _seen;

	/// <summary>Whether anything hangs below <paramref name="root"/> on the context chain.</summary>
	internal static bool HasDescendants(DependencyObject root)
	{
		lock (_lock)
			return (_children.TryGetValue(root, out var c) && c.Items.Count > 0)
				|| (_contextChildren.TryGetValue(root, out var x) && x.Items.Count > 0);
	}

	/// <summary>Adds <paramref name="root"/> and everything below it on the context chain.</summary>
	internal static void CollectSubtree(DependencyObject root, List<DependencyObject> into)
	{
		var seen = _seen ??= new HashSet<DependencyObject>(ReferenceEqualityComparer.Instance);
		seen.Clear();
		seen.Add(root);
		into.Add(root);
		for (int i = 0; i < into.Count; i++)
			lock (_lock)
			{
				AddChildren(_children, into[i], seen, into);
				AddChildren(_contextChildren, into[i], seen, into);
			}
		seen.Clear();
	}

	static void AddChildren(ConditionalWeakTable<DependencyObject, ChildList> table, DependencyObject parent,
		HashSet<DependencyObject> seen, List<DependencyObject> into)
	{
		if (!table.TryGetValue(parent, out var list)) return;
		foreach (var weak in list.Items)
			if (weak.Target is DependencyObject child && seen.Add(child))
				into.Add(child);
	}
}
