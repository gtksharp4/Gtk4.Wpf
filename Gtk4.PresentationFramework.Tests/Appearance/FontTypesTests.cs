using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

public class FontTypesTests
{
	[Fact]
	public void FontFamily_source_tostring_and_fallback_list_preserved()
	{
		var f = new FontFamily("Segoe UI, Arial");
		Assert.Equal("Segoe UI, Arial", f.Source);
		Assert.Equal("Segoe UI, Arial", f.ToString());
		Assert.Equal(string.Empty, new FontFamily().Source);
	}

	[Fact]
	public void FontFamily_equality_is_ordinal_ignore_case()
	{
		Assert.Equal(new FontFamily("Arial"), new FontFamily("ARIAL"));
		Assert.Equal(new FontFamily("Arial").GetHashCode(), new FontFamily("aRiAl").GetHashCode());
		Assert.NotEqual(new FontFamily("Arial"), new FontFamily("Arial Black"));
		Assert.Throws<ArgumentNullException>(() => new FontFamily(null!));
	}

	[Fact]
	public void FontStretch_open_type_values_and_names()
	{
		Assert.Equal(1, FontStretches.UltraCondensed.ToOpenTypeStretch());
		Assert.Equal(5, FontStretches.Normal.ToOpenTypeStretch());
		Assert.Equal(9, FontStretches.UltraExpanded.ToOpenTypeStretch());
		Assert.Equal(FontStretches.Medium, FontStretches.Normal);
		Assert.True(FontStretches.Normal == default(FontStretch));
		Assert.True(FontStretches.Condensed != FontStretches.Expanded);
		Assert.Equal("SemiCondensed", FontStretches.SemiCondensed.ToString());
		Assert.Throws<ArgumentOutOfRangeException>(() => FontStretch.FromOpenTypeStretch(10));
	}

	[Fact]
	public void FontWeight_parses_names_and_numbers()
	{
		Assert.Equal(FontWeights.Bold, FontConverters.ParseFontWeight("bold"));
		Assert.Equal(FontWeights.SemiBold, FontConverters.ParseFontWeight(" SemiBold "));
		Assert.Equal(650, FontConverters.ParseFontWeight("650").ToOpenTypeWeight());
		Assert.Throws<FormatException>(() => FontConverters.ParseFontWeight("1000"));
		Assert.Throws<FormatException>(() => FontConverters.ParseFontWeight("chunky"));
	}

	[Fact]
	public void FontStyle_and_stretch_parse_by_name()
	{
		Assert.Equal(FontStyles.Italic, FontConverters.ParseFontStyle("ITALIC"));
		Assert.Equal(FontStyles.Oblique, FontConverters.ParseFontStyle("oblique"));
		Assert.Equal(FontStretches.ExtraCondensed, FontConverters.ParseFontStretch("extracondensed"));
		Assert.Equal(FontStretches.Normal, FontConverters.ParseFontStretch("Medium"));
		Assert.Throws<FormatException>(() => FontConverters.ParseFontStyle("slanted"));
		Assert.Throws<FormatException>(() => FontConverters.ParseFontStretch("wide"));
	}

	[Theory]
	[InlineData("12", 12.0)]
	[InlineData("12px", 12.0)]
	[InlineData("12pt", 16.0)]
	[InlineData("1in", 96.0)]
	[InlineData("2.54cm", 96.0)]
	[InlineData(" 10.5 PT ", 14.0)]
	public void FontSize_units(string raw, double expected) =>
		Assert.Equal(expected, FontConverters.ParseFontSize(raw), 9);

	[Theory]
	[InlineData("")]
	[InlineData("big")]
	[InlineData("12em")]
	[InlineData("NaN")]
	public void FontSize_invalid_throws(string raw) =>
		Assert.Throws<FormatException>(() => FontConverters.ParseFontSize(raw));

	[Fact]
	public void FontFamily_empty_string_throws() =>
		Assert.Throws<FormatException>(() => FontConverters.ParseFontFamily("  "));

	[Fact]
	public void XamlValueConverter_uses_font_converters_and_units()
	{
		var fs = TextElement.FontSizeProperty;
		Assert.Equal(16.0, XamlValueConverter.Convert(typeof(object), fs, "12pt"));
		Assert.Equal(FontWeights.Bold,
			XamlValueConverter.Convert(typeof(object), TextElement.FontWeightProperty, "Bold"));
		var fam = (FontFamily)XamlValueConverter.Convert(
			typeof(object), TextElement.FontFamilyProperty, "Arial, sans-serif")!;
		Assert.Equal("Arial, sans-serif", fam.Source);
		Assert.Throws<FormatException>(() => XamlValueConverter.Convert(typeof(object), fs, "huge"));
	}

	[Fact]
	public void FromOpenType_factories_live_on_the_structs_and_validate_range()
	{
		Assert.Equal(FontWeights.Bold, FontWeight.FromOpenTypeWeight(700));
		Assert.Equal(FontStretches.Normal, FontStretch.FromOpenTypeStretch(5));
		Assert.Throws<ArgumentOutOfRangeException>(() => FontWeight.FromOpenTypeWeight(0));
		Assert.Throws<ArgumentOutOfRangeException>(() => FontWeight.FromOpenTypeWeight(1000));
		Assert.Throws<ArgumentOutOfRangeException>(() => FontStretch.FromOpenTypeStretch(0));
		Assert.Throws<ArgumentOutOfRangeException>(() => FontStretch.FromOpenTypeStretch(10));
	}

	[Fact]
	public void FontStretch_compares_by_open_type_value()
	{
		Assert.True(FontStretches.Condensed < FontStretches.Expanded);
		Assert.True(FontStretches.Normal >= FontStretches.Medium);
		Assert.Equal(0, FontStretch.Compare(FontStretches.Normal, FontStretches.Medium));
		Assert.True(FontStretch.Compare(FontStretches.Expanded, FontStretches.Normal) > 0);
	}
}
