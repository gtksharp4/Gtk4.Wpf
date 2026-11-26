using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;

// ── Minimal concrete Freezable for testing ──────────────────────────────────────

class TestFreezable : Freezable
{
	public static readonly DependencyProperty ValueProperty =
		DependencyProperty.Register(nameof(Value), typeof(int), typeof(TestFreezable),
			new PropertyMetadata(0));

	public int Value
	{
		get => (int)(GetValue(ValueProperty) ?? 0);
		set => SetValue(ValueProperty, value);
	}

	protected override Freezable CreateInstanceCore() => new TestFreezable();
}

// ── Tests ────────────────────────────────────────────────────────────────────────

public class FreezableTests
{
	[Fact]
	public void NewInstance_CanFreeze_and_not_frozen()
	{
		var f = new TestFreezable();
		Assert.True(f.CanFreeze);
		Assert.False(f.IsFrozen);
	}

	[Fact]
	public void Freeze_sets_IsFrozen_true()
	{
		var f = new TestFreezable();
		f.Freeze();
		Assert.True(f.IsFrozen);
	}

	[Fact]
	public void Freeze_twice_is_idempotent()
	{
		var f = new TestFreezable();
		f.Freeze();
		f.Freeze(); // must not throw
		Assert.True(f.IsFrozen);
	}

	[Fact]
	public void SetValue_on_frozen_throws()
	{
		var f = new TestFreezable { Value = 1 };
		f.Freeze();
		Assert.Throws<InvalidOperationException>(() => f.Value = 2);
	}

	[Fact]
	public void Clone_produces_unfrozen_copy_with_equal_values()
	{
		var f = new TestFreezable { Value = 42 };
		f.Freeze();

		var clone = f.Clone();

		Assert.False(clone.IsFrozen);
		Assert.IsType<TestFreezable>(clone);
		Assert.Equal(42, ((TestFreezable)clone).Value);
	}

	[Fact]
	public void Clone_original_still_frozen_after_clone()
	{
		var f = new TestFreezable { Value = 7 };
		f.Freeze();
		var clone = f.Clone();
		Assert.True(f.IsFrozen);
		Assert.False(clone.IsFrozen);
	}

	[Fact]
	public void Changed_fires_on_mutation_of_unfrozen_instance()
	{
		var f = new TestFreezable();
		bool fired = false;
		f.Changed += (_, _) => fired = true;

		f.Value = 99;

		Assert.True(fired);
	}

	[Fact]
	public void Changed_does_not_fire_after_freeze()
	{
		var f = new TestFreezable();
		f.Freeze();
		bool fired = false;
		f.Changed += (_, _) => fired = true;

		// Setting on frozen throws; Changed must not have fired.
		try { f.Value = 1; } catch (InvalidOperationException) { }

		Assert.False(fired);
	}

	[Fact]
	public void GetAsFrozen_returns_frozen_clone()
	{
		var f = new TestFreezable { Value = 5 };
		var frozen = f.GetAsFrozen();
		Assert.True(frozen.IsFrozen);
		Assert.Equal(5, ((TestFreezable)frozen).Value);
	}

	[Fact]
	public void CloneCurrentValue_produces_unfrozen_copy()
	{
		var f = new TestFreezable { Value = 3 };
		var clone = f.CloneCurrentValue();
		Assert.False(clone.IsFrozen);
		Assert.Equal(3, ((TestFreezable)clone).Value);
	}
}
