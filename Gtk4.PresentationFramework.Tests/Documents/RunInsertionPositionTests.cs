using System.Windows;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class RunInsertionPositionTests
{
	static string[] Texts(BlockCollection blocks) =>
		blocks.Cast<Paragraph>().Select(p => string.Concat(p.Inlines.Cast<Run>().Select(r => r.Text))).ToArray();

	[Fact]
	public void An_empty_document_gets_a_paragraph()
	{
		var doc = new FlowDocument();
		var run = new Run("x", doc.ContentStart);
		Assert.Equal(new[] { "x" }, Texts(doc.Blocks));
		Assert.Same(doc.Blocks[0], run.Parent);
	}

	[Fact]
	public void Between_two_blocks_a_paragraph_is_created_there()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("a")));
		doc.Blocks.Add(new Paragraph(new Run("c")));
		var between = doc.ContentStart.GetPositionAtOffset(5)!;
		Assert.Equal(TextPointerContext.ElementStart, between.GetPointerContext(LogicalDirection.Forward));
		new Run("b", between);
		Assert.Equal(new[] { "a", "b", "c" }, Texts(doc.Blocks));
	}

	[Fact]
	public void At_the_document_end_a_paragraph_is_appended()
	{
		var doc = new FlowDocument(new Paragraph(new Run("a")));
		new Run("z", doc.ContentEnd);
		Assert.Equal(new[] { "a", "z" }, Texts(doc.Blocks));
	}

	[Fact]
	public void An_empty_section_gets_a_paragraph()
	{
		var section = new Section();
		var doc = new FlowDocument(section);
		new Run("in", section.ContentStart);
		Assert.Equal(new[] { "in" }, Texts(section.Blocks));
	}

	[Fact]
	public void An_empty_list_item_gets_a_paragraph()
	{
		var item = new ListItem();
		var list = new List();
		list.ListItems.Add(item);
		var doc = new FlowDocument(list);
		new Run("li", item.ContentStart);
		Assert.Equal(new[] { "li" }, Texts(item.Blocks));
	}

	[Fact]
	public void Inside_a_paragraph_between_inlines_the_run_is_inserted_directly()
	{
		var para = new Paragraph();
		para.Inlines.Add(new Run("a"));
		para.Inlines.Add(new Run("c"));
		var doc = new FlowDocument(para);
		new Run("b", doc.ContentStart.GetPositionAtOffset(4)!);
		Assert.Equal(new[] { "abc" }, Texts(doc.Blocks));
		Assert.Equal(new[] { "a", "b", "c" }, para.Inlines.Cast<Run>().Select(r => r.Text).ToArray());
	}

	[Fact]
	public void Inside_a_run_the_run_is_split()
	{
		var para = new Paragraph(new Run("abcd"));
		var doc = new FlowDocument(para);
		new Run("X", doc.ContentStart.GetPositionAtOffset(4)!);
		Assert.Equal(new[] { "ab", "X", "cd" }, para.Inlines.Cast<Run>().Select(r => r.Text).ToArray());
	}

	[Fact]
	public void Inside_embedded_containers_the_constructor_throws()
	{
		var button = new Button();
		var inline = new InlineUIContainer(button);
		var doc = new FlowDocument(new Paragraph(inline));
		Assert.Throws<InvalidOperationException>(() => new Run("x", inline.ContentStart));
		var block = new BlockUIContainer { Child = new Button() };
		var doc2 = new FlowDocument(block);
		Assert.Throws<InvalidOperationException>(() => new Run("x", block.ContentStart));
	}

	[Fact]
	public void Between_list_items_the_run_moves_to_an_insertion_position()
	{
		var item = new ListItem();
		item.Blocks.Add(new Paragraph(new Run("one")));
		var list = new List();
		list.ListItems.Add(item);
		var doc = new FlowDocument(list);
		var run = new Run("pre", list.ContentStart);
		Assert.NotNull(run.Parent);
		Assert.Equal("pre", run.Text);
		Assert.Same(item, ((Paragraph)run.Parent!).Parent);
		Assert.Equal(1, item.Blocks.Count);
		Assert.Same(run.Parent, item.Blocks[0]);
		var paragraph = (Paragraph)item.Blocks[0];
		Assert.Same(run, paragraph.Inlines[0]);
		Assert.Equal(2, paragraph.Inlines.Count);
		Assert.Equal("preone", new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text);
	}
}
