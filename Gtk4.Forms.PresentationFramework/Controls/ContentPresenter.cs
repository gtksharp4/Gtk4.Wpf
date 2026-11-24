namespace System.Windows.Controls;

/// <summary>
/// Displays a piece of content inside a <see cref="ControlTemplate"/> or other
/// container. Resolution order:
/// <list type="number">
///   <item><see cref="Content"/> is a <see cref="UIElement"/> — hosted directly.</item>
///   <item><see cref="ContentTemplate"/> is set — its visual tree wraps the item.</item>
///   <item>Otherwise <see cref="Content"/> is shown as a string in a <see cref="Gtk.Label"/>.</item>
/// </list>
/// Backed by a single-child <see cref="Gtk.Box"/> container.
/// </summary>
public partial class ContentPresenter : FrameworkElement
{
	// ── Content DP ───────────────────────────────────────────────────────────

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

	/// <summary>
	/// Gets or sets the name of the templated parent property this presenter takes its content from
	/// (WPF default <c>Content</c>). Store-only: the layer does not auto-bind presenters to their templated parent.
	/// </summary>
	public string ContentSource
	{
		get => (string)GetValue(ContentSourceProperty)!;
		set => SetValue(ContentSourceProperty, value);
	}

	/// <summary>Identifies the <see cref="ContentSource"/> dependency property.</summary>
	public static readonly DependencyProperty ContentSourceProperty = DependencyProperty.Register(
		"ContentSource", typeof(string), typeof(ContentPresenter), new FrameworkPropertyMetadata("Content"));

	// ── ContentTemplate DP ───────────────────────────────────────────────────

	/// <summary>The template used to present a non-UIElement content item.</summary>
	public static readonly DependencyProperty ContentTemplateProperty =
		DependencyProperty.Register(
			nameof(ContentTemplate),
			typeof(DataTemplate),
			typeof(ContentPresenter),
			new PropertyMetadata(null, OnContentTemplateChanged));

	static void OnContentTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		((ContentPresenter)d).RefreshContent();
	}

	/// <summary>Gets or sets the content template.</summary>
	public DataTemplate? ContentTemplate
	{
		get => (DataTemplate?)GetValue(ContentTemplateProperty);
		set => SetValue(ContentTemplateProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	Gtk.Box? _box;
	// Tracks the widget currently hosted in _box so we can swap it.
	Gtk.Widget? _hostedWidget;

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

	// Tracks any UIElement we have parented so we can clear it on swap.
	UIElement? _hostedElement;

	void ApplyContent()
	{
		// Remove previous hosted widget and clear logical parent.
		if (_hostedWidget != null)
		{
			_box!.Remove(_hostedWidget);
			_hostedWidget = null;
		}
		if (_hostedElement != null)
		{
			_hostedElement.LogicalParent = null;
			_hostedElement = null;
		}

		_appliedTemplate = null;
		var content = Content;

		// 1. Direct UIElement.
		if (content is UIElement element)
		{
			// D1: wire logical parent so TryFindResource walks the tree.
			element.LogicalParent = this;
			_hostedElement = element;
			_hostedWidget = element.Widget;
			_box!.Add(_hostedWidget);
			return;
		}

		// 2. DataTemplate.
		if (ContentTemplate != null && ContentTemplate.HasContent)
		{
			var visual = ContentTemplate.CreateVisualTreeForItem(content);
			if (visual != null)
			{
				// D1: wire logical parent.
				visual.LogicalParent = this;
				_appliedTemplate = ContentTemplate;
				StampTemplatedParentTree(visual, this);
				_hostedElement = visual;
				_hostedWidget = visual.Widget;
				_box!.Add(_hostedWidget);
				return;
			}
		}

		// 3. String / ToString fallback. A string with '_' hosts an AccessText when RecognizesAccessKey is set
		// (WPF SelectTemplateForString); otherwise underscores stay literal.
		if (RecognizesAccessKey && content is string accessText && accessText.IndexOf('_') >= 0)
		{
			var access = new AccessText { Text = accessText, LogicalParent = this };
			_hostedElement = access;
			_hostedWidget = access.Widget;
			_box!.Add(_hostedWidget);
			return;
		}
		var text = content?.ToString() ?? string.Empty;
		var label = new Gtk.Label(string.Empty);
		label.Text = text;
		_hostedWidget = label;
		_box!.Add(_hostedWidget);
	}
}
