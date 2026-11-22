using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace System.Windows.Annotations;

sealed class FlowDocumentScrollViewerHost : IAnnotationHost
{
	readonly FlowDocumentScrollViewer _viewer;

	internal FlowDocumentScrollViewerHost(FlowDocumentScrollViewer viewer) => _viewer = viewer;

	public UIElement Element => _viewer;

	public FlowDocument? Document => _viewer.Document;

	public TextRange? Selection => _viewer.AnnotationSelection;
}

sealed class FlowDocumentReaderHost : IAnnotationHost
{
	readonly FlowDocumentReader _viewer;

	internal FlowDocumentReaderHost(FlowDocumentReader viewer) => _viewer = viewer;

	public UIElement Element => _viewer;

	public FlowDocument? Document => _viewer.Document;

	public TextRange? Selection => _viewer.AnnotationSelection;
}

sealed class DocumentViewerBaseHost : IAnnotationHost
{
	readonly DocumentViewerBase _viewer;

	internal DocumentViewerBaseHost(DocumentViewerBase viewer) => _viewer = viewer;

	public UIElement Element => _viewer;

	public FlowDocument? Document => _viewer.Document as FlowDocument;

	public TextRange? Selection => _viewer.AnnotationSelection;
}
