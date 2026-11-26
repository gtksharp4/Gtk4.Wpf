using System.Windows.Documents;

namespace System.Windows.Controls;

/// <summary>
/// RequestBringIntoView handling shared by the flow-document viewers. The target is "in" a viewer when it is an
/// element of the viewer's document. The scroll position is the target's share of the document text, the same
/// approximation the page viewer uses to place a page.
/// </summary>
static class FlowDocumentBringIntoView
{
	internal static TextElement? TargetIn(FlowDocument? document, DependencyObject target) =>
		document != null && target is TextElement te && ReferenceEquals(te.OwningDocument, document) ? te : null;

	internal static void ScrollTo(Gtk.ScrolledWindow? window, FlowDocument document, TextElement target)
	{
		if (window == null) return;
		TextTree.PlainChars(document, target, out int before, out int total);
		var adjustment = window.Vadjustment;
		double range = adjustment.Upper - adjustment.PageSize;
		if (total > 0 && range > 0) adjustment.Value = range * before / total;
	}

	internal static void Register<T>(Action<T, RequestBringIntoViewEventArgs> handle)
		where T : FrameworkElement =>
		EventManager.RegisterClassHandler(typeof(T), FrameworkElement.RequestBringIntoViewEvent,
			new RequestBringIntoViewEventHandler((s, e) =>
			{
				if (!e.Handled) handle((T)s, e);
			}));
}
