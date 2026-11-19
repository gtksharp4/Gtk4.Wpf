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

	internal void ClearLayerResourceReference(DependencyProperty dp, object contributor)
	{
		if (_resRefs == null) return;
		_resRefs.RemoveAll(r => ReferenceEquals(r.Dp, dp) && ReferenceEquals(r.Contributor, contributor));
		ClearTriggerValue(dp, contributor);
		if (_resRefs.Count == 0)
			UnsubscribeFromResourceChain();
	}
}
