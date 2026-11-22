using System.Windows.Documents;

namespace System.Windows.Controls.Primitives;

public abstract partial class DocumentViewerBase
{
	static void RegisterBringIntoView() => FlowDocumentBringIntoView.Register<DocumentViewerBase>((v, e) =>
	{
		var document = v.Document as FlowDocument;
		var target = FlowDocumentBringIntoView.TargetIn(document, e.TargetObject);
		if (target == null || !(v._paginator is DynamicDocumentPaginator paginator)) return;
		int page = paginator.GetPageNumber(paginator.GetObjectPosition(target));
		v.GoToPage(page + 1);
		e.Handled = true;
	});
}
