using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace System.Windows.Controls;

/// <summary>
/// Shows one page of a <see cref="FlowDocument"/> at a time, with page navigation. The page is drawn by
/// <see cref="FlowDocumentPageSurface"/> from the block-aware <see cref="FlowDocumentPageMeter"/> (columns,
/// ColumnRule, PagePadding, wrapped text, WPF paragraph margins), and the paginator counts pages with the same
/// meter. The page is the viewer's content area wherever PageWidth/PageHeight are NaN, so resizing repaginates.
/// Gaps: Zoom, selection brush and opacity are stored, not applied; KeepTogether, KeepWithNext, widows and orphans
/// are stored only (see <see cref="FlowDocumentPageMeter"/>); tables, lists and floaters draw as one text block.
/// </summary>
public partial class FlowDocumentPageViewer : DocumentViewerBase
{
	FlowDocumentPageSurface? _surface;
	FlowDocument? _shown;
	Gtk.Label? _status;
	Gtk.Button? _first;
	Gtk.Button? _previous;
	Gtk.Button? _next;
	Gtk.Button? _last;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_surface = new FlowDocumentPageSurface(OnViewport);
		var root = new Gtk.Box(Gtk.Orientation.Vertical, 0);
		root.Append(_surface.Area);
		root.Append(CreateBar());
		SyncView();
		return root;
	}

	/// <summary>The drawing area that paints the page; for tests.</summary>
	internal Gtk.Widget? PageWidget => _surface?.Area;

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

	internal override void OnPagesRefreshed()
	{
		SyncStatus();
		_surface?.Redraw();
	}

	void OnViewport(Size size)
	{
		if (_shown != null && ((IDocumentPaginatorSource)_shown).DocumentPaginator is FlowDocumentPaginator p)
			p.Viewport = size;
	}

	void SyncView()
	{
		if (_surface == null) return;
		var doc = Document as FlowDocument;
		if (!ReferenceEquals(doc, _shown))
		{
			if (_shown != null)
			{
				if (((IDocumentPaginatorSource)_shown).DocumentPaginator is FlowDocumentPaginator old)
					old.Viewport = null;
				_shown.ViewChanged -= OnDocumentViewChanged;
				_shown.ContentChanged -= OnDocumentViewChanged;
			}
			_shown = doc;
			if (doc != null)
			{
				doc.ViewChanged += OnDocumentViewChanged;
				doc.ContentChanged += OnDocumentViewChanged;
				if (_surface.Viewport is Size v
					&& ((IDocumentPaginatorSource)doc).DocumentPaginator is FlowDocumentPaginator p)
					p.Viewport = v;
			}
		}
		_surface.Document = doc;
		_surface.PageIndex = Math.Max(0, MasterPageNumber - 1);
		_surface.Redraw();
		SyncStatus();
	}

	void OnDocumentViewChanged() => _surface?.Redraw();

	void SyncStatus()
	{
		if (_surface == null) return;
		_status!.Text = PageCount == 0 ? string.Empty : $"{MasterPageNumber} / {PageCount}";
		_first!.Sensitive = _previous!.Sensitive = CanGoToPreviousPage;
		_next!.Sensitive = _last!.Sensitive = CanGoToNextPage;
	}
}
