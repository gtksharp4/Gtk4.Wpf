namespace System.Windows.Controls;

public partial class TabItem
{
	// WPF TabControl.UpdateSelectedContent: the selected tab with no content template, selector or format of its
	// own is shown with the TabControl's.
	TabControl? ContentOwner =>
		OwnerTabControl is { } owner && owner.IsSelectedTab(this)
			&& base.EffectiveContentTemplate == null && base.EffectiveContentTemplateSelector == null
			&& string.IsNullOrEmpty(base.EffectiveContentStringFormat)
			? owner
			: null;

	internal override DataTemplate? EffectiveContentTemplate => ContentOwner?.ContentTemplate ?? ContentTemplate;

	internal override DataTemplateSelector? EffectiveContentTemplateSelector =>
		ContentOwner?.ContentTemplateSelector ?? ContentTemplateSelector;

	internal override string? EffectiveContentStringFormat => ContentOwner?.ContentStringFormat ?? ContentStringFormat;
}
