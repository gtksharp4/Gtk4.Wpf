using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A TextRange edit on a RichTextBox document must reach the real GTK text buffer.</summary>
public class TextRangeGtkTests : WpfGtkTestBase
{
	static Gtk.TextBuffer BufferOf(RichTextBox rtb)
	{
		var sw = (Gtk.ScrolledWindow)rtb.Widget;
		var viewport = sw.Child as Gtk.Viewport;
		return ((Gtk.TextView)(viewport != null ? viewport.Child! : sw.Child!)).Buffer;
	}

	[Fact]
	public void Range_text_edit_re_syncs_the_gtk_buffer() => Run(() =>
	{
		var rtb = new RichTextBox();
		rtb.AppendText("hello world");
		var buf = BufferOf(rtb);
		Assert.Equal("hello world", buf.Text);

		var doc = rtb.Document!;
		var range = new TextRange(doc.ContentStart.GetPositionAtOffset(8)!, doc.ContentStart.GetPositionAtOffset(13)!);
		range.Text = "there";
		Assert.Equal("hello there", buf.Text);
	});

	[Fact]
	public void Load_into_the_whole_document_re_syncs_the_gtk_buffer() => Run(() =>
	{
		var rtb = new RichTextBox();
		var buf = BufferOf(rtb);
		var all = new TextRange(rtb.Document!.ContentStart, rtb.Document.ContentEnd);
		all.Load(new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("one\ntwo")), System.Windows.DataFormats.Text);
		Assert.Equal("one\ntwo", buf.Text);
	});

	[Fact]
	public void Formatting_applied_to_a_range_keeps_the_text() => Run(() =>
	{
		var rtb = new RichTextBox();
		rtb.AppendText("abcdef");
		var doc = rtb.Document!;
		var range = new TextRange(doc.ContentStart.GetPositionAtOffset(4)!, doc.ContentStart.GetPositionAtOffset(6)!);
		range.ApplyPropertyValue(TextElement.FontWeightProperty, System.Windows.FontWeights.Bold);
		Assert.Equal("abcdef", BufferOf(rtb).Text);
	});
}
