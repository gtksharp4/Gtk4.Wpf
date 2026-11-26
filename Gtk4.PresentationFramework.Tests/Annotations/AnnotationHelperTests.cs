using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Annotations.AnnotationServiceTests;
using static Gtk4.PresentationFramework.Tests.Annotations.XmlStreamStoreTests;

namespace Gtk4.PresentationFramework.Tests.Annotations;

public class AnnotationHelperTests : WpfGtkTestBase
{
	[Fact]
	public void Highlight_is_stored_with_a_character_range_anchor_and_color() => Run(() =>
	{
		var (viewer, service) = Setup();
		service.Enable(NewStore());
		Select(viewer, 2, 8);
		var h = AnnotationHelper.CreateHighlightForSelection(service, "ann", Brushes.Red);
		Assert.Single(service.Store!.GetAnnotations());
		var part = ((ContentLocator)h.Anchors[0].ContentLocators[0]).Parts[0];
		Assert.Equal("CharacterRange", part.PartType.Name);
		Assert.Equal("1", part.NameValuePairs["Count"]);
		Assert.Equal("2,8", part.NameValuePairs["Segment0"]);
		Assert.Equal("#FFFF0000", h.Cargos[0].Contents[0].GetAttribute("Background"));
		var run = Assert.Single(Runs(viewer.Document!));
		Assert.Equal("hello wonderful world", run.Text);
		Assert.Null(run.Background);
		Assert.Contains(h, service.Store.GetAnnotations());
	});

	[Fact]
	public void Highlight_rejects_bad_input() => Run(() =>
	{
		var (viewer, service) = Setup();
		Assert.Throws<ArgumentNullException>(() => AnnotationHelper.CreateHighlightForSelection(null!, "a", Brushes.Red));
		Assert.Throws<ArgumentException>(() => AnnotationHelper.CreateHighlightForSelection(service, "a", Brushes.Red));
		service.Enable(NewStore());
		Assert.Throws<InvalidOperationException>(() => AnnotationHelper.CreateHighlightForSelection(service, "a", Brushes.Red));
		Select(viewer, 3, 3);
		Assert.Throws<InvalidOperationException>(() => AnnotationHelper.CreateTextStickyNoteForSelection(service, "a"));
		Select(viewer, 2, 5);
		Assert.Throws<ArgumentException>(() => AnnotationHelper.CreateHighlightForSelection(
			service, "a", new LinearGradientBrush()));
	});

	[Fact]
	public void ClearHighlights_trims_trims_and_deletes() => Run(() =>
	{
		var (viewer, service) = Setup();
		service.Enable(NewStore());
		Select(viewer, 2, 14);
		var h = AnnotationHelper.CreateHighlightForSelection(service, "a", Brushes.Red);
		Select(viewer, 6, 8);
		AnnotationHelper.ClearHighlightsForSelection(service);
		var part = ((ContentLocator)h.Anchors[0].ContentLocators[0]).Parts[0];
		Assert.Equal("2", part.NameValuePairs["Count"]);
		Assert.Equal("2,6", part.NameValuePairs["Segment0"]);
		Assert.Equal("8,14", part.NameValuePairs["Segment1"]);
		Select(viewer, 0, 21);
		AnnotationHelper.ClearHighlightsForSelection(service);
		Assert.Empty(service.Store!.GetAnnotations());
	});

	[Fact]
	public void A_new_highlight_replaces_the_overlapped_part_of_an_older_one() => Run(() =>
	{
		var (viewer, service) = Setup();
		service.Enable(NewStore());
		Select(viewer, 2, 10);
		var old = AnnotationHelper.CreateHighlightForSelection(service, "a", Brushes.Red);
		Select(viewer, 6, 14);
		AnnotationHelper.CreateHighlightForSelection(service, "b", Brushes.Blue);
		var part = ((ContentLocator)old.Anchors[0].ContentLocators[0]).Parts[0];
		Assert.Equal("2,6", part.NameValuePairs["Segment0"]);
		Assert.Equal(2, service.Store!.GetAnnotations().Count);
	});

	[Fact]
	public void Delete_notes_removes_only_the_notes_the_selection_touches() => Run(() =>
	{
		var (viewer, service) = Setup();
		service.Enable(NewStore());
		Select(viewer, 2, 5);
		AnnotationHelper.CreateTextStickyNoteForSelection(service, "a");
		Select(viewer, 10, 14);
		var far = AnnotationHelper.CreateTextStickyNoteForSelection(service, "b");
		AnnotationHelper.CreateHighlightForSelection(service, "b", Brushes.Red);
		Select(viewer, 3, 4);
		AnnotationHelper.DeleteTextStickyNotesForSelection(service);
		AnnotationHelper.DeleteInkStickyNotesForSelection(service);
		Assert.Equal(2, service.Store!.GetAnnotations().Count);
		Assert.Contains(far, service.Store.GetAnnotations());
	});

	[Fact]
	public void GetAnchorInfo_resolves_to_text_pointers() => Run(() =>
	{
		var (viewer, service) = Setup();
		service.Enable(NewStore());
		Select(viewer, 3, 9);
		var note = AnnotationHelper.CreateTextStickyNoteForSelection(service, "a");
		var info = AnnotationHelper.GetAnchorInfo(service, note)!;
		Assert.Same(note, info.Annotation);
		Assert.Same(note.Anchors[0], info.Anchor);
		var anchor = Assert.IsType<TextAnchor>(info.ResolvedAnchor);
		var start = (TextPointer)anchor.BoundingStart;
		var end = (TextPointer)anchor.BoundingEnd;
		Assert.Equal(0, viewer.Document!.ContentStart.GetPositionAtOffset(3)!.CompareTo(start));
		Assert.Equal(0, viewer.Document.ContentStart.GetPositionAtOffset(9)!.CompareTo(end));
		Assert.Same(viewer.Document.Blocks.First(), start.Paragraph);
		Assert.Null(AnnotationHelper.GetAnchorInfo(service, new Annotation(AnnotationModelTests.Type)));
		Assert.Throws<ArgumentNullException>(() => AnnotationHelper.GetAnchorInfo(service, null!));
	});

	[Fact]
	public void Enable_and_Disable_leave_the_document_alone_and_keep_the_stored_highlight() => Run(() =>
	{
		var (viewer, service) = Setup();
		var store = NewStore();
		var h = new Annotation(AnnotationModelTests.Type);
		AnnotationAnchors.Set(h, new List<(int, int)> { (2, 8) });
		store.AddAnnotation(h);
		var run = Assert.Single(Runs(viewer.Document!));
		service.Enable(store);
		Assert.Same(run, Assert.Single(Runs(viewer.Document!)));
		Assert.Null(run.Background);
		service.Disable();
		Assert.Same(run, Assert.Single(Runs(viewer.Document!)));
		Assert.Contains(h, store.GetAnnotations());
	});
}
