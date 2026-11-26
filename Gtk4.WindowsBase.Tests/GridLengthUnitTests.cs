using System;
using System.Windows;
using System.Windows.Markup;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class GridLengthUnitTests
{
	[Theory]
	[InlineData("100px", 100)]
	[InlineData("1in", 96)]
	[InlineData("2.54cm", 96)]
	[InlineData("72pt", 96)]
	[InlineData("  50 px ", 50)]
	[InlineData("12PX", 12)]
	[InlineData("300", 300)]
	public void Unit_suffixes_convert_to_pixels(string text, double expected)
	{
		GridLength length = PrimitiveConverters.ParseGridLength(text);

		Assert.Equal(GridUnitType.Pixel, length.GridUnitType);
		Assert.Equal(expected, length.Value, 9);
	}

	[Fact]
	public void Auto_and_star_are_unchanged()
	{
		Assert.True(PrimitiveConverters.ParseGridLength("Auto").IsAuto);
		Assert.Equal(new GridLength(1, GridUnitType.Star), PrimitiveConverters.ParseGridLength("*"));
		Assert.Equal(new GridLength(2, GridUnitType.Star), PrimitiveConverters.ParseGridLength("2*"));
	}

	[Theory]
	[InlineData("abc")]
	[InlineData("2px*")]
	[InlineData("px")]
	public void Invalid_text_throws_FormatException(string text) =>
		Assert.Throws<FormatException>(() => PrimitiveConverters.ParseGridLength(text));
}
