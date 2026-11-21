using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;

namespace System.Windows.Annotations;

/// <summary>Creates, clears and resolves annotations for the selection of the viewer a service serves.</summary>
public static class AnnotationHelper
{
	/// <summary>Highlights the selection, trimming highlights it overlaps. The brush must be a SolidColorBrush.</summary>
	/// <exception cref="ArgumentException">The service is not enabled, or the brush is not solid.</exception>
	/// <exception cref="InvalidOperationException">The selection is empty.</exception>
	public static Annotation CreateHighlightForSelection(AnnotationService service, string? author, Brush? highlightBrush)
	{
		var range = SelectionRange(service);
		var color = ColorOf(highlightBrush);
		TrimHighlights(service, range);
		var annotation = New(AnnotationXmlConstants.Highlight, author, range);
		if (color != null)
		{
			var colors = new XmlDocument().CreateElement(AnnotationXmlConstants.BasePrefix, "Colors",
				AnnotationXmlConstants.BaseNamespace);
			colors.SetAttribute("Background", color.Value.ToString());
			var cargo = new AnnotationResource(AnnotationService.HighlightCargo);
			cargo.Contents.Add(colors);
			annotation.Cargos.Add(cargo);
		}
		service.Store!.AddAnnotation(annotation);
		return annotation;
	}

	/// <summary>Adds a text sticky note anchored on the selection.</summary>
	public static Annotation CreateTextStickyNoteForSelection(AnnotationService service, string? author) =>
		CreateNote(service, StickyNoteData.TextType, author);

	/// <summary>Adds an ink sticky note anchored on the selection. Gap: stored, but no ink surface is shown.</summary>
	public static Annotation CreateInkStickyNoteForSelection(AnnotationService service, string? author) =>
		CreateNote(service, StickyNoteData.InkType, author);

	/// <summary>Removes the highlighted part of the selection from every highlight.</summary>
	public static void ClearHighlightsForSelection(AnnotationService service) =>
		TrimHighlights(service, SelectionRange(service));

	/// <summary>Deletes the text sticky notes the selection touches.</summary>
	public static void DeleteTextStickyNotesForSelection(AnnotationService service) =>
		DeleteSpanned(service, StickyNoteData.TextType);

	/// <summary>Deletes the ink sticky notes the selection touches.</summary>
	public static void DeleteInkStickyNotesForSelection(AnnotationService service) =>
		DeleteSpanned(service, StickyNoteData.InkType);

	/// <summary>How an annotation resolved on the viewer's document, or null when it does not.</summary>
	public static IAnchorInfo? GetAnchorInfo(AnnotationService service, Annotation annotation)
	{
		Check(service);
		if (annotation == null) throw new ArgumentNullException(nameof(annotation));
		var segments = AnnotationAnchors.Read(annotation, out var anchor);
		var doc = service.Document;
		if (segments == null || doc == null) return null;
		return new AnchorInfo(annotation, anchor!, AnnotationService.Resolve(doc, segments));
	}

	static void Check(AnnotationService service)
	{
		if (service == null) throw new ArgumentNullException(nameof(service));
		if (!service.IsEnabled) throw new ArgumentException("The annotation service is not enabled.", nameof(service));
	}

	static TextRange SelectionRange(AnnotationService service)
	{
		Check(service);
		var selection = service.Selection;
		if (selection == null || selection.IsEmpty)
			throw new InvalidOperationException("An empty selection is not supported.");
		return selection;
	}

	static Color? ColorOf(Brush? brush)
	{
		if (brush == null) return null;
		if (!(brush is SolidColorBrush solid)) throw new ArgumentException("The highlight brush must be solid.", nameof(brush));
		byte alpha = solid.Opacity <= 0 ? (byte)0 : solid.Opacity >= 1 ? solid.Color.A : (byte)(solid.Opacity * solid.Color.A);
		return Color.FromArgb(alpha, solid.Color.R, solid.Color.G, solid.Color.B);
	}

	static Annotation New(XmlQualifiedName type, string? author, TextRange range)
	{
		var annotation = new Annotation(type);
		AnnotationAnchors.Set(annotation, AnnotationAnchors.Of(range));
		if (author != null) annotation.Authors.Add(author);
		return annotation;
	}

	static Annotation CreateNote(AnnotationService service, XmlQualifiedName type, string? author)
	{
		var annotation = New(type, author, SelectionRange(service));
		StickyNoteData.AddCargos(annotation);
		service.Store!.AddAnnotation(annotation);
		return annotation;
	}

	static void TrimHighlights(AnnotationService service, TextRange range)
	{
		int start = range.Start.Offset, end = range.End.Offset;
		foreach (var highlight in service.Store!.GetAnnotations()
			.Where(a => a.AnnotationType.Equals(AnnotationXmlConstants.Highlight)).ToList())
		{
			var segments = AnnotationAnchors.Read(highlight, out _);
			if (segments == null || !AnnotationAnchors.Overlaps(segments, start, end)) continue;
			var rest = AnnotationAnchors.Subtract(segments, start, end);
			if (rest.Count == 0) service.Store.DeleteAnnotation(highlight.Id);
			else AnnotationAnchors.Set(highlight, rest);
		}
	}

	static void DeleteSpanned(AnnotationService service, XmlQualifiedName type)
	{
		Check(service);
		var selection = service.Selection ?? throw new InvalidOperationException("The viewer has no selection.");
		int start = selection.Start.Offset, end = selection.End.Offset;
		foreach (var note in service.Store!.GetAnnotations().Where(a => a.AnnotationType.Equals(type)).ToList())
		{
			var segments = AnnotationAnchors.Read(note, out _);
			if (segments != null && AnnotationAnchors.Overlaps(segments, start, end))
				service.Store.DeleteAnnotation(note.Id);
		}
	}
}
