using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace System.Windows.Controls;

/// <summary>
/// Shows a <see cref="FlowDocument"/> with page navigation, on the scrolled Pango label that
/// <see cref="FlowDocumentReader"/> uses. Gap: the label is not paginated, so the whole document is one scrolling
/// column and a page number only scrolls it to the matching fraction of the text. Zoom, selection brush and
/// opacity are stored; the label does not apply them.
/// </summary>
public partial class FlowDocumentPageViewer : DocumentViewerBase
{
	FlowDocumentPresenter? _presenter;
	Gtk.Label? _status;
	Gtk.Button? _first;
	Gtk.Button? _previous;
	Gtk.Button? _next;
	Gtk.Button? _last;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_presenter = new FlowDocumentPresenter();
		var root = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		root.Append(_presenter.Window);
		root.Append(CreateBar());
		SyncView();
		return root;
	}

	Gtk.Box CreateBar()
	{
		var bar = new Gtk.Box(Gtk.Orientation.Horizontal, 4) { Halign = Gtk.Align.Center };
		_first = Nav("|<", FirstPage);
		_previous = Nav("<", PreviousPage);
		_status = new Gtk.Label(string.Empty);
		_next = Nav(">", NextPage);
		_last = Nav(">|", LastPage);
		foreach (var w in new Gtk.Widget[] { _first, _previous, _status, _next, _last }) bar.Append(w);
		return bar;
	}

	static Gtk.Button Nav(string label, Action go)
	{
		var button = new Gtk.Button { Label = label };
		button.Clicked += (s, e) => go();
		return button;
	}

	/// <inheritdoc/>
	protected override void OnDocumentChanged() => SyncView();

	/// <inheritdoc/>
	protected override void OnMasterPageNumberChanged() => SyncView();

	void SyncView()
	{
		if (_presenter == null) return;
		_presenter.Show(Document as FlowDocument);
		_status!.Text = PageCount == 0 ? string.Empty : $"{MasterPageNumber} / {PageCount}";
		_first!.Sensitive = _previous!.Sensitive = CanGoToPreviousPage;
		_next!.Sensitive = _last!.Sensitive = CanGoToNextPage;
		ScrollToPage();
	}

	void ScrollToPage()
	{
		var adjustment = _presenter!.Window.Vadjustment;
		double range = adjustment.Upper - adjustment.PageSize;
		if (PageCount > 1 && range > 0) adjustment.Value = range * (MasterPageNumber - 1) / (PageCount - 1);
	}
}
