using System;
using System.Windows;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TextPointerTests
{
	static FlowDocument Doc(params string[] paragraphs)
	{
		var d = new FlowDocument();
		foreach (var t in paragraphs)
		{
			var p = new Paragraph();
			p.Inlines.Add(new Run { Text = t });
			d.Blocks.Add(p);
		}
		return d;
	}

	[Fact]
	public void ParagraphWithRun_counts_edges_and_characters()
	{
		var d = Doc("abc");
		Assert.Equal(7, d.ContentStart.GetOffsetToPosition(d.ContentEnd));
	}

	[Fact]
	public void ContentStart_and_End_have_WPF_directions()
	{
		var d = Doc("a");
		Assert.Equal(LogicalDirection.Backward, d.ContentStart.LogicalDirection);
		Assert.Equal(LogicalDirection.Forward, d.ContentEnd.LogicalDirection);
	}

	[Fact]
	public void Contexts_walk_edges_text_and_none()
	{
		var d = Doc("ab");
		var p = d.ContentStart;
		Assert.Equal(TextPointerContext.None, p.GetPointerContext(LogicalDirection.Backward));
		Assert.Equal(TextPointerContext.ElementStart, p.GetPointerContext(LogicalDirection.Forward));
		var inRun = p.GetPositionAtOffset(2)!;
		Assert.Equal(TextPointerContext.Text, inRun.GetPointerContext(LogicalDirection.Forward));
		Assert.Equal(TextPointerContext.ElementStart, inRun.GetPointerContext(LogicalDirection.Backward));
		var end = p.GetPositionAtOffset(4)!;
		Assert.Equal(TextPointerContext.ElementEnd, end.GetPointerContext(LogicalDirection.Forward));
		Assert.Equal(TextPointerContext.Text, end.GetPointerContext(LogicalDirection.Backward));
		Assert.Equal(TextPointerContext.None, d.ContentEnd.GetPointerContext(LogicalDirection.Forward));
	}

	[Fact]
	public void TextRun_helpers_report_adjacent_text()
	{
		var d = Doc("hello");
		var mid = d.ContentStart.GetPositionAtOffset(4)!;
		Assert.Equal(3, mid.GetTextRunLength(LogicalDirection.Forward));
		Assert.Equal("llo", mid.GetTextInRun(LogicalDirection.Forward));
		Assert.Equal("he", mid.GetTextInRun(LogicalDirection.Backward));
		var buf = new char[10];
		Assert.Equal(2, mid.GetTextInRun(LogicalDirection.Forward, buf, 1, 2));
		Assert.Equal("ll", new string(buf, 1, 2));
		Assert.Equal(0, d.ContentStart.GetTextRunLength(LogicalDirection.Forward));
	}

	[Fact]
	public void GetPositionAtOffset_returns_null_outside()
	{
		var d = Doc("a");
		Assert.Null(d.ContentStart.GetPositionAtOffset(-1));
		Assert.Null(d.ContentEnd.GetPositionAtOffset(1));
		Assert.NotNull(d.ContentStart.GetPositionAtOffset(5));
	}

	[Fact]
	public void CompareTo_orders_and_throws_across_documents()
	{
		var d = Doc("a");
		Assert.True(d.ContentStart.CompareTo(d.ContentEnd) < 0);
		Assert.True(d.ContentEnd.CompareTo(d.ContentStart) > 0);
		Assert.Equal(0, d.ContentStart.CompareTo(d.ContentStart));
		Assert.Throws<ArgumentException>(() => d.ContentStart.CompareTo(Doc("a").ContentStart));
		Assert.False(d.ContentStart.IsInSameDocument(Doc("a").ContentStart));
	}

	[Fact]
	public void NextContextPosition_skips_a_whole_text_run()
	{
		var d = Doc("abc");
		var p = d.ContentStart.GetPositionAtOffset(2)!;
		Assert.Equal(3, p.GetOffsetToPosition(p.GetNextContextPosition(LogicalDirection.Forward)!));
		Assert.Equal(-1, p.GetOffsetToPosition(p.GetNextContextPosition(LogicalDirection.Backward)!));
		Assert.Null(d.ContentEnd.GetNextContextPosition(LogicalDirection.Forward));
	}

	[Fact]
	public void Parent_and_Paragraph_follow_the_tree()
	{
		var d = Doc("abc");
		Assert.Same(d, d.ContentStart.Parent);
		var inRun = d.ContentStart.GetPositionAtOffset(3)!;
		Assert.IsType<Run>(inRun.Parent);
		Assert.Same(d.Blocks[0], inRun.Paragraph);
		Assert.Null(d.ContentStart.Paragraph);
	}

	[Fact]
	public void InsertionPositions_are_inside_runs_not_on_block_edges()
	{
		var d = Doc("ab");
		Assert.False(d.ContentStart.IsAtInsertionPosition);
		Assert.True(d.ContentStart.GetPositionAtOffset(2)!.IsAtInsertionPosition);
		Assert.False(d.ContentStart.GetPositionAtOffset(1)!.IsAtInsertionPosition);
		var ins = d.ContentStart.GetInsertionPosition(LogicalDirection.Forward);
		Assert.Equal(2, d.ContentStart.GetOffsetToPosition(ins));
		var back = d.ContentEnd.GetInsertionPosition(LogicalDirection.Backward);
		Assert.Equal(4, d.ContentStart.GetOffsetToPosition(back));
	}

	[Fact]
	public void NextInsertionPosition_moves_one_character()
	{
		var d = Doc("ab");
		var p = d.ContentStart.GetPositionAtOffset(2)!;
		Assert.Equal(1, p.GetOffsetToPosition(p.GetNextInsertionPosition(LogicalDirection.Forward)!));
		Assert.Equal(-1, d.ContentStart.GetPositionAtOffset(3)!
			.GetOffsetToPosition(d.ContentStart.GetPositionAtOffset(3)!.GetNextInsertionPosition(LogicalDirection.Backward)!));
	}

	[Fact]
	public void InsertTextInRun_and_DeleteTextInRun_edit_and_pointers_survive()
	{
		var d = Doc("hello");
		var afterEnd = d.ContentEnd;
		var at = d.ContentStart.GetPositionAtOffset(4)!; // after "he"
		var later = d.ContentStart.GetPositionAtOffset(6)!; // after "hell"
		at.InsertTextInRun("XY");
		Assert.Equal("heXYllo", ((Run)((Paragraph)d.Blocks[0]).Inlines[0]).Text);
		Assert.Equal(11, d.ContentStart.GetOffsetToPosition(afterEnd));
		Assert.Equal(8, d.ContentStart.GetOffsetToPosition(later));
		Assert.Equal(2, d.ContentStart.GetPositionAtOffset(4)!.DeleteTextInRun(2));
		Assert.Equal("hello", ((Run)((Paragraph)d.Blocks[0]).Inlines[0]).Text);
		Assert.Equal(6, d.ContentStart.GetOffsetToPosition(later));
	}

	[Fact]
	public void DeleteTextInRun_negative_deletes_backward_and_clamps()
	{
		var d = Doc("abc");
		var p = d.ContentStart.GetPositionAtOffset(4)!;
		Assert.Equal(2, p.DeleteTextInRun(-5));
		Assert.Equal("c", ((Run)((Paragraph)d.Blocks[0]).Inlines[0]).Text);
	}

	[Fact]
	public void InsertTextInRun_between_blocks_creates_a_paragraph_and_in_paragraph_creates_a_run()
	{
		var d = new FlowDocument();
		d.ContentStart.InsertTextInRun("x");
		Assert.Equal("x", ((Run)((Paragraph)d.Blocks[0]).Inlines[0]).Text);
		var p = (Paragraph)d.Blocks[0];
		d.ContentStart.GetPositionAtOffset(1)!.InsertTextInRun("y");
		Assert.Equal(2, p.Inlines.Count);
	}

	[Fact]
	public void Embedded_element_and_list_are_counted()
	{
		var d = new FlowDocument();
		var p = new Paragraph();
		p.Inlines.Add(new InlineUIContainer(new System.Windows.Controls.Button()));
		d.Blocks.Add(p);
		Assert.Equal(5, d.ContentStart.GetOffsetToPosition(d.ContentEnd));
		Assert.Equal(TextPointerContext.EmbeddedElement,
			d.ContentStart.GetPositionAtOffset(2)!.GetPointerContext(LogicalDirection.Forward));
	}
}
