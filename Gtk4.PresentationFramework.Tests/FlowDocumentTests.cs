using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;
using WpfListItem = System.Windows.Documents.ListItem;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for FlowDocument, block hierarchy, and markup/plain-text rendering.
/// No GTK loop required.
/// </summary>
public class FlowDocumentTests
{
	// ── Block hierarchy ──────────────────────────────────────────────────────

	[Fact]
	public void Paragraph_holds_inlines()
	{
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = "hello" });
		Assert.Equal(1, p.Inlines.Count);
	}

	[Fact]
	public void Section_holds_blocks()
	{
		var s = new Section();
		s.Blocks.Add(new Paragraph());
		s.Blocks.Add(new Paragraph());
		Assert.Equal(2, s.Blocks.Count);
	}

	[Fact]
	public void List_holds_listItems()
	{
		var list = new List();
		list.ListItems.Add(new WpfListItem());
		list.ListItems.Add(new WpfListItem());
		Assert.Equal(2, list.ListItems.Count);
	}

	[Fact]
	public void ListItem_holds_blocks()
	{
		var item = new WpfListItem();
		item.Blocks.Add(new Paragraph());
		Assert.Equal(1, item.Blocks.Count);
	}

	[Fact]
	public void BlockUIContainer_holds_UIElement()
	{
		var tb = new System.Windows.Controls.TextBlock { Text = "hi" };
		var c = new BlockUIContainer { Child = tb };
		Assert.Same(tb, c.Child);
	}

	// ── FlowDocument BuildPlainText ──────────────────────────────────────────

	[Fact]
	public void Two_paragraphs_build_plaintext_separated_by_blank_line()
	{
		var doc = new FlowDocument();
		var p1 = new Paragraph(); p1.Inlines.Add(new Run { Text = "First" });
		var p2 = new Paragraph(); p2.Inlines.Add(new Run { Text = "Second" });
		doc.Blocks.Add(p1);
		doc.Blocks.Add(p2);

		Assert.Equal("First\n\nSecond", doc.BuildPlainText());
	}

	[Fact]
	public void Paragraph_with_bold_builds_plaintext_without_tags()
	{
		var doc = new FlowDocument();
		var p = new Paragraph();
		var bold = new Bold(); bold.Inlines.Add(new Run { Text = "strong" });
		p.Inlines.Add(bold);
		doc.Blocks.Add(p);

		Assert.Equal("strong", doc.BuildPlainText());
	}

	[Fact]
	public void Empty_doc_builds_empty_plaintext()
	{
		var doc = new FlowDocument();
		Assert.Equal(string.Empty, doc.BuildPlainText());
	}

	// ── FlowDocument BuildMarkup ─────────────────────────────────────────────

	[Fact]
	public void Two_paragraphs_build_markup_separated_by_double_newline()
	{
		var doc = new FlowDocument();
		var p1 = new Paragraph(); p1.Inlines.Add(new Run { Text = "A" });
		var p2 = new Paragraph(); p2.Inlines.Add(new Run { Text = "B" });
		doc.Blocks.Add(p1);
		doc.Blocks.Add(p2);

		Assert.Equal("A\n\nB", doc.BuildMarkup());
	}

	[Fact]
	public void Paragraph_with_bold_run_builds_markup_with_b_tags()
	{
		var doc = new FlowDocument();
		var p = new Paragraph();
		var bold = new Bold(); bold.Inlines.Add(new Run { Text = "hello" });
		p.Inlines.Add(new Run { Text = "plain " });
		p.Inlines.Add(bold);
		doc.Blocks.Add(p);

		Assert.Equal("plain <b>hello</b>", doc.BuildMarkup());
	}

	[Fact]
	public void Markup_escapes_special_chars_in_run()
	{
		var doc = new FlowDocument();
		var p = new Paragraph();
		p.Inlines.Add(new Run { Text = "a & b < c > d" });
		doc.Blocks.Add(p);

		Assert.Equal("a &amp; b &lt; c &gt; d", doc.BuildMarkup());
	}

	// ── List rendering ───────────────────────────────────────────────────────

	[Fact]
	public void Disc_list_renders_bullet_markers()
	{
		var doc = new FlowDocument();
		var list = new List { MarkerStyle = TextMarkerStyle.Disc };
		var item1 = new WpfListItem();
		var p1 = new Paragraph(); p1.Inlines.Add(new Run { Text = "Alpha" });
		item1.Blocks.Add(p1);
		var item2 = new WpfListItem();
		var p2 = new Paragraph(); p2.Inlines.Add(new Run { Text = "Beta" });
		item2.Blocks.Add(p2);
		list.ListItems.Add(item1);
		list.ListItems.Add(item2);
		doc.Blocks.Add(list);

		var text = doc.BuildPlainText();
		Assert.Contains("• Alpha", text);
		Assert.Contains("• Beta", text);
	}

	[Fact]
	public void Decimal_list_renders_number_markers()
	{
		var doc = new FlowDocument();
		var list = new List { MarkerStyle = TextMarkerStyle.Decimal };
		for (int i = 1; i <= 3; i++)
		{
			var item = new WpfListItem();
			var p = new Paragraph(); p.Inlines.Add(new Run { Text = "Item" });
			item.Blocks.Add(p);
			list.ListItems.Add(item);
		}
		doc.Blocks.Add(list);

		var text = doc.BuildPlainText();
		Assert.Contains("1 Item", text);
		Assert.Contains("2 Item", text);
		Assert.Contains("3 Item", text);
	}

	// ── Section nesting ──────────────────────────────────────────────────────

	[Fact]
	public void Section_nests_blocks_in_markup()
	{
		var doc = new FlowDocument();
		var section = new Section();
		var p1 = new Paragraph(); p1.Inlines.Add(new Run { Text = "One" });
		var p2 = new Paragraph(); p2.Inlines.Add(new Run { Text = "Two" });
		section.Blocks.Add(p1);
		section.Blocks.Add(p2);
		doc.Blocks.Add(section);

		var markup = doc.BuildMarkup();
		Assert.Contains("One", markup);
		Assert.Contains("Two", markup);
	}

	// ── BlockUIContainer ─────────────────────────────────────────────────────

	[Fact]
	public void BlockUIContainer_emits_nothing_in_markup()
	{
		var doc = new FlowDocument();
		var container = new BlockUIContainer
		{
			Child = new System.Windows.Controls.TextBlock { Text = "widget" }
		};
		doc.Blocks.Add(container);
		Assert.Equal(string.Empty, doc.BuildMarkup());
	}

	[Fact]
	public void BlockUIContainer_emits_nothing_in_plaintext()
	{
		var doc = new FlowDocument();
		doc.Blocks.Add(new BlockUIContainer
		{
			Child = new System.Windows.Controls.Button()
		});
		Assert.Equal(string.Empty, doc.BuildPlainText());
	}

	// ── FlowDocument properties ──────────────────────────────────────────────

	[Fact]
	public void FlowDocument_stores_FontFamily()
	{
		var doc = new FlowDocument { FontFamily = new FontFamily("Segoe UI") };
		Assert.Equal("Segoe UI", doc.FontFamily.Source);
	}

	[Fact]
	public void FlowDocument_stores_FontSize()
	{
		var doc = new FlowDocument { FontSize = 14.0 };
		Assert.Equal(14.0, doc.FontSize);
	}

	[Fact]
	public void FlowDocument_stores_TextAlignment()
	{
		var doc = new FlowDocument { TextAlignment = TextAlignment.Center };
		Assert.Equal(TextAlignment.Center, doc.TextAlignment);
	}
}