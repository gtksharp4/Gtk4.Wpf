using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class TextBlockContentPointerTests : WpfGtkTestBase
{
	[Fact]
	public void A_non_empty_block_has_ContentStart_before_ContentEnd()
	{
		var tb = new TextBlock { Text = "hello" };
		Assert.True(tb.ContentStart.CompareTo(tb.ContentEnd) < 0);
		Assert.True(tb.ContentStart.IsInSameDocument(tb.ContentEnd));
	}

	[Fact]
	public void An_empty_block_has_equal_pointers()
	{
		var tb = new TextBlock();
		Assert.Equal(0, tb.ContentStart.CompareTo(tb.ContentEnd));
		Assert.Equal(string.Empty, new TextRange(tb.ContentStart, tb.ContentEnd).Text);
	}

	[Fact]
	public void The_range_text_equals_Text()
	{
		var tb = new TextBlock { Text = "hello world" };
		Assert.Equal("hello world", new TextRange(tb.ContentStart, tb.ContentEnd).Text);
	}

	[Fact]
	public void The_range_follows_later_Text_and_Inlines_edits()
	{
		var tb = new TextBlock { Text = "one" };
		_ = tb.ContentStart;
		tb.Text = "two";
		Assert.Equal("two", new TextRange(tb.ContentStart, tb.ContentEnd).Text);
		tb.Inlines.Add(new Run(" more"));
		Assert.Equal("two more", new TextRange(tb.ContentStart, tb.ContentEnd).Text);
	}

	[Fact]
	public void Inlines_keep_the_TextBlock_as_their_parent_after_pointers_are_taken()
	{
		var tb = new TextBlock { Text = "hello" };
		_ = tb.ContentEnd;
		Assert.Same(tb, tb.Inlines.FirstInline().Parent);
	}

	[Fact]
	public void ApplyPropertyValue_reaches_the_inline_of_a_Text_block()
	{
		var tb = new TextBlock { Text = "hello" };
		new TextRange(tb.ContentStart, tb.ContentEnd).ApplyPropertyValue(TextElement.FontFamilyProperty,
			new FontFamily("Monospace"));
		Assert.Equal("Monospace", tb.Inlines.FirstInline().FontFamily.Source);
		Assert.Contains("font_family=\"Monospace\"", tb.BuildMarkup());
	}

	[Fact]
	public void A_partial_range_splits_the_inline_and_the_block_sees_the_split()
	{
		var tb = new TextBlock { Text = "hello world" };
		var start = tb.ContentStart;
		var range = new TextRange(start, start.GetPositionAtOffset(6)!);
		range.ApplyPropertyValue(TextElement.FontFamilyProperty, new FontFamily("Monospace"));
		Assert.Equal(2, tb.Inlines.Count);
		Assert.Equal("hello world", new TextRange(tb.ContentStart, tb.ContentEnd).Text);
		Assert.Contains("font_family=\"Monospace\"", tb.BuildMarkup());
	}

	[Fact]
	public void The_font_change_reaches_the_rendered_GTK_label() => Run(() =>
	{
		var tb = new TextBlock { Text = "hello" };
		var label = (Gtk.Label)tb.Widget;
		Assert.False(label.UseMarkup);
		new TextRange(tb.ContentStart, tb.ContentEnd).ApplyPropertyValue(TextElement.FontFamilyProperty,
			new FontFamily("Monospace"));
		Assert.True(label.UseMarkup);
		Assert.Equal("hello", label.Text);
		Assert.Contains("font_family=\"Monospace\"", tb.BuildMarkup());
	});

	sealed class DerivedBlock : TextBlock { }

	[Fact]
	public void A_TextBlock_subclass_exposes_the_pointers()
	{
		var tb = new DerivedBlock { Text = "abc" };
		Assert.Equal("abc", new TextRange(tb.ContentStart, tb.ContentEnd).Text);
	}

	[Fact(Skip = "lane B: TextPointer.Parent must map the TextBlockHost root to its TextBlock")]
	public void The_pointers_Parent_is_the_TextBlock()
	{
		var tb = new TextBlock { Text = "hello" };
		Assert.Same(tb, tb.ContentStart.Parent);
		Assert.Same(tb, tb.ContentEnd.Parent);
	}
}

static class InlineCollectionTestExtensions
{
	internal static Inline FirstInline(this InlineCollection inlines) => inlines[0];
}
