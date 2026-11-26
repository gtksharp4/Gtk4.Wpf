using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace System.Windows.Controls;

/// <summary>
/// Shows the pages of a paginator one under another (or <see cref="MaxPagesAcross"/> to a row) in a scrolled GTK
/// box; each page is painted from its drawing visual. Gaps: Zoom is stored and the Fit methods only set it, the
/// pages are not scaled; the find, print, copy and thumbnail commands have no UI.
/// </summary>
public partial class DocumentViewer : DocumentViewerBase
{
	Gtk.ScrolledWindow? _scroller;
	Gtk.Box? _pages;

	/// <inheritdoc/>
	protected override Gtk.Widget CreateWidget()
	{
		_scroller = new Gtk.ScrolledWindow();
		Rebuild();
		return _scroller;
	}

	/// <inheritdoc/>
	protected override void OnDocumentChanged() => Rebuild();

	void Rebuild()
	{
		if (_scroller == null) return;
		_pages = new Gtk.Box(Gtk.Orientation.Vertical, (int)VerticalPageSpacing)
		{
			Halign = Gtk.Align.Center,
			Valign = Gtk.Align.Start
		};
		var paginator = Document?.DocumentPaginator;
		int across = Math.Max(1, MaxPagesAcross);
		Gtk.Box? row = null;
		for (int i = 0; paginator != null && i < PageCount; i++)
		{
			if (i % across == 0)
			{
				row = new Gtk.Box(Gtk.Orientation.Horizontal, (int)HorizontalPageSpacing) { Halign = Gtk.Align.Center };
				_pages.Append(row);
			}
			row!.Append(PageWidget(paginator, i));
		}
		_scroller.Child = _pages;
	}

	Gtk.Widget PageWidget(DocumentPaginator paginator, int index)
	{
		var fixedPage = FixedPageOf(paginator, index);
		if (fixedPage != null)
			return HostFixedPage(fixedPage);
		var view = new DocumentPageView { DocumentPaginator = paginator, PageNumber = index };
		if (!ShowPageBorders) return view.Widget;
		var frame = new Gtk.Frame(null!);
		frame.Child = view.Widget;
		return frame;
	}
}
