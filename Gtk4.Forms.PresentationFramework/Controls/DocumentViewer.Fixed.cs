using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class DocumentViewer
{
	// Fixed pages are element trees, not drawings: they are hosted as widgets instead of painted.
	static FixedPage? FixedPageOf(DocumentPaginator paginator, int index) =>
		paginator is FixedDocumentPaginator || paginator is FixedDocumentSequencePaginator
			? paginator.GetPage(index).Visual as FixedPage : null;

	Gtk.Widget HostFixedPage(FixedPage page)
	{
		var widget = page.Widget;
		if (widget.Parent != null)
			widget.Unparent();
		if (!ShowPageBorders) return widget;
		var frame = new Gtk.Frame(null!);
		frame.Child = widget;
		return frame;
	}
}
