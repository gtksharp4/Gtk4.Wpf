using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.VisualLayer;

public class EffectsAndConvertersTests
{
	static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

	[Fact]
	public void Bitmap_effect_defaults_match_WPF()
	{
		var bevel = new BevelBitmapEffect();
		Assert.Equal((5.0, 0.3, 135.0, 0.2, EdgeProfile.Linear),
			(bevel.BevelWidth, bevel.Relief, bevel.LightAngle, bevel.Smoothness, bevel.EdgeProfile));
		var blur = new BlurBitmapEffect();
		Assert.Equal((5.0, KernelType.Gaussian), (blur.Radius, blur.KernelType));
		var drop = new DropShadowBitmapEffect();
		Assert.Equal((5.0, Colors.Black, 315.0, 0.0, 1.0, 0.5),
			(drop.ShadowDepth, drop.Color, drop.Direction, drop.Noise, drop.Opacity, drop.Softness));
		var emboss = new EmbossBitmapEffect();
		Assert.Equal((45.0, 0.44), (emboss.LightAngle, emboss.Relief));
		var glow = new OuterGlowBitmapEffect();
		Assert.Equal((Colors.Gold, 5.0, 0.0, 1.0), (glow.GlowColor, glow.GlowSize, glow.Noise, glow.Opacity));
		Assert.Equal(new[] { 0, 1, 2, 3 }, Array.ConvertAll((EdgeProfile[])Enum.GetValues(typeof(EdgeProfile)), v => (int)v));
	}

	[Fact]
	public void Bitmap_effect_properties_are_real_DPs_that_storyboards_can_animate()
	{
		var blur = new BlurBitmapEffect();
		blur.BeginAnimation(BlurBitmapEffect.RadiusProperty, new DoubleAnimation(1, 9, TimeSpan.FromSeconds(1)));
		Assert.True(blur.HasAnimatedProperties);
		Assert.IsAssignableFrom<Animatable>(blur);
		blur.Radius = 7;
		Assert.Equal(7.0, blur.GetValue(BlurBitmapEffect.RadiusProperty) is double d ? d : -1, 3);
	}

	[Fact]
	public void BitmapEffectGroup_clones_its_children_and_BitmapEffectInput_has_DPs()
	{
		var group = new BitmapEffectGroup();
		group.Children.Add(new BlurBitmapEffect { Radius = 2 });
		var clone = (BitmapEffectGroup)group.Clone();
		Assert.Equal(2.0, ((BlurBitmapEffect)clone.Children[0]).Radius);
		Assert.NotSame(group.Children[0], clone.Children[0]);
		var input = new BitmapEffectInput();
		Assert.Equal(BrushMappingMode.RelativeToBoundingBox, input.AreaToApplyEffectUnits);
		Assert.True(input.AreaToApplyEffect.IsEmpty);
	}

	[Fact]
	public void UIElement_BitmapEffect_is_stored_and_obsolete()
	{
		var prop = typeof(UIElement).GetProperty("BitmapEffect")!;
		Assert.NotNull(prop.GetCustomAttribute<ObsoleteAttribute>());
		Assert.NotNull(typeof(UIElement).GetProperty("BitmapEffectInput")!.GetCustomAttribute<ObsoleteAttribute>());
		var el = new System.Windows.Controls.Border();
#pragma warning disable CS0618
		el.BitmapEffect = new BlurBitmapEffect();
		Assert.IsType<BlurBitmapEffect>(el.BitmapEffect);
#pragma warning restore CS0618
	}

	[Fact]
	public void Color_Add_and_Multiply_clamp_per_channel()
	{
		var sum = Color.FromArgb(200, 100, 10, 0) + Color.FromArgb(100, 200, 20, 0);
		Assert.Equal(Color.FromArgb(255, 255, 30, 0), sum);
		Assert.Equal(Color.FromArgb(128, 50, 5, 0), Color.Multiply(Color.FromArgb(255, 100, 10, 0), 0.5f));
		Assert.Equal(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(9, 9, 9, 9) * 0f);
	}

	[Fact]
	public void ColorInterpolationMode_defaults_to_SRgb_and_clones()
	{
		var b = new LinearGradientBrush(Colors.Red, Colors.Blue, 0);
		Assert.Equal(ColorInterpolationMode.SRgbLinearInterpolation, b.ColorInterpolationMode);
		b.ColorInterpolationMode = ColorInterpolationMode.ScRgbLinearInterpolation;
		Assert.Equal(ColorInterpolationMode.ScRgbLinearInterpolation, ((LinearGradientBrush)b.Clone()).ColorInterpolationMode);
		Assert.Equal(BrushMappingMode.RelativeToBoundingBox, b.MappingMode);
	}

	[Fact]
	public void BrushConverter_named_colours_return_the_same_frozen_brush()
	{
		var c = new BrushConverter();
		var a = (SolidColorBrush)c.ConvertFromString(null, Inv, "Red")!;
		var b = (SolidColorBrush)c.ConvertFromString(null, Inv, "red")!;
		Assert.Same(a, b);
		Assert.True(a.IsFrozen);
		Assert.Equal(Colors.Red, a.Color);
	}

	[Theory]
	[InlineData("#FF0000", 255, 255, 0, 0)]
	[InlineData("#80FF0000", 128, 255, 0, 0)]
	[InlineData("#F00", 255, 255, 0, 0)]
	[InlineData("sc# 1 0 0", 255, 255, 0, 0)]
	public void BrushConverter_parses_hex_and_scRGB(string text, int a, int r, int g, int b)
	{
		var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(null, Inv, text)!;
		Assert.Equal(Color.FromArgb((byte)a, (byte)r, (byte)g, (byte)b), brush.Color);
		Assert.False(brush.IsFrozen);
	}

	[Fact]
	public void BrushConverter_scRGB_with_alpha_matches_WPF()
	{
		var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(null, Inv, "sc#1,0.5,0.5,0.5")!;
		Assert.Equal(Color.FromArgb(255, 0xBC, 0xBC, 0xBC), brush.Color);
	}

	[Fact]
	public void BrushConverter_errors_and_string_target()
	{
		var c = new BrushConverter();
		Assert.Throws<FormatException>(() => c.ConvertFromString(null, Inv, ""));
		Assert.Throws<FormatException>(() => c.ConvertFromString(null, Inv, "NotAColour"));
		Assert.False(c.CanConvertFrom(typeof(int)));
		Assert.Equal("#FFFF0000", c.ConvertTo(null, Inv, Brushes.Red, typeof(string)));
		Assert.Equal("#80010203", c.ConvertTo(null, Inv, new SolidColorBrush(Color.FromArgb(0x80, 1, 2, 3)), typeof(string)));
		Assert.Equal(string.Empty, c.ConvertTo(null, Inv, null, typeof(string)));
	}

	[Fact]
	public void TypeDescriptor_resolves_the_Brush_converter_and_runtime_parse_stays_identical()
	{
		Assert.IsType<BrushConverter>(TypeDescriptor.GetConverter(typeof(Brush)));
		var viaXaml = (SolidColorBrush)System.Windows.Markup.XamlValueConverter.ConvertByType(typeof(Brush), "#336699")!;
		var viaConverter = (SolidColorBrush)TypeDescriptor.GetConverter(typeof(Brush)).ConvertFromInvariantString("#336699")!;
		Assert.Equal(viaXaml.Color, viaConverter.Color);
		var thickness = System.Windows.Markup.XamlValueConverter.ConvertByType(typeof(Thickness), "1,2,3,4");
		Assert.Equal(thickness, TypeDescriptor.GetConverter(typeof(Thickness)).ConvertFromInvariantString("1,2,3,4"));
		var rect = System.Windows.Markup.XamlValueConverter.ConvertByType(typeof(Rect), "1,2,3,4");
		Assert.Equal(rect, TypeDescriptor.GetConverter(typeof(Rect)).ConvertFromInvariantString("1,2,3,4"));
	}
}
