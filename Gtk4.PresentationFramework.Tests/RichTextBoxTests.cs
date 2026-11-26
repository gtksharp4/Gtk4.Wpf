using System;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="RichTextBox"/>, <see cref="TextRange"/>, and
/// <see cref="TextSelection"/>. No GTK loop required.
/// </summary>
public class RichTextBoxTests
{
	// ── Document ─────────────────────────────────────────────────────────────

	[Fact]
	public void Document_set_is_returned()
	{
		var rtb = new RichTextBox();
		var doc = new FlowDocument();
		rtb.Document = doc;
		Assert.Same(doc, rtb.Document);
	}

	[Fact]
	public void Document_is_an_empty_paragraph_by_default()
	{
		var rtb = new RichTextBox();
		Assert.NotNull(rtb.Document);
		Assert.Equal(1, rtb.Document!.Blocks.Count);
		Assert.IsType<Paragraph>(rtb.Document.Blocks[0]);
		Assert.True(rtb.Selection.IsEmpty);
	}

	// ── AppendText ────────────────────────────────────────────────────────────

	[Fact]
	public void AppendText_creates_document_when_null()
	{
		var rtb = new RichTextBox();
		rtb.AppendText("hello");
		Assert.NotNull(rtb.Document);
	}

	[Fact]
	public void AppendText_adds_paragraph_with_run()
	{
		var rtb = new RichTextBox();
		rtb.AppendText("hello");
		Assert.Equal(1, rtb.Document!.Blocks.Count);
		var p = Assert.IsType<Paragraph>(rtb.Document.Blocks[0]);
		Assert.Equal(1, p.Inlines.Count);
		var run = Assert.IsType<Run>(p.Inlines[0]);
		Assert.Equal("hello", run.Text);
	}

	[Fact]
	public void AppendText_twice_creates_two_paragraphs()
	{
		var rtb = new RichTextBox();
		rtb.AppendText("first");
		rtb.AppendText("second");
		Assert.Equal(2, rtb.Document!.Blocks.Count);
	}

	// ── Selection.Text ────────────────────────────────────────────────────────

	[Fact]
	public void Selection_Text_returns_document_text()
	{
		var rtb = new RichTextBox();
		var doc = new FlowDocument();
		var p = new Paragraph(); p.Inlines.Add(new Run { Text = "hello" });
		doc.Blocks.Add(p);
		rtb.Document = doc;

		Assert.Equal("hello", rtb.Selection.Text);
	}

	[Fact]
	public void Selection_Text_set_replaces_document()
	{
		var rtb = new RichTextBox();
		rtb.Document = new FlowDocument();
		rtb.Selection.Text = "world";

		Assert.Equal("world", rtb.Selection.Text);
	}

	[Fact]
	public void Selection_Text_roundtrip()
	{
		var rtb = new RichTextBox();
		rtb.Document = new FlowDocument();
		rtb.Selection.Text = "ping";
		Assert.Equal("ping", rtb.Selection.Text);
	}

	// ── TextRange.Text ────────────────────────────────────────────────────────

	[Fact]
	public void TextRange_Text_set_replaces_content()
	{
		var rtb = new RichTextBox();
		rtb.Document = new FlowDocument();
		var range = new TextRange(rtb);
		range.Text = "content";
		Assert.Equal("content", range.Text);
	}

	[Fact]
	public void TextRange_Text_empty_string_clears_content()
	{
		var rtb = new RichTextBox();
		rtb.AppendText("something");
		var range = new TextRange(rtb);
		range.Text = string.Empty;
		Assert.Equal(string.Empty, range.Text);
	}

	// ── IsReadOnly ────────────────────────────────────────────────────────────

	[Fact]
	public void IsReadOnly_false_by_default()
	{
		var rtb = new RichTextBox();
		Assert.False(rtb.IsReadOnly);
	}

	[Fact]
	public void IsReadOnly_set_true_stored()
	{
		var rtb = new RichTextBox();
		rtb.IsReadOnly = true;
		Assert.True(rtb.IsReadOnly);
	}

	[Fact]
	public void IsReadOnly_toggle_roundtrip()
	{
		var rtb = new RichTextBox();
		rtb.IsReadOnly = true;
		Assert.True(rtb.IsReadOnly);
		rtb.IsReadOnly = false;
		Assert.False(rtb.IsReadOnly);
	}

	// ── GetAllText ────────────────────────────────────────────────────────────

	[Fact]
	public void Multi_paragraph_document_GetAllText_joined_by_newline()
	{
		var doc = new FlowDocument();
		var p1 = new Paragraph(); p1.Inlines.Add(new Run { Text = "Line1" });
		var p2 = new Paragraph(); p2.Inlines.Add(new Run { Text = "Line2" });
		doc.Blocks.Add(p1);
		doc.Blocks.Add(p2);

		Assert.Equal("Line1\nLine2", doc.GetAllText());
	}
}