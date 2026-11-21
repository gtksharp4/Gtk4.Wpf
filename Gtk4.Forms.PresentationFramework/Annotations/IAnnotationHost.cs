using System.Windows.Documents;

namespace System.Windows.Annotations;

/// <summary>
/// The one seam between <see cref="AnnotationService"/> and the viewer it serves: the element that carries
/// the command bindings, the displayed flow document and the current selection.
/// </summary>
interface IAnnotationHost
{
	UIElement Element { get; }

	FlowDocument? Document { get; }

	/// <summary>The selection, or null when the viewer has none (not even a caret).</summary>
	TextRange? Selection { get; }
}
