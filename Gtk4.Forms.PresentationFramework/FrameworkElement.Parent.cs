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
			var parent = LogicalParent;
			return parent != null && ReferenceEquals(parent, TemplatedParent) ? null : parent;
		}
	}
}
