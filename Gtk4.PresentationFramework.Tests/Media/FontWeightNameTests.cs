using System;
using System.Globalization;
using System.Windows;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

public class FontWeightNameTests
{
	[Theory]
	[InlineData(100, "Thin")]
	[InlineData(200, "ExtraLight")]
	[InlineData(300, "Light")]
	[InlineData(400, "Normal")]
	[InlineData(500, "Medium")]
	[InlineData(600, "SemiBold")]
	[InlineData(700, "Bold")]
	[InlineData(800, "ExtraBold")]
	[InlineData(900, "Black")]
	[InlineData(950, "ExtraBlack")]
	[InlineData(450, "450")]
	public void ToString_returns_the_WPF_name_or_the_number(int weight, string expected) =>
		Assert.Equal(expected, FontWeight.FromOpenTypeWeight(weight).ToString());

	[Fact]
	public void IFormattable_ToString_uses_the_provider_for_numbers()
	{
		var weight = (IFormattable)FontWeight.FromOpenTypeWeight(450);
		Assert.Equal("450", weight.ToString(null, CultureInfo.InvariantCulture));
		Assert.Equal("Bold", ((IFormattable)FontWeights.Bold).ToString(null, CultureInfo.InvariantCulture));
	}

	[Fact]
	public void Default_weight_is_Normal_in_equality_hash_and_text()
	{
		FontWeight d = default;
		Assert.Equal(FontWeights.Normal, d);
		Assert.True(d == FontWeights.Normal);
		Assert.False(d != FontWeights.Normal);
		Assert.Equal(FontWeights.Normal.GetHashCode(), d.GetHashCode());
		Assert.Equal("Normal", d.ToString());
		Assert.True(d.Equals((object)FontWeights.Normal));
		Assert.Equal(400, d.Weight);
	}

	[Fact]
	public void Compare_and_relational_operators_order_by_weight()
	{
		Assert.True(FontWeight.Compare(FontWeights.Light, FontWeights.Bold) < 0);
		Assert.True(FontWeight.Compare(FontWeights.Bold, FontWeights.Bold) == 0);
		Assert.True(FontWeights.Light < FontWeights.Bold);
		Assert.True(FontWeights.Light <= FontWeights.Light);
		Assert.True(FontWeights.Black > FontWeights.Bold);
		Assert.True(FontWeights.Black >= FontWeights.Black);
		FontWeight d = default;
		Assert.True(d < FontWeights.Bold);
		Assert.False(d < FontWeights.Normal);
	}

	[Fact]
	public void Converter_round_trips_the_names()
	{
		var converter = new FontWeightConverter();
		foreach (var weight in new[] { FontWeights.Thin, FontWeights.Normal, FontWeights.Bold, FontWeights.ExtraBlack })
		{
			string text = (string)converter.ConvertTo(null, CultureInfo.InvariantCulture, weight, typeof(string))!;
			Assert.Equal(weight, (FontWeight)converter.ConvertFrom(null, CultureInfo.InvariantCulture, text));
		}
	}
}
