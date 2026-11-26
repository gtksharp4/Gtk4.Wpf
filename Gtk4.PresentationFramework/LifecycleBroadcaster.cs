using System.Collections.Generic;
using System.Windows.Threading;

namespace System.Windows;

/// <summary>
/// The managed half of WPF's element lifecycle (WPF's <c>BroadcastEventHelper</c>): pre-order
/// <see cref="FrameworkElement.Loaded"/>/<see cref="FrameworkElement.Unloaded"/> broadcast over
/// the logical tree with raise-once/<see cref="FrameworkElement.IsLoaded"/> bookkeeping.
/// </summary>
/// <remarks>
/// <para>
/// GTK only triggers the root: <see cref="Window"/> broadcasts Loaded when its toplevel maps
/// and Unloaded when it closes (<c>Window.Lifecycle.cs</c>). Per-widget map/unmap is
/// deliberately <b>not</b> used — GTK unmaps hidden widgets, which would make a hidden element
/// raise Unloaded (WPF never does that for visibility changes).
/// </para>
/// <para>
/// Attaching a subtree to an already-loaded parent (or detaching one from it) is reported via
/// <see cref="OnLogicalParentChanged"/> and broadcast later, at
/// <see cref="DispatcherPriority.Loaded"/>, re-checking the tree state when the work runs so a
/// detach immediately followed by a re-attach raises nothing (as in WPF).
/// </para>
/// </remarks>
internal static class LifecycleBroadcaster
{
	/// <summary>Enumerates <paramref name="root"/> and its descendants, root first (pre-order).</summary>
	internal static IEnumerable<UIElement> EnumeratePreOrder(UIElement root)
	{
		if (root == null) throw new ArgumentNullException(nameof(root));
		var stack = new Stack<UIElement>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			var current = stack.Pop();
			yield return current;
			var children = GetChildren(current);
			for (int i = children.Count - 1; i >= 0; i--)
				stack.Push(children[i]);
		}
	}

	/// <summary>
	/// Direct children of <paramref name="element"/>: the logical-collection order first
	/// (<see cref="LogicalTreeHelper.GetChildren"/>), then any other element parented to it
	/// (template roots, presenters, adorners) in attach order. Returned as a snapshot so
	/// handlers may mutate the tree during a broadcast.
	/// </summary>
	internal static List<UIElement> GetChildren(UIElement element)
	{
		var result = new List<UIElement>();
		var tracked = element.LifecycleChildren;
		if (tracked.Count == 0) return result;
		// Identity, not Equals: an element type may override equality.
		var seen = new HashSet<UIElement>(ReferenceEqualityComparer.Instance);
		foreach (var o in LogicalTreeHelper.GetChildren(element))
		{
			if (o is UIElement child && ReferenceEquals(child.LogicalParent, element) && seen.Add(child))
				result.Add(child);
		}
		foreach (var child in tracked)
		{
			if (seen.Add(child))
				result.Add(child);
		}
		return result;
	}

	/// <summary>
	/// Enumerates <paramref name="root"/> and every element parented to it, root first, in attach
	/// order (no logical-collection ordering or de-duplication) — linear, for the per-frame
	/// allocation sync, which needs coverage but not the logical order.
	/// </summary>
	internal static IEnumerable<UIElement> EnumerateAttached(UIElement root)
	{
		var stack = new Stack<UIElement>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			var current = stack.Pop();
			yield return current;
			var children = current.LifecycleChildren;
			for (int i = children.Count - 1; i >= 0; i--)
				stack.Push(children[i]);
		}
	}

	/// <summary>
	/// Raises <see cref="FrameworkElement.Loaded"/> on every not-yet-loaded element of the
	/// subtree, pre-order. Each element is first initialized (WPF guarantees Initialized precedes
	/// Loaded) and has <see cref="FrameworkElement.IsLoaded"/> set before its handlers run.
	/// </summary>
	internal static void BroadcastLoaded(UIElement root)
	{
		if (root is FrameworkElement fe && !fe.IsLoaded)
		{
			fe.TryFireInitialized();
			fe.SetLoadedAndRaise(true);
		}
		// Children are read after the element's handlers ran, so children they add are reached.
		foreach (var child in GetChildren(root))
			BroadcastLoaded(child);
	}

	/// <summary>Raises <see cref="FrameworkElement.Unloaded"/> on every loaded element of the subtree, pre-order.</summary>
	internal static void BroadcastUnloaded(UIElement root)
	{
		if (root is FrameworkElement fe && fe.IsLoaded)
			fe.SetLoadedAndRaise(false);
		foreach (var child in GetChildren(root))
			BroadcastUnloaded(child);
	}

	/// <summary>
	/// Called from the <see cref="UIElement.LogicalParent"/> setter: fires the lazy
	/// <see cref="FrameworkElement.Initialized"/> on attach and schedules the deferred subtree
	/// Loaded/Unloaded broadcast when the element enters or leaves a loaded tree.
	/// </summary>
	internal static void OnLogicalParentChanged(UIElement element, UIElement? newParent)
	{
		if (newParent != null && element is FrameworkElement fe)
			fe.TryFireInitialized();

		bool parentLoaded = IsInLoadedTree(newParent);
		bool elementLoaded = IsSubtreeLoaded(element);
		if (parentLoaded == elementLoaded)
			return;

		var dispatcher = Dispatcher.CurrentDispatcher;
		if (parentLoaded)
		{
			dispatcher.BeginInvoke(() =>
			{
				if (!IsInLoadedTree(element.LogicalParent))
					return;
				BroadcastLoaded(element);
				// The subtree joined a live window: publish its current GTK allocation now.
				GtkAllocationSync.Sync(element);
			}, DispatcherPriority.Loaded);
		}
		else
		{
			dispatcher.BeginInvoke(() =>
			{
				if (!IsInLoadedTree(element.LogicalParent))
					BroadcastUnloaded(element);
			}, DispatcherPriority.Loaded);
		}
	}

	/// <summary>
	/// <see langword="true"/> when the nearest <see cref="FrameworkElement"/> at or above
	/// <paramref name="node"/> is loaded.
	/// </summary>
	internal static bool IsInLoadedTree(UIElement? node)
	{
		for (var current = node; current != null; current = current.LogicalParent)
		{
			if (current is FrameworkElement fe)
				return fe.IsLoaded;
		}
		return false;
	}

	// The subtree counts as loaded when its first FrameworkElement (pre-order) is loaded.
	static bool IsSubtreeLoaded(UIElement element)
	{
		foreach (var node in EnumeratePreOrder(element))
		{
			if (node is FrameworkElement fe)
				return fe.IsLoaded;
		}
		return false;
	}
}
