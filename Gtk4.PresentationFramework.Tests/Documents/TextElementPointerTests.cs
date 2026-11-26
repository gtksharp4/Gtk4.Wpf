using System.Windows;
using System.Linq;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TextElementPointerTests
{
	static (FlowDocument doc, Section sec, Run run) Doc()
	{
		var run = new Run("abc");
		var sec = new Section();
		sec.Blocks.Add(new Paragraph(run));
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("z")));
		doc.Blocks.Add(sec);
		return (doc, sec, run);
	}

	[Fact]
	public void Section_pointers_bracket_the_start_and_end_tags()
	{
		var (doc, sec, _) = Doc();
		Assert.Equal(1, sec.ElementStart.GetOffsetToPosition(sec.ContentStart));
		Assert.Equal(1, sec.ContentEnd.GetOffsetToPosition(sec.ElementEnd));
		Assert.Equal(2 + 2 + 3, sec.ContentStart.GetOffsetToPosition(sec.ContentEnd));
		Assert.Same(doc, sec.ElementStart.Parent);
		Assert.Same(sec, sec.ContentStart.Parent);
		Assert.Equal(TextPointerContext.ElementStart, sec.ElementStart.GetPointerContext(LogicalDirection.Forward));
		Assert.Equal(TextPointerContext.ElementEnd, sec.ElementEnd.GetPointerContext(LogicalDirection.Backward));
	}

	[Fact]
	public void Run_pointers_hold_the_run_text()
	{
		var (doc, _, run) = Doc();
		Assert.Equal("abc", new TextRange(run.ContentStart, run.ContentEnd).Text);
		Assert.Equal(5, run.ElementStart.GetOffsetToPosition(run.ElementEnd));
		Assert.Same(doc, run.ContentStart.Parent is Run ? doc : null);
	}

	[Fact]
	public void Gravity_is_forward_at_ElementStart_and_ContentEnd()
	{
		var (_, sec, _) = Doc();
		Assert.Equal(LogicalDirection.Forward, sec.ElementStart.LogicalDirection);
		Assert.Equal(LogicalDirection.Backward, sec.ContentStart.LogicalDirection);
		Assert.Equal(LogicalDirection.Forward, sec.ContentEnd.LogicalDirection);
		Assert.Equal(LogicalDirection.Backward, sec.ElementEnd.LogicalDirection);
	}

	[Fact]
	public void A_detached_section_gets_a_private_container_and_keeps_a_null_parent()
	{
		var sec = new Section();
		sec.Blocks.Add(new Paragraph(new Run("hello")));
		var range = new TextRange(sec.ContentStart, sec.ContentEnd);
		Assert.Null(sec.Parent);
		Assert.Equal("hello\r\n", range.Text);
	}

	[Fact]
	public void A_detached_section_range_saves_and_loads()
	{
		var sec = new Section();
		sec.Blocks.Add(new Paragraph(new Run("hello")));
		var range = new TextRange(sec.ContentStart, sec.ContentEnd);
		var ms = new System.IO.MemoryStream();
		range.Save(ms, System.Windows.DataFormats.Text);
		Assert.Equal("hello\r\n", System.Text.Encoding.UTF8.GetString(ms.ToArray()));
		var data = System.Text.Encoding.UTF8.GetBytes("one\ntwo");
		new TextRange(sec.ContentStart, sec.ContentEnd)
			.Load(new System.IO.MemoryStream(data), System.Windows.DataFormats.Text);
		Assert.Equal(new[] { "one", "two" },
			sec.Blocks.Cast<Paragraph>().Select(p => ((Run)p.Inlines.First()).Text).ToArray());
	}

	[Fact]
	public void Pointers_of_a_section_added_later_are_emptied_and_new_ones_belong_to_the_document()
	{
		var sec = new Section();
		sec.Blocks.Add(new Paragraph(new Run("hello")));
		var old = new TextRange(sec.ContentStart, sec.ContentEnd);
		var doc = new FlowDocument();
		doc.Blocks.Add(new Paragraph(new Run("pre")));
		doc.Blocks.Add(sec);
		Assert.Equal(string.Empty, old.Text);
		Assert.Equal("hello\r\n", new TextRange(sec.ContentStart, sec.ContentEnd).Text);
		Assert.Equal("pre\r\nhello\r\n", new TextRange(doc.ContentStart, doc.ContentEnd).Text);
	}

	[Fact]
	public void A_detached_run_reads_its_text()
	{
		var run = new Run("solo");
		Assert.Equal("solo", new TextRange(run.ContentStart, run.ContentEnd).Text);
	}

	static ListItem Item(string text)
	{
		var item = new ListItem();
		item.Blocks.Add(new Paragraph(new Run(text)));
		return item;
	}

	[Fact]
	public void TextRange_text_spans_a_nested_list()
	{
		var list = new List();
		list.ListItems.Add(Item("one"));
		var inner = new List();
		inner.ListItems.Add(Item("two"));
		var second = Item("three");
		second.Blocks.Add(inner);
		list.ListItems.Add(second);
		var doc = new FlowDocument(list);
		Assert.Equal("one\r\nthree\r\ntwo\r\n", new TextRange(doc.ContentStart, doc.ContentEnd).Text);
		Assert.Equal("two\r\n", new TextRange(inner.ContentStart, inner.ContentEnd).Text);
	}
}
