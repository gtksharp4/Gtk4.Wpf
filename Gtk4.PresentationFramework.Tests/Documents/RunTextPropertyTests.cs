using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;
using DRun = System.Windows.Documents.Run;

namespace Gtk4.PresentationFramework.Tests.Documents;

public class RunTextPropertyTests : WpfGtkTestBase
{
	static Gtk.Label LabelOf(FlowDocumentScrollViewer v)
	{
		var child = ((Gtk.ScrolledWindow)v.Widget).Child;
		return (Gtk.Label)(child is Gtk.Viewport vp ? vp.Child! : child!);
	}

	[Fact]
	public void Text_default_is_empty_and_metadata_matches_WPF()
	{
		var meta = (FrameworkPropertyMetadata)DRun.TextProperty.GetMetadata(typeof(Run));
		Assert.Equal(string.Empty, meta.DefaultValue);
		Assert.True(meta.BindsTwoWayByDefault);
		Assert.False(meta.AffectsMeasure);
		Assert.Equal(string.Empty, new Run().Text);
		Assert.Equal(typeof(string), DRun.TextProperty.PropertyType);
	}

	[Fact]
	public void Text_null_is_coerced_to_empty()
	{
		var r = new Run("x");
		r.Text = null!;
		Assert.Equal(string.Empty, r.Text);
		Assert.Equal(string.Empty, new Run(null!).Text);
	}

	[Fact]
	public void Text_is_backed_by_the_dependency_property()
	{
		var r = new Run("a");
		Assert.Equal("a", r.GetValue(DRun.TextProperty));
		r.SetValue(DRun.TextProperty, "b");
		Assert.Equal("b", r.Text);
	}

	[Fact]
	public void Text_change_rerenders_the_hosting_TextBlock() => Run(() =>
	{
		var tb = new TextBlock();
		var run = new Run("one");
		tb.Inlines.Add(run);
		var label = (Gtk.Label)tb.Widget;
		Assert.Equal("one", label.Text);
		run.Text = "two";
		Assert.Equal("two", label.Text);
		Assert.Equal("two", tb.Text);
	});

	[Fact]
	public void Text_change_rerenders_the_hosting_FlowDocument_viewer() => Run(() =>
	{
		var run = new Run("hello");
		var doc = new FlowDocument(new Paragraph(run));
		var viewer = new FlowDocumentScrollViewer { Document = doc };
		var label = LabelOf(viewer);
		Assert.Equal("hello", label.Text);
		run.Text = "bye";
		Assert.Equal("bye", label.Text);
	});

	[Fact]
	public void Text_change_keeps_text_tree_offsets_consistent()
	{
		var run = new Run("abc");
		var doc = new FlowDocument(new Paragraph(run));
		int before = new TextRange(doc.ContentStart, doc.ContentEnd).Text.Length;
		run.Text = "abcdef";
		Assert.Equal(before + 3, new TextRange(doc.ContentStart, doc.ContentEnd).Text.Length);
		Assert.Equal("abcdef", new TextRange(doc.ContentStart, doc.ContentEnd).Text.TrimEnd('\r', '\n'));
	}

	[Fact]
	public void Run_ctor_with_insertion_position_inserts_into_the_paragraph()
	{
		var first = new Run("a");
		var para = new Paragraph(first);
		var doc = new FlowDocument(para);
		var inserted = new Run("b", doc.ContentStart.GetPositionAtOffset(4)!);
		Assert.Equal(2, para.Inlines.Count);
		Assert.Same(inserted, para.Inlines[1]);
		Assert.Same(para, inserted.Parent);
	}

	[Fact]
	public void Run_ctor_at_the_document_start_creates_a_paragraph_before_the_first_block()
	{
		var doc = new FlowDocument(new Paragraph(new Run("a")));
		var inserted = new Run("b", doc.ContentStart);
		Assert.Equal(2, doc.Blocks.Count);
		var first = Assert.IsType<Paragraph>(doc.Blocks[0]);
		Assert.Same(inserted, Assert.Single(first.Inlines));
		Assert.Same(first, inserted.Parent);
		Assert.Equal("a", ((Run)((Paragraph)doc.Blocks[1]).Inlines[0]).Text);
	}
}

public class TextBlockInlineCtorTests : WpfGtkTestBase
{
	[Fact]
	public void TextBlock_ctor_with_an_inline_adds_it_and_parents_it() => Run(() =>
	{
		var run = new System.Windows.Documents.Run("hi");
		var tb = new TextBlock(run);
		Assert.Equal(1, tb.Inlines.Count);
		Assert.Same(tb, run.Parent);
		Assert.Equal("hi", ((Gtk.Label)tb.Widget).Text);
		Assert.Throws<ArgumentNullException>(() => new TextBlock(null!));
	});
}
