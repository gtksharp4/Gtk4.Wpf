using System.Collections.Generic;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	// Live {DynamicResource} links owned by a trigger contributor: they write the trigger layer
	// (keyed by the contributor) instead of the dynamic-resource layer, and share the dictionary
	// subscriptions of SetResourceReference.

	internal void SetLayerResourceReference(DependencyProperty dp, object key, object contributor)
	{
		_resRefs ??= new List<ResRef>();
		_resRefs.RemoveAll(r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, contributor));
		_resRefs.Add(new ResRef(dp, key, contributor));
		SubscribeToResourceChain();
		ApplyResourceRef(dp, key, contributor);
	}

	// Contributor for style-setter {DynamicResource} links: they write the style layer, so they rank
	// below triggers and BasedOn-derived values and never touch a user's own resource reference.
	internal static readonly object StyleLayerContributor = new object();

	internal void SetStyleResourceReference(DependencyProperty dp, object key)
	{
		_resRefs ??= new List<ResRef>();
		_resRefs.RemoveAll(r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, StyleLayerContributor));
		_resRefs.Add(new ResRef(dp, key, StyleLayerContributor));
		SubscribeToResourceChain();
		ApplyResourceRef(dp, key, StyleLayerContributor);
	}

	// Removes the link only; the style value is cleared by the style teardown.
	internal void DropStyleResourceReference(DependencyProperty dp)
	{
		if (_resRefs == null) return;
		_resRefs.RemoveAll(r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, StyleLayerContributor));
		if (_resRefs.Count == 0)
			UnsubscribeFromResourceChain();
	}

	internal void ClearLayerResourceReference(DependencyProperty dp, object contributor)
	{
		if (_resRefs == null) return;
		_resRefs.RemoveAll(r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, contributor));
		ClearTriggerValue(dp, contributor);
		if (_resRefs.Count == 0)
			UnsubscribeFromResourceChain();
	}
}
