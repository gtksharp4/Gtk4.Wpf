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
		Assert.Same(DependencyProperty.UnsetValue, runs[0].ReadLocalValue(TextElement.FontWeightProperty));
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
		Assert.Equal(FontWeights.Bold,
			new TextRange(d.ContentStart, d.ContentEnd).GetPropertyValue(TextElement.FontWeightProperty));
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
