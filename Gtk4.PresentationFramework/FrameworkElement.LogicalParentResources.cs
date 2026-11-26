namespace System.Windows;

public abstract partial class FrameworkElement
{
	// Attaching or detaching changes the resource chain: re-resolve the subtree's references against it.
	internal override void OnLogicalParentChangedResources() => RefreshResourceReferencesInSubtree();
}
