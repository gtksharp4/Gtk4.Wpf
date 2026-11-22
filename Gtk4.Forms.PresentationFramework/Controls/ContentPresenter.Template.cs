namespace System.Windows.Controls;

public partial class ContentPresenter
{
	// The DataTemplate that produced the hosted visual (null when the content is shown without a template).
	DataTemplate? _appliedTemplate;

	internal override FrameworkTemplate? TemplateInternal => _appliedTemplate;

	internal override UIElement? TemplateRootInternal => _appliedTemplate != null ? _hostedElement : null;

	internal override System.Collections.Generic.IEnumerable<UIElement> VisualChildren =>
		_hostedElement != null
			? new[] { _hostedElement }
			: System.Linq.Enumerable.Empty<UIElement>();
}
