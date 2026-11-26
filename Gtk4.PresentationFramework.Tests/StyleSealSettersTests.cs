using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class StyleSealSettersTests : WpfGtkTestBase
{
	[Fact]
	public void A_sealed_style_seals_its_setters_and_trigger_setters() => Run(() =>
	{
		var style = new Style(typeof(Button));
		style.Setters.Add(new Setter(Control.WidthProperty, 10.0));
		var trigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
		trigger.Setters.Add(new Setter(Control.HeightProperty, 5.0));
		style.Triggers.Add(trigger);
		var loose = new Setter(Control.WidthProperty, 1.0);
		Assert.False(loose.IsSealed);

		style.Seal();

		var setter = (Setter)style.Setters[0];
		Assert.True(setter.IsSealed);
		Assert.Throws<InvalidOperationException>(() => setter.Value = 20.0);
		Assert.Throws<InvalidOperationException>(() => setter.Property = Control.HeightProperty);
		Assert.Throws<InvalidOperationException>(() => setter.TargetName = "x");
		var inner = (Setter)trigger.Setters[0];
		Assert.True(inner.IsSealed);
		Assert.Throws<InvalidOperationException>(() => inner.Value = 6.0);
		loose.Value = 2.0;
	});
}
