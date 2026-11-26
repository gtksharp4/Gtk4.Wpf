using System.Linq;

namespace System.Windows.Controls;

public partial class TabControl
{
	/// <summary>Identifies the <see cref="ContentTemplate"/> dependency property.</summary>
	public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register(
		nameof(ContentTemplate), typeof(DataTemplate), typeof(TabControl),
		new FrameworkPropertyMetadata(null, OnContentChanged));

	/// <summary>Identifies the <see cref="ContentTemplateSelector"/> dependency property.</summary>
	public static readonly DependencyProperty ContentTemplateSelectorProperty = DependencyProperty.Register(
		nameof(ContentTemplateSelector), typeof(DataTemplateSelector), typeof(TabControl),
		new FrameworkPropertyMetadata(null, OnContentChanged));

	/// <summary>Identifies the <see cref="ContentStringFormat"/> dependency property.</summary>
	public static readonly DependencyProperty ContentStringFormatProperty = DependencyProperty.Register(
		nameof(ContentStringFormat), typeof(string), typeof(TabControl),
		new FrameworkPropertyMetadata(null, OnContentChanged));

	/// <summary>Gets or sets the content template of the selected tab when it has none of its own.</summary>
	public DataTemplate? ContentTemplate
	{
		get => (DataTemplate?)GetValue(ContentTemplateProperty);
		set => SetValue(ContentTemplateProperty, value);
	}

	/// <summary>Gets or sets the content template selector of the selected tab when it has none of its own.</summary>
	public DataTemplateSelector? ContentTemplateSelector
	{
		get => (DataTemplateSelector?)GetValue(ContentTemplateSelectorProperty);
		set => SetValue(ContentTemplateSelectorProperty, value);
	}

	/// <summary>Gets or sets the content string format of the selected tab when it has none of its own.</summary>
	public string? ContentStringFormat
	{
		get => (string?)GetValue(ContentStringFormatProperty);
		set => SetValue(ContentStringFormatProperty, value);
	}

	static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((TabControl)d).RefreshTabContent();

	bool HasContentTemplating =>
		ContentTemplate != null || ContentTemplateSelector != null || !string.IsNullOrEmpty(ContentStringFormat);

	// The selected tab shows the TabControl's content template, selector and format (see TabItem.OwnerContent).
	internal void RefreshTabContent()
	{
		foreach (var tab in _tabItems.ToArray()) tab.RefreshPresentation();
	}

	internal bool IsSelectedTab(TabItem tab) =>
		ReferenceEquals(SelectedItem, tab)
		|| (SelectedIndex >= 0 && SelectedIndex < _tabItems.Count && ReferenceEquals(_tabItems[SelectedIndex], tab));

	// WPF PrepareHeaderedContentControl: the item is the Header (not for a Visual) under the ItemTemplate rules.
	TabItem CreateTabFor(object? item) => new TabItem
	{
		Header = item is UIElement ? null : item,
		HeaderTemplate = ItemTemplate,
		HeaderTemplateSelector = ItemTemplateSelector,
		Content = item,
	};
}
