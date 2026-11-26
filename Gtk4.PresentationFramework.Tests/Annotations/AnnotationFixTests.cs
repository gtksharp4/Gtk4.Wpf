using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Xunit;
using static Gtk4.PresentationFramework.Tests.Annotations.AnnotationServiceTests;
using static Gtk4.PresentationFramework.Tests.Annotations.XmlStreamStoreTests;

namespace Gtk4.PresentationFramework.Tests.Annotations;

public class AnnotationFixTests : WpfGtkTestBase
{
	static string AnchorText(AnnotationService service, Annotation annotation)
	{
		var anchor = (TextAnchor)AnnotationHelper.GetAnchorInfo(service, annotation)!.ResolvedAnchor;
		return new TextRange((TextPointer)anchor.BoundingStart, (TextPointer)anchor.BoundingEnd).Text;
	}

	[Fact]
	public void Highlights_do_not_move_note_anchors_or_split_runs() => Run(() =>
	{
		var (viewer, service) = Setup();
		service.Enable(NewStore());
		Select(viewer, 18, 23);
		var note = AnnotationHelper.CreateTextStickyNoteForSelection(service, "a");
		Select(viewer, 8, 12);
		var highlight = AnnotationHelper.CreateHighlightForSelection(service, "a", Brushes.Red);
		Assert.Equal("world", AnchorText(service, note));
		Assert.Equal("wond", AnchorText(service, highlight));
		Assert.Equal("hello wonderful world", Assert.Single(Runs(viewer.Document!)).Text);
	});

	[Fact]
	public void Disable_keeps_author_backgrounds() => Run(() =>
	{
		var (viewer, service) = Setup();
		var green = Brushes.Green;
		Runs(viewer.Document!).First().Background = green;
		service.Enable(NewStore());
		Select(viewer, 8, 12);
		AnnotationHelper.CreateHighlightForSelection(service, "a", Brushes.Red);
		service.Disable();
		var run = Assert.Single(Runs(viewer.Document!));
		Assert.Same(green, run.Background);
	});

	[Fact]
	public void Note_anchor_survives_a_store_round_trip_after_a_highlight() => Run(() =>
	{
		var stream = new MemoryStream();
		var (viewer, service) = Setup();
		service.Enable(new XmlStreamStore(stream));
		Select(viewer, 18, 23);
		AnnotationHelper.CreateTextStickyNoteForSelection(service, "a");
		Select(viewer, 8, 12);
		AnnotationHelper.CreateHighlightForSelection(service, "a", Brushes.Red);
		service.Store!.Flush();
		service.Disable();

		stream.Position = 0;
		var (viewer2, service2) = Setup();
		service2.Enable(new XmlStreamStore(stream));
		var note = service2.Store!.GetAnnotations().Single(a => a.AnnotationType.Equals(StickyNoteData_TextType));
		Assert.Equal("world", AnchorText(service2, note));
	});

	static readonly System.Xml.XmlQualifiedName StickyNoteData_TextType =
		new System.Xml.XmlQualifiedName("TextStickyNote", "http://schemas.microsoft.com/windows/annotations/2003/11/base");

	[Fact]
	public void Custom_cargo_content_survives_load_and_flush()
	{
		var xml = WpfDocument.Replace("<anb:Colors Background=\"#54FFFF00\" />",
			"<x:Payload xmlns:x=\"urn:custom\" a=\"1\">v</x:Payload>");
		var stream = Stream(xml);
		var store = new XmlStreamStore(stream) { AutoFlush = true };
		var cargo = store.GetAnnotations()[0].Cargos[0];
		var content = Assert.Single(cargo.Contents);
		Assert.Equal("Payload", content.LocalName);
		Assert.Equal("urn:custom", content.NamespaceURI);
		Assert.Contains(new System.Uri("urn:custom"), store.IgnoredNamespaces);
		store.AddAnnotation(Note());
		var text = Encoding.UTF8.GetString(stream.ToArray());
		Assert.Contains("urn:custom", text);
		Assert.Contains("Payload", text);
		stream.Position = 0;
		var reloaded = new XmlStreamStore(stream);
		Assert.Single(reloaded.GetAnnotations()[0].Cargos[0].Contents);
	}

	[Fact]
	public void MetaData_cargo_writes_a_namespaced_ZOrder() => Run(() =>
	{
		var (viewer, service) = Setup();
		service.Enable(NewStore());
		Select(viewer, 2, 8);
		var note = AnnotationHelper.CreateTextStickyNoteForSelection(service, "a");
		var meta = note.Cargos.Single(c => c.Name == "Meta Data").Contents[0];
		Assert.Equal("0", meta.GetAttribute("ZOrder", "http://schemas.microsoft.com/windows/annotations/2003/11/base"));
		Assert.DoesNotContain(meta.Attributes.Cast<System.Xml.XmlAttribute>(),
			a => a.NamespaceURI.Length == 0);
	});

	[Theory]
	[InlineData("CreateHighlightCommand", "_Highlight")]
	[InlineData("CreateTextStickyNoteCommand", "Add Text _Note")]
	[InlineData("CreateInkStickyNoteCommand", "Add In_k Note")]
	[InlineData("ClearHighlightsCommand", "_Clear Highlight")]
	[InlineData("DeleteStickyNotesCommand", "D_elete Notes")]
	[InlineData("DeleteAnnotationsCommand", "_Delete Notes and Highlights")]
	public void Command_texts_match_WPF(string field, string text)
	{
		var command = (RoutedUICommand)typeof(AnnotationService).GetField(field)!.GetValue(null)!;
		Assert.Equal(text, command.Text);
	}

	static (FlowDocumentPageViewer, FlowDocument) PageViewer()
	{
		var doc = Doc();
		return (new FlowDocumentPageViewer { Document = doc }, doc);
	}

	[Fact]
	public void DocumentViewerBase_service_hosts_highlights() => Run(() =>
	{
		var (viewer, doc) = PageViewer();
		Assert.Throws<System.ArgumentNullException>(() => new AnnotationService((DocumentViewerBase)null!));
		Assert.Throws<System.ArgumentNullException>(() => AnnotationService.GetService((DocumentViewerBase)null!));
		var service = new AnnotationService(viewer);
		service.Enable(NewStore());
		Assert.Same(service, AnnotationService.GetService(viewer));
		viewer.SelectForAnnotation(doc.ContentStart.GetPositionAtOffset(2)!, doc.ContentStart.GetPositionAtOffset(8)!);
		var h = AnnotationHelper.CreateHighlightForSelection(service, "a", Brushes.Red);
		Assert.Contains(h, service.Store!.GetAnnotations());
		var second = new AnnotationService(viewer);
		Assert.Throws<System.InvalidOperationException>(() => second.Enable(NewStore()));
	});
}
