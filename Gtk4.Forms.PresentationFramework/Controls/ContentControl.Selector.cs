namespace System.Windows.Controls;

public partial class ContentControl
{
	/// <summary>Identifies the <see cref="ContentTemplateSelector"/> dependency property.</summary>
	public static readonly DependencyProperty ContentTemplateSelectorProperty = DependencyProperty.Register(
		nameof(ContentTemplateSelector), typeof(DataTemplateSelector), typeof(ContentControl),
		new FrameworkPropertyMetadata(null, (d, _) => ((ContentControl)d).RefreshPresentation()));

	/// <summary>Identifies the <see cref="ContentStringFormat"/> dependency property.</summary>
	public static readonly DependencyProperty ContentStringFormatProperty = DependencyProperty.Register(
		nameof(ContentStringFormat), typeof(string), typeof(ContentControl),
		new FrameworkPropertyMetadata(null, (d, _) => ((ContentControl)d).RefreshPresentation()));

	/// <summary>Gets or sets the selector consulted when <see cref="ContentTemplate"/> is null.</summary>
	public DataTemplateSelector? ContentTemplateSelector
	{
		get => (DataTemplateSelector?)GetValue(ContentTemplateSelectorProperty);
		set => SetValue(ContentTemplateSelectorProperty, value);
	}

	/// <summary>Gets or sets the format applied to content shown as text.</summary>
	public string? ContentStringFormat
	{
		get => (string?)GetValue(ContentStringFormatProperty);
		set => SetValue(ContentStringFormatProperty, value);
	}
}
