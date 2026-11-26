using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class TypographyTailTests
{
	static string Features(DependencyObject o) => TypographyPango.BuildFontFeatures(o);

	[Fact]
	public void Defaults_match_WPF_and_emit_no_features()
	{
		var o = new TextBlock();
		Assert.False(Typography.GetStylisticSet1(o));
		Assert.False(Typography.GetStylisticSet20(o));
		Assert.Equal(0, Typography.GetStandardSwashes(o));
		Assert.Equal(0, Typography.GetContextualSwashes(o));
		Assert.Equal(0, Typography.GetStylisticAlternates(o));
		Assert.Equal(0, Typography.GetAnnotationAlternates(o));
		Assert.True(Typography.GetContextualLigatures(o));
		Assert.False(Typography.GetDiscretionaryLigatures(o));
		Assert.False(Typography.GetHistoricalLigatures(o));
		Assert.True(Typography.GetContextualAlternates(o));
		Assert.False(Typography.GetHistoricalForms(o));
		Assert.False(Typography.GetCaseSensitiveForms(o));
		Assert.False(Typography.GetCapitalSpacing(o));
		Assert.False(Typography.GetSlashedZero(o));
		Assert.False(Typography.GetMathematicalGreek(o));
		Assert.False(Typography.GetEastAsianExpertForms(o));
		Assert.Equal(FontNumeralAlignment.Normal, Typography.GetNumeralAlignment(o));
		Assert.Equal(FontEastAsianWidths.Normal, Typography.GetEastAsianWidths(o));
		Assert.Equal(FontEastAsianLanguage.Normal, Typography.GetEastAsianLanguage(o));
		Assert.Equal("", Features(o));
	}

	[Fact]
	public void Property_identity_and_metadata()
	{
		Assert.Equal("StylisticSet7", Typography.StylisticSet7Property.Name);
		Assert.Equal(typeof(bool), Typography.StylisticSet7Property.PropertyType);
		Assert.Equal(typeof(int), Typography.StandardSwashesProperty.PropertyType);
		Assert.Equal(typeof(FontEastAsianWidths), Typography.EastAsianWidthsProperty.PropertyType);
		Assert.True(((FrameworkPropertyMetadata)Typography.SlashedZeroProperty.DefaultMetadata).Inherits);
		Assert.True(((FrameworkPropertyMetadata)Typography.StylisticSet20Property.DefaultMetadata).Inherits);
	}

	[Fact]
	public void Values_inherit_from_a_parent_element()
	{
		var panel = new StackPanel();
		var child = new TextBlock();
		panel.Children.Add(child);
		Typography.SetStylisticSet3(panel, true);
		Typography.SetEastAsianLanguage(panel, FontEastAsianLanguage.Simplified);
		Assert.True(Typography.GetStylisticSet3(child));
		Assert.Equal(FontEastAsianLanguage.Simplified, Typography.GetEastAsianLanguage(child));
	}

	[Fact]
	public void Negative_swashes_and_undefined_enums_are_rejected()
	{
		var o = new TextBlock();
		Assert.Throws<ArgumentException>(() => Typography.SetStandardSwashes(o, -1));
		Assert.Throws<ArgumentException>(() => Typography.SetNumeralAlignment(o, (FontNumeralAlignment)9));
		Assert.Throws<ArgumentException>(() => Typography.SetEastAsianWidths(o, (FontEastAsianWidths)99));
		Assert.Throws<ArgumentNullException>(() => Typography.GetSlashedZero(null!));
	}

	[Fact]
	public void Enums_have_the_WPF_members()
	{
		Assert.Equal(new[] { "Normal", "Proportional", "Tabular" }, Enum.GetNames(typeof(FontNumeralAlignment)));
		Assert.Equal(new[] { "Normal", "Proportional", "Full", "Half", "Third", "Quarter" },
			Enum.GetNames(typeof(FontEastAsianWidths)));
		Assert.Equal(new[] { "Normal", "Jis78", "Jis83", "Jis90", "Jis04", "HojoKanji", "NlcKanji", "Simplified",
			"Traditional", "TraditionalNames" }, Enum.GetNames(typeof(FontEastAsianLanguage)));
	}

	[Fact]
	public void Stylistic_sets_map_to_ss01_to_ss20()
	{
		var o = new TextBlock();
		Typography.SetStylisticSet1(o, true);
		Typography.SetStylisticSet9(o, true);
		Typography.SetStylisticSet20(o, true);
		Assert.Equal("ss01=1,ss09=1,ss20=1", Features(o));
	}

	[Fact]
	public void Boolean_features_map_to_their_tags()
	{
		var o = new TextBlock();
		Typography.SetDiscretionaryLigatures(o, true);
		Typography.SetHistoricalLigatures(o, true);
		Typography.SetHistoricalForms(o, true);
		Typography.SetCaseSensitiveForms(o, true);
		Typography.SetCapitalSpacing(o, true);
		Typography.SetSlashedZero(o, true);
		Typography.SetMathematicalGreek(o, true);
		Typography.SetEastAsianExpertForms(o, true);
		Assert.Equal("dlig=1,hlig=1,hist=1,case=1,cpsp=1,zero=1,mgrk=1,expt=1", Features(o));
	}

	[Fact]
	public void Defaults_that_are_true_emit_only_when_disabled()
	{
		var o = new TextBlock();
		Typography.SetContextualLigatures(o, false);
		Typography.SetContextualAlternates(o, false);
		Assert.Equal("clig=0,calt=0", Features(o));
	}

	[Fact]
	public void Integer_features_emit_the_value()
	{
		var o = new TextBlock();
		Typography.SetStandardSwashes(o, 2);
		Typography.SetContextualSwashes(o, 1);
		Typography.SetStylisticAlternates(o, 3);
		Typography.SetAnnotationAlternates(o, 4);
		Assert.Equal("swsh=2,cswh=1,salt=3,nalt=4", Features(o));
	}

	[Theory]
	[InlineData(FontNumeralAlignment.Proportional, "pnum=1")]
	[InlineData(FontNumeralAlignment.Tabular, "tnum=1")]
	public void NumeralAlignment_maps(FontNumeralAlignment v, string expected)
	{
		var o = new TextBlock();
		Typography.SetNumeralAlignment(o, v);
		Assert.Equal(expected, Features(o));
	}

	[Theory]
	[InlineData(FontEastAsianWidths.Proportional, "pwid=1")]
	[InlineData(FontEastAsianWidths.Full, "fwid=1")]
	[InlineData(FontEastAsianWidths.Half, "hwid=1")]
	[InlineData(FontEastAsianWidths.Third, "twid=1")]
	[InlineData(FontEastAsianWidths.Quarter, "qwid=1")]
	public void EastAsianWidths_maps(FontEastAsianWidths v, string expected)
	{
		var o = new TextBlock();
		Typography.SetEastAsianWidths(o, v);
		Assert.Equal(expected, Features(o));
	}

	[Theory]
	[InlineData(FontEastAsianLanguage.Jis78, "jp78=1")]
	[InlineData(FontEastAsianLanguage.Jis83, "jp83=1")]
	[InlineData(FontEastAsianLanguage.Jis90, "jp90=1")]
	[InlineData(FontEastAsianLanguage.Jis04, "jp04=1")]
	[InlineData(FontEastAsianLanguage.HojoKanji, "hojo=1")]
	[InlineData(FontEastAsianLanguage.NlcKanji, "nlck=1")]
	[InlineData(FontEastAsianLanguage.Simplified, "smpl=1")]
	[InlineData(FontEastAsianLanguage.Traditional, "trad=1")]
	[InlineData(FontEastAsianLanguage.TraditionalNames, "tnam=1")]
	public void EastAsianLanguage_maps(FontEastAsianLanguage v, string expected)
	{
		var o = new TextBlock();
		Typography.SetEastAsianLanguage(o, v);
		Assert.Equal(expected, Features(o));
	}

	[Fact]
	public void Tail_features_follow_the_original_ones()
	{
		var o = new TextBlock();
		Typography.SetCapitals(o, FontCapitals.SmallCaps);
		Typography.SetSlashedZero(o, true);
		Assert.Equal("smcp=1,zero=1", Features(o));
	}
}
