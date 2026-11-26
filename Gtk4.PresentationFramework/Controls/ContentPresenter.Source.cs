using System.Windows.Data;

namespace System.Windows.Controls;

public partial class ContentPresenter
{
	/// <summary>Identifies the <see cref="ContentSource"/> dependency property.</summary>
	public static readonly DependencyProperty ContentSourceProperty = DependencyProperty.Register(
		"ContentSource", typeof(string), typeof(ContentPresenter), new FrameworkPropertyMetadata("Content"));

	/// <summary>
	/// Gets or sets the name of the templated parent property this presenter takes its content from
	/// (WPF default <c>Content</c>; <c>Header</c> aliases Header, HeaderTemplate and so on). Empty disables it.
	/// </summary>
	public string ContentSource
	{
		get => (string)GetValue(ContentSourceProperty)!;
		set => SetValue(ContentSourceProperty, value);
	}

	bool _aliased;

	// WPF OnPreApplyTemplate: a presenter inside a template follows the templated parent's content properties
	// unless they are set locally.
	void EnsureSourceAliases()
	{
		if (_aliased || !InControlTemplate()) return;
		var source = ContentSource;
		if (string.IsNullOrEmpty(source)) return;
		_aliased = true;
		Alias(ContentProperty, source);
		Alias(ContentTemplateProperty, source + "Template");
		Alias(ContentTemplateSelectorProperty, source + "TemplateSelector");
		Alias(ContentStringFormatProperty, source + "StringFormat");
	}

	// Only a presenter in a ControlTemplate aliases; one a DataTemplate produced (its TemplatedParent is the host
	// that applied the template) must not, or it would present its owner's own content endlessly.
	bool InControlTemplate() =>
		TemplatedParent is Control owner && owner.TemplateRootInternal is { } root && InTemplateTree(root);

	bool InTemplateTree(UIElement node)
	{
		if (ReferenceEquals(node, this)) return true;
		if (node is ContentPresenter) return false;
		foreach (var child in node.VisualChildren)
			if (InTemplateTree(child)) return true;
		return false;
	}

	void Alias(DependencyProperty target, string path)
	{
		if (!ReferenceEquals(ReadLocalValue(target), DependencyProperty.UnsetValue)) return;
		if (DependencyProperty.FindByName(path, TemplatedParent!.GetType()) == null) return;
		SetBinding(target, new Binding(path) { Mode = BindingMode.OneWay, RelativeSource = RelativeSource.TemplatedParent });
	}
}
