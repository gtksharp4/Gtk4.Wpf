using System.Windows.Documents;

namespace System.Windows.Controls;

public partial class FlowDocumentScrollViewer
{
	/// <summary>
	/// The selection the annotation service acts on. Gap: GTK's label selection is not mapped to document
	/// offsets, so this is set through <see cref="SelectForAnnotation"/> (code, or a future selection bridge).
	/// </summary>
	internal TextRange? AnnotationSelection { get; set; }

	internal void SelectForAnnotation(TextPointer start, TextPointer end) =>
		AnnotationSelection = new TextRange(start, end);
}
