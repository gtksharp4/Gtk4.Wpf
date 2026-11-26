using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

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


	// ── GetSet on plain DependencyObject ─────────────────────────────────────────

	[Fact]
	public void Capitals_roundtrip_on_plain_DependencyObject()
	{
		// Typography DPs are attached — they work on any DependencyObject.
		var obj = new TextBlock();
		Typography.SetCapitals(obj, FontCapitals.Titling);
		Assert.Equal(FontCapitals.Titling, Typography.GetCapitals(obj));
	}
}
