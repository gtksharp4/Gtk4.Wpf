namespace System.Windows.Controls;

public abstract partial class HeaderedItemsControl
{
	/// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
		nameof(HeaderTemplate), typeof(DataTemplate), typeof(HeaderedItemsControl),
		new FrameworkPropertyMetadata(null, OnHeaderPresentationChanged));

	/// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.Register(
		nameof(HeaderTemplateSelector), typeof(DataTemplateSelector), typeof(HeaderedItemsControl),
		new FrameworkPropertyMetadata(null, OnHeaderPresentationChanged));

	/// <summary>Identifies the <see cref="HeaderStringFormat"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderStringFormatProperty = DependencyProperty.Register(
		nameof(HeaderStringFormat), typeof(string), typeof(HeaderedItemsControl),
		new FrameworkPropertyMetadata(null, OnHeaderPresentationChanged));

	static readonly DependencyPropertyKey _hasHeaderKey = DependencyProperty.RegisterReadOnly(
		nameof(HasHeader), typeof(bool), typeof(HeaderedItemsControl), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the <see cref="HasHeader"/> dependency property.</summary>
	public static readonly DependencyProperty HasHeaderProperty = _hasHeaderKey.DependencyProperty;

	static void OnHeaderPresentationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var c = (HeaderedItemsControl)d;
		c.OnHeaderChanged(c.Header, c.Header);
	}

	/// <summary>Gets or sets the template of the header.</summary>
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

	/// <summary>Gets whether <see cref="Header"/> is non-null.</summary>
	public bool HasHeader => (bool)GetValue(HasHeaderProperty)!;

	UIElement? _headerElement;
	object? _headerContent;

	/// <summary>The widget presenting a header that is not plain text, or null when it is text.</summary>
	internal Gtk.Widget? ResolveHeaderWidget()
	{
		ContentResolution.Release(_headerElement, _headerContent);
		_headerElement = ContentResolution.ResolveStructured(
			this, Header, HeaderTemplate, HeaderTemplateSelector, out _);
		_headerContent = Header;
		return _headerElement?.Widget;
	}
}
