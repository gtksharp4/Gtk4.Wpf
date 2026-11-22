using System.Collections;

namespace System.Windows;

public abstract partial class FrameworkElement
{
	static readonly IEnumerator _noLogicalChildren = Array.Empty<object>().GetEnumerator();

	/// <summary>
	/// Enumerates this element's logical children; override to expose a custom logical tree.
	/// The default is empty.
	/// </summary>
	protected internal virtual IEnumerator LogicalChildren => _noLogicalChildren;

	/// <summary>Makes <paramref name="child"/> a logical child of this element.</summary>
	/// <exception cref="ArgumentNullException"/>
	/// <exception cref="InvalidOperationException">The child already has another logical parent.</exception>
	protected internal void AddLogicalChild(object child)
	{
		if (child == null) throw new ArgumentNullException(nameof(child));
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
