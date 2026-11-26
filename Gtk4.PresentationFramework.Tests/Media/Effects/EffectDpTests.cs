using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media.Effects;

/// <summary>
/// Pure DP tests for <see cref="UIElement.Effect"/>. No GTK runtime required.
/// </summary>
public class EffectDpTests
{
	// ── Default value ─────────────────────────────────────────────────────────

	[Fact]
	public void Effect_default_is_null()
	{
		var el = new TextBlock();
		Assert.Null(el.Effect);
	}

	// ── Set / get ─────────────────────────────────────────────────────────────

	[Fact]
	public void Effect_set_BlurEffect_persists()
	{
		var el = new TextBlock();
		var blur = new BlurEffect { Radius = 8 };
		el.Effect = blur;
		Assert.Same(blur, el.Effect);
		Assert.IsType<BlurEffect>(el.Effect);
		Assert.Equal(8, ((BlurEffect)el.Effect!).Radius);
	}

	[Fact]
	public void Effect_set_DropShadowEffect_persists()
	{
		var el = new TextBlock();
		var drop = new DropShadowEffect { BlurRadius = 10, ShadowDepth = 4, Direction = 45, Opacity = 0.8 };
		el.Effect = drop;
		Assert.Same(drop, el.Effect);
		var d = (DropShadowEffect)el.Effect!;
		Assert.Equal(10, d.BlurRadius);
		Assert.Equal(4, d.ShadowDepth);
		Assert.Equal(45, d.Direction);
		Assert.Equal(0.8, d.Opacity);
	}

	[Fact]
	public void Effect_set_then_null_clears()
	{
		var el = new TextBlock();
		el.Effect = new BlurEffect { Radius = 3 };
		el.Effect = null;
		Assert.Null(el.Effect);
	}

	// ── Headless: setting Effect before widget creation must not throw ────────

	[Fact]
	public void Effect_set_before_widget_creation_does_not_throw()
	{
		var el = new TextBlock();
		// No widget materialized yet
		var ex = Record.Exception(() => el.Effect = new BlurEffect { Radius = 5 });
		Assert.Null(ex);
	}

	[Fact]
	public void Effect_null_before_widget_creation_does_not_throw()
	{
		var el = new TextBlock();
		var ex = Record.Exception(() => el.Effect = null);
		Assert.Null(ex);
	}

	// ── KernelType ────────────────────────────────────────────────────────────

	[Fact]
	public void BlurEffect_default_KernelType_is_Gaussian()
	{
		var blur = new BlurEffect();
		Assert.Equal(KernelType.Gaussian, blur.KernelType);
	}

	[Fact]
	public void BlurEffect_KernelType_Box_persists()
	{
		var blur = new BlurEffect { KernelType = KernelType.Box };
		Assert.Equal(KernelType.Box, blur.KernelType);
	}

	// ── DropShadowEffect defaults ─────────────────────────────────────────────

	[Fact]
	public void DropShadowEffect_default_color_is_black_opaque()
	{
		var d = new DropShadowEffect();
		Assert.Equal(255, d.Color.A);
		Assert.Equal(0, d.Color.R);
		Assert.Equal(0, d.Color.G);
		Assert.Equal(0, d.Color.B);
	}

	[Fact]
	public void DropShadowEffect_default_direction_is_315()
	{
		var d = new DropShadowEffect();
		Assert.Equal(315.0, d.Direction);
	}
}
