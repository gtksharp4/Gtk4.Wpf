namespace System.Windows.Controls;

/// <summary>
/// Partial extension of <see cref="ContentControl"/> that adds the <see cref="ContentTemplate"/> dependency
/// property, used by the shared content resolution to present <see cref="ContentControl.Content"/>.
/// </summary>
public partial class ContentControl
{
	/// <summary>The template used to present the content.</summary>
	public static readonly DependencyProperty ContentTemplateProperty =
		DependencyProperty.Register(
			nameof(ContentTemplate),
			typeof(DataTemplate),
			typeof(ContentControl),
			new PropertyMetadata(null, OnContentTemplateChanged));

	static void OnContentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentControl)d).RefreshPresentation();
	}

	/// <summary>Gets or sets the content template.</summary>
	public DataTemplate? ContentTemplate
	{
		get => (DataTemplate?)GetValue(ContentTemplateProperty);
		set => SetValue(ContentTemplateProperty, value);
	}
}
