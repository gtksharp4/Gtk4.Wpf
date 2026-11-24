namespace System.Windows.Controls;

public abstract partial class DataGridColumn
{
	/// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
		nameof(HeaderTemplate), typeof(DataTemplate), typeof(DataGridColumn), new PropertyMetadata(null));

	/// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.Register(
		nameof(HeaderTemplateSelector), typeof(DataTemplateSelector), typeof(DataGridColumn),
		new PropertyMetadata(null));

	/// <summary>Identifies the <see cref="HeaderStringFormat"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderStringFormatProperty = DependencyProperty.Register(
		nameof(HeaderStringFormat), typeof(string), typeof(DataGridColumn), new PropertyMetadata(null));

	/// <summary>Gets or sets the template of the column header.</summary>
	public DataTemplate? HeaderTemplate
	{
		get => (DataTemplate?)GetValue(HeaderTemplateProperty);
		set => SetValue(HeaderTemplateProperty, value);
	}

	/// <summary>Gets or sets the selector consulted when <see cref="HeaderTemplate"/> is null.</summary>
	public DataTemplateSelector? HeaderTemplateSelector
	{
		get => (DataTemplateSelector?)GetValue(HeaderTemplateSelectorProperty);
		set => SetValue(HeaderTemplateSelectorProperty, value);
	}

	/// <summary>Gets or sets the format applied to a header shown as text.</summary>
	public string? HeaderStringFormat
	{
		get => (string?)GetValue(HeaderStringFormatProperty);
		set => SetValue(HeaderStringFormatProperty, value);
	}
}
