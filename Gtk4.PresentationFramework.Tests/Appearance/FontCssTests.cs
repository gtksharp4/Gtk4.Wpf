using System.Windows;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

public class FontCssTests
{
	[Fact]
	public void Family_is_quoted_with_fallbacks_preserved()
	{
		Assert.Equal("\"Segoe UI\", \"Arial\"", FontCss.Family(new FontFamily("Segoe UI, Arial")));
	}

	[Fact]
	public void Family_escapes_quotes_and_backslashes()
	{
		Assert.Equal("\"A\\\"B\\\\C\"", FontCss.Family(new FontFamily("A\"B\\C")));
	}

	[Fact]
	public void Size_is_px_numeric_weight_and_keywords()
	{
		string css = FontCss.Build(new FontFamily("Arial"), 14.5, FontWeights.Bold,
			FontStyles.Italic, FontStretches.SemiCondensed);
		Assert.Equal(
			"font-family: \"Arial\"; font-size: 14.5px; font-weight: 700; font-style: italic; " +
			"font-stretch: semi-condensed;", css);
	}

	[Fact]
	public void Oblique_and_default_weight_stretch()
	{
		string css = FontCss.Build(null, null, default(FontWeight), FontStyles.Oblique, default(FontStretch));
		Assert.Equal("font-weight: 400; font-style: oblique; font-stretch: normal;", css);
	}

	[Fact]
	public void Nothing_set_is_empty()
	{
		Assert.Equal(string.Empty, FontCss.Build(null, null, null, null, null));
	}

	[Fact]
	public void Each_stretch_has_a_distinct_keyword()
	{
		var seen = new System.Collections.Generic.HashSet<string>();
		for (int i = 1; i <= 9; i++)
			Assert.True(seen.Add(FontCss.Build(null, null, null, null, FontStretch.FromOpenTypeStretch(i))));
	}

	[Fact]
	public void Family_reduces_a_font_location_to_its_family_name()
	{
		Assert.Equal("\"Roboto\", \"Arial\"", FontCss.Family(new FontFamily("./Fonts/#Roboto, Arial")));
	}

	[Fact]
	public void Non_positive_or_non_finite_sizes_are_not_emitted()
	{
		Assert.Equal(string.Empty, FontCss.Build(null, -1, null, null, null));
		Assert.Equal(string.Empty, FontCss.Build(null, 0, null, null, null));
		Assert.Equal(string.Empty, FontCss.Build(null, double.NaN, null, null, null));
		Assert.Equal(string.Empty, FontCss.Build(null, double.PositiveInfinity, null, null, null));
	}
}
