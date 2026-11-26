using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Annotations;

public class StickyNoteControlTests : WpfGtkTestBase
{

	static StickyNoteControl NoteFor(out Annotation annotation, out AnnotationService service, out FlowDocumentReader reader)
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("hello wonderful world")));
		reader = new FlowDocumentReader { Document = doc };
		service = new AnnotationService(reader);
		service.Enable(new XmlStreamStore(new MemoryStream()));
		reader.SelectForAnnotation(doc.ContentStart.GetPositionAtOffset(2)!, doc.ContentStart.GetPositionAtOffset(8)!);
		annotation = AnnotationHelper.CreateTextStickyNoteForSelection(service, "ann");
		return service.NoteFor(annotation.Id)!;
	}

	[Fact]
	public void Defaults_match_WPF() => Run(() =>
	{
		var note = NoteFor(out _, out _, out _);
		Assert.True(note.IsExpanded);
		Assert.False(note.IsActive);
		Assert.False(note.IsMouseOverAnchor);
		Assert.Equal("ann", note.Author);
		Assert.Equal(StickyNoteType.Text, note.StickyNoteType);
		Assert.Equal(SystemFonts.MessageFontSize, note.CaptionFontSize);
		Assert.Equal(SystemFonts.MessageFontWeight, note.CaptionFontWeight);
		Assert.Equal(SystemFonts.MessageFontStyle, note.CaptionFontStyle);
		Assert.Equal(FontStretches.Normal, note.CaptionFontStretch);
		Assert.Equal(2.0031496062992127, note.PenWidth);
		Assert.Equal(2.0031496062992127,
			(double)StickyNoteControl.PenWidthProperty.GetMetadata(typeof(StickyNoteControl)).DefaultValue!);
		Assert.NotNull(note.AnchorInfo);
	});

	[Fact]
	public void Dependency_properties_have_WPF_metadata() => Run(() =>
	{
		Assert.True((bool)StickyNoteControl.IsExpandedProperty.DefaultMetadata.DefaultValue!);
		Assert.True(StickyNoteControl.AuthorProperty.ReadOnly);
		Assert.True(StickyNoteControl.StickyNoteTypeProperty.ReadOnly);
		Assert.True(StickyNoteControl.IsMouseOverAnchorProperty.ReadOnly);
		Assert.False(StickyNoteControl.IsExpandedProperty.ReadOnly);
		Assert.Equal("IsActive", StickyNoteControl.IsActiveProperty.Name);
		Assert.Equal(string.Empty, StickyNoteControl.AuthorProperty.DefaultMetadata.DefaultValue);
		Assert.Equal(StickyNoteType.Text, StickyNoteControl.StickyNoteTypeProperty.DefaultMetadata.DefaultValue);
	});

	[Fact]
	public void IsExpanded_and_caption_fonts_are_settable() => Run(() =>
	{
		var note = NoteFor(out _, out _, out _);
		note.IsExpanded = false;
		note.CaptionFontSize = 20;
		note.PenWidth = 5;
		Assert.False(note.IsExpanded);
		Assert.Equal(20, note.CaptionFontSize);
		Assert.Equal(5, note.PenWidth);
	});

	[Fact]
	public void Commands_and_schema_names_are_public_statics() => Run(() =>
	{
		Assert.Equal("DeleteNote", StickyNoteControl.DeleteNoteCommand.Name);
		Assert.Equal("Ink", StickyNoteControl.InkCommand.Name);
		Assert.Equal(typeof(StickyNoteControl), StickyNoteControl.InkCommand.OwnerType);
		Assert.Equal("TextStickyNote", StickyNoteControl.TextSchemaName.Name);
		Assert.Equal("InkStickyNote", StickyNoteControl.InkSchemaName.Name);
	});

	[Fact]
	public void Text_note_cargo_has_the_layout_the_sample_binds_to() => Run(() =>
	{
		NoteFor(out var annotation, out _, out _);
		Assert.Equal("Meta Data", annotation.Cargos[0].Name);
		Assert.Equal("Text Data", annotation.Cargos[1].Name);
		var data = Convert.FromBase64String(annotation.Cargos[1].Contents[0].InnerText);
		Assert.Contains("<Section", Encoding.UTF8.GetString(data));
	});

	[Fact]
	public void Ink_note_is_stored_without_a_popover() => Run(() =>
	{
		NoteFor(out _, out var service, out var reader);
		var ink = AnnotationHelper.CreateInkStickyNoteForSelection(service, "ink");
		Assert.Equal("Ink Data", ink.Cargos[1].Name);
		Assert.Null(service.NoteFor(ink.Id));
	});

	[Fact]
	public void Editing_the_note_text_updates_the_cargo_and_fires_CargoChanged() => Run(() =>
	{
		var note = NoteFor(out var annotation, out var service, out _);
		var seen = 0;
		annotation.CargoChanged += (s, e) => seen++;
		note.NoteText = "remember <this> & that";
		Assert.True(seen > 0);
		Assert.Equal("remember <this> & that", StickyNoteData.ReadText(annotation));
		Assert.Equal("remember <this> & that", note.NoteText);
	});

	[Fact]
	public void The_popover_is_created_on_the_viewer_and_released_when_the_note_is_deleted() => Run(() =>
	{
		var note = NoteFor(out var annotation, out var service, out var reader);
		var popover = note.GtkPopover;
		Assert.NotNull(popover);
		Assert.Equal(reader.Widget.Handle, popover!.Parent!.Handle);
		service.Store!.DeleteAnnotation(annotation.Id);
		Assert.Null(note.GtkPopover);
		Assert.Null(service.NoteFor(annotation.Id));
	});

	[Fact]
	public void Disable_dismisses_notes_and_Enable_shows_them_again() => Run(() =>
	{
		var note = NoteFor(out var annotation, out var service, out _);
		var store = service.Store!;
		service.Disable();
		Assert.Null(note.GtkPopover);
		Assert.Null(service.NoteFor(annotation.Id));
		service.Enable(store);
		Assert.NotNull(service.NoteFor(annotation.Id)!.GtkPopover);
	});

	[Fact]
	public void A_text_note_on_a_realised_reader_shows_its_popover() => Run(() =>
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("hello wonderful world")));
		var reader = new FlowDocumentReader { Document = doc };
		var window = new Window { Content = reader, Width = 400, Height = 300 };
		window.Show();
		Assert.True(GtkPump.Until(() => reader.Widget.IsMapped));
		var service = new AnnotationService(reader);
		service.Enable(new XmlStreamStore(new MemoryStream()));
		reader.SelectForAnnotation(doc.ContentStart.GetPositionAtOffset(2)!, doc.ContentStart.GetPositionAtOffset(8)!);
		var note = AnnotationHelper.CreateTextStickyNoteForSelection(service, "ann");
		var control = service.NoteFor(note.Id)!;
		Assert.True(GtkPump.Until(() => control.GtkPopover!.IsVisible));
		control.IsExpanded = false;
		Assert.True(GtkPump.Until(() => !control.GtkPopover!.IsVisible));
		service.Disable();
		window.Close();
	});
}
