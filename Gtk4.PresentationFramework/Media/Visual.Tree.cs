namespace System.Windows.Media;

public abstract partial class Visual
{
	/// <summary>Adds <paramref name="child"/> as a visual child; sets its parent.</summary>
	/// <exception cref="ArgumentException">The child already has a visual parent.</exception>
	protected void AddVisualChild(Visual child)
	{
		if (child == null) return;
		AdoptVisualChild(child);
	}

	/// <summary>Removes <paramref name="child"/> from the visual children; clears its parent.</summary>
	protected void RemoveVisualChild(Visual child)
	{
		if (child == null || !ReferenceEquals(child.VisualParent, this)) return;
		ReleaseVisualChild(child);
	}

	/// <summary>Called after the visual parent changed (from Add/RemoveVisualChild).</summary>
	protected internal virtual void OnVisualParentChanged(DependencyObject? oldParent) { }

	/// <summary>Called after a visual child was added or removed.</summary>
	protected internal virtual void OnVisualChildrenChanged(
		DependencyObject? visualAdded, DependencyObject? visualRemoved) { }

	internal void AdoptVisualChild(Visual child)
	{
		if (child.VisualParent != null && !SharesParentLink(child))
			throw new ArgumentException(
				"Specified Visual is already a child of another Visual or the root of a CompositionTarget.");
		child.VisualParent = this;
		OnVisualChildAdopted(child);
		OnVisualChildrenChanged(child, null);
		child.OnVisualParentChanged(null);
	}

	internal void ReleaseVisualChild(Visual child)
	{
		child.VisualParent = null;
		OnVisualChildReleased(child);
		OnVisualChildrenChanged(null, child);
		child.OnVisualParentChanged(this);
	}

	// A UIElement's logical parent doubles as its visual parent, so AddLogicalChild may precede AddVisualChild.
	bool SharesParentLink(Visual child) => ReferenceEquals(child.VisualParent, this) && child is UIElement;

	/// <summary>Hook for hosts to place a UIElement child's widget.</summary>
	internal virtual void OnVisualChildAdopted(Visual child) { }

	internal virtual void OnVisualChildReleased(Visual child) { }

	/// <summary>Whether this visual is an ancestor of <paramref name="descendant"/> (a visual is its own).</summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="ArgumentException">The argument is not a <see cref="Visual"/>.</exception>
	public bool IsAncestorOf(DependencyObject descendant)
	{
		if (descendant == null) throw new ArgumentNullException(nameof(descendant));
		if (!(descendant is Visual v)) throw new ArgumentException("The type is not a Visual.", nameof(descendant));
		return v.IsDescendantOf(this);
	}

	/// <summary>Whether this visual is a descendant of <paramref name="ancestor"/> (a visual is its own).</summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="ArgumentException">The argument is not a <see cref="Visual"/>.</exception>
	public bool IsDescendantOf(DependencyObject ancestor)
	{
		if (ancestor == null) throw new ArgumentNullException(nameof(ancestor));
		if (!(ancestor is Visual)) throw new ArgumentException("The type is not a Visual.", nameof(ancestor));
		for (Visual? v = this; v != null; v = v.VisualParent)
			if (ReferenceEquals(v, ancestor)) return true;
		return false;
	}

	/// <summary>The nearest visual that is an ancestor of both (or one of them); <c>null</c> if none.</summary>
	/// <exception cref="ArgumentNullException"/>
	public DependencyObject? FindCommonVisualAncestor(DependencyObject otherVisual)
	{
		if (otherVisual == null) throw new ArgumentNullException(nameof(otherVisual));
		if (otherVisual is not Visual other) return null;
		for (Visual? a = this; a != null; a = a.VisualParent)
			for (Visual? b = other; b != null; b = b.VisualParent)
				if (ReferenceEquals(a, b)) return a;
		return null;
	}
}
