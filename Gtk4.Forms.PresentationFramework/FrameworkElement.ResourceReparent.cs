namespace System.Windows;

public abstract partial class FrameworkElement
{
	// A {DynamicResource} set on an unparented element resolves against the chain it has then. When the
	// element (or an ancestor of it) joins a tree the chain changes, so every link in the subtree is
	// re-subscribed and re-resolved.
	internal void RefreshResourceReferencesInSubtree()
	{
		if (_resRefs != null)
		{
			SubscribeToResourceChain();
			foreach (var r in _resRefs.ToArray())
				ApplyResourceRef(r.Dp, r.Key, r.Contributor);
		}
		foreach (var child in LifecycleChildren)
			(child as FrameworkElement)?.RefreshResourceReferencesInSubtree();
	}
}
