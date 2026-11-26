using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using Xunit;
using WpfProgressBar = System.Windows.Controls.ProgressBar;
using WpfSlider = System.Windows.Controls.Slider;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class RangeBaseTests : WpfGtkTestBase
{
	sealed class Range : RangeBase { }

	[Fact]
	public void Defaults_match_WPF()
	{
		var r = new Range();
		Assert.Equal(0.0, r.Minimum);
		Assert.Equal(1.0, r.Maximum);
		Assert.Equal(0.0, r.Value);
		Assert.Equal(0.1, r.SmallChange);
		Assert.Equal(1.0, r.LargeChange);
	}

	[Fact]
	public void Slider_and_ProgressBar_share_the_RangeBase_properties()
	{
		Assert.Same(RangeBase.ValueProperty, WpfSlider.ValueProperty);
		Assert.Same(RangeBase.MinimumProperty, WpfSlider.MinimumProperty);
		Assert.Same(RangeBase.MaximumProperty, WpfProgressBar.MaximumProperty);
		Assert.Same(RangeBase.ValueProperty, WpfProgressBar.ValueProperty);
	}

	[Fact]
	public void Slider_Maximum_defaults_to_10_and_ProgressBar_to_100()
	{
		Assert.Equal(10.0, new WpfSlider().Maximum);
		Assert.Equal(100.0, new WpfProgressBar().Maximum);
		Assert.Equal(1.0, new Range().Maximum);
	}

	[Fact]
	public void Value_binds_two_way_by_default_and_journals()
	{
		var meta = (FrameworkPropertyMetadata)RangeBase.ValueProperty.GetMetadata(typeof(Range));
		Assert.True(meta.BindsTwoWayByDefault);
		Assert.True(meta.Journal);
	}

	[Theory]
	[InlineData(double.NaN)]
	[InlineData(double.PositiveInfinity)]
	[InlineData(double.NegativeInfinity)]
	public void Minimum_Maximum_Value_reject_non_finite(double bad)
	{
		var r = new Range();
		Assert.Throws<ArgumentException>(() => r.Minimum = bad);
		Assert.Throws<ArgumentException>(() => r.Maximum = bad);
		Assert.Throws<ArgumentException>(() => r.Value = bad);
		Assert.Throws<ArgumentException>(() => r.SmallChange = bad);
		Assert.Throws<ArgumentException>(() => r.LargeChange = bad);
	}

	[Fact]
	public void Changes_reject_negative()
	{
		var r = new Range();
		Assert.Throws<ArgumentException>(() => r.SmallChange = -1);
		Assert.Throws<ArgumentException>(() => r.LargeChange = -0.5);
	}

	[Fact]
	public void Value_is_coerced_into_the_range()
	{
		var r = new Range { Maximum = 5 };
		r.Value = 9;
		Assert.Equal(5.0, r.Value);
		r.Value = -3;
		Assert.Equal(0.0, r.Value);
	}

	[Fact]
	public void Raising_Minimum_above_Value_coerces_Maximum_then_Value()
	{
		var r = new Range { Maximum = 10, Value = 4 };
		r.Minimum = 6;
		Assert.Equal(6.0, r.Value);
		Assert.Equal(10.0, r.Maximum);
		r.Minimum = 20;
		Assert.Equal(20.0, r.Maximum);
		Assert.Equal(20.0, r.Value);
	}

	[Fact]
	public void Coercion_is_reversible_so_the_requested_value_returns()
	{
		var r = new Range { Maximum = 10, Value = 8 };
		r.Maximum = 5;
		Assert.Equal(5.0, r.Value);
		r.Maximum = 10;
		Assert.Equal(8.0, r.Value);
	}

	[Fact]
	public void ValueChanged_is_raised_once_per_effective_change()
	{
		var r = new Range { Maximum = 10 };
		var seen = new List<(double, double)>();
		r.ValueChanged += (_, e) => seen.Add((e.OldValue, e.NewValue));
		r.Value = 3;
		r.Value = 3;
		r.Value = 50;
		r.Value = 10;
		Assert.Equal(new[] { (0.0, 3.0), (3.0, 10.0) }, seen);
	}

	[Fact]
	public void ValueChanged_fires_when_the_range_squeezes_the_value()
	{
		var r = new Range { Maximum = 10, Value = 8 };
		var seen = new List<double>();
		r.ValueChanged += (_, e) => seen.Add(e.NewValue);
		r.Maximum = 5;
		Assert.Equal(new[] { 5.0 }, seen);
	}

	[Fact]
	public void Virtual_hooks_run_for_each_property()
	{
		var r = new Hooked();
		r.Minimum = 1;
		r.Maximum = 9;
		r.Value = 4;
		Assert.Equal(new[] { "val:0>1", "min:0>1", "max:1>9", "val:1>4" }, r.Log);
	}

	sealed class Hooked : RangeBase
	{
		public List<string> Log { get; } = new List<string>();
		protected override void OnMinimumChanged(double o, double n) => Log.Add($"min:{o}>{n}");
		protected override void OnMaximumChanged(double o, double n) => Log.Add($"max:{o}>{n}");
		protected override void OnValueChanged(double o, double n)
		{
			Log.Add($"val:{o}>{n}");
			base.OnValueChanged(o, n);
		}
	}

	[Fact]
	public void Slider_Value_reaches_the_real_Gtk_Scale()
	{
		Run(() =>
		{
			var s = new WpfSlider();
			var scale = (Gtk.Scale)s.Widget;
			s.Value = 7;
			Assert.Equal(7.0, scale.Value);
			s.Maximum = 4;
			Assert.Equal(4.0, s.Value);
			Assert.Equal(4.0, scale.Value);
			scale.Value = 2;
			Assert.Equal(2.0, s.Value);
		});
	}

	[Fact]
	public void ProgressBar_fills_from_the_range()
	{
		Run(() =>
		{
			var p = new WpfProgressBar { Value = 25 };
			var bar = (Gtk.ProgressBar)p.Widget;
			Assert.Equal(0.25, bar.Fraction, 6);
			p.Maximum = 50;
			Assert.Equal(0.5, bar.Fraction, 6);
		});
	}
}
