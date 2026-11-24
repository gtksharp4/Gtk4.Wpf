namespace System.Windows.Controls;

public partial class TabControl
{
	/// <summary>Identifies the <see cref="ContentTemplate"/> dependency property.</summary>
	public static readonly DependencyProperty ContentTemplateProperty = DependencyProperty.Register(
		nameof(ContentTemplate), typeof(DataTemplate), typeof(TabControl), new FrameworkPropertyMetadata(null));

	/// <summary>Identifies the <see cref="ContentTemplateSelector"/> dependency property.</summary>
	public static readonly DependencyProperty ContentTemplateSelectorProperty = DependencyProperty.Register(
		nameof(ContentTemplateSelector), typeof(DataTemplateSelector), typeof(TabControl),
		new FrameworkPropertyMetadata(null));

	/// <summary>Identifies the <see cref="ContentStringFormat"/> dependency property.</summary>
	public static readonly DependencyProperty ContentStringFormatProperty = DependencyProperty.Register(
		nameof(ContentStringFormat), typeof(string), typeof(TabControl), new FrameworkPropertyMetadata(null));

	/// <summary>Gets or sets the content template of generated tabs (<c>ItemTemplate</c> is the header's).</summary>
	public DataTemplate? ContentTemplate
	{
		get => (DataTemplate?)GetValue(ContentTemplateProperty);
		set => SetValue(ContentTemplateProperty, value);
	}

	/// <summary>Gets or sets the selector of the content template of generated tabs.</summary>
	public DataTemplateSelector? ContentTemplateSelector
	{
		get => (DataTemplateSelector?)GetValue(ContentTemplateSelectorProperty);
		set => SetValue(ContentTemplateSelectorProperty, value);
	}

	/// <summary>Gets or sets the string format of the content of generated tabs.</summary>
	public string? ContentStringFormat
	{
		get => (string?)GetValue(ContentStringFormatProperty);
		set => SetValue(ContentStringFormatProperty, value);
	}

	// WPF PrepareHeaderedContentControl: the item is the Header (not for a Visual) under the ItemTemplate rules.
	TabItem CreateTabFor(object? item) => new TabItem
	{
		Header = item is UIElement ? null : item,
		HeaderTemplate = ItemTemplate,
		HeaderTemplateSelector = ItemTemplateSelector,
		Content = item,
		ContentTemplate = ContentTemplate,
		ContentTemplateSelector = ContentTemplateSelector,
		ContentStringFormat = ContentStringFormat,
	};
}
