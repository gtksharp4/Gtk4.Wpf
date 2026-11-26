using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Unit tests for <see cref="TextDecoration"/>, <see cref="TextDecorationCollection"/>,
/// <see cref="TextDecorations"/> (static), <see cref="TextDecorationMarkup"/>,
/// and the <see cref="TextBlock"/>/<see cref="Run"/> TextDecorations properties.
/// No GTK loop required; <see cref="TextBlock.BuildMarkup"/> is accessible via InternalsVisibleTo.
/// </summary>
public class TextDecorationTests
{
	// ── TextDecorations static collections ───────────────────────────────────────

	[Fact]
	public void Underline_collection_is_non_null()
	{
		Assert.NotNull(TextDecorations.Underline);
	}

	[Fact]
	public void Underline_collection_has_one_Underline_decoration()
	{
		var col = TextDecorations.Underline;
		Assert.Equal(1, col.Count);
		Assert.Equal(TextDecorationLocation.Underline, col[0].Location);
	}

	[Fact]
	public void Strikethrough_collection_is_non_null()
	{
		Assert.NotNull(TextDecorations.Strikethrough);
	}

	[Fact]
	public void Strikethrough_collection_has_one_Strikethrough_decoration()
	{
		var col = TextDecorations.Strikethrough;
		Assert.Equal(1, col.Count);
		Assert.Equal(TextDecorationLocation.Strikethrough, col[0].Location);
	}

	[Fact]
	public void OverLine_collection_has_one_OverLine_decoration()
	{
		var col = TextDecorations.OverLine;
		Assert.Equal(1, col.Count);
		Assert.Equal(TextDecorationLocation.OverLine, col[0].Location);
	}

	[Fact]
	public void Baseline_collection_has_one_Baseline_decoration()
	{
		var col = TextDecorations.Baseline;
		Assert.Equal(1, col.Count);
		Assert.Equal(TextDecorationLocation.Baseline, col[0].Location);
	}

	// ── TextDecorationMarkup.ToPangoAttributes ────────────────────────────────────

	[Fact]
	public void ToPangoAttributes_null_returns_empty()
	{
		Assert.Equal(string.Empty, TextDecorationMarkup.ToPangoAttributes(null));
	}

	[Fact]
	public void ToPangoAttributes_empty_collection_returns_empty()
	{
		Assert.Equal(string.Empty, TextDecorationMarkup.ToPangoAttributes(new TextDecorationCollection()));
	}

	[Fact]
	public void ToPangoAttributes_Underline_contains_underline_single()
	{
		var attrs = TextDecorationMarkup.ToPangoAttributes(TextDecorations.Underline);
		Assert.Contains("underline=\"single\"", attrs);
	}

	[Fact]
	public void ToPangoAttributes_Strikethrough_contains_strikethrough_true()
	{
		var attrs = TextDecorationMarkup.ToPangoAttributes(TextDecorations.Strikethrough);
		Assert.Contains("strikethrough=\"true\"", attrs);
	}

	[Fact]
	public void ToPangoAttributes_OverLine_contains_overline_single()
	{
		var attrs = TextDecorationMarkup.ToPangoAttributes(TextDecorations.OverLine);
		Assert.Contains("overline=\"single\"", attrs);
	}

	[Fact]
	public void ToPangoAttributes_Baseline_maps_to_underline_single()
	{
		// Pango has no baseline decoration; closest approximation is underline.
		var attrs = TextDecorationMarkup.ToPangoAttributes(TextDecorations.Baseline);
		Assert.Contains("underline=\"single\"", attrs);
	}

	[Fact]
	public void ToPangoAttributes_combined_Underline_and_Strikethrough_contains_both()
	{
		var col = new TextDecorationCollection
		{
			new TextDecoration(TextDecorationLocation.Underline),
			new TextDecoration(TextDecorationLocation.Strikethrough),
		};
		var attrs = TextDecorationMarkup.ToPangoAttributes(col);
		Assert.Contains("underline=\"single\"", attrs);
		Assert.Contains("strikethrough=\"true\"", attrs);
	}

	[Fact]
	public void ToPangoAttributes_Underline_does_not_contain_strikethrough()
	{
		var attrs = TextDecorationMarkup.ToPangoAttributes(TextDecorations.Underline);
		Assert.DoesNotContain("strikethrough", attrs);
	}

	// ── TextDecoration get/set ────────────────────────────────────────────────────

	[Fact]
	public void TextDecoration_default_location_is_Underline()
	{
		var dec = new TextDecoration();
		Assert.Equal(TextDecorationLocation.Underline, dec.Location);
	}

	[Fact]
	public void TextDecoration_location_roundtrip()
	{
		var dec = new TextDecoration(TextDecorationLocation.Strikethrough);
		Assert.Equal(TextDecorationLocation.Strikethrough, dec.Location);
	}

	// ── Run.TextDecorations property ─────────────────────────────────────────────

	[Fact]
	public void Run_TextDecorations_roundtrip()
	{
		var run = new Run();
		run.TextDecorations = TextDecorations.Underline;
		Assert.Same(TextDecorations.Underline, run.TextDecorations);
	}

	[Fact]
	public void Run_TextDecorations_default_is_null()
	{
		var run = new Run();
		Assert.Null(run.TextDecorations);
	}

	// ── TextBlock.TextDecorations property ───────────────────────────────────────

	[Fact]
	public void TextBlock_TextDecorations_roundtrip()
	{
		var tb = new TextBlock();
		tb.TextDecorations = TextDecorations.Strikethrough;
		Assert.Same(TextDecorations.Strikethrough, tb.TextDecorations);
	}

	[Fact]
	public void TextBlock_TextDecorations_default_is_null()
	{
		var tb = new TextBlock();
		Assert.Null(tb.TextDecorations);
	}

	// ── Pango markup generation ───────────────────────────────────────────────────

	[Fact]
	public void Run_with_Underline_decoration_emits_underline_in_markup()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "u", TextDecorations = TextDecorations.Underline });
		var markup = tb.BuildMarkup();
		Assert.Contains("underline=\"single\"", markup);
		Assert.Contains("u", markup);
	}

	[Fact]
	public void Run_with_Strikethrough_decoration_emits_strikethrough_in_markup()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "s", TextDecorations = TextDecorations.Strikethrough });
		var markup = tb.BuildMarkup();
		Assert.Contains("strikethrough=\"true\"", markup);
	}

	[Fact]
	public void Run_with_no_decoration_emits_no_decoration_attrs()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "plain" });
		var markup = tb.BuildMarkup();
		Assert.DoesNotContain("underline", markup);
		Assert.DoesNotContain("strikethrough", markup);
	}

	[Fact]
	public void TextBlock_TextDecorations_wraps_all_inlines()
	{
		var tb = new TextBlock { TextDecorations = TextDecorations.Underline };
		tb.Inlines.Add(new Run { Text = "block" });
		var markup = tb.BuildMarkup();
		Assert.Contains("underline=\"single\"", markup);
		Assert.Contains("block", markup);
	}

	[Fact]
	public void Setting_decorations_and_building_markup_does_not_throw()
	{
		var tb = new TextBlock();
		tb.TextDecorations = TextDecorations.Strikethrough;
		tb.Inlines.Add(new Run { Text = "no crash" });
		var ex = Record.Exception(() => tb.BuildMarkup());
		Assert.Null(ex);
	}

	[Fact]
	public void Run_Underline_decoration_combined_with_Foreground_emits_both_spans()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run
		{
			Text = "both",
			Foreground = new System.Windows.Media.SolidColorBrush(
				System.Windows.Media.Color.FromRgb(0xFF, 0, 0)),
			TextDecorations = TextDecorations.Underline,
		});
		var markup = tb.BuildMarkup();
		Assert.Contains("foreground=", markup);
		Assert.Contains("underline=\"single\"", markup);
	}
}
