using System;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class PropertyMetadataSealMergeTests
{
	class Base : DependencyObject
	{
		public static int BaseCalls;

		public static readonly DependencyProperty P = DependencyProperty.Register(
			"SmP", typeof(int), typeof(Base),
			new FrameworkPropertyMetadata(
				7, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure,
				(_, _) => BaseCalls++, (_, v) => v));
	}

	class Derived : Base { }

	class DerivedWithFlags : Base { }

	class Derived2 : Base { }

	class Derived3 : Base { }

	[Fact]
	public void Register_seals_metadata_so_setters_throw()
	{
		var m = new FrameworkPropertyMetadata(1);
		DependencyProperty.Register("SealP", typeof(int), typeof(PropertyMetadataSealMergeTests), m);
		Assert.Throws<InvalidOperationException>(() => m.DefaultValue = 2);
		Assert.Throws<InvalidOperationException>(() => m.PropertyChangedCallback = null);
		Assert.Throws<InvalidOperationException>(() => m.CoerceValueCallback = null);
		Assert.Throws<InvalidOperationException>(() => m.BindsTwoWayByDefault = true);
		Assert.Throws<InvalidOperationException>(() => m.Inherits = true);
		Assert.Throws<InvalidOperationException>(() => m.DefaultUpdateSourceTrigger = UpdateSourceTrigger.Explicit);
	}

	[Fact]
	public void DefaultValue_Unset_throws_ArgumentException()
	{
		Assert.Throws<ArgumentException>(() => new PropertyMetadata().DefaultValue = DependencyProperty.UnsetValue);
	}

	[Fact]
	public void Override_inherits_unset_default_callbacks_and_flags_from_base()
	{
		Base.P.OverrideMetadata(typeof(Derived), new FrameworkPropertyMetadata());
		var m = (FrameworkPropertyMetadata)Base.P.GetMetadata(typeof(Derived));
		Assert.Equal(7, m.DefaultValue);
		Assert.NotNull(m.CoerceValueCallback);
		Assert.True(m.BindsTwoWayByDefault);
		Assert.True(m.AffectsMeasure);
	}

	[Fact]
	public void Override_with_explicit_value_wins_and_chains_change_callbacks_base_first()
	{
		var order = new System.Collections.Generic.List<string>();
		var p = DependencyProperty.Register("ChainP", typeof(int), typeof(Derived2),
			new FrameworkPropertyMetadata(0, (_, _) => order.Add("base")));
		p.OverrideMetadata(typeof(Derived3), new FrameworkPropertyMetadata(5, (_, _) => order.Add("derived")));
		var d = new Derived3();
		Assert.Equal(5, d.GetValue(p));
		d.SetValue(p, 9);
		Assert.Equal(new[] { "base", "derived" }, order);
	}

	[Fact]
	public void Override_explicit_flag_false_overrides_base_true()
	{
		Base.P.OverrideMetadata(typeof(DerivedWithFlags), new FrameworkPropertyMetadata { BindsTwoWayByDefault = false });
		var m = (FrameworkPropertyMetadata)Base.P.GetMetadata(typeof(DerivedWithFlags));
		Assert.False(m.BindsTwoWayByDefault);
		Assert.True(m.AffectsMeasure);
	}

	[Fact]
	public void Override_with_base_type_metadata_throws()
	{
		var p = DependencyProperty.Register("TypeP", typeof(int), typeof(Derived3), new FrameworkPropertyMetadata(0));
		Assert.Throws<ArgumentException>(() => p.OverrideMetadata(typeof(Derived2), new PropertyMetadata(1)));
	}

	[Fact]
	public void Override_with_invalid_default_throws()
	{
		var p = DependencyProperty.Register("OvInvalid", typeof(int), typeof(Derived3),
			new PropertyMetadata(1), v => (int)v! > 0);
		Assert.Throws<ArgumentException>(() => p.OverrideMetadata(typeof(Derived2), new PropertyMetadata(-1)));
	}

	[Fact]
	public void AddOwner_with_metadata_merges_and_validates()
	{
		var p = DependencyProperty.Register("AoMerge", typeof(int), typeof(Base),
			new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault), v => (int)v! >= 0);
		p.AddOwner(typeof(Derived2), new FrameworkPropertyMetadata());
		var m = (FrameworkPropertyMetadata)p.GetMetadata(typeof(Derived2));
		Assert.Equal(3, m.DefaultValue);
		Assert.True(m.BindsTwoWayByDefault);
		Assert.Throws<ArgumentException>(() => p.AddOwner(typeof(Derived3), new FrameworkPropertyMetadata(-5)));
	}

	[Fact]
	public void Existing_three_arg_PropertyMetadata_ctor_still_chains()
	{
		var m = new PropertyMetadata(1, null, null);
		Assert.Equal(1, m.DefaultValue);
	}
}
