using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Tests for the Typography → Pango <c>font_features</c> mapping (<c>TypographyPango</c>)
/// and the TextBlock markup integration.
/// </summary>
public class TypographyFontFeaturesTests
{
	// ── BuildFontFeatures: individual tokens ──────────────────────────────────────

	[Fact]
	public void BuildFontFeatures_SmallCaps_contains_smcp()
	{
		var tb = new TextBlock();
		Typography.SetCapitals(tb, FontCapitals.SmallCaps);
		Assert.Contains("smcp=1", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_AllSmallCaps_contains_c2sc_and_smcp()
	{
		var tb = new TextBlock();
		Typography.SetCapitals(tb, FontCapitals.AllSmallCaps);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("c2sc=1", features);
		Assert.Contains("smcp=1", features);
	}

	[Fact]
	public void BuildFontFeatures_OldStyle_contains_onum()
	{
		var tb = new TextBlock();
		Typography.SetNumeralStyle(tb, FontNumeralStyle.OldStyle);
		Assert.Contains("onum=1", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_Lining_contains_lnum()
	{
		var tb = new TextBlock();
		Typography.SetNumeralStyle(tb, FontNumeralStyle.Lining);
		Assert.Contains("lnum=1", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_SlashedFraction_contains_frac()
	{
		var tb = new TextBlock();
		Typography.SetFraction(tb, FontFraction.Slashed);
		Assert.Contains("frac=1", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_StackedFraction_contains_afrc()
	{
		var tb = new TextBlock();
		Typography.SetFraction(tb, FontFraction.Stacked);
		Assert.Contains("afrc=1", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_StandardLigatures_false_contains_liga0()
	{
		var tb = new TextBlock();
		Typography.SetStandardLigatures(tb, false);
		Assert.Contains("liga=0", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_Kerning_false_contains_kern0()
	{
		var tb = new TextBlock();
		Typography.SetKerning(tb, false);
		Assert.Contains("kern=0", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_Superscript_contains_sups()
	{
		var tb = new TextBlock();
		Typography.SetVariants(tb, FontVariants.Superscript);
		Assert.Contains("sups=1", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_Subscript_contains_subs()
	{
		var tb = new TextBlock();
		Typography.SetVariants(tb, FontVariants.Subscript);
		Assert.Contains("subs=1", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void Variants_roundtrip_Ordinal_emits_ordn()
	{
		var tb = new TextBlock();
		Typography.SetVariants(tb, FontVariants.Ordinal);
		Assert.Contains("ordn=1", TypographyPango.BuildFontFeatures(tb));
	}

	// ── BuildFontFeatures: combined / suppression ─────────────────────────────────

	[Fact]
	public void BuildFontFeatures_combined_SmallCaps_OldStyle_Slashed_has_all_tokens()
	{
		var tb = new TextBlock();
		Typography.SetCapitals(tb, FontCapitals.SmallCaps);
		Typography.SetNumeralStyle(tb, FontNumeralStyle.OldStyle);
		Typography.SetFraction(tb, FontFraction.Slashed);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("smcp=1", features);
		Assert.Contains("onum=1", features);
		Assert.Contains("frac=1", features);
	}

	[Fact]
	public void BuildFontFeatures_all_defaults_returns_empty()
	{
		Assert.Equal(string.Empty, TypographyPango.BuildFontFeatures(new TextBlock()));
	}

	[Fact]
	public void BuildFontFeatures_StandardLigatures_true_does_not_emit_liga()
	{
		var tb = new TextBlock();
		Typography.SetStandardLigatures(tb, true);
		Assert.DoesNotContain("liga", TypographyPango.BuildFontFeatures(tb));
	}

	[Fact]
	public void BuildFontFeatures_Kerning_true_does_not_emit_kern()
	{
		var tb = new TextBlock();
		Typography.SetKerning(tb, true);
		Assert.DoesNotContain("kern", TypographyPango.BuildFontFeatures(tb));
	}

	// ── TextBlock.BuildMarkup integration ─────────────────────────────────────────

	[Fact]
	public void BuildMarkup_with_SmallCaps_contains_font_features_attribute()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "hello" });
		Typography.SetCapitals(tb, FontCapitals.SmallCaps);
		string markup = tb.BuildMarkup();
		Assert.Contains("font_features=", markup);
		Assert.Contains("smcp=1", markup);
	}

	[Fact]
	public void BuildMarkup_with_OldStyle_contains_onum_in_font_features()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "123" });
		Typography.SetNumeralStyle(tb, FontNumeralStyle.OldStyle);
		string markup = tb.BuildMarkup();
		Assert.Contains("font_features=", markup);
		Assert.Contains("onum=1", markup);
	}

	[Fact]
	public void BuildMarkup_no_typography_does_not_contain_font_features()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "plain", FontWeight = FontWeights.Bold });
		Assert.DoesNotContain("font_features=", tb.BuildMarkup());
	}

	[Fact]
	public void BuildMarkup_Typography_does_not_throw_headless()
	{
		var tb = new TextBlock();
		tb.Inlines.Add(new Run { Text = "test" });
		Typography.SetCapitals(tb, FontCapitals.SmallCaps);
		Typography.SetNumeralStyle(tb, FontNumeralStyle.OldStyle);
		Typography.SetFraction(tb, FontFraction.Slashed);
		Typography.SetStandardLigatures(tb, false);
		Typography.SetKerning(tb, false);
		Typography.SetVariants(tb, FontVariants.Superscript);
		Assert.NotNull(tb.BuildMarkup());
	}
}
