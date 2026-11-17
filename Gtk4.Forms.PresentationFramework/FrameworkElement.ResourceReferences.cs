using System;
using System.Collections.Generic;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	// ── Live resource-reference tracking ────────────────────────────────────────
	//
	// Each entry is (dp, key, subscribed-dicts). The list is null until the first
	// SetResourceReference call, keeping the common case allocation-free.

	record struct ResRef(DependencyProperty Dp, object Key);

	List<ResRef>? _resRefs;

	// Dicts we are currently subscribed to for change notifications.
	// Shared across all references on this element (all share the same resource chain).
	List<ResourceDictionary>? _subscribedDicts;

	/// <summary>
	/// Establishes a live link between <paramref name="dp"/> and the resource
	/// identified by <paramref name="resourceKey"/>.  The current value is
	/// resolved immediately; whenever the effective resource for that key changes in
	/// any dictionary in this element's resource chain, the DP is updated.
	/// A subsequent call with the same <paramref name="dp"/> replaces the previous
	/// reference.  Call <see cref="ClearResourceReference"/> to detach.
	/// </summary>
	/// <remarks>
	/// The change subscription is a strong link held by each dictionary in the chain
	/// (including <see cref="Application"/>-level resources, which outlive the element).
	/// Until a weak-event path is wired, callers must invoke
	/// <see cref="ClearResourceReference"/> when the element is torn down to avoid the
	/// dictionary pinning the element alive.
	/// </remarks>
	/// <param name="dp">The dependency property to drive.</param>
	/// <param name="resourceKey">The resource key to track.</param>
	public void SetResourceReference(DependencyProperty dp, object resourceKey)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));

		// Replace an existing reference for this dp, or add a new one.
		_resRefs ??= new List<ResRef>();
		bool replaced = false;
		for (int i = 0; i < _resRefs.Count; i++)
		{
			if (ReferenceEquals(_resRefs[i].Dp, dp))
			{
				_resRefs[i] = new ResRef(dp, resourceKey);
				replaced = true;
				break;
			}
		}
		if (!replaced)
			_resRefs.Add(new ResRef(dp, resourceKey));

		// Subscribe to all dicts in the current resource chain (idempotent).
		SubscribeToResourceChain();

		// Resolve and push the current value immediately.
		ApplyResourceRef(dp, resourceKey);
	}

	/// <summary>
	/// Detaches the live resource link for <paramref name="dp"/> and clears the
	/// dynamic-resource layer value, reverting to the next lower precedence layer.
	/// No-op if no reference is registered for <paramref name="dp"/>.
	/// </summary>
	/// <param name="dp">The dependency property to detach.</param>
	public void ClearResourceReference(DependencyProperty dp)
	{
		if (dp == null || _resRefs == null) return;

		for (int i = _resRefs.Count - 1; i >= 0; i--)
		{
			if (ReferenceEquals(_resRefs[i].Dp, dp))
			{
				_resRefs.RemoveAt(i);
				break;
			}
		}

		ClearDynResValue(dp);

		if (_resRefs.Count == 0)
			UnsubscribeFromResourceChain();
	}

	// ── Internal helpers ─────────────────────────────────────────────────────────

	void ApplyResourceRef(DependencyProperty dp, object key)
	{
		if (TryFindResourceCore(key, out var value))
			SetDynResValue(dp, value);
		else
			ClearDynResValue(dp);  // key absent — revert to next layer
	}

	void SubscribeToResourceChain()
	{
		// Collect all ResourceDictionaries reachable from this element's resource chain.
		var chain = CollectResourceChain();

		// Unsubscribe from dicts no longer in the chain.
		if (_subscribedDicts != null)
		{
			foreach (var dict in _subscribedDicts)
			{
				if (!chain.Contains(dict))
					dict.ResourceChanged -= OnAnyDictChanged;
			}
		}

		_subscribedDicts ??= new List<ResourceDictionary>();

		// Subscribe to new dicts.
		foreach (var dict in chain)
		{
			if (!_subscribedDicts.Contains(dict))
			{
				_subscribedDicts.Add(dict);
				dict.ResourceChanged += OnAnyDictChanged;
			}
		}
	}

	void UnsubscribeFromResourceChain()
	{
		if (_subscribedDicts == null) return;
		foreach (var dict in _subscribedDicts)
			dict.ResourceChanged -= OnAnyDictChanged;
		_subscribedDicts.Clear();
	}

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

		foreach (var r in _resRefs)
		{
			if (clearAll || Equals(r.Key, key))
				ApplyResourceRef(r.Dp, r.Key);
		}
	}
}
