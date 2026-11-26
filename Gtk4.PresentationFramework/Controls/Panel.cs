using System.Collections.Generic;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// Base class for layout panels. Owns a <see cref="UIElementCollection"/> and
/// packs each child's backing GTK widget into the panel's GTK container.
/// </summary>
[ContentProperty("Children")]
public abstract partial class Panel : FrameworkElement
{
	/// <summary>The ordered collection of child elements.</summary>
	public UIElementCollection Children { get; }

	protected Panel()
	{
		Children = new UIElementCollection(this);
	}

	// Called by UIElementCollection when a child is added.
	internal void OnChildAdded(UIElement child)
	{
		// Always wire the logical parent — no GTK needed.
		child.LogicalParent = this;

		// Only touch GTK when the panel's backing widget exists.
		if (IsWidgetCreated)
			AddChildWidget(child);
	}

	// Called by UIElementCollection when a child is removed.
	internal void OnChildRemoved(UIElement child)
	{
		child.LogicalParent = null;

		if (IsWidgetCreated)
			RemoveChildWidget(child);
	}

	/// <summary>True when this panel's backing GTK widget has been materialised.</summary>
	internal abstract override bool IsWidgetCreated { get; }

	/// <summary>
	/// Packs <paramref name="child"/>'s backing widget into this panel's GTK container.
	/// Called after the logical parent has been set.
	/// </summary>
	protected abstract void AddChildWidget(UIElement child);

	/// <summary>
	/// Removes <paramref name="child"/>'s backing widget from this panel's GTK container.
	/// </summary>
	protected abstract void RemoveChildWidget(UIElement child);

	/// <inheritdoc/>
	internal override IEnumerable<UIElement> VisualChildren => Children;
}