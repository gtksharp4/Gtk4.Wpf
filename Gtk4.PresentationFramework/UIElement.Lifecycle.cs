using System.Collections.Generic;

namespace System.Windows;

public abstract partial class UIElement
{
	// Every element whose LogicalParent is this one, in attach order. Kept here (rather than
	// derived from each container's child collection) because every container in this layer —
	// Panel, Border, ContentControl, ContentPresenter, ItemsControl, templates, adorners —
	// parents through the single LogicalParent setter, so this is the complete child set the
	// Loaded/Unloaded broadcast must reach.
	List<UIElement>? _lifecycleChildren;

	/// <summary>Direct children tracked through <see cref="LogicalParent"/>, in attach order.</summary>
	internal IReadOnlyList<UIElement> LifecycleChildren =>
		(IReadOnlyList<UIElement>?)_lifecycleChildren ?? System.Array.Empty<UIElement>();

	/// <summary>
	/// <see langword="true"/> once <see cref="Widget"/> has been materialised. Unlike the
	/// overridable <see cref="IsWidgetCreated"/> (which some controls redefine as "my GTK
	/// container exists"), this is the raw backing-widget state.
	/// </summary>
	internal bool HasBackingWidget => _widget != null;

	partial void OnLogicalParentChangedLifecycle(UIElement? oldParent, UIElement? newParent)
	{
		oldParent?._lifecycleChildren?.Remove(this);
		if (newParent != null)
			(newParent._lifecycleChildren ??= new List<UIElement>()).Add(this);
		LifecycleBroadcaster.OnLogicalParentChanged(this, newParent);
	}
}
