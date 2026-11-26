using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public partial class TextRangeTests
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

	static TextRange Range(FlowDocument d, int from, int to) =>
		new TextRange(d.ContentStart.GetPositionAtOffset(from)!, d.ContentStart.GetPositionAtOffset(to)!);

	static string[] Texts(FlowDocument d) =>
		d.Blocks.Cast<Paragraph>().Select(p => string.Concat(p.Inlines.OfType<Run>().Select(r => r.Text))).ToArray();

	[Fact]
	public void Ctor_orders_the_ends_and_validates()
	{
		var d = Doc("hello");
		var r = new TextRange(d.ContentEnd, d.ContentStart);
		Assert.Equal(0, r.Start.CompareTo(d.ContentStart));
		Assert.Equal(0, r.End.CompareTo(d.ContentEnd));
		Assert.Throws<ArgumentNullException>(() => new TextRange(null!, d.ContentEnd));
		Assert.Throws<ArgumentException>(() => new TextRange(d.ContentStart, Doc("x").ContentEnd));
	}

	[Fact]
	public void Text_reads_the_exact_range()
	{
		var d = Doc("hello world");
		Assert.Equal("world", Range(d, 8, 13).Text);
		Assert.Equal("hello world\r\n", new TextRange(d.ContentStart, d.ContentEnd).Text);
	}

	[Fact]
	public void Text_across_paragraphs_uses_crlf()
	{
		var d = Doc("ab", "cd");
		Assert.Equal("ab\r\ncd\r\n", new TextRange(d.ContentStart, d.ContentEnd).Text);
		Assert.Equal("b\r\nc", Range(d, 3, 9).Text);
	}

	[Fact]
	public void Text_setter_replaces_only_the_range_content()
	{
		var d = Doc("hello world");
		var r = Range(d, 8, 13);
		r.Text = "there";
		Assert.Equal(new[] { "hello there" }, Texts(d));
		Assert.Equal("there", r.Text);
	}

	[Fact]
	public void Text_setter_across_paragraphs_merges_them()
	{
		var d = Doc("abc", "def");
		Range(d, 4, 10).Text = "X";
		Assert.Equal(new[] { "abXef" }, Texts(d));
	}

	[Fact]
	public void Text_setter_with_newlines_makes_paragraphs()
	{
		var d = Doc("abcd");
		Range(d, 4, 6).Text = "1\r\n2\n3";
		Assert.Equal(new[] { "ab1", "2", "3" }, Texts(d));
	}

	[Fact]
	public void Text_setter_in_an_empty_document_creates_a_paragraph()
	{
		var d = new FlowDocument();
		new TextRange(d.ContentStart, d.ContentEnd).Text = "hi";
		Assert.Equal(new[] { "hi" }, Texts(d));
	}

	[Fact]
	public void Text_setter_removes_fully_covered_elements()
	{
		var d = Doc("a", "b", "c");
		new TextRange(d.ContentStart.GetPositionAtOffset(5)!, d.ContentStart.GetPositionAtOffset(10)!).Text = "";
		Assert.Equal(new[] { "a", "c" }, Texts(d));
	}

	[Fact]
	public void IsEmpty_and_Contains()
	{
		var d = Doc("abc");
		Assert.True(Range(d, 3, 3).IsEmpty);
		Assert.False(Range(d, 2, 4).IsEmpty);
		var r = Range(d, 2, 4);
		Assert.True(r.Contains(d.ContentStart.GetPositionAtOffset(3)!));
		Assert.True(r.Contains(r.End));
		Assert.False(r.Contains(d.ContentEnd));
		Assert.Throws<ArgumentException>(() => r.Contains(Doc("x").ContentStart));
	}

	[Fact]
	public void Select_moves_the_range_and_raises_Changed()
	{
		var d = Doc("abc");
		var r = Range(d, 2, 3);
		int n = 0;
		r.Changed += (_, _) => n++;
		r.Select(d.ContentStart.GetPositionAtOffset(3)!, d.ContentStart.GetPositionAtOffset(5)!);
		Assert.Equal("bc", r.Text);
		Assert.Equal(1, n);
	}
}
