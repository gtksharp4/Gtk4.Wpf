using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Style-setter DynamicResource lives in the style layer, below BasedOn overrides and local links.</summary>
public class StyleLayerResourceGtkTests : WpfGtkTestBase
{
	sealed class Vm
	{
		public Brush Fg { get; set; } = new SolidColorBrush(Colors.Green);
	}

	static Style DynStyle(string key, Style? basedOn = null)
	{
		var s = new Style(typeof(Button)) { BasedOn = basedOn };
		s.Setters.Add(new Setter(Control.BackgroundProperty, new DynamicResourceExtension(key)));
		return s;
	}

	[Fact]
	public void Derived_plain_value_wins_over_base_DynamicResource_and_stays_after_dictionary_change() => Run(() =>
	{
		var red = new SolidColorBrush(Colors.Red);
		var button = new Button();
		button.Resources["B"] = new SolidColorBrush(Colors.Blue);
		var derived = new Style(typeof(Button)) { BasedOn = DynStyle("B") };
		derived.Setters.Add(new Setter(Control.BackgroundProperty, red));

		button.Style = derived;
		Assert.Same(red, button.Background);

		button.Resources["B"] = new SolidColorBrush(Colors.Yellow);
		Assert.Same(red, button.Background);
	});

	[Fact]
	public void Local_resource_reference_survives_style_apply_and_unapply_and_stays_live() => Run(() =>
	{
		var a1 = new SolidColorBrush(Colors.Red);
		var a2 = new SolidColorBrush(Colors.Blue);
		var button = new Button();
		button.Resources["A"] = a1;
		button.Resources["S"] = new SolidColorBrush(Colors.Green);
		button.SetResourceReference(Control.BackgroundProperty, "A");

		button.Style = DynStyle("S");
		Assert.Same(a1, button.Background);

		button.Style = null;
		Assert.Same(a1, button.Background);

		button.Resources["A"] = a2;
		Assert.Same(a2, button.Background);
	});

	[Fact]
	public void Style_DynamicResource_alone_still_applies_and_clears() => Run(() =>
	{
		var s = new SolidColorBrush(Colors.Green);
		var button = new Button();
		button.Resources["S"] = s;
		button.Style = DynStyle("S");
		Assert.Same(s, button.Background);
		button.Style = null;
		Assert.NotSame(s, button.Background);
	});

	[Fact]
	public void Trigger_setter_does_not_show_over_style_Binding_known_gap() => Run(() =>
	{
		var vm = new Vm();
		var style = new Style(typeof(Button));
		style.Setters.Add(new Setter(Control.ForegroundProperty, new Binding("Fg")));
		var trigger = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
		trigger.Setters.Add(new Setter(Control.ForegroundProperty, new SolidColorBrush(Colors.Red)));
		style.Triggers.Add(trigger);
		var button = new Button { DataContext = vm, Style = style };

		button.IsEnabled = false;

		Assert.Same(vm.Fg, button.Foreground);
	});
}
