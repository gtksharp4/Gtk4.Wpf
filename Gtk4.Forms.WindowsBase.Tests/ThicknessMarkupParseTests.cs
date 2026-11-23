using System;
using System.Windows;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.Forms.WindowsBase.Tests;

/// <summary>Thickness and CornerRadius attribute text as the markup converters read it (WPF converter rules).</summary>
public class ThicknessMarkupParseTests
{
	[Theory]
	[InlineData("12pt", 16)]
	[InlineData("1in", 96)]
	[InlineData("10px", 10)]
	[InlineData("10PX", 10)]
	[InlineData("5", 5)]
	public void A_single_value_with_a_unit_sets_all_sides(string text, double expected) =>
		Assert.Equal(new Thickness(expected), PrimitiveConverters.ParseThickness(text));

	[Fact]
	public void Centimetres_convert_at_96_dpi() =>
		Assert.Equal(96, PrimitiveConverters.ParseThickness("2.54cm").Left, 9);

	[Theory]
	[InlineData("1,2 3,4", 1, 2, 3, 4)]
	[InlineData("1 2 3 4", 1, 2, 3, 4)]
	[InlineData(" 1 , 2 ", 1, 2, 1, 2)]
	[InlineData("0.75in,1px", 72, 1, 72, 1)]
	public void Tokens_are_separated_by_commas_and_or_whitespace(string text, double l, double t, double r, double b) =>
		Assert.Equal(new Thickness(l, t, r, b), PrimitiveConverters.ParseThickness(text));

	[Fact]
	public void Auto_is_NaN() => Assert.True(double.IsNaN(PrimitiveConverters.ParseThickness("Auto").Top));

	[Theory]
	[InlineData("1,2,3")]
	[InlineData("1,2,3,4,5")]
	[InlineData("")]
	[InlineData("1 pt")]
	[InlineData("12 pt,1")]
	[InlineData("abc")]
	public void Wrong_token_counts_or_values_throw_FormatException(string text) =>
		Assert.Throws<FormatException>(() => PrimitiveConverters.ParseThickness(text));

	[Theory]
	[InlineData("4", 4, 4, 4, 4)]
	[InlineData("1,2,3,4", 1, 2, 3, 4)]
	[InlineData("1 2 3 4", 1, 2, 3, 4)]
	public void CornerRadius_takes_one_or_four_plain_numbers(string text, double a, double b, double c, double d) =>
		Assert.Equal(new CornerRadius(a, b, c, d), PrimitiveConverters.ParseCornerRadius(text));

	[Theory]
	[InlineData("4pt")]
	[InlineData("1,2")]
	[InlineData("1,2,3")]
	public void CornerRadius_rejects_unit_suffixes_and_other_counts(string text) =>
		Assert.Throws<FormatException>(() => PrimitiveConverters.ParseCornerRadius(text));
}
