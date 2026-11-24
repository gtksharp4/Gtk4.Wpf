namespace System.Windows.Controls;

public partial class ContentPresenter
{
	// The implicit template the last presentation used; a resource scope change only matters if the lookup differs.
	DataTemplate? _implicitSeen;

	DataTemplate? LookUpImplicitTemplate() =>
		Content is null or UIElement || ContentTemplate != null || ContentTemplateSelector != null
			? null
			: FindTemplateResource(this, Content);

	internal override void OnResourceScopeRefreshed()
	{
		base.OnResourceScopeRefreshed();
		if (!IsWidgetCreated || LogicalParent == null) return;
		if (!ReferenceEquals(LookUpImplicitTemplate(), _implicitSeen)) ApplyContent();
	}
}
