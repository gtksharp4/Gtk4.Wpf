namespace System.Windows.Controls;

/// <summary>
/// Displays a piece of content inside a <see cref="ControlTemplate"/> or other container. The content is
/// resolved through <see cref="ContentResolution"/>: a UIElement, the template, the selector, an implicit
/// DataTemplate, an Inline, then text. Backed by a single-child <see cref="Gtk.Box"/>.
/// </summary>
public partial class ContentPresenter : FrameworkElement
{
	/// <summary>The content to present.</summary>
	public static readonly DependencyProperty ContentProperty =
		DependencyProperty.Register(
			nameof(Content),
			typeof(object),
			typeof(ContentPresenter),
			new PropertyMetadata(null, OnContentChanged));

	static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentPresenter)d).RefreshContent();
	}

	/// <summary>Gets or sets the content.</summary>
	public object? Content
	{
		get => GetValue(ContentProperty);
		set => SetValue(ContentProperty, value);
	}

	/// <summary>The template used to present a non-UIElement content item.</summary>
	public static readonly DependencyProperty ContentTemplateProperty =
		DependencyProperty.Register(
			nameof(ContentTemplate),
			typeof(DataTemplate),
			typeof(ContentPresenter),
			new PropertyMetadata(null, OnContentChanged));

	/// <summary>Gets or sets the content template.</summary>
	public DataTemplate? ContentTemplate
	{
		get => (DataTemplate?)GetValue(ContentTemplateProperty);
		set => SetValue(ContentTemplateProperty, value);
	}

	Gtk.Box? _box;
	// The widget currently hosted in _box, so it can be swapped.
	Gtk.Widget? _hostedWidget;
	UIElement? _hostedElement;
	object? _hostedContent;

	internal override bool IsWidgetCreated => _box != null;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		ApplyContent();
		return _box;
	}

	void RefreshContent()
	{
		if (!IsWidgetCreated) return;
		ApplyContent();
	}

	void ApplyContent()
	{
		EnsureSourceAliases();
		if (_hostedWidget != null)
		{
			_box!.Remove(_hostedWidget);
			_hostedWidget = null;
		}
		ContentResolution.Release(_hostedElement, _hostedContent);
		_hostedElement = null;
		_hostedContent = null;
		_appliedTemplate = null;

		var content = Content;
		var element = ContentResolution.ResolveStructured(
			this, content, ContentTemplate, ContentTemplateSelector, out var applied);
		if (element != null)
		{
			_appliedTemplate = applied;
			_hostedElement = element;
			_hostedContent = content;
			_hostedWidget = element.Widget;
			_box!.Add(_hostedWidget);
			return;
		}

		// A string with '_' hosts an AccessText when RecognizesAccessKey is set (WPF SelectTemplateForString).
		if (RecognizesAccessKey && content is string accessText && accessText.IndexOf('_') >= 0
			&& string.IsNullOrEmpty(ContentStringFormat))
		{
			var access = new AccessText { Text = accessText, LogicalParent = this };
			_hostedElement = access;
			_hostedWidget = access.Widget;
			_box!.Add(_hostedWidget);
			return;
		}
		var label = new Gtk.Label(string.Empty);
		label.Text = ContentResolution.ToText(content, ContentStringFormat);
		_hostedWidget = label;
		_box!.Add(_hostedWidget);
	}
}
