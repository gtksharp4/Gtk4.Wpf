namespace System.Windows;

public abstract partial class FrameworkElement
{
	/// <summary>
	/// The logical parent. <c>null</c> for a root and for template parts, whose parent is the
	/// templated control, not a logical parent.
	/// </summary>
	public DependencyObject? Parent
	{
		get
		{
			DependencyObject? parent = LogicalParent;
			if (parent is FrameworkElement { ParentOfChildren: { } stand }) parent = stand;
			return parent != null && ReferenceEquals(parent, TemplatedParent) ? null : parent;
		}
	}

	/// <summary>The parent reported for this element's children when it is an internal host.</summary>
	internal virtual DependencyObject? ParentOfChildren => null;
}
