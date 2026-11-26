using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Xunit;
using WpfSlider = System.Windows.Controls.Slider;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class SliderRoutedEventTests : WpfGtkTestBase
{
	sealed class Probe : WpfSlider
	{
		public List<(double, double)> Calls = new List<(double, double)>();
		protected override void OnValueChanged(double oldValue, double newValue)
		{
			Calls.Add((oldValue, newValue));
			base.OnValueChanged(oldValue, newValue);
		}
	}

	[Fact]
	public void ValueChanged_is_a_routed_event_with_old_and_new_value() => Run(() =>
	{
		var s = new WpfSlider { Maximum = 100 };
		Assert.Equal(RoutingStrategy.Bubble, RangeBase.ValueChangedEvent.RoutingStrategy);
		Assert.Equal(typeof(RoutedPropertyChangedEventHandler<double>), RangeBase.ValueChangedEvent.HandlerType);
		Assert.IsAssignableFrom<RangeBase>(s);
		RoutedPropertyChangedEventArgs<double>? seen = null;
		s.ValueChanged += (sender, e) => seen = e;
		s.Value = 40;
		Assert.Equal(0, seen!.OldValue);
		Assert.Equal(40, seen.NewValue);
		Assert.Same(RangeBase.ValueChangedEvent, seen.RoutedEvent);
		Assert.Same(s, seen.Source);
	});

	[Fact]
	public void ValueChanged_bubbles_to_a_parent() => Run(() =>
	{
		var s = new WpfSlider { Maximum = 100 };
		var panel = new StackPanel();
		panel.Children.Add(s);
		double got = -1;
		panel.AddHandler(RangeBase.ValueChangedEvent,
			new RoutedPropertyChangedEventHandler<double>((_, e) => got = e.NewValue));
		s.Value = 12;
		Assert.Equal(12, got);
	});

	[Fact]
	public void OnValueChanged_is_a_protected_virtual_called_before_handlers() => Run(() =>
	{
		var s = new Probe();
		var order = new List<string>();
		s.ValueChanged += (_, _) => order.Add("handler");
		s.Value = 5;
		Assert.Equal(new[] { (0.0, 5.0) }, s.Calls);
		Assert.Single(order);
	});

	[Fact]
	public void A_clamped_value_reports_the_coerced_value() => Run(() =>
	{
		var s = new Probe { Maximum = 100 };
		s.Value = 500;
		Assert.Equal((0.0, 100.0), s.Calls[0]);
	});

	[Fact]
	public void GTK_origin_changes_raise_the_routed_event() => Run(() =>
	{
		var s = new WpfSlider { Maximum = 100 };
		double got = -1;
		s.ValueChanged += (_, e) => got = e.NewValue;
		var scale = (Gtk.Scale)s.Widget;
		scale.Value = 33;
		Assert.Equal(33, got);
	});

	[Fact]
	public void AutoToolTip_properties_have_WPF_defaults_and_store_values() => Run(() =>
	{
		var s = new WpfSlider { Maximum = 100 };
		Assert.Equal(AutoToolTipPlacement.None, s.AutoToolTipPlacement);
		Assert.Equal(0, s.AutoToolTipPrecision);
		s.AutoToolTipPlacement = AutoToolTipPlacement.BottomRight;
		s.AutoToolTipPrecision = 2;
		Assert.Equal(AutoToolTipPlacement.BottomRight, s.AutoToolTipPlacement);
		Assert.Equal(2, s.AutoToolTipPrecision);
		Assert.Throws<ArgumentException>(() => s.AutoToolTipPrecision = -1);
		Assert.Equal(new[] { 0, 1, 2 }, new[] { (int)AutoToolTipPlacement.None, 1, 2 });
	});

	static WpfSlider Configured() =>
		new WpfSlider { Minimum = 0, Maximum = 100, SmallChange = 2, LargeChange = 20, Value = 50 };

	[Fact]
	public void The_four_step_commands_change_Value_by_SmallChange_and_LargeChange() => Run(() =>
	{
		var s = Configured();
		Assert.True(WpfSlider.IncreaseSmall.CanExecute(null, s));
		WpfSlider.IncreaseSmall.Execute(null, s);
		Assert.Equal(52, s.Value);
		WpfSlider.DecreaseSmall.Execute(null, s);
		WpfSlider.DecreaseSmall.Execute(null, s);
		Assert.Equal(48, s.Value);
		WpfSlider.IncreaseLarge.Execute(null, s);
		Assert.Equal(68, s.Value);
		WpfSlider.DecreaseLarge.Execute(null, s);
		Assert.Equal(48, s.Value);
	});

	[Fact]
	public void MinimizeValue_and_MaximizeValue_jump_to_the_ends() => Run(() =>
	{
		var s = Configured();
		WpfSlider.MaximizeValue.Execute(null, s);
		Assert.Equal(100, s.Value);
		WpfSlider.MinimizeValue.Execute(null, s);
		Assert.Equal(0, s.Value);
	});

	[Fact]
	public void Commands_are_clamped_at_the_range_ends() => Run(() =>
	{
		var s = Configured();
		s.Value = 99;
		WpfSlider.IncreaseLarge.Execute(null, s);
		Assert.Equal(100, s.Value);
		s.Value = 1;
		WpfSlider.DecreaseLarge.Execute(null, s);
		Assert.Equal(0, s.Value);
	});

	[Fact]
	public void The_commands_are_RoutedCommands_owned_by_Slider() => Run(() =>
	{
		foreach (var c in new[]
		{
			WpfSlider.IncreaseLarge, WpfSlider.DecreaseLarge, WpfSlider.IncreaseSmall, WpfSlider.DecreaseSmall,
			WpfSlider.MinimizeValue, WpfSlider.MaximizeValue,
		})
		{
			Assert.IsType<RoutedCommand>(c);
			Assert.Equal(typeof(WpfSlider), c.OwnerType);
		}
		Assert.Equal("IncreaseLarge", WpfSlider.IncreaseLarge.Name);
	});

	[Fact]
	public void A_command_bound_button_reaches_the_slider_through_CommandTarget() => Run(() =>
	{
		var s = Configured();
		var panel = new StackPanel();
		var up = new RepeatButton { Command = WpfSlider.IncreaseSmall, CommandTarget = s };
		panel.Children.Add(s);
		panel.Children.Add(up);
		Assert.True(WpfSlider.IncreaseSmall.CanExecute(null, s));
		WpfSlider.IncreaseSmall.Execute(null, up.CommandTarget as UIElement);
		Assert.Equal(52, s.Value);
	});
}
