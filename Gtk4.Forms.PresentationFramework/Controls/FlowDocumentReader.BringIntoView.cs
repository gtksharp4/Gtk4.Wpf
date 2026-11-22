namespace System.Windows.Controls;

public partial class FlowDocumentReader
{
	static FlowDocumentReader() => FlowDocumentBringIntoView.Register<FlowDocumentReader>((r, e) =>
	{
		var target = FlowDocumentBringIntoView.TargetIn(r.Document, e.TargetObject);
		if (target == null) return;
		FlowDocumentBringIntoView.ScrollTo(r._presenter?.Window, r.Document!, target);
		e.Handled = true;
	});
}
