using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class StyleSealTests : WpfGtkTestBase
{
	[Fact]
	public void The_default_focus_visual_style_is_sealed() => Run(() =>
	{
		var style = new Button().FocusVisualStyle!;
		Assert.True(style.IsSealed);
		Assert.Throws<InvalidOperationException>(() =>
			style.Setters.Add(new Setter(UIElement.OpacityProperty, 0.5)));
		Assert.Throws<InvalidOperationException>(() => style.TargetType = typeof(Button));
		Assert.Throws<InvalidOperationException>(() => style.BasedOn = new Style());
		Assert.Throws<InvalidOperationException>(() => style.Triggers.Clear());
	});

	[Fact]
	public void A_fresh_style_stays_mutable_until_sealed() => Run(() =>
	{
		var style = new Style();
		Assert.False(style.IsSealed);
		style.Setters.Add(new Setter(UIElement.OpacityProperty, 0.5));
		style.TargetType = typeof(Button);
		style.Seal();
		Assert.True(style.IsSealed);
		Assert.Throws<InvalidOperationException>(() => style.Setters.RemoveAt(0));
		Assert.Equal(typeof(Button), style.TargetType);
	});
}
