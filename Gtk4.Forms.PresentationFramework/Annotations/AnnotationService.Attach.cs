using System.Collections.Generic;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Documents;

namespace System.Windows.Annotations;

public sealed partial class AnnotationService
{
	internal const string HighlightCargo = "Highlight";

	// Highlights are stored annotations only; nothing is painted or edited in the document.
	readonly HashSet<Guid> _highlights = new HashSet<Guid>();
	readonly Dictionary<Guid, StickyNoteControl> _notes = new Dictionary<Guid, StickyNoteControl>();

	internal TextRange? Selection => _host.Selection;

	internal FlowDocument? Document => _host.Document;

	internal StickyNoteControl? NoteFor(Guid annotationId) =>
		_notes.TryGetValue(annotationId, out var note) ? note : null;

	void AttachAll()
	{
		foreach (var annotation in _store!.GetAnnotations()) Attach(annotation);
	}

	void DetachAll()
	{
		foreach (var annotation in _store!.GetAnnotations()) Detach(annotation);
	}

	void OnStoreContentChanged(object sender, StoreContentChangedEventArgs e)
	{
		if (e.Action == StoreContentAction.Added) Attach(e.Annotation);
		else Detach(e.Annotation);
	}

	void OnAnchorChanged(object sender, AnnotationResourceChangedEventArgs e)
	{
		Detach(e.Annotation);
		Attach(e.Annotation);
	}

	void Attach(Annotation annotation)
	{
		var doc = Document;
		var segments = AnnotationAnchors.Read(annotation, out _);
		if (doc == null || segments == null) return;
		if (annotation.AnnotationType.Equals(AnnotationXmlConstants.Highlight))
		{
			_highlights.Add(annotation.Id);
		}
		else if (annotation.AnnotationType.Equals(StickyNoteData.TextType) && !_notes.ContainsKey(annotation.Id))
		{
			var anchor = new AnchorInfo(annotation, annotation.Anchors[0], Resolve(doc, segments));
			var note = new StickyNoteControl(annotation, anchor);
			_notes[annotation.Id] = note;
			note.Present(_host.Element);
		}
	}

	void Detach(Annotation annotation)
	{
		_highlights.Remove(annotation.Id);
		if (_notes.TryGetValue(annotation.Id, out var note))
		{
			_notes.Remove(annotation.Id);
			note.Dismiss();
		}
	}

	internal static TextAnchor Resolve(FlowDocument doc, IList<(int Start, int End)> segments)
	{
		int start = int.MaxValue, end = int.MinValue;
		foreach (var (s, e) in segments)
		{
			start = Math.Min(start, s);
			end = Math.Max(end, e);
		}
		return new TextAnchor(new TextPointer(doc, start, LogicalDirection.Backward),
			new TextPointer(doc, end, LogicalDirection.Forward));
	}
}
