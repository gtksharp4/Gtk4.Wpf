namespace System.Windows.Controls;

public partial class HeaderedContentControl
{
	/// <summary>Identifies the <see cref="HeaderTemplate"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
		nameof(HeaderTemplate), typeof(DataTemplate), typeof(HeaderedContentControl),
		new FrameworkPropertyMetadata(null, OnHeaderPresentationChanged));

	/// <summary>Identifies the <see cref="HeaderTemplateSelector"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderTemplateSelectorProperty = DependencyProperty.Register(
		nameof(HeaderTemplateSelector), typeof(DataTemplateSelector), typeof(HeaderedContentControl),
		new FrameworkPropertyMetadata(null, OnHeaderPresentationChanged));

	/// <summary>Identifies the <see cref="HeaderStringFormat"/> dependency property.</summary>
	public static readonly DependencyProperty HeaderStringFormatProperty = DependencyProperty.Register(
		nameof(HeaderStringFormat), typeof(string), typeof(HeaderedContentControl),
		new FrameworkPropertyMetadata(null, OnHeaderPresentationChanged));

	static readonly DependencyPropertyKey _hasHeaderKey = DependencyProperty.RegisterReadOnly(
		nameof(HasHeader), typeof(bool), typeof(HeaderedContentControl), new FrameworkPropertyMetadata(false));

	/// <summary>Identifies the <see cref="HasHeader"/> dependency property.</summary>
	public static readonly DependencyProperty HasHeaderProperty = _hasHeaderKey.DependencyProperty;

	static void OnHeaderPresentationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var c = (HeaderedContentControl)d;
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

	/// <summary>
	/// The widget presenting a header that is not plain text (an element, a template result or an inline),
	/// or null when the header is text.
	/// </summary>
	internal Gtk.Widget? ResolveHeaderWidget()
	{
		ContentResolution.Release(_headerElement, _headerContent);
		_headerElement = ContentResolution.ResolveStructured(
			this, Header, HeaderTemplate, HeaderTemplateSelector, out _);
		_headerContent = Header;
		return _headerElement?.Widget;
	}

	/// <summary>The header as text, with the string format applied.</summary>
	internal string HeaderTextProjection => ContentResolution.ToText(Header, HeaderStringFormat);

	/// <summary>Whether the header is a plain string that recognizes access keys.</summary>
	internal bool HeaderIsAccessText => Header is string && string.IsNullOrEmpty(HeaderStringFormat);

	// Hosts for the single-child GTK containers of the headered controls.
	internal virtual void SetChildWidget(Gtk.Widget? child) { }

	internal override void HostElement(Gtk.Widget widget) => SetChildWidget(widget);

	internal override void UnhostElement(Gtk.Widget widget) => SetChildWidget(null);

	internal override void ShowContentText(string text) => ShowText(text);

	internal override void ShowText(string text) =>
		SetChildWidget(text.Length == 0 ? null : new Gtk.Label(text) { Xalign = 0f });
}
