using System.Collections.Generic;
using System.Globalization;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace System.Windows.Annotations;

public sealed partial class AnnotationService
{
	internal const string HighlightCargo = "Highlight";

	// The look of a highlight without a stored color: WPF's default, a translucent yellow.
	static readonly Color DefaultHighlight = Color.FromArgb(0x54, 0xFF, 0xFF, 0x00);

	readonly Dictionary<Guid, List<(int Start, int End)>> _painted = new Dictionary<Guid, List<(int, int)>>();
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
			var brush = new SolidColorBrush(HighlightColor(annotation));
			foreach (var (start, end) in segments) AnnotationAnchors.ToRange(doc, start, end).ApplyBackground(brush);
			_painted[annotation.Id] = segments;
		}
		else if (annotation.AnnotationType.Equals(StickyNoteData.TextType) && !_notes.ContainsKey(annotation.Id))
		{
			var note = new StickyNoteControl(annotation, new AnchorInfo(annotation, annotation.Anchors[0], Resolve(doc, segments)));
			_notes[annotation.Id] = note;
			note.Present(_host.Element);
		}
	}

	void Detach(Annotation annotation)
	{
		if (_painted.TryGetValue(annotation.Id, out var segments))
		{
			_painted.Remove(annotation.Id);
			if (Document is { } doc)
				foreach (var (start, end) in segments) AnnotationAnchors.ToRange(doc, start, end).ApplyBackground(null);
		}
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

	static Color HighlightColor(Annotation annotation)
	{
		var content = StickyNoteData.Cargo(annotation, HighlightCargo)?.Contents;
		if (content != null)
			foreach (var element in content)
				if (element.LocalName == "Colors" && TryParseColor(element.GetAttribute("Background"), out var color))
					return color;
		return DefaultHighlight;
	}

	internal static bool TryParseColor(string text, out Color color)
	{
		color = default;
		if (text == null || text.Length != 9 || text[0] != '#'
			|| !uint.TryParse(text.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint argb))
			return false;
		color = Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
		return true;
	}
}
