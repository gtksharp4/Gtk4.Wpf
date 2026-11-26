using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

// ── Minimal type hierarchy for testing ──────────────────────────────────────────

class BaseWidget : DependencyObject { }
class DerivedWidget : BaseWidget { }

// ── Tests ────────────────────────────────────────────────────────────────────────

public class DependencyObjectTypeTests
{
	[Fact]
	public void FromSystemType_round_trips_SystemType()
	{
		var dot = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		Assert.Equal(typeof(BaseWidget), dot.SystemType);
	}

	[Fact]
	public void FromSystemType_Name_matches_type_name()
	{
		var dot = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		Assert.Equal(nameof(BaseWidget), dot.Name);
	}

	[Fact]
	public void FromSystemType_same_type_returns_same_instance()
	{
		var a = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		var b = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		Assert.Same(a, b);
	}

	[Fact]
	public void FromSystemType_different_types_different_ids()
	{
		var a = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		var b = DependencyObjectType.FromSystemType(typeof(DerivedWidget));
		Assert.NotEqual(a.Id, b.Id);
	}

	[Fact]
	public void BaseType_navigates_to_base_DependencyObjectType()
	{
		var derived = DependencyObjectType.FromSystemType(typeof(DerivedWidget));
		var expected = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		Assert.Same(expected, derived.BaseType);
	}

	[Fact]
	public void IsInstanceOfType_true_for_exact_type()
	{
		var dot = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		var obj = new BaseWidget();
		Assert.True(dot.IsInstanceOfType(obj));
	}

	[Fact]
	public void IsInstanceOfType_true_for_derived_type()
	{
		var dot = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		var obj = new DerivedWidget();
		Assert.True(dot.IsInstanceOfType(obj));
	}

	[Fact]
	public void IsInstanceOfType_false_for_null()
	{
		var dot = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		Assert.False(dot.IsInstanceOfType(null));
	}

	[Fact]
	public void IsSubclassOf_true_for_derived()
	{
		var derived = DependencyObjectType.FromSystemType(typeof(DerivedWidget));
		var base_ = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		Assert.True(derived.IsSubclassOf(base_));
	}

	[Fact]
	public void IsSubclassOf_false_for_same()
	{
		var dot = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		Assert.False(dot.IsSubclassOf(dot));
	}

	[Fact]
	public void IsSubclassOf_false_for_base_vs_derived()
	{
		var base_ = DependencyObjectType.FromSystemType(typeof(BaseWidget));
		var derived = DependencyObjectType.FromSystemType(typeof(DerivedWidget));
		Assert.False(base_.IsSubclassOf(derived));
	}

	[Fact]
	public void DependencyObject_DependencyObjectType_property_returns_correct_type()
	{
		var obj = new DerivedWidget();
		var dot = obj.DependencyObjectType;
		Assert.Equal(typeof(DerivedWidget), dot.SystemType);
	}

	[Fact]
	public void FromSystemType_non_DependencyObject_throws()
	{
		Assert.Throws<ArgumentException>(() =>
			DependencyObjectType.FromSystemType(typeof(string)));
	}
}
