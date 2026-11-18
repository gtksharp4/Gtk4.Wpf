using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>
/// Tests for <see cref="Typography"/> attached DPs and <see cref="TypographyPango"/>.
/// All tests are headless — no GTK widget is created.
/// </summary>
public class TypographyTests
{
	// ── Default values ────────────────────────────────────────────────────────────

	[Fact]
	public void Capitals_default_is_Normal()
	{
		var tb = new TextBlock();
		Assert.Equal(FontCapitals.Normal, Typography.GetCapitals(tb));
	}

	[Fact]
	public void NumeralStyle_default_is_Normal()
	{
		var tb = new TextBlock();
		Assert.Equal(FontNumeralStyle.Normal, Typography.GetNumeralStyle(tb));
	}

	[Fact]
	public void Fraction_default_is_Normal()
	{
		var tb = new TextBlock();
		Assert.Equal(FontFraction.Normal, Typography.GetFraction(tb));
	}

	[Fact]
	public void StandardLigatures_default_is_true()
	{
		var tb = new TextBlock();
		Assert.True(Typography.GetStandardLigatures(tb));
	}

	[Fact]
	public void Kerning_default_is_true()
	{
		var tb = new TextBlock();
		Assert.True(Typography.GetKerning(tb));
	}

	[Fact]
	public void Variants_default_is_Normal()
	{
		var tb = new TextBlock();
		Assert.Equal(FontVariants.Normal, Typography.GetVariants(tb));
	}

	// ── Round-trip Get/Set ────────────────────────────────────────────────────────

	[Fact]
	public void Capitals_roundtrip_SmallCaps()
	{
		var tb = new TextBlock();
		Typography.SetCapitals(tb, FontCapitals.SmallCaps);
		Assert.Equal(FontCapitals.SmallCaps, Typography.GetCapitals(tb));
	}

	[Fact]
	public void Capitals_roundtrip_AllSmallCaps()
	{
		var tb = new TextBlock();
		Typography.SetCapitals(tb, FontCapitals.AllSmallCaps);
		Assert.Equal(FontCapitals.AllSmallCaps, Typography.GetCapitals(tb));
	}

	[Fact]
	public void NumeralStyle_roundtrip_OldStyle()
	{
		var tb = new TextBlock();
		Typography.SetNumeralStyle(tb, FontNumeralStyle.OldStyle);
		Assert.Equal(FontNumeralStyle.OldStyle, Typography.GetNumeralStyle(tb));
	}

	[Fact]
	public void Fraction_roundtrip_Slashed()
	{
		var tb = new TextBlock();
		Typography.SetFraction(tb, FontFraction.Slashed);
		Assert.Equal(FontFraction.Slashed, Typography.GetFraction(tb));
	}

	[Fact]
	public void StandardLigatures_roundtrip_false()
	{
		var tb = new TextBlock();
		Typography.SetStandardLigatures(tb, false);
		Assert.False(Typography.GetStandardLigatures(tb));
	}

	[Fact]
	public void Kerning_roundtrip_false()
	{
		var tb = new TextBlock();
		Typography.SetKerning(tb, false);
		Assert.False(Typography.GetKerning(tb));
	}

	[Fact]
	public void Variants_roundtrip_Superscript()
	{
		var tb = new TextBlock();
		Typography.SetVariants(tb, FontVariants.Superscript);
		Assert.Equal(FontVariants.Superscript, Typography.GetVariants(tb));
	}

	// ── BuildFontFeatures: individual tokens ──────────────────────────────────────

	[Fact]
	public void BuildFontFeatures_SmallCaps_contains_smcp()
	{
		var tb = new TextBlock();
		Typography.SetCapitals(tb, FontCapitals.SmallCaps);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("smcp=1", features);
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
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("onum=1", features);
	}

	[Fact]
	public void BuildFontFeatures_Lining_contains_lnum()
	{
		var tb = new TextBlock();
		Typography.SetNumeralStyle(tb, FontNumeralStyle.Lining);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("lnum=1", features);
	}

	[Fact]
	public void BuildFontFeatures_SlashedFraction_contains_frac()
	{
		var tb = new TextBlock();
		Typography.SetFraction(tb, FontFraction.Slashed);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("frac=1", features);
	}

	[Fact]
	public void BuildFontFeatures_StackedFraction_contains_afrc()
	{
		var tb = new TextBlock();
		Typography.SetFraction(tb, FontFraction.Stacked);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("afrc=1", features);
	}

	[Fact]
	public void BuildFontFeatures_StandardLigatures_false_contains_liga0()
	{
		var tb = new TextBlock();
		Typography.SetStandardLigatures(tb, false);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("liga=0", features);
	}

	[Fact]
	public void BuildFontFeatures_Kerning_false_contains_kern0()
	{
		var tb = new TextBlock();
		Typography.SetKerning(tb, false);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("kern=0", features);
	}

	[Fact]
	public void BuildFontFeatures_Superscript_contains_sups()
	{
		var tb = new TextBlock();
		Typography.SetVariants(tb, FontVariants.Superscript);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("sups=1", features);
	}

	[Fact]
	public void BuildFontFeatures_Subscript_contains_subs()
	{
		var tb = new TextBlock();
		Typography.SetVariants(tb, FontVariants.Subscript);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("subs=1", features);
	}

	// ── BuildFontFeatures: combined ───────────────────────────────────────────────

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
		var tb = new TextBlock();
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Equal(string.Empty, features);
	}

	[Fact]
	public void BuildFontFeatures_StandardLigatures_true_does_not_emit_liga()
	{
		var tb = new TextBlock();
		Typography.SetStandardLigatures(tb, true);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.DoesNotContain("liga", features);
	}

	[Fact]
	public void BuildFontFeatures_Kerning_true_does_not_emit_kern()
	{
		var tb = new TextBlock();
		Typography.SetKerning(tb, true);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.DoesNotContain("kern", features);
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
		string markup = tb.BuildMarkup();
		Assert.DoesNotContain("font_features=", markup);
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
		// No GTK label — BuildMarkup is pure markup building.
		string markup = tb.BuildMarkup();
		Assert.NotNull(markup);
	}

	// ── GetSet on plain DependencyObject ─────────────────────────────────────────

	[Fact]
	public void Capitals_roundtrip_on_plain_DependencyObject()
	{
		// Typography DPs are attached — they work on any DependencyObject.
		var obj = new TextBlock();
		Typography.SetCapitals(obj, FontCapitals.Titling);
		Assert.Equal(FontCapitals.Titling, Typography.GetCapitals(obj));
	}

	[Fact]
	public void Variants_roundtrip_Ordinal_on_TextBlock()
	{
		var tb = new TextBlock();
		Typography.SetVariants(tb, FontVariants.Ordinal);
		string features = TypographyPango.BuildFontFeatures(tb);
		Assert.Contains("ordn=1", features);
	}
}
