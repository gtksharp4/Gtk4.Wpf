using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Annotations.Storage;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Annotations;

public class AnnotationServiceTests : WpfGtkTestBase
{
	internal static FlowDocument Doc()
	{
		var d = new FlowDocument();
		d.Blocks.Add(new Paragraph(new Run("hello wonderful world")));
		return d;
	}

	internal static IEnumerable<Run> Runs(FlowDocument d) => ((Paragraph)d.Blocks.First()).Inlines.OfType<Run>();

	internal static XmlStreamStore NewStore() => new XmlStreamStore(new MemoryStream());

	internal static (FlowDocumentScrollViewer Viewer, AnnotationService Service) Setup()
	{
		var viewer = new FlowDocumentScrollViewer { Document = Doc() };
		return (viewer, new AnnotationService(viewer));
	}

	internal static void Select(FlowDocumentScrollViewer v, int from, int to) =>
		v.SelectForAnnotation(v.Document!.ContentStart.GetPositionAtOffset(from)!,
			v.Document.ContentStart.GetPositionAtOffset(to)!);

	[Fact]
	public void Ctors_reject_null() => Run(() =>
	{
		Assert.Throws<ArgumentNullException>(() => new AnnotationService((FlowDocumentScrollViewer)null!));
		Assert.Throws<ArgumentNullException>(() => new AnnotationService((FlowDocumentReader)null!));
		Assert.Throws<ArgumentNullException>(() => AnnotationService.GetService((FlowDocumentReader)null!));
		Assert.Throws<ArgumentNullException>(() => AnnotationService.GetService((FlowDocumentScrollViewer)null!));
	});

	[Fact]
	public void Enable_Disable_and_GetService_follow_the_lifecycle() => Run(() =>
	{
		var (viewer, service) = Setup();
		Assert.False(service.IsEnabled);
		Assert.Null(service.Store);
		Assert.Null(AnnotationService.GetService(viewer));
		using var store = NewStore();
		service.Enable(store);
		Assert.True(service.IsEnabled);
		Assert.Same(store, service.Store);
		Assert.Same(service, AnnotationService.GetService(viewer));
		Assert.Throws<InvalidOperationException>(() => service.Enable(store));
		service.Disable();
		Assert.False(service.IsEnabled);
		Assert.Null(AnnotationService.GetService(viewer));
		Assert.Throws<InvalidOperationException>(() => service.Disable());
		service.Enable(store);
		Assert.True(service.IsEnabled);
	});

	[Fact]
	public void A_second_service_on_the_same_viewer_cannot_be_enabled() => Run(() =>
	{
		var (viewer, first) = Setup();
		using var store = NewStore();
		first.Enable(store);
		var second = new AnnotationService(viewer);
		Assert.Throws<InvalidOperationException>(() => second.Enable(NewStore()));
	});

	[Fact]
	public void A_reader_gets_its_own_service() => Run(() =>
	{
		var reader = new FlowDocumentReader { Document = Doc() };
		var service = new AnnotationService(reader);
		service.Enable(NewStore());
		Assert.Same(service, AnnotationService.GetService(reader));
		Assert.Throws<ArgumentNullException>(() => service.Enable(null!));
	});

	[Fact]
	public void Commands_are_public_static_with_WPF_names() => Run(() =>
	{
		Assert.Equal("CreateHighlight", AnnotationService.CreateHighlightCommand.Name);
		Assert.Equal("CreateTextStickyNote", AnnotationService.CreateTextStickyNoteCommand.Name);
		Assert.Equal("CreateInkStickyNote", AnnotationService.CreateInkStickyNoteCommand.Name);
		Assert.Equal("ClearHighlights", AnnotationService.ClearHighlightsCommand.Name);
		Assert.Equal("DeleteStickyNotes", AnnotationService.DeleteStickyNotesCommand.Name);
		Assert.Equal("DeleteAnnotations", AnnotationService.DeleteAnnotationsCommand.Name);
		Assert.All(new[]
		{
			AnnotationService.CreateHighlightCommand, AnnotationService.CreateTextStickyNoteCommand,
			AnnotationService.ClearHighlightsCommand
		}, c => Assert.Equal(typeof(AnnotationService), c.OwnerType));
	});

	[Fact]
	public void CanExecute_needs_an_enabled_service_and_a_selection() => Run(() =>
	{
		var (viewer, service) = Setup();
		var create = AnnotationService.CreateTextStickyNoteCommand;
		Assert.False(create.CanExecute(null, viewer));
		service.Enable(NewStore());
		Assert.False(create.CanExecute(null, viewer));
		Select(viewer, 2, 2);
		Assert.False(create.CanExecute(null, viewer));
		Assert.True(AnnotationService.DeleteStickyNotesCommand.CanExecute(null, viewer));
		Select(viewer, 2, 8);
		Assert.True(create.CanExecute(null, viewer));
		Assert.True(AnnotationService.CreateHighlightCommand.CanExecute(null, viewer));
		service.Disable();
		Assert.False(create.CanExecute(null, viewer));
	});

	[Fact]
	public void The_create_command_adds_a_text_note_with_the_parameter_as_author() => Run(() =>
	{
		var (viewer, service) = Setup();
		service.Enable(NewStore());
		Select(viewer, 2, 8);
		AnnotationService.CreateTextStickyNoteCommand.Execute("zed", viewer);
		var note = Assert.Single(service.Store!.GetAnnotations());
		Assert.Equal(StickyNoteControl.TextSchemaName, note.AnnotationType);
		Assert.Equal(new[] { "zed" }, note.Authors);
	});
}
