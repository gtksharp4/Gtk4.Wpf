using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.Windows.Data;

/// <summary>
/// Context listeners indexed per target and per ElementName, so a tree or name-scope change touches only the
/// listeners it can affect instead of every registered one.
/// </summary>
internal static partial class BindingEngine
{
	static readonly object _gate = new object();

	static readonly ConditionalWeakTable<DependencyObject, List<IContextListener>> _byTarget =
		new ConditionalWeakTable<DependencyObject, List<IContextListener>>();

	static readonly Dictionary<string, List<WeakReference<IContextListener>>> _byName =
		new Dictionary<string, List<WeakReference<IContextListener>>>();

	// Registrations minus unregistrations; a listener whose target was collected without Dispose keeps it above 0.
	static int _listenerCount;

	internal static void RegisterContextListener(IContextListener listener)
	{
		if (listener.Target is not { } target) return;
		lock (_gate)
		{
			_byTarget.GetOrCreateValue(target).Add(listener);
			if (listener.ElementName is { } name)
			{
				if (!_byName.TryGetValue(name, out var named))
					_byName[name] = named = new List<WeakReference<IContextListener>>();
				// Prune dead registrations at each doubling so rebuilt trees cannot grow the list without bound.
				if (named.Count >= 16 && (named.Count & (named.Count - 1)) == 0)
					named.RemoveAll(w => !w.TryGetTarget(out _));
				named.Add(new WeakReference<IContextListener>(listener));
			}
			_listenerCount++;
		}
	}

	internal static void UnregisterContextListener(IContextListener listener)
	{
		lock (_gate)
		{
			if (listener.Target is { } target && _byTarget.TryGetValue(target, out var list) && list.Remove(listener))
				_listenerCount--;
			if (listener.ElementName is { } name && _byName.TryGetValue(name, out var named))
			{
				named.RemoveAll(w => !w.TryGetTarget(out var l) || ReferenceEquals(l, listener));
				if (named.Count == 0) _byName.Remove(name);
			}
		}
	}

	/// <summary>The listeners registered on <paramref name="target"/> (a copy: callbacks may re-register).</summary>
	static IContextListener[] ListenersOn(DependencyObject target)
	{
		lock (_gate)
			return _byTarget.TryGetValue(target, out var list) && list.Count > 0
				? list.ToArray()
				: Array.Empty<IContextListener>();
	}

	[ThreadStatic]
	static List<DependencyObject>? _subtree;

	/// <summary>
	/// The listeners of everything at or below <paramref name="root"/> on the context chain. A leaf with no
	/// listener returns without allocating; larger subtrees walk with a pooled list.
	/// </summary>
	static IContextListener[] ListenersUnder(DependencyObject root, bool namedOnly = false, string? name = null)
	{
		if (_listenerCount == 0) return Array.Empty<IContextListener>();
		List<IContextListener>? found = null;
		if (!DependencyObjectInheritance.HasDescendants(root))
		{
			Collect(root, namedOnly, name, ref found);
			return found?.ToArray() ?? Array.Empty<IContextListener>();
		}
		var subtree = _subtree ??= new List<DependencyObject>();
		subtree.Clear();
		DependencyObjectInheritance.CollectSubtree(root, subtree);
		for (int i = 0; i < subtree.Count; i++) Collect(subtree[i], namedOnly, name, ref found);
		subtree.Clear();
		return found?.ToArray() ?? Array.Empty<IContextListener>();
	}

	static void Collect(DependencyObject node, bool namedOnly, string? name, ref List<IContextListener>? found)
	{
		foreach (var listener in ListenersOn(node))
		{
			if (namedOnly && (name == null ? listener.ElementName == null : listener.ElementName != name)) continue;
			(found ??= new List<IContextListener>()).Add(listener);
		}
	}

	/// <summary>The listeners using ElementName <paramref name="name"/>; dead registrations are dropped.</summary>
	static List<IContextListener> ListenersNamed(string name)
	{
		var found = new List<IContextListener>();
		lock (_gate)
		{
			if (!_byName.TryGetValue(name, out var named)) return found;
			named.RemoveAll(w => !w.TryGetTarget(out _));
			if (named.Count == 0) _byName.Remove(name);
			foreach (var weak in named)
				if (weak.TryGetTarget(out var listener)) found.Add(listener);
		}
		return found;
	}

	/// <summary>Diagnostics for tests: ReResolve calls so far, and live plus dead ElementName registrations.</summary>
	internal static int ReResolveCalls;

	internal static int NamedRegistrations(string name)
	{
		lock (_gate) return _byName.TryGetValue(name, out var named) ? named.Count : 0;
	}

	internal static IContextListener[] ListenersUnderForTest(DependencyObject root) => ListenersUnder(root);
}
