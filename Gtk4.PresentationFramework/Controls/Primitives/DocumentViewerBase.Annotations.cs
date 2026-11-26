using System.Windows.Documents;

namespace System.Windows.Controls.Primitives;

public abstract partial class DocumentViewerBase
{
	/// <summary>The selection the annotation service acts on; set through <see cref="SelectForAnnotation"/>.</summary>
	internal TextRange? AnnotationSelection { get; set; }

	internal void SelectForAnnotation(TextPointer start, TextPointer end) =>
		AnnotationSelection = new TextRange(start, end);
}
