using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

/// <summary>Callback-only metadata ctors, metadata reuse rules and validation order.</summary>
public class MetadataReviewFixTests
{
	class Owner : DependencyObject { }

	class Derived : Owner { }

	static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

	static object? Coerce(DependencyObject d, object? v) => v;

	static object? OtherCoerce(DependencyObject d, object? v) => v;

	[Fact]
	public void Callback_and_coerce_ctor_leaves_the_default_unset_so_value_types_get_default_of_T()
	{
		var dp = DependencyProperty.Register("X", typeof(double), typeof(Owner),
			new FrameworkPropertyMetadata(Changed, Coerce));

		Assert.Equal(0.0, dp.GetMetadata(typeof(Owner)).DefaultValue);
		Assert.Equal(0.0, new Owner().GetValue(dp));
	}

	[Fact]
	public void Callback_and_coerce_override_keeps_the_base_default_and_uses_its_own_coerce()
	{
		var dp = DependencyProperty.Register("Y", typeof(double), typeof(Owner), new FrameworkPropertyMetadata(3.0));
		CoerceValueCallback coerce = OtherCoerce;

		dp.OverrideMetadata(typeof(Derived), new FrameworkPropertyMetadata(Changed, coerce));

		var meta = dp.GetMetadata(typeof(Derived));
		Assert.Equal(3.0, meta.DefaultValue);
		Assert.Equal(coerce, meta.CoerceValueCallback);
		Assert.Equal(3.0, new Derived().GetValue(dp));
	}

	[Fact]
	public void Reusing_a_metadata_instance_throws()
	{
		var meta = new PropertyMetadata(1);
		DependencyProperty.Register("Reuse1", typeof(int), typeof(Owner), meta);

		Assert.Throws<ArgumentException>(() => DependencyProperty.Register("Reuse2", typeof(int), typeof(Owner), meta));

		var other = DependencyProperty.Register("Reuse3", typeof(int), typeof(Owner), new PropertyMetadata(1));
		Assert.Throws<ArgumentException>(() => other.OverrideMetadata(typeof(Derived), meta));
	}

	[Fact]
	public void Second_override_for_the_same_type_throws()
	{
		var dp = DependencyProperty.Register("Twice", typeof(int), typeof(Owner), new PropertyMetadata(1));
		dp.OverrideMetadata(typeof(Derived), new PropertyMetadata(2));

		Assert.Throws<ArgumentException>(() => dp.OverrideMetadata(typeof(Derived), new PropertyMetadata(3)));
		Assert.Equal(2, dp.GetMetadata(typeof(Derived)).DefaultValue);
	}

	[Fact]
	public void Rejected_override_default_throws_and_leaves_the_metadata_unsealed()
	{
		var dp = DependencyProperty.Register("Valid", typeof(int), typeof(Owner), new PropertyMetadata(1),
			v => (int)v! > 0);
		var meta = new PropertyMetadata(-5);

		Assert.Throws<ArgumentException>(() => dp.OverrideMetadata(typeof(Derived), meta));

		meta.DefaultValue = 5; // would throw InvalidOperationException if sealed
		dp.OverrideMetadata(typeof(Derived), meta);
		Assert.Equal(5, dp.GetMetadata(typeof(Derived)).DefaultValue);
	}

	[Fact]
	public void Throwing_coerce_callback_stores_nothing()
	{
		bool fail = false;
		var dp = DependencyProperty.Register("Coerced", typeof(int), typeof(Owner),
			new PropertyMetadata(1, null, (d, v) => fail ? throw new InvalidOperationException() : v));
		var o = new Owner();
		o.SetValue(dp, 5);

		fail = true;
		Assert.Throws<InvalidOperationException>(() => o.SetValue(dp, 9));
		fail = false;

		Assert.Equal(5, o.GetValue(dp));
		Assert.Equal(5, o.ReadLocalValue(dp));
	}

	[Fact]
	public void Throwing_coerce_on_a_first_set_leaves_no_local_value()
	{
		var dp = DependencyProperty.Register("CoercedFresh", typeof(int), typeof(Owner),
			new PropertyMetadata(1, null, (d, v) => (int)v! == 9 ? throw new InvalidOperationException() : v));
		var o = new Owner();

		Assert.Throws<InvalidOperationException>(() => o.SetValue(dp, 9));

		Assert.Equal(1, o.GetValue(dp));
		Assert.Same(DependencyProperty.UnsetValue, o.ReadLocalValue(dp));
	}
}
