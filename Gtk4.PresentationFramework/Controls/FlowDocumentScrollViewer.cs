using System.Windows.Documents;
using System.Windows.Markup;

namespace System.Windows.Controls;

/// <summary>
/// Displays a <see cref="FlowDocument"/> read-only inside a scrollable area.
/// Backed by a <see cref="Gtk.ScrolledWindow"/> containing a <see cref="Gtk.Label"/>
/// (Pango markup rendering).  Editing is not supported.
/// </summary>
[ContentProperty("Document")]
public partial class FlowDocumentScrollViewer : Control
{
	// ── Document DP ──────────────────────────────────────────────────────────

	/// <summary>The flow document to display.</summary>
	public static readonly DependencyProperty DocumentProperty =
		DependencyProperty.Register(
			nameof(Document),
			typeof(FlowDocument),
			typeof(FlowDocumentScrollViewer),
			new PropertyMetadata(null, OnDocumentChanged));

	static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var viewer = (FlowDocumentScrollViewer)d;
		if (e.OldValue is FlowDocument old)
		{
			old.ViewChanged -= viewer.SyncLabel;
			old.SetLogicalParent(null);
		}
		if (e.NewValue is FlowDocument doc)
		{
			if (doc.Parent != null && !ReferenceEquals(doc.Parent, viewer))
				throw new InvalidOperationException("Specified document is already the child of another element.");
			doc.SetLogicalParent(viewer);
			doc.ViewChanged += viewer.SyncLabel;
		}
		viewer.SyncLabel();
	}

	/// <summary>Gets or sets the flow document to display.</summary>
	public FlowDocument? Document
	{
		get => (FlowDocument?)GetValue(DocumentProperty);
		set => SetValue(DocumentProperty, value);
	}

	// ── GTK backing ──────────────────────────────────────────────────────────

	FlowDocumentPresenter? _presenter;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_presenter = new FlowDocumentPresenter();
		_presenter.Label.Selectable = IsSelectionEnabled;
		_presenter.SetScrollBars(HorizontalScrollBarVisibility, VerticalScrollBarVisibility);
		SyncLabel();
		return _presenter.Window;
	}

	void SyncLabel() => _presenter?.Show(Document);
}
