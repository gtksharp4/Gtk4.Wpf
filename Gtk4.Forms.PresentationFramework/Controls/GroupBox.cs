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
		RefreshPresentation();
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

	internal override bool PresentationReady => _frame != null;

	internal override void SetChildWidget(Gtk.Widget? child) => _frame.Child = child!;

	Gtk.Label? _headerLabel;

	void ApplyHeader()
	{
		var headerWidget = ResolveHeaderWidget();
		if (headerWidget != null)
		{
			AccessKeyBinding.Register(this, null);
			_frame.LabelWidget = headerWidget;
			_headerLabel = null;
			return;
		}
		_headerLabel ??= new Gtk.Label(string.Empty);
		if (HeaderIsAccessText)
			AccessKeyBinding.SetLabel(_headerLabel, AccessKeyBinding.Apply(this, (string)Header!));
		else
		{
			AccessKeyBinding.Register(this, null);
			_headerLabel.Text = HeaderTextProjection;
		}
		_frame.LabelWidget = _headerLabel;
	}
}
