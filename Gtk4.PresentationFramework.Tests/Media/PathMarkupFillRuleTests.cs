using System;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>The path mini-language starts with an optional fill-rule command: F0 even-odd, F1 non-zero.</summary>
public class PathMarkupFillRuleTests
{
	static PathGeometry Parse(string s) => Assert.IsType<PathGeometry>(Geometry.Parse(s));

	[Fact]
	public void The_default_fill_rule_is_EvenOdd() =>
		Assert.Equal(FillRule.EvenOdd, Parse("M0,0 L10,0 L10,10 Z").FillRule);

	[Theory]
	[InlineData("F0 M0,0 L10,0 L10,10 Z", FillRule.EvenOdd)]
	[InlineData("F1 M0,0 L10,0 L10,10 Z", FillRule.Nonzero)]
	[InlineData("F1M0,0L10,0L10,10Z", FillRule.Nonzero)]
	[InlineData("  F1 M0,0 L10,0 Z", FillRule.Nonzero)]
	public void A_leading_F_command_sets_the_fill_rule_and_the_figure_still_parses(string markup, FillRule rule)
	{
		var pg = Parse(markup);
		Assert.Equal(rule, pg.FillRule);
		Assert.Single(pg.Figures);
		Assert.Equal(new System.Windows.Point(0, 0), pg.Figures[0].StartPoint);
	}

	[Theory]
	[InlineData("F2 M0,0 L1,1")]
	[InlineData("M0,0 L1,1 F1")]
	public void A_bad_fill_rule_value_or_a_late_F_is_a_FormatException(string markup) =>
		Assert.Throws<FormatException>(() => Geometry.Parse(markup));
}
