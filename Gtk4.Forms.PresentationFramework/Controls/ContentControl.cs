using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// A control that hosts a single piece of content (a UIElement or a string).
/// When <see cref="Content"/> is a <see cref="UIElement"/> its backing widget is parented
/// into this control's GTK container; when it is a string a <see cref="Gtk.Label"/> shows it.
/// </summary>
[ContentProperty("Content")]
public partial class ContentControl : Control
{
	/// <summary>The content of this control.</summary>
	public static readonly DependencyProperty ContentProperty =
		DependencyProperty.Register(
			nameof(Content),
			typeof(object),
			typeof(ContentControl),
			new PropertyMetadata(null, OnContentChanged));

	static void OnContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var cc = (ContentControl)d;
		cc.SetValue(_hasContentKey, e.NewValue != null);
		cc.UpdateContent(e.OldValue, e.NewValue);
	}

	/// <summary>Gets or sets the content.</summary>
	public object? Content
	{
		get => GetValue(ContentProperty);
		set => SetValue(ContentProperty, value);
	}

	void UpdateContent(object? oldContent, object? newContent)
	{
		if (PresentationReady)
		{
			RefreshPresentation();
			return;
		}

		// No widget yet: only the logical parent is kept current.
		if (oldContent is UIElement oldElement)
			oldElement.LogicalParent = null;
		if (newContent is UIElement newElement)
			newElement.LogicalParent = this;
	}

	/// <summary>True when <see cref="Widget"/> has been materialised (GTK objects are live).</summary>
	internal override bool IsWidgetCreated => _box != null;

	// ── GTK backing ─────────────────────────────────────────────────────────

	/// <summary>
	/// The GTK box that hosts the content. Subclasses (e.g. <see cref="Button"/>) may
	/// override <see cref="CreateWidget"/> to return a different widget and override
	/// <see cref="ContentContainer"/> and <see cref="ContentLabel"/> accordingly.
	/// </summary>
	protected Gtk.Box? _box;

	/// <summary>Label used when <see cref="Content"/> is a string.</summary>
	protected Gtk.Label? _label;

	/// <summary>The GTK container that hosts child UIElement widgets.</summary>
	protected virtual Gtk.Box ContentContainer => _box!;

	/// <summary>The GTK label used to display string content.</summary>
	protected virtual Gtk.Label ContentLabel => _label!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_box = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		_label = new Gtk.Label(string.Empty);
		_box.Add(_label);
		RefreshPresentation();
		ApplyTemplateRoot();
		return _box;
	}
}