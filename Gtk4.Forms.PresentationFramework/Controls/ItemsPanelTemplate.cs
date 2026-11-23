namespace System.Windows.Controls;

/// <summary>
/// Specifies the panel an <see cref="ItemsControl"/> uses to lay out its items.
/// </summary>
/// <remarks>
/// WPF builds the template from a <c>FrameworkElementFactory</c> root; this layer has no such factory, so the
/// root is produced by <see cref="FrameworkTemplate.Factory"/> (or the <see cref="Func{UIElement}"/> constructor)
/// and must return a <see cref="Panel"/>. Unlike WPF the check happens when the panel is created, not on seal.
/// </remarks>
public class ItemsPanelTemplate : FrameworkTemplate
{
	/// <summary>Creates an empty template.</summary>
	public ItemsPanelTemplate()
	{
	}

	/// <summary>Creates a template whose root is produced by <paramref name="factory"/> (must return a Panel).</summary>
	public ItemsPanelTemplate(Func<UIElement> factory) => Factory = factory;

	/// <summary>True once the template is sealed (the default <see cref="ItemsControl.ItemsPanelProperty"/> value).</summary>
	public bool IsSealed { get; private set; }

	internal void Seal() => IsSealed = true;

	/// <summary>The panel type the template targets when one is hosted by an ItemsPresenter (WPF: ItemsPresenter).</summary>
	internal static Type DefaultTargetType => typeof(Panel);

	internal Panel CreatePanel()
	{
		var root = CreateVisualTree();
		if (root is Panel panel)
			return panel;
		throw new InvalidOperationException(
			"ItemsPanelTemplate root must be a Panel, but it is " + (root?.GetType().ToString() ?? "null") + ".");
	}
}
