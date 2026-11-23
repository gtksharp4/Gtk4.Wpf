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

	/// <summary>The listeners of everything at or below <paramref name="root"/> on the context chain.</summary>
	static List<IContextListener> ListenersUnder(DependencyObject root)
	{
		var found = new List<IContextListener>();
		if (_listenerCount == 0) return found;
		var subtree = new List<DependencyObject>();
		DependencyObjectInheritance.CollectSubtree(root, subtree);
		foreach (var node in subtree) found.AddRange(ListenersOn(node));
		return found;
	}

	/// <summary>The listeners using ElementName <paramref name="name"/>, or all ElementName users when null.</summary>
	static List<IContextListener> ListenersNamed(string? name)
	{
		var found = new List<IContextListener>();
		lock (_gate)
			foreach (var pair in _byName)
			{
				if (name != null && pair.Key != name) continue;
				foreach (var weak in pair.Value)
					if (weak.TryGetTarget(out var listener)) found.Add(listener);
			}
		return found;
	}
}
