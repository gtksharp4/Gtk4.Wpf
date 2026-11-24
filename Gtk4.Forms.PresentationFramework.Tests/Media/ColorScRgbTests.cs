using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>scRGB conversion as in WPF's <c>Color.FromScRgb</c> / <c>ScR</c> (the sRGB transfer function).</summary>
public class ColorScRgbTests
{
	[Fact]
	public void FromScRgb_applies_the_srgb_transfer_with_wpf_rounding()
	{
		var c = Color.FromScRgb(1f, 0.5f, 0f, 0f);
		Assert.Equal(Color.FromArgb(255, 188, 0, 0), c);
		Assert.Equal(Color.FromArgb(255, 89, 0, 0), Color.FromScRgb(1f, 0.1f, 0f, 0f));
		Assert.Equal(Color.FromArgb(255, 3, 0, 0), Color.FromScRgb(1f, 0.001f, 0f, 0f));
	}

	[Fact]
	public void FromScRgb_clamps_channels_and_alpha()
	{
		Assert.Equal(Color.FromArgb(255, 255, 0, 255), Color.FromScRgb(2f, 7f, -1f, 1f));
		Assert.Equal(Color.FromArgb(0, 0, 0, 0), Color.FromScRgb(-1f, 0f, 0f, 0f));
		// Alpha is linear: (byte)(0.5 * 255 + 0.5).
		Assert.Equal(128, Color.FromScRgb(0.5f, 0f, 0f, 0f).A);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(10)]
	[InlineData(128)]
	[InlineData(200)]
	[InlineData(255)]
	public void ScR_ScG_ScB_are_linear_light_and_round_trip_the_byte(byte value)
	{
		var c = Color.FromRgb(value, value, value);
		Assert.InRange(c.ScR, 0f, 1f);
		Assert.Equal(Color.FromScRgb(1f, c.ScR, c.ScG, c.ScB), c);
	}

	[Fact]
	public void ScR_of_mid_grey_is_the_linear_value_not_byte_over_255()
	{
		Assert.Equal(0.2158605, Color.FromRgb(128, 0, 0).ScR, 3);
		Assert.Equal(1f, Color.FromRgb(255, 0, 0).ScR, 3);
	}

	[Fact]
	public void The_ScR_setter_converts_through_the_transfer()
	{
		var c = Color.FromRgb(0, 0, 0);
		c.ScR = 0.5f;
		c.ScG = 0.5f;
		c.ScB = 0.5f;
		c.ScA = 0.5f;
		Assert.Equal(Color.FromArgb(128, 188, 188, 188), c);
	}

	[Fact]
	public void The_three_HexSphereDemo_gradient_stops_parse_to_the_wpf_bytes()
	{
		Assert.Equal(Color.FromArgb(255, 89, 0, 0), Parse("sc#1, 0.1, 0, 0"));
		Assert.Equal(Color.FromArgb(255, 255, 89, 0), Parse("sc#1, 1, 0.1, 0"));
		Assert.Equal(Color.FromArgb(255, 255, 255, 89), Parse("sc#1, 1, 1, 0.1"));
		var brush = (SolidColorBrush)new BrushConverter().ConvertFromString("sc#1, 0.1, 0, 0")!;
		Assert.Equal(Color.FromArgb(255, 89, 0, 0), brush.Color);
	}

	static Color Parse(string text) => ScRgbColor.Parse(text.Trim(), text);
}
