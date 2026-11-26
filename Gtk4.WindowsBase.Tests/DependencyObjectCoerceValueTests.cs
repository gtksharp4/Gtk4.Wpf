using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>DependencyObject.CoerceValue / InvalidateProperty against the stored base value.</summary>
public class DependencyObjectCoerceValueTests
{
	class Box : DependencyObject
	{
		public int Max = 10;
		public int Callbacks;

		public static readonly DependencyProperty AmountProperty =
			DependencyProperty.Register("Amount", typeof(int), typeof(Box),
				new PropertyMetadata(0, (d, e) => ((Box)d).Callbacks++, Coerce));

		static object? Coerce(DependencyObject d, object? v) => Math.Min((int)v!, ((Box)d).Max);
	}

	class Plain : DependencyObject
	{
		public static readonly DependencyProperty NProperty =
			DependencyProperty.Register("N", typeof(int), typeof(Plain), new PropertyMetadata(0));
	}

	[Fact]
	public void CoerceValue_reruns_coercion_against_the_base_value()
	{
		var b = new Box();
		b.SetValue(Box.AmountProperty, 50);
		Assert.Equal(10, b.GetValue(Box.AmountProperty));

		b.Max = 20;
		b.CoerceValue(Box.AmountProperty);

		Assert.Equal(20, b.GetValue(Box.AmountProperty));
	}

	[Fact]
	public void Relaxed_coercion_restores_the_base_value()
	{
		var b = new Box();
		b.SetValue(Box.AmountProperty, 50);
		b.Max = 100;
		b.CoerceValue(Box.AmountProperty);
		Assert.Equal(50, b.GetValue(Box.AmountProperty));
	}

	[Fact]
	public void CoerceValue_raises_change_callback_only_on_effective_change()
	{
		var b = new Box();
		b.SetValue(Box.AmountProperty, 50);
		int before = b.Callbacks;

		b.CoerceValue(Box.AmountProperty);
		Assert.Equal(before, b.Callbacks);

		b.Max = 5;
		b.CoerceValue(Box.AmountProperty);
		Assert.Equal(before + 1, b.Callbacks);
	}

	[Fact]
	public void CoerceValue_without_a_local_value_coerces_the_default()
	{
		var b = new Box { Max = -1 };
		b.CoerceValue(Box.AmountProperty);
		Assert.Equal(-1, b.GetValue(Box.AmountProperty));

		b.Max = 10;
		b.CoerceValue(Box.AmountProperty);
		Assert.Equal(0, b.GetValue(Box.AmountProperty));
	}

	[Fact]
	public void InvalidateProperty_reevaluates_the_effective_value()
	{
		var b = new Box();
		b.SetValue(Box.AmountProperty, 7);
		b.Max = 3;
		b.InvalidateProperty(Box.AmountProperty);
		Assert.Equal(3, b.GetValue(Box.AmountProperty));
	}

	[Fact]
	public void ClearValue_drops_the_base_value()
	{
		var b = new Box();
		b.SetValue(Box.AmountProperty, 50);
		b.ClearValue(Box.AmountProperty);
		b.Max = 100;
		b.CoerceValue(Box.AmountProperty);
		Assert.Equal(0, b.GetValue(Box.AmountProperty));
	}

	[Fact]
	public void SetValue_after_a_coercion_only_result_replaces_it()
	{
		var b = new Box { Max = -1 };
		b.CoerceValue(Box.AmountProperty);
		b.Max = 10;
		b.SetValue(Box.AmountProperty, 4);
		Assert.Equal(4, b.GetValue(Box.AmountProperty));
	}

	[Fact]
	public void Null_property_throws_ArgumentNullException()
	{
		var b = new Box();
		Assert.Throws<ArgumentNullException>(() => b.CoerceValue(null!));
		Assert.Throws<ArgumentNullException>(() => b.InvalidateProperty(null!));
	}

	[Fact]
	public void CoerceValue_on_a_property_without_a_callback_is_a_no_op()
	{
		var b = new Plain();
		b.SetValue(Plain.NProperty, 4);
		b.CoerceValue(Plain.NProperty);
		Assert.Equal(4, b.GetValue(Plain.NProperty));
	}
}
