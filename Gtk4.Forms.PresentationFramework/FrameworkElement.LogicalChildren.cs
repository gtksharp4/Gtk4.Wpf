using System.Collections;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	/// <summary>
	/// Enumerates this element's logical children; override to expose a custom logical tree.
	/// The default is <c>null</c> (no children), as in WPF; consumers treat null as empty.
	/// </summary>
	protected internal virtual IEnumerator? LogicalChildren => null;

	/// <summary>Makes <paramref name="child"/> a logical child of this element.</summary>
	/// <exception cref="InvalidOperationException">The child already has another logical parent.</exception>
	protected internal void AddLogicalChild(object child)
	{
		if (child == null) return;
		if (child is not UIElement element) return;
		if (element.LogicalParent != null && !ReferenceEquals(element.LogicalParent, this))
			throw new InvalidOperationException("Specified element is already the logical child of another element.");
		element.LogicalParent = this;
	}

	/// <summary>Removes <paramref name="child"/> from the logical children, clearing its logical parent.</summary>
	protected internal void RemoveLogicalChild(object child)
	{
		if (child is UIElement element && ReferenceEquals(element.LogicalParent, this))
			element.LogicalParent = null;
	}
}
