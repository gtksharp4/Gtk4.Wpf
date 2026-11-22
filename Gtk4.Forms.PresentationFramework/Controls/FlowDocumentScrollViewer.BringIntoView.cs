namespace System.Windows.Controls;

public partial class FlowDocumentScrollViewer
{
	static FlowDocumentScrollViewer() => FlowDocumentBringIntoView.Register<FlowDocumentScrollViewer>((v, e) =>
	{
		var target = FlowDocumentBringIntoView.TargetIn(v.Document, e.TargetObject);
		if (target == null) return;
		FlowDocumentBringIntoView.ScrollTo(v._presenter?.Window, v.Document!, target);
		e.Handled = true;
	});
}
