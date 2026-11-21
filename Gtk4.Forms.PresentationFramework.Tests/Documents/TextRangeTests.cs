using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class TextRangeTests
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

	[Fact]
	public void Range_follows_edits_made_before_it()
	{
		var d = Doc("abcdef");
		var r = Range(d, 4, 6); // "cd"
		d.ContentStart.GetPositionAtOffset(2)!.InsertTextInRun("123");
		Assert.Equal("cd", r.Text);
	}

	[Fact]
	public void ApplyPropertyValue_splits_runs_at_the_edges()
	{
		var d = Doc("abcdef");
		var r = Range(d, 4, 6);
		r.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
		var runs = ((Paragraph)d.Blocks[0]).Inlines.Cast<Run>().ToList();
		Assert.Equal(new[] { "ab", "cd", "ef" }, runs.Select(x => x.Text).ToArray());
		Assert.Equal(FontWeights.Bold, runs[1].FontWeight);
		Assert.Null(runs[0].FontWeight);
		Assert.Equal("cd", r.Text);
		Assert.Equal(FontWeights.Bold, r.GetPropertyValue(TextElement.FontWeightProperty));
	}

	[Fact]
	public void GetPropertyValue_is_Unset_when_mixed_and_default_otherwise()
	{
		var d = Doc("abcdef");
		Range(d, 4, 6).ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
		Assert.Same(DependencyProperty.UnsetValue, Range(d, 2, 8).GetPropertyValue(TextElement.FontStyleProperty));
		Assert.Equal(FontStyles.Normal, Range(d, 2, 4).GetPropertyValue(TextElement.FontStyleProperty));
	}

	[Fact]
	public void FontSize_and_Foreground_apply_and_read_back()
	{
		var d = Doc("abc");
		var r = new TextRange(d.ContentStart, d.ContentEnd);
		r.ApplyPropertyValue(TextElement.FontSizeProperty, 20.0);
		var red = new SolidColorBrush(Colors.Red);
		r.ApplyPropertyValue(TextElement.ForegroundProperty, red);
		Assert.Equal(20.0, r.GetPropertyValue(TextElement.FontSizeProperty));
		Assert.Same(red, r.GetPropertyValue(TextElement.ForegroundProperty));
		Assert.Throws<ArgumentException>(() => r.ApplyPropertyValue(TextElement.FontSizeProperty, "x"));
		Assert.Throws<ArgumentNullException>(() => r.ApplyPropertyValue(null!, 1));
	}

	[Fact]
	public void Bold_span_reports_bold_weight()
	{
		var d = new FlowDocument();
		var p = new Paragraph();
		var b = new Bold();
		b.Inlines.Add(new Run { Text = "xy" });
		p.Inlines.Add(b);
		d.Blocks.Add(p);
		Assert.Equal(FontWeights.Bold, new TextRange(d.ContentStart, d.ContentEnd).GetPropertyValue(TextElement.FontWeightProperty));
	}

	[Fact]
	public void RichTextBox_Selection_keeps_whole_document_behaviour()
	{
		var rtb = new RichTextBox();
		rtb.Selection.Text = "hello";
		Assert.Equal("hello", rtb.Selection.Text);
		Assert.False(rtb.Selection.IsEmpty);
		Assert.Equal(0, rtb.Selection.Start.CompareTo(rtb.Document!.ContentStart));
	}

	[Fact]
	public void RichTextBox_raises_TextChanged_when_a_range_edits_its_document()
	{
		var rtb = new RichTextBox { Document = Doc("hello world") };
		int n = 0;
		rtb.TextChanged += (_, _) => n++;
		Range(rtb.Document, 8, 13).Text = "there";
		Assert.Equal(1, n);
		Assert.Equal("hello there", rtb.Selection.Text);
	}

	[Fact]
	public void RichTextBox_CaretPosition_is_stored_and_validated()
	{
		var rtb = new RichTextBox { Document = Doc("abc") };
		Assert.Equal(0, rtb.CaretPosition.CompareTo(rtb.Document!.ContentStart));
		var p = rtb.Document.ContentStart.GetPositionAtOffset(3)!;
		rtb.CaretPosition = p;
		Assert.Same(p, rtb.CaretPosition);
		Assert.Throws<ArgumentException>(() => rtb.CaretPosition = Doc("z").ContentStart);
	}
}
