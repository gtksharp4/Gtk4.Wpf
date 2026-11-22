namespace System.Windows.Documents;

/// <summary>Logical-parent bookkeeping shared by the flow-content collections, as in WPF.</summary>
static class ContentParenting
{
	/// <exception cref="InvalidOperationException">
	/// <paramref name="child"/> already has a parent and <paramref name="strict"/> is set.
	/// </exception>
	internal static void Adopt(DependencyObject? owner, FrameworkContentElement child, bool strict = true)
	{
		if (owner == null) return;
		if (strict && child.Parent != null)
			throw new InvalidOperationException("Specified element is already the logical child of another element.");
		child.SetLogicalParent(owner);
	}

	/// <summary>
	/// Tells the FlowDocument above <paramref name="owner"/>, if any, that its content changed (after a collection
	/// edit or a layout-affecting property change). Viewers re-render and the paginator repaginates.
	/// </summary>
	internal static void Changed(DependencyObject? owner)
	{
		for (var e = owner; e != null; e = (e as FrameworkContentElement)?.Parent)
		{
			if (!(e is FlowDocument doc)) continue;
			doc.NotifyContentEdited();
			return;
		}
	}

	internal static void Release(DependencyObject? owner, FrameworkContentElement child)
	{
		if (owner != null && ReferenceEquals(child.Parent, owner)) child.SetLogicalParent(null);
	}
}
