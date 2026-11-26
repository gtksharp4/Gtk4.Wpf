using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>
/// Pure tests for <see cref="UIElement.RenderTransform"/> and
/// <see cref="UIElement.RenderTransformOrigin"/> dependency properties, and for
/// the <see cref="RenderTransformCss"/> CSS mapping helper.
/// No GTK runtime required.
/// </summary>
public class RenderTransformDpTests
{
	const double Eps = 1e-9;

	// ── RenderTransform DP ────────────────────────────────────────────────────

	[Fact]
	public void RenderTransform_default_is_identity()
	{
		var el = new TextBlock();
		Assert.NotNull(el.RenderTransform);
		Assert.True(el.RenderTransform!.Value.IsIdentity);
	}

	[Fact]
	public void RenderTransformOrigin_default_is_zero_zero()
	{
		var el = new TextBlock();
		var o = el.RenderTransformOrigin;
		Assert.Equal(0, o.X);
		Assert.Equal(0, o.Y);
	}

	[Fact]
	public void RenderTransform_set_before_widget_creation_does_not_throw()
	{
		var el = new TextBlock();
		el.RenderTransform = new TranslateTransform(10, 20);
		var m = el.RenderTransform!.Value;
		Assert.Equal(10, m.OffsetX, Eps);
		Assert.Equal(20, m.OffsetY, Eps);
	}

	[Fact]
	public void RenderTransformOrigin_set_persists()
	{
		var el = new TextBlock();
		el.RenderTransformOrigin = new Point(0.5, 0.5);
		Assert.Equal(0.5, el.RenderTransformOrigin.X);
		Assert.Equal(0.5, el.RenderTransformOrigin.Y);
	}

	[Fact]
	public void RenderTransform_null_resets_to_identity()
	{
		var el = new TextBlock();
		el.RenderTransform = new TranslateTransform(5, 5);
		el.RenderTransform = null;
		Assert.True(el.RenderTransform!.Value.IsIdentity);
	}

	[Fact]
	public void RenderTransform_TransformGroup_value_composes_correctly()
	{
		var el = new TextBlock();
		var g = new TransformGroup();
		g.Children.Add(new TranslateTransform(3, 4));
		g.Children.Add(new ScaleTransform(2, 2));
		el.RenderTransform = g;
		// (0,0) → translate(3,4) → (3,4) → scale(2) → (6,8)
		var p = el.RenderTransform!.Value.Transform(new Point(0, 0));
		Assert.Equal(6, p.X, Eps);
		Assert.Equal(8, p.Y, Eps);
	}

	// ── RenderTransformCss ────────────────────────────────────────────────────

	[Fact]
	public void BuildCssTransform_identity_returns_none()
	{
		Assert.Equal("none", RenderTransformCss.BuildCssTransform(Transform.Identity));
	}

	[Fact]
	public void BuildCssTransform_null_returns_none()
	{
		Assert.Equal("none", RenderTransformCss.BuildCssTransform(null));
	}

	[Fact]
	public void BuildCssTransform_translate_produces_matrix_string()
	{
		var t = new TranslateTransform(30, 50);
		string css = RenderTransformCss.BuildCssTransform(t);
		Assert.StartsWith("matrix(", css);
		Assert.Contains("30", css);
		Assert.Contains("50", css);
	}

	[Fact]
	public void BuildCssTransform_rotate90_produces_matrix_string()
	{
		var r = new RotateTransform(90);
		string css = RenderTransformCss.BuildCssTransform(r);
		Assert.StartsWith("matrix(", css);
	}

	[Fact]
	public void BuildCssOrigin_zero_zero_produces_percent_string()
	{
		Assert.Equal("0% 0%", RenderTransformCss.BuildCssOrigin(new Point(0, 0)));
	}

	[Fact]
	public void BuildCssOrigin_half_half_produces_50_percent()
	{
		Assert.Equal("50% 50%", RenderTransformCss.BuildCssOrigin(new Point(0.5, 0.5)));
	}

	[Fact]
	public void BuildCssOrigin_one_one_produces_100_percent()
	{
		Assert.Equal("100% 100%", RenderTransformCss.BuildCssOrigin(new Point(1, 1)));
	}

	[Fact]
	public void BuildCssSnippet_uses_selector_and_contains_transform()
	{
		var t = new TranslateTransform(5, 10);
		string css = RenderTransformCss.BuildCssSnippet("#foo", t, new Point(0.5, 0.5));
		Assert.StartsWith("#foo {", css);
		Assert.Contains("transform:", css);
		Assert.Contains("transform-origin:", css);
	}
}
