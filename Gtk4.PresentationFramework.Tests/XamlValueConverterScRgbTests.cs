using System;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The scRGB "sc#" colour form (linear-light floats through the sRGB transfer).</summary>
public class XamlValueConverterScRgbTests
{
	static Color Parse(string raw) => MediaConverters.ParseColor(raw);

	[Fact]
	public void SampleString_ParsesFourValueForm()
	{
		Assert.Equal(Color.FromArgb(255, 227, 235, 255), Parse("sc#1.000000, 0.769689, 0.831936, 1.000000"));
	}

	[Fact]
	public void ThreeValueForm_IsOpaque()
	{
		Assert.Equal(Color.FromArgb(255, 255, 0, 255), Parse("sc#1, 0, 1"));
	}

	[Fact]
	public void Alpha_IsLinearAndClamped()
	{
		Assert.Equal(Color.FromArgb(128, 0, 0, 0), Parse("sc#0.5, 0, 0, 0"));
		Assert.Equal(Color.FromArgb(255, 255, 255, 255), Parse("sc#2, 5, 5, 5"));
		Assert.Equal(Color.FromArgb(0, 0, 0, 0), Parse("sc#-1, -1, -1, -1"));
	}

	[Fact]
	public void Channel_UsesLinearSegmentBelowThreshold()
	{
		Assert.Equal(Color.FromArgb(255, 7, 0, 0), Parse("sc#1, 0.002, 0, 0"));
	}

	[Fact]
	public void SurroundingWhitespace_IsAccepted()
	{
		Assert.Equal(Color.FromArgb(255, 255, 255, 255), Parse("  sc#1,1,1  "));
	}

	[Fact]
	public void UpperCasePrefix_IsNotScRgb()
	{
		Assert.Throws<FormatException>(() => Parse("SC#1,1,1"));
	}

	[Fact]
	public void WhitespaceSeparators_AreAccepted()
	{
		Assert.Equal(Color.FromArgb(255, 255, 0, 0), Parse("sc#1 0 0"));
		Assert.Equal(Color.FromArgb(255, 255, 188, 0), Parse("sc# 1, 0.5 , 0"));
	}

	[Fact]
	public void NaN_ChannelMapsToZero()
	{
		Assert.Equal(0, Parse("sc#1,NaN,0,0").R);
	}

	[Theory]
	[InlineData("sc#1")]
	[InlineData("sc#1, 2")]
	[InlineData("sc#1, 2, 3, 4, 5")]
	[InlineData("sc#")]
	[InlineData("sc#a, b, c")]
	[InlineData("sc#1,,1,1")]
	[InlineData("sc#1,1,1,")]
	[InlineData("sc#,1,1,1")]
	public void BadTokenCount_ThrowsFormatException(string raw)
	{
		Assert.Throws<FormatException>(() => Parse(raw));
	}
}
