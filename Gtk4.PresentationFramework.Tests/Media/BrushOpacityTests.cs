using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Media;

/// <summary>Brush.Opacity is a dependency property: path resolution, frozen writes, Changed and Clone.</summary>
public class BrushOpacityTests : WpfGtkTestBase
{
	[Fact]
	public void Button_background_brush_opacity_path_resolves_to_the_brush_property() => Run(() =>
	{
		var brush = new SolidColorBrush(Colors.Red);
		var button = new Button { Background = brush };
		var (target, dp) = StoryboardPathResolver.Resolve(
			button, new PropertyPath("(Button.Background).(SolidColorBrush.Opacity)"));
		Assert.Same(brush, target);
		Assert.Same(Brush.OpacityProperty, dp);
	});

	[Fact]
	public void Rectangle_fill_opacity_path_resolves_to_the_brush_property() => Run(() =>
	{
		var brush = new SolidColorBrush(Colors.Red);
		var rect = new Rectangle { Fill = brush };
		var (target, dp) = StoryboardPathResolver.Resolve(rect, new PropertyPath("Fill.Opacity"));
		Assert.Same(brush, target);
		Assert.Same(Brush.OpacityProperty, dp);
	});

	[Fact]
	public void Frozen_brush_rejects_an_opacity_write()
	{
		var b = new SolidColorBrush(Colors.Red);
		b.Freeze();
		Assert.Throws<InvalidOperationException>(() => b.Opacity = 0.5);
	}

	[Fact]
	public void Setting_opacity_raises_Changed()
	{
		var b = new SolidColorBrush(Colors.Red);
		var count = 0;
		b.Changed += (s, e) => count++;
		b.Opacity = 0.25;
		Assert.Equal(1, count);
		Assert.Equal(0.25, b.Opacity);
	}

	[Fact]
	public void Clone_preserves_opacity_and_default_is_one()
	{
		Assert.Equal(1.0, new SolidColorBrush().Opacity);
		var b = new SolidColorBrush(Colors.Red) { Opacity = 0.4 };
		Assert.Equal(0.4, ((SolidColorBrush)b.Clone()).Opacity);
	}
}
