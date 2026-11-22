using System;
using System.Collections.Generic;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	// ── Live resource-reference tracking ────────────────────────────────────────
	//
	// Each entry is (dp, key, subscribed-dicts). The list is null until the first
	// SetResourceReference call, keeping the common case allocation-free.

	record struct ResRef(DependencyProperty Dp, object Key, object? Contributor = null);

	List<ResRef>? _resRefs;

	// Dicts we are currently subscribed to for change notifications.
	// Shared across all references on this element (all share the same resource chain).
	List<ResourceDictionary>? _subscribedDicts;

	// Cached delegate for OnAnyDictChanged.  One instance per element, created on first
	// SetResourceReference call and held here (strong reference on the element itself).
	// ResourceDictionary stores it weakly; because the element holds it strongly, the
	// delegate survives exactly as long as the element does — which is the desired
	// lifetime.  When the element is collected the weak reference in the dict goes dead
	// and the entry is pruned on the next raise or subscribe.
	Action<object?, object?>? _onAnyDictChangedDelegate;

	/// <summary>
	/// Establishes a live link between <paramref name="dp"/> and the resource
	/// identified by <paramref name="resourceKey"/>.  The current value is
	/// resolved immediately; whenever the effective resource for that key changes in
	/// any dictionary in this element's resource chain, the DP is updated.
	/// A subsequent call with the same <paramref name="dp"/> replaces the previous
	/// reference.  Call <see cref="ClearResourceReference"/> to detach.
	/// </summary>
	/// <remarks>
	/// Subscriptions are held as <em>weak</em> references by each dictionary, so a
	/// collected element is not kept alive by a long-lived dictionary such as
	/// <see cref="Application.Current"/>-level resources.
	/// <see cref="ClearResourceReference"/> still provides an immediate deterministic
	/// detach when you no longer need the live link.
	/// </remarks>
	/// <param name="dp">The dependency property to drive.</param>
	/// <param name="resourceKey">The resource key to track.</param>
	public void SetResourceReference(DependencyProperty dp, object resourceKey)
	{
		if (dp == null) throw new ArgumentNullException(nameof(dp));
		if (resourceKey == null) throw new ArgumentNullException(nameof(resourceKey));

		// WPF writes at Local precedence: a XAML-attribute reference on this dp is replaced in one change.
		if (HasLocalResourceRef(dp))
		{
			SetLocalResourceReference(dp, resourceKey);
			return;
		}

		// Replace an existing reference for this dp, or add a new one.
		_resRefs ??= new List<ResRef>();
		bool replaced = false;
		for (int i = 0; i < _resRefs.Count; i++)
		{
			if (ReferenceEquals(_resRefs[i].Dp, dp) && _resRefs[i].Contributor == null)
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
		ApplyResourceRef(dp, resourceKey, null);
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
			if (ReferenceEquals(_resRefs[i].Dp, dp) && _resRefs[i].Contributor == null)
			{
				_resRefs.RemoveAt(i);
				break;
			}
		}

		if (HasLocalResourceRef(dp)) ClearValue(dp);
		ClearDynResValue(dp);

		if (_resRefs.Count == 0)
			UnsubscribeFromResourceChain();
	}

	// ── Internal helpers ─────────────────────────────────────────────────────────

	void ApplyResourceRef(DependencyProperty dp, object key, object? contributor)
	{
		if (ReferenceEquals(contributor, StyleLayerContributor))
		{
			if (TryFindResourceCore(key, out var sv)) SetStyleValue(dp, sv);
			else ClearStyleValue(dp);
			return;
		}
		if (ReferenceEquals(contributor, LocalLayerContributor))
		{
			ApplyLocalResourceRef(dp, key);
			return;
		}
		if (contributor != null)
		{
			if (TryFindResourceCore(key, out var tv)) SetTriggerValue(dp, contributor, tv);
			else ClearTriggerValue(dp, contributor);
			return;
		}
		if (TryFindResourceCore(key, out var value))
			SetDynResValue(dp, value);
		else
			ClearDynResValue(dp);  // key absent — revert to next layer
	}

	void SubscribeToResourceChain()
	{
		// Lazily create the cached delegate so it is allocated once per element.
		_onAnyDictChangedDelegate ??= OnAnyDictChanged;

		// Collect all ResourceDictionaries reachable from this element's resource chain.
		var chain = CollectResourceChain();

		// Unsubscribe from dicts no longer in the chain.
		if (_subscribedDicts != null)
		{
			for (int i = _subscribedDicts.Count - 1; i >= 0; i--)
			{
				var dict = _subscribedDicts[i];
				if (chain.Contains(dict)) continue;
				dict.ResourceChanged -= _onAnyDictChangedDelegate;
				_subscribedDicts.RemoveAt(i);
			}
		}

		_subscribedDicts ??= new List<ResourceDictionary>();

		// Subscribe to new dicts.
		foreach (var dict in chain)
		{
			if (!_subscribedDicts.Contains(dict))
			{
				_subscribedDicts.Add(dict);
				dict.ResourceChanged += _onAnyDictChangedDelegate;
			}
		}
	}

	void UnsubscribeFromResourceChain()
	{
		if (_subscribedDicts == null) return;
		if (_onAnyDictChangedDelegate != null)
		{
			foreach (var dict in _subscribedDicts)
				dict.ResourceChanged -= _onAnyDictChangedDelegate;
		}
		_subscribedDicts.Clear();
	}
}
