using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Documents;

/// <summary>Review fixes: coalesced view changes, detached delete, symbol cache, SystemFonts keys.</summary>
public class DocumentsReviewFixTests : WpfGtkTestBase
{
	static FlowDocument Doc(int paragraphs)
	{
		var doc = new FlowDocument();
		for (int i = 0; i < paragraphs; i++)
		{
			var paragraph = new Paragraph(new Run("one " + i));
			paragraph.Inlines.Add(new Run("two " + i));
			doc.Blocks.Add(paragraph);
		}
		return doc;
	}

	[Fact]
	public void An_inherited_font_size_change_raises_ViewChanged_once_per_dispatcher_turn() => Run(() =>
	{
		using var scope = new GtkDispatcherScope();
		var doc = Doc(50);
		int views = 0;
		doc.ViewChanged += () => views++;
		doc.FontSize = 20;
		GtkPump.For(100);
		Assert.InRange(views, 1, 1);
	});

	[Fact]
	public void Adding_a_ten_run_paragraph_to_a_document_with_a_font_family_raises_ViewChanged_once() => Run(() =>
	{
		using var scope = new GtkDispatcherScope();
		var doc = Doc(2);
		doc.FontFamily = new FontFamily("Serif");
		GtkPump.For(50);
		int views = 0;
		doc.ViewChanged += () => views++;
		var paragraph = new Paragraph();
		for (int i = 0; i < 10; i++) paragraph.Inlines.Add(new Run("r" + i));
		doc.Blocks.Add(paragraph);
		GtkPump.For(100);
		Assert.InRange(views, 1, 1);
	});

	[Fact]
	public void The_view_markup_carries_the_document_font_family_and_size() => Run(() =>
	{
		var doc = Doc(1);
		doc.FontFamily = new FontFamily("Serif");
		doc.FontSize = 20;
		string markup = doc.BuildViewMarkup();
		Assert.Contains("font_family=\"Serif\"", markup);
		Assert.Contains("size=\"15360\"", markup);
	});

	[Fact]
	public void Replacing_the_text_of_a_detached_paragraph_span_or_run_deletes_the_old_text() => Run(() =>
	{
		var paragraph = new Paragraph(new Run("hello"));
		new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text = "bye";
		Assert.Equal("bye", new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text);

		var span = new Span();
		span.Inlines.Add(new Run("hello"));
		new TextRange(span.ContentStart, span.ContentEnd).Text = "bye";
		Assert.Equal("bye", new TextRange(span.ContentStart, span.ContentEnd).Text);

		var run = new Run("hello");
		new TextRange(run.ContentStart, run.ContentEnd).Text = "bye";
		Assert.Equal("bye", new TextRange(run.ContentStart, run.ContentEnd).Text);
	});

	[Fact]
	public void Clearing_the_text_of_a_detached_list_deletes_the_old_text() => Run(() =>
	{
		var list = new System.Windows.Documents.List();
		var item = new ListItem();
		item.Blocks.Add(new Paragraph(new Run("hello")));
		list.ListItems.Add(item);
		new TextRange(list.ContentStart, list.ContentEnd).Text = "";
		Assert.DoesNotContain("hello", new TextRange(list.ContentStart, list.ContentEnd).Text);
	});

	[Fact]
	public void Setting_the_child_of_an_attached_container_updates_the_symbol_list() => Run(() =>
	{
		var doc = new FlowDocument();
		var block = new BlockUIContainer();
		doc.Blocks.Add(block);
		var inline = new InlineUIContainer();
		doc.Blocks.Add(new Paragraph(inline));
		int total = doc.ContentEnd.Offset;
		block.Child = new Button();
		Assert.Equal(total + 1, doc.ContentEnd.Offset);
		inline.Child = new Button();
		Assert.Equal(total + 2, doc.ContentEnd.Offset);
		block.Child = null;
		Assert.Equal(total + 1, doc.ContentEnd.Offset);
	});

	[Fact]
	public void An_inherited_right_to_left_direction_reaches_the_document_markup() => Run(() =>
	{
		var doc = new FlowDocument(new Paragraph(new Run("abc")));
		string ltr = doc.BuildMarkup();
		var host = new FlowDocumentScrollViewer { FlowDirection = FlowDirection.RightToLeft, Document = doc };
		Assert.Equal(FlowDirection.RightToLeft, doc.FlowDirection);
		Assert.NotEqual(ltr, doc.BuildMarkup());
	});

	[Fact]
	public void The_request_event_args_have_no_public_constructor() =>
		Assert.Empty(typeof(RequestBringIntoViewEventArgs).GetConstructors(BindingFlags.Public | BindingFlags.Instance));

	[Fact]
	public void A_detached_element_has_no_parent_at_its_root_level() => Run(() =>
	{
		var paragraph = new Paragraph(new Run("x"));
		Assert.Null(paragraph.ElementStart.Parent);
		Assert.Null(paragraph.ElementStart.DocumentStart.Parent);
		Assert.NotNull(paragraph.ContentStart.Parent);
	});

	[Fact]
	public void Repeated_offset_reads_build_the_symbol_list_once_and_direct_edits_are_seen() => Run(() =>
	{
		var doc = Doc(3);
		var pointer = doc.ContentStart;
		int offset = pointer.Offset;
		int builds = TextTree.BuildCount;
		for (int i = 0; i < 20; i++) offset = pointer.Offset;
		Assert.Equal(builds, TextTree.BuildCount);

		int total = doc.ContentEnd.Offset;
		((Paragraph)doc.Blocks[0]).Inlines.Add(new Run("more"));
		Assert.Equal(total + 6, doc.ContentEnd.Offset);
		Assert.Equal(offset, pointer.Offset);
		builds = TextTree.BuildCount;
		for (int i = 0; i < 20; i++) _ = pointer.Offset;
		Assert.Equal(builds, TextTree.BuildCount);

		((Run)((Paragraph)doc.Blocks[0]).Inlines[0]).Text = "a much longer text";
		Assert.True(doc.ContentEnd.Offset > total + 6);
	});

	[Fact]
	public void SystemFonts_keys_resolve_to_the_font_values_through_FindResource() => Run(() =>
	{
		Assert.NotNull(SystemFonts.MessageFontSizeKey);
		Assert.Same(SystemFonts.MessageFontSizeKey, SystemFonts.MessageFontSizeKey);
		var element = new Border();
		Assert.Equal(SystemFonts.MessageFontSize, element.FindResource(SystemFonts.MessageFontSizeKey));
		Assert.Equal(SystemFonts.MessageFontStyle, element.FindResource(SystemFonts.MessageFontStyleKey));
		Assert.Equal(SystemFonts.MessageFontWeight, element.FindResource(SystemFonts.MessageFontWeightKey));
		Assert.Equal(SystemFonts.MessageFontFamily.Source,
			((FontFamily)element.FindResource(SystemFonts.MessageFontFamilyKey)!).Source);
		Assert.NotSame(SystemFonts.CaptionFontSizeKey, SystemFonts.MessageFontSizeKey);
	});

	[Fact]
	public void FlowDocument_FlowDirection_is_a_dependency_property_that_round_trips() => Run(() =>
	{
		var doc = new FlowDocument();
		Assert.Equal(FlowDirection.LeftToRight, doc.GetValue(FlowDocument.FlowDirectionProperty));
		doc.SetValue(FlowDocument.FlowDirectionProperty, FlowDirection.RightToLeft);
		Assert.Equal(FlowDirection.RightToLeft, doc.FlowDirection);
		doc.FlowDirection = FlowDirection.LeftToRight;
		Assert.Equal(FlowDirection.LeftToRight, doc.GetValue(FlowDocument.FlowDirectionProperty));
		Assert.Throws<ArgumentException>(() => doc.FlowDirection = (FlowDirection)5);
	});
}
