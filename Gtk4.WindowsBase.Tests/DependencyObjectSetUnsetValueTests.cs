using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>SetValue(dp, UnsetValue) behaves like ClearValue(dp).</summary>
public class DependencyObjectSetUnsetValueTests
{
	class Clamp : DependencyObject
	{
		public int Changes;

		public static readonly DependencyProperty AmountProperty =
			DependencyProperty.Register("Amount", typeof(int), typeof(Clamp),
				new PropertyMetadata(20, (d, e) => ((Clamp)d).Changes++, (d, v) => System.Math.Min((int)v!, 10)));
	}

	[Fact]
	public void Removes_the_local_value()
	{
		var c = new Clamp();
		c.SetValue(Clamp.AmountProperty, 5);
		c.SetValue(Clamp.AmountProperty, DependencyProperty.UnsetValue);
		Assert.Same(DependencyProperty.UnsetValue, c.ReadLocalValue(Clamp.AmountProperty));
	}

	[Fact]
	public void Reverts_to_the_recoerced_default_and_notifies()
	{
		var c = new Clamp();
		c.SetValue(Clamp.AmountProperty, 5);
		c.Changes = 0;
		c.SetValue(Clamp.AmountProperty, DependencyProperty.UnsetValue);
		Assert.Equal(10, c.GetValue(Clamp.AmountProperty));
		Assert.Equal(1, c.Changes);
	}

	[Fact]
	public void Is_a_no_op_without_a_local_value()
	{
		var c = new Clamp();
		c.SetValue(Clamp.AmountProperty, DependencyProperty.UnsetValue);
		Assert.Equal(0, c.Changes);
		Assert.Same(DependencyProperty.UnsetValue, c.ReadLocalValue(Clamp.AmountProperty));
	}
}
