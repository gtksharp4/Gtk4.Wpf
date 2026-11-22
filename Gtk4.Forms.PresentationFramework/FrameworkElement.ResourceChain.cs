using System.Collections.Generic;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	/// <summary>
	/// Collects all <see cref="ResourceDictionary"/> instances reachable from this
	/// element's resource chain: own Resources plus every ancestor's Resources, then
	/// Application.Current.Resources — each including its merged dictionaries
	/// (recursively), so a key resolved from a merged dictionary also live-updates.
	/// </summary>
	List<ResourceDictionary> CollectResourceChain()
	{
		var result = new List<ResourceDictionary>();
		DependencyObject? current = this;
		int steps = 0;
		while (current != null && steps < 1000)
		{
			steps++;
			if (current is FrameworkElement fe)
				AddWithMerged(fe.Resources, result);
			current = (current as UIElement)?.LogicalParent ?? current.InheritanceParent;
		}
		if (Application.Current != null)
			AddWithMerged(Application.Current.Resources, result);
		return result;
	}

	// Adds a dictionary and all of its merged dictionaries (depth-first, de-duplicated),
	// so change subscriptions cover merged entries the resolver (TryGetValue) can see.
	static void AddWithMerged(ResourceDictionary dict, List<ResourceDictionary> result)
	{
		if (result.Contains(dict)) return;
		result.Add(dict);
		foreach (var merged in dict.MergedDictionaries)
			AddWithMerged(merged, result);
	}

	void OnAnyDictChanged(object? key, object? newValue)
	{
		if (_resRefs == null) return;

		// null key == Clear() — invalidate all references.
		bool clearAll = key == null;
		if (clearAll) SubscribeToResourceChain();

		foreach (var r in _resRefs.ToArray())
		{
			if ((clearAll || Equals(r.Key, key)) && _resRefs != null && _resRefs.Contains(r))
				ApplyResourceRef(r.Dp, r.Key, r.Contributor);
		}
	}
}
