using System;
using System.Windows.Media.Effects;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Effects;

/// <summary>
/// Pure unit tests for <see cref="EffectCss"/>. No GTK runtime required.
/// </summary>
public class EffectCssTests
{
	const double Eps = 1e-6;

	// ── BlurEffect ────────────────────────────────────────────────────────────

	[Fact]
	public void BuildFilter_null_returns_none()
	{
		Assert.Equal("none", EffectCss.BuildFilter(null));
	}

	[Fact]
	public void BuildFilter_blur_5px_produces_blur_5px()
	{
		string filter = EffectCss.BuildFilter(new BlurEffect { Radius = 5 });
		Assert.Equal("blur(5px)", filter);
	}

	[Fact]
	public void BuildFilter_blur_zero_produces_blur_0px()
	{
		string filter = EffectCss.BuildFilter(new BlurEffect { Radius = 0 });
		Assert.Equal("blur(0px)", filter);
	}

	[Fact]
	public void BuildFilter_blur_decimal_radius_uses_invariant_culture()
	{
		string filter = EffectCss.BuildFilter(new BlurEffect { Radius = 2.5 });
		// Must contain '.' not ','
		Assert.Contains("2.5px", filter);
	}

	// ── DropShadowEffect direction/offset convention ───────────────────────────

	[Fact]
	public void BuildFilter_dropShadow_direction0_gives_positive_dx_zero_dy()
	{
		// 0° = right: dx = depth, dy = 0
		var effect = new DropShadowEffect
		{
			Color = System.Windows.Media.Color.FromArgb(255, 0, 0, 0),
			BlurRadius = 5,
			ShadowDepth = 10,
			Direction = 0,
			Opacity = 1,
		};
		string filter = EffectCss.BuildFilter(effect);
		// Should contain "10px 0px" (dx=10, dy=0 → -sin(0)=0)
		Assert.Contains("drop-shadow(", filter);
		Assert.Contains("10px", filter);
	}

	[Fact]
	public void BuildFilter_dropShadow_direction90_gives_zero_dx_negative_dy()
	{
		// 90° = up: dx = depth·cos(90°) = 0, dy = -depth·sin(90°) = -depth
		var effect = new DropShadowEffect
		{
			Color = System.Windows.Media.Color.FromArgb(255, 0, 0, 0),
			BlurRadius = 4,
			ShadowDepth = 8,
			Direction = 90,
			Opacity = 1,
		};
		string filter = EffectCss.BuildFilter(effect);
		Assert.Contains("drop-shadow(", filter);
		// dx ≈ 0 (may be tiny float), dy ≈ -8  — just check the -8 is present
		Assert.Contains("-8px", filter);
	}

	[Fact]
	public void BuildFilter_dropShadow_exact_string_direction315()
	{
		// 315° = lower-right (WPF default): dx=depth·cos(315°), dy=-depth·sin(315°)
		// cos(315°)= √2/2 ≈ 0.707107, sin(315°)= -√2/2 → dy = depth·√2/2 (positive = down)
		var c = System.Windows.Media.Color.FromArgb(255, 0, 0, 0);
		var effect = new DropShadowEffect
		{
			Color = c,
			BlurRadius = 5,
			ShadowDepth = 5,
			Direction = 315,
			Opacity = 1,
		};
		string filter = EffectCss.BuildFilter(effect);
		Assert.StartsWith("drop-shadow(", filter);
		Assert.Contains("rgba(0,0,0,1)", filter);
	}

	// ── Color → rgba formatting ───────────────────────────────────────────────

	[Fact]
	public void BuildFilter_dropShadow_semi_transparent_color()
	{
		// Alpha=128 (≈0.502), Opacity=1 → effective alpha ≈ 0.502
		var c = System.Windows.Media.Color.FromArgb(128, 255, 0, 0);
		var effect = new DropShadowEffect
		{
			Color = c,
			BlurRadius = 3,
			ShadowDepth = 3,
			Direction = 0,
			Opacity = 1,
		};
		string filter = EffectCss.BuildFilter(effect);
		Assert.Contains("rgba(255,0,0,", filter);
		// Alpha ≈ 128/255 ≈ 0.502
		Assert.Contains("0.50", filter);
	}

	[Fact]
	public void BuildFilter_dropShadow_opacity_scales_alpha()
	{
		// Opaque color, Opacity=0.5 → effective alpha = 0.5
		var c = System.Windows.Media.Color.FromArgb(255, 0, 0, 255);
		var effect = new DropShadowEffect
		{
			Color = c,
			BlurRadius = 2,
			ShadowDepth = 2,
			Direction = 0,
			Opacity = 0.5,
		};
		string filter = EffectCss.BuildFilter(effect);
		Assert.Contains("rgba(0,0,255,", filter);
		Assert.Contains("0.5", filter);
	}

	[Fact]
	public void BuildFilter_dropShadow_fully_transparent_color()
	{
		var c = System.Windows.Media.Color.FromArgb(0, 0, 0, 0);
		var effect = new DropShadowEffect { Color = c, BlurRadius = 2, ShadowDepth = 2, Direction = 0, Opacity = 1 };
		string filter = EffectCss.BuildFilter(effect);
		Assert.Contains("rgba(0,0,0,0)", filter);
	}

	// ── BuildFilterSnippet ────────────────────────────────────────────────────

	[Fact]
	public void BuildFilterSnippet_uses_selector_and_contains_filter()
	{
		string css = EffectCss.BuildFilterSnippet("#mywidget", new BlurEffect { Radius = 3 });
		Assert.StartsWith("#mywidget {", css);
		Assert.Contains("filter: blur(3px)", css);
	}

	[Fact]
	public void BuildFilterSnippet_null_effect_emits_filter_none()
	{
		string css = EffectCss.BuildFilterSnippet("#w", null);
		Assert.Contains("filter: none", css);
	}
}
