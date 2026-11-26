using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="TextBlock.Inlines"/> model and Pango markup generation.
/// No GTK loop required; <see cref="TextBlock.BuildMarkup"/> is an internal method.
/// </summary>
public class TextBlockInlineTests
{
	// ── Text ↔ Inlines interplay ─────────────────────────────────────────────────

	[Fact]
	public void Setting_Text_creates_one_Run()
	{
		var tb = new TextBlock { Text = "hello" };
		Assert.Equal(1, tb.Inlines.Count);
		var run = Assert.IsType<Run>(tb.Inlines[0]);
		Assert.Equal("hello", run.Text);
	}

	[Fact]
	public void Setting_Text_to_empty_clears_inlines()
	{
		var tb = new TextBlock { Text = "hi" };
		tb.Text = string.Empty;
		Assert.Equal(0, tb.Inlines.Count);
	}

	[Fact]
	public void Adding_rich_inline_clears_Text()
	{
		var tb = new TextBlock { Text = "plain" };
		tb.Inlines.Clear();
		tb.Inlines.Add(new Bold { Inlines = { new Run { Text = "bold" } } });
		Assert.Equal(string.Empty, tb.Text);
	}

	[Fact]
	public void Adding_plain_Run_updates_Text()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "simple" });
		Assert.Equal("simple", tb.Text);
	}

	// ── Markup generation ─────────────────────────────────────────────────────────

	[Fact]
	public void Run_generates_plain_text_no_tags()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "hello" });
		// A plain Run with no formatting has no Pango tags.
		Assert.Equal("hello", tb.BuildMarkup());
	}

	[Fact]
	public void Bold_wraps_in_b_tags()
	{
		var tb = new TextBlock();
		var bold = new Bold();
		bold.Inlines.Add(new Run { Text = "strong" });
		tb.Inlines.Add(bold);
		Assert.Equal("<b>strong</b>", tb.BuildMarkup());
	}

	[Fact]
	public void Italic_wraps_in_i_tags()
	{
		var tb = new TextBlock();
		var italic = new Italic();
		italic.Inlines.Add(new Run { Text = "em" });
		tb.Inlines.Add(italic);
		Assert.Equal("<i>em</i>", tb.BuildMarkup());
	}

	[Fact]
	public void Underline_wraps_in_u_tags()
	{
		var tb = new TextBlock();
		var ul = new Underline();
		ul.Inlines.Add(new Run { Text = "ul" });
		tb.Inlines.Add(ul);
		Assert.Equal("<u>ul</u>", tb.BuildMarkup());
	}

	[Fact]
	public void LineBreak_emits_newline()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "a" });
		tb.Inlines.Add(new LineBreak());
		tb.Inlines.Add(new Run { Text = "b" });
		Assert.Equal("a\nb", tb.BuildMarkup());
	}

	[Fact]
	public void Run_with_Foreground_emits_span_foreground()
	{
		var tb = new TextBlock();
		var run = new Run
		{
			Text = "red",
			Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x00, 0x00))
		};
		tb.Inlines.Add(run);
		Assert.Contains("<span foreground=\"#FF0000\">", tb.BuildMarkup());
		Assert.Contains("red", tb.BuildMarkup());
		Assert.Contains("</span>", tb.BuildMarkup());
	}

	[Fact]
	public void Hyperlink_with_NavigateUri_emits_a_tag()
	{
		var tb = new TextBlock();
		var link = new Hyperlink { NavigateUri = new Uri("https://example.com") };
		link.Inlines.Add(new Run { Text = "click" });
		tb.Inlines.Add(link);
		var markup = tb.BuildMarkup();
		Assert.Contains("<a href=\"https://example.com/\"", markup);
		Assert.Contains("click", markup);
		Assert.Contains("</a>", markup);
	}

	[Fact]
	public void Hyperlink_without_NavigateUri_emits_underline()
	{
		var tb = new TextBlock();
		var link = new Hyperlink();
		link.Inlines.Add(new Run { Text = "link" });
		tb.Inlines.Add(link);
		var markup = tb.BuildMarkup();
		Assert.Contains("<u>", markup);
		Assert.DoesNotContain("<a ", markup);
	}
}