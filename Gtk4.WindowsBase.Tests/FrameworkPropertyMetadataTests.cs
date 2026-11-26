using System;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

public class FrameworkPropertyMetadataTests
{
	[Theory]
	[InlineData(FrameworkPropertyMetadataOptions.AffectsMeasure)]
	[InlineData(FrameworkPropertyMetadataOptions.AffectsArrange)]
	[InlineData(FrameworkPropertyMetadataOptions.AffectsParentMeasure)]
	[InlineData(FrameworkPropertyMetadataOptions.AffectsParentArrange)]
	[InlineData(FrameworkPropertyMetadataOptions.AffectsRender)]
	[InlineData(FrameworkPropertyMetadataOptions.Inherits)]
	[InlineData(FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior)]
	[InlineData(FrameworkPropertyMetadataOptions.NotDataBindable)]
	[InlineData(FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)]
	[InlineData(FrameworkPropertyMetadataOptions.Journal)]
	[InlineData(FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender)]
	public void Options_ctor_maps_each_flag_to_exactly_one_property(FrameworkPropertyMetadataOptions flag)
	{
		var m = new FrameworkPropertyMetadata(0, flag);
		var on = new[]
		{
			m.AffectsMeasure, m.AffectsArrange, m.AffectsParentMeasure, m.AffectsParentArrange, m.AffectsRender,
			m.Inherits, m.OverridesInheritanceBehavior, m.IsNotDataBindable, m.BindsTwoWayByDefault,
			m.Journal, m.SubPropertiesDoNotAffectRender,
		};
		Assert.Single(on, true);
	}

	[Fact]
	public void Options_ctor_combines_flags()
	{
		var m = new FrameworkPropertyMetadata(
			1, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.Journal);
		Assert.True(m.AffectsMeasure);
		Assert.True(m.Journal);
		Assert.False(m.Inherits);
	}

	[Fact]
	public void All_wpf_ctors_carry_default_callbacks_and_flags()
	{
		PropertyChangedCallback pcc = (_, _) => { };
		CoerceValueCallback cvc = (_, v) => v;
		var m = new FrameworkPropertyMetadata(
			5, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, pcc, cvc, true, UpdateSourceTrigger.LostFocus);
		Assert.Equal(5, m.DefaultValue);
		Assert.Same(pcc, m.PropertyChangedCallback);
		Assert.Same(cvc, m.CoerceValueCallback);
		Assert.True(m.IsAnimationProhibited);
		Assert.True(m.BindsTwoWayByDefault);
		Assert.Equal(UpdateSourceTrigger.LostFocus, m.DefaultUpdateSourceTrigger);
		Assert.Same(pcc, new FrameworkPropertyMetadata(pcc).PropertyChangedCallback);
		Assert.Same(cvc, new FrameworkPropertyMetadata(pcc, cvc).CoerceValueCallback);
		Assert.Same(pcc, new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.None, pcc)
			.PropertyChangedCallback);
		Assert.Same(cvc, new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.None, pcc, cvc)
			.CoerceValueCallback);
	}

	[Fact]
	public void Legacy_bool_ctors_still_work()
	{
		var a = new FrameworkPropertyMetadata(1.5, true);
		Assert.True(a.Inherits);
		var b = new FrameworkPropertyMetadata(1.5, false, true, true, true);
		Assert.True(b.AffectsMeasure && b.AffectsArrange && b.AffectsRender);
		Assert.False(b.Inherits);
	}

	[Fact]
	public void Each_flag_setter_round_trips()
	{
		var m = new FrameworkPropertyMetadata(0)
		{
			AffectsParentMeasure = true,
			AffectsParentArrange = true,
			OverridesInheritanceBehavior = true,
			IsNotDataBindable = true,
			BindsTwoWayByDefault = true,
			Journal = true,
			SubPropertiesDoNotAffectRender = true,
		};
		Assert.True(m.AffectsParentMeasure && m.AffectsParentArrange && m.OverridesInheritanceBehavior);
		Assert.True(m.IsNotDataBindable && m.BindsTwoWayByDefault && m.Journal && m.SubPropertiesDoNotAffectRender);
		m.BindsTwoWayByDefault = false;
		Assert.False(m.BindsTwoWayByDefault);
	}

	[Fact]
	public void DefaultUpdateSourceTrigger_defaults_to_PropertyChanged_and_rejects_Default()
	{
		var m = new FrameworkPropertyMetadata(0);
		Assert.Equal(UpdateSourceTrigger.PropertyChanged, m.DefaultUpdateSourceTrigger);
		Assert.Throws<ArgumentException>(() => m.DefaultUpdateSourceTrigger = UpdateSourceTrigger.Default);
	}

	[Fact]
	public void IsDataBindingAllowed_false_when_not_bindable_or_read_only()
	{
		Assert.True(new FrameworkPropertyMetadata(0).IsDataBindingAllowed);
		Assert.False(new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.NotDataBindable)
			.IsDataBindingAllowed);
		var ro = new FrameworkPropertyMetadata(0);
		DependencyProperty.RegisterReadOnly("FpmRo", typeof(int), typeof(FrameworkPropertyMetadataTests), ro);
		Assert.False(ro.IsDataBindingAllowed);
	}

	[Fact]
	public void UIPropertyMetadata_stores_IsAnimationProhibited()
	{
		var m = new UIPropertyMetadata(1, null, null, true);
		Assert.True(m.IsAnimationProhibited);
		m.IsAnimationProhibited = false;
		Assert.False(m.IsAnimationProhibited);
		Assert.IsAssignableFrom<UIPropertyMetadata>(new FrameworkPropertyMetadata(0));
	}
}
