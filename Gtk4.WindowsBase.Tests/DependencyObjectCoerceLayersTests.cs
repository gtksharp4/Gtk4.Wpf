using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>Coercion is applied on top of raw layer values, so it is reversible whatever the source.</summary>
public class DependencyObjectCoerceLayersTests
{
	class Clamp : DependencyObject
	{
		public int Max = 10;

		public static readonly DependencyProperty AmountProperty =
			DependencyProperty.Register("Amount", typeof(int), typeof(Clamp),
				new PropertyMetadata(20, null, (d, v) => Math.Min((int)v!, ((Clamp)d).Max)));
	}

	[Fact]
	public void ClearValue_recoerces_the_default()
	{
		var c = new Clamp();
		c.SetValue(Clamp.AmountProperty, 50);
		c.ClearValue(Clamp.AmountProperty);
		Assert.Equal(10, c.GetValue(Clamp.AmountProperty));
	}

	[Fact]
	public void ReadLocalValue_returns_the_raw_value()
	{
		var c = new Clamp();
		c.SetValue(Clamp.AmountProperty, 50);
		Assert.Equal(10, c.GetValue(Clamp.AmountProperty));
		Assert.Equal(50, c.ReadLocalValue(Clamp.AmountProperty));
	}

	[Fact]
	public void Style_value_is_restored_when_the_coercion_relaxes()
	{
		var c = new Clamp { Max = 5 };
		c.SetStyleValue(Clamp.AmountProperty, 8);
		Assert.Equal(5, c.GetValue(Clamp.AmountProperty));

		c.Max = 100;
		c.CoerceValue(Clamp.AmountProperty);
		Assert.Equal(8, c.GetValue(Clamp.AmountProperty));
	}

	[Fact]
	public void Trigger_dynamic_and_animation_values_recoerce_on_set_and_clear()
	{
		var c = new Clamp { Max = 5 };
		var owner = new object();
		c.SetTriggerValue(Clamp.AmountProperty, owner, 8);
		Assert.Equal(5, c.GetValue(Clamp.AmountProperty));
		c.Max = 100;
		c.CoerceValue(Clamp.AmountProperty);
		Assert.Equal(8, c.GetValue(Clamp.AmountProperty));

		c.Max = 3;
		c.ClearTriggerValue(Clamp.AmountProperty, owner);
		Assert.Equal(3, c.GetValue(Clamp.AmountProperty)); // default 20 is re-coerced

		c.SetAnimationValue(Clamp.AmountProperty, 2);
		c.SetAnimationValue(Clamp.AmountProperty, 9);
		Assert.Equal(3, c.GetValue(Clamp.AmountProperty));
		c.ClearAnimationValue(Clamp.AmountProperty);
		c.SetDynResValue(Clamp.AmountProperty, 7);
		Assert.Equal(3, c.GetValue(Clamp.AmountProperty));
		c.Max = 50;
		c.CoerceValue(Clamp.AmountProperty);
		Assert.Equal(7, c.GetValue(Clamp.AmountProperty));
	}

	[Fact]
	public void ClearStyleValues_recoerces_the_remaining_default()
	{
		var c = new Clamp { Max = 5 };
		c.SetStyleValue(Clamp.AmountProperty, 2);
		var snap = c.SnapshotStyleValues();
		c.ClearStyleValues(snap);
		Assert.Equal(5, c.GetValue(Clamp.AmountProperty));
	}
}
