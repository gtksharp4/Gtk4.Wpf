using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// A named border around a group of related controls, backed by <see cref="Gtk.Frame"/>
/// with a label widget derived from <see cref="HeaderedContentControl.Header"/>.
/// </summary>
[ContentProperty("Content")]
public partial class GroupBox : HeaderedContentControl
{
	// ── GTK backing ─────────────────────────────────────────────────────────

	Gtk.Frame _frame = null!;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_frame = new Gtk.Frame();
		ApplyHeader();
		if (Content is UIElement child)
			_frame.Child = child.Widget;
		return _frame;
	}

	/// <inheritdoc/>
	protected override Gtk.Box ContentContainer =>
		throw new InvalidOperationException("GroupBox manages its own content.");

	/// <inheritdoc/>
	protected override Gtk.Label ContentLabel =>
		throw new InvalidOperationException("GroupBox manages its own content.");

	/// <inheritdoc/>
	protected override void OnHeaderChanged(object? oldHeader, object? newHeader)
	{
		if (_frame != null)
			ApplyHeader();
		else
			AccessKeyBinding.Apply(this, newHeader as string);
	}

	/// <inheritdoc/>
	protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
	{
		base.OnPropertyChanged(e);
		if (e.Property == ContentProperty && _frame != null)
		{
			if (e.OldValue is UIElement old && _frame.Child != null)
				_frame.Child = null;
			if (e.NewValue is UIElement newEl)
				_frame.Child = newEl.Widget;
		}
	}

	Gtk.Label? _headerLabel;

	void ApplyHeader()
	{
		if (Header is UIElement headerEl)
		{
			AccessKeyBinding.Register(this, null);
			_frame.LabelWidget = headerEl.Widget;
			_headerLabel = null;
			return;
		}
		_headerLabel ??= new Gtk.Label(string.Empty);
		if (Header is string text)
			AccessKeyBinding.SetLabel(_headerLabel, AccessKeyBinding.Apply(this, text));
		else
		{
			AccessKeyBinding.Register(this, null);
			_headerLabel.Text = Header?.ToString() ?? string.Empty;
		}
		_frame.LabelWidget = _headerLabel;
	}
}
