using System;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Media;

/// <summary>
/// Pure tests for <see cref="Color"/>, <see cref="Colors"/>, <see cref="Brushes"/>
/// and <see cref="SolidColorBrush.ToGdkRgba"/>.
/// </summary>
public class ColorTests
{
	// ── Color.FromRgb ────────────────────────────────────────────────────────

	[Fact]
	public void FromRgb_sets_A_to_255()
	{
		var c = Color.FromRgb(10, 20, 30);
		Assert.Equal(255, c.A);
	}

	[Fact]
	public void FromRgb_stores_rgb_bytes()
	{
		var c = Color.FromRgb(1, 2, 3);
		Assert.Equal(1, c.R);
		Assert.Equal(2, c.G);
		Assert.Equal(3, c.B);
	}

	[Fact]
	public void FromArgb_stores_all_channels()
	{
		var c = Color.FromArgb(100, 50, 60, 70);
		Assert.Equal(100, c.A);
		Assert.Equal(50, c.R);
		Assert.Equal(60, c.G);
		Assert.Equal(70, c.B);
	}

	// ── Equality ─────────────────────────────────────────────────────────────

	[Fact]
	public void Same_argb_values_are_equal()
	{
		var a = Color.FromArgb(255, 100, 150, 200);
		var b = Color.FromArgb(255, 100, 150, 200);
		Assert.Equal(a, b);
	}

	[Fact]
	public void Different_alpha_are_not_equal()
	{
		var a = Color.FromArgb(255, 100, 150, 200);
		var b = Color.FromArgb(128, 100, 150, 200);
		Assert.NotEqual(a, b);
	}

	[Fact]
	public void Equality_operator_matches_Equals()
	{
		var a = Color.FromRgb(255, 0, 128);
		var b = Color.FromRgb(255, 0, 128);
		Assert.True(a == b);
		Assert.False(a != b);
	}

	// ── ScA / ScR / ScG / ScB ────────────────────────────────────────────────

	[Fact]
	public void ScA_of_opaque_colour_is_one()
	{
		var c = Color.FromRgb(0, 0, 0);
		Assert.Equal(1f, c.ScA, precision: 3);
	}

	[Fact]
	public void ScR_of_full_red_is_one()
	{
		var c = Color.FromRgb(255, 0, 0);
		Assert.Equal(1f, c.ScR, precision: 3);
	}

	// ── ToGdkRgba ────────────────────────────────────────────────────────────

	[Fact]
	public void ToGdkRgba_white_gives_all_ones()
	{
		var c = Colors.White;
		var rgba = c.ToGdkRgba();
		Assert.Equal(1f, rgba.Red, precision: 3);
		Assert.Equal(1f, rgba.Green, precision: 3);
		Assert.Equal(1f, rgba.Blue, precision: 3);
		Assert.Equal(1f, rgba.Alpha, precision: 3);
	}

	[Fact]
	public void ToGdkRgba_black_gives_rgb_zero()
	{
		var c = Colors.Black;
		var rgba = c.ToGdkRgba();
		Assert.Equal(0f, rgba.Red, precision: 3);
		Assert.Equal(0f, rgba.Green, precision: 3);
		Assert.Equal(0f, rgba.Blue, precision: 3);
	}

	[Fact]
	public void ToGdkRgba_transparent_gives_alpha_zero()
	{
		var c = Colors.Transparent;
		var rgba = c.ToGdkRgba();
		Assert.Equal(0f, rgba.Alpha, precision: 3);
	}

	// ── Colors named values ──────────────────────────────────────────────────

	[Fact]
	public void Colors_Red_has_full_red_channel()
	{
		Assert.Equal(255, Colors.Red.R);
		Assert.Equal(0, Colors.Red.G);
		Assert.Equal(0, Colors.Red.B);
	}

	[Fact]
	public void Colors_Blue_has_full_blue_channel()
	{
		Assert.Equal(0, Colors.Blue.R);
		Assert.Equal(0, Colors.Blue.G);
		Assert.Equal(255, Colors.Blue.B);
	}

	// ── SolidColorBrush / Brushes ────────────────────────────────────────────

	[Fact]
	public void SolidColorBrush_ToGdkRgba_matches_color()
	{
		var brush = new SolidColorBrush(Colors.Red);
		var rgba = brush.ToGdkRgba();
		Assert.Equal(1f, rgba.Red, precision: 3);
		Assert.Equal(0f, rgba.Green, precision: 3);
		Assert.Equal(0f, rgba.Blue, precision: 3);
	}

	[Fact]
	public void SolidColorBrush_ToGdkRgba_applies_opacity()
	{
		var brush = new SolidColorBrush(Colors.White) { Opacity = 0.5 };
		var rgba = brush.ToGdkRgba();
		Assert.Equal(0.5f, rgba.Alpha, precision: 3);
	}

	[Fact]
	public void Brushes_Red_has_Red_color()
	{
		Assert.Equal(Colors.Red, Brushes.Red.Color);
	}

	[Fact]
	public void Brushes_Black_has_Black_color()
	{
		Assert.Equal(Colors.Black, Brushes.Black.Color);
	}
}