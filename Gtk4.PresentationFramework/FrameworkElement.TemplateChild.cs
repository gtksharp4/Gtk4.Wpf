namespace System.Windows;

public abstract partial class FrameworkElement
{
	/// <summary>The template this element was built from (<c>Template</c> for a control).</summary>
	internal virtual FrameworkTemplate? TemplateInternal => null;

	/// <summary>The root of the visual tree <see cref="TemplateInternal"/> produced for this element.</summary>
	internal virtual UIElement? TemplateRootInternal => null;

	/// <summary>Returns the element named <paramref name="childName"/> in this element's template, or null.</summary>
	protected internal DependencyObject? GetTemplateChild(string childName) =>
		TemplateInternal?.FindName(childName, this) as DependencyObject;

	/// <summary>Marks the subtree as produced by <paramref name="templatedParent"/>'s template.</summary>
	internal static void StampTemplatedParentTree(UIElement root, FrameworkElement templatedParent)
	{
		if (root is FrameworkElement fe) fe.TemplatedParent = templatedParent;
		foreach (var child in root.VisualChildren)
			StampTemplatedParentTree(child, templatedParent);
	}

	/// <summary>
	/// Finds the element named <paramref name="name"/> among those <paramref name="templatedParent"/> produced.
	/// The walk does not enter elements a nested template produced (their TemplatedParent differs).
	/// </summary>
	internal static FrameworkElement? FindInTemplate(UIElement root, string name, FrameworkElement templatedParent)
	{
		if (root is FrameworkElement fe)
		{
			if (!ReferenceEquals(fe.TemplatedParent, templatedParent)) return null;
			if (fe.Name == name) return fe;
		}
		foreach (var child in root.VisualChildren)
			if (FindInTemplate(child, name, templatedParent) is { } found) return found;
		return null;
	}
}
