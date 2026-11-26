using System;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;
// ── Minimal test-domain types ────────────────────────────────────────────────────

class Widget : DependencyObject
{
	public static readonly DependencyProperty NameProp =
		DependencyProperty.Register(nameof(NameProp), typeof(string), typeof(Widget),
			new PropertyMetadata("default"));

	public static readonly DependencyProperty ScoreProp =
		DependencyProperty.Register(nameof(ScoreProp), typeof(int), typeof(Widget),
			new PropertyMetadata(0));

	public static readonly DependencyProperty CallbackProp =
		DependencyProperty.Register(nameof(CallbackProp), typeof(string), typeof(Widget),
			new PropertyMetadata(null, OnCallbackPropChanged));

	static void OnCallbackPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((Widget)d).LastChange = (e.OldValue, e.NewValue);

	public (object? Old, object? New) LastChange;

	public static readonly DependencyProperty CoercedProp =
		DependencyProperty.Register(nameof(CoercedProp), typeof(int), typeof(Widget),
			new PropertyMetadata(0, null, CoerceNotNegative));

	static object? CoerceNotNegative(DependencyObject d, object? value) =>
		value is int i && i < 0 ? 0 : value;

	// Attached property declared on a separate type to avoid registry clashes.
	public static readonly DependencyProperty AttachedFontSizeProp =
		DependencyProperty.RegisterAttached("AttachedFontSize", typeof(double), typeof(Widget),
			new PropertyMetadata(12.0));

	public static double GetAttachedFontSize(DependencyObject target) =>
		(double)target.GetValue(AttachedFontSizeProp)!;
	public static void SetAttachedFontSize(DependencyObject target, double value) =>
		target.SetValue(AttachedFontSizeProp, value);
}

// ── Tests ────────────────────────────────────────────────────────────────────────

public class DependencyPropertyTests
{
	[Fact]
	public void DefaultValue_comes_from_metadata()
	{
		var w = new Widget();
		Assert.Equal("default", w.GetValue(Widget.NameProp));
	}

	[Fact]
	public void DefaultValue_int_is_zero_when_not_specified()
	{
		var w = new Widget();
		Assert.Equal(0, w.GetValue(Widget.ScoreProp));
	}

	[Fact]
	public void SetValue_stores_local_value()
	{
		var w = new Widget();
		w.SetValue(Widget.NameProp, "hello");
		Assert.Equal("hello", w.GetValue(Widget.NameProp));
	}

	[Fact]
	public void ClearValue_reverts_to_default()
	{
		var w = new Widget();
		w.SetValue(Widget.NameProp, "hello");
		w.ClearValue(Widget.NameProp);
		Assert.Equal("default", w.GetValue(Widget.NameProp));
	}

	[Fact]
	public void ReadLocalValue_returns_UnsetValue_when_not_set()
	{
		var w = new Widget();
		Assert.Same(DependencyProperty.UnsetValue, w.ReadLocalValue(Widget.NameProp));
	}

	[Fact]
	public void ReadLocalValue_returns_local_value_after_set()
	{
		var w = new Widget();
		w.SetValue(Widget.NameProp, "set");
		Assert.Equal("set", w.ReadLocalValue(Widget.NameProp));
	}

	[Fact]
	public void PropertyChangedCallback_fires_with_correct_old_and_new()
	{
		var w = new Widget();
		w.SetValue(Widget.CallbackProp, "first");
		w.SetValue(Widget.CallbackProp, "second");
		Assert.Equal(("first", "second"), w.LastChange);
	}

	[Fact]
	public void PropertyChangedCallback_fires_null_old_on_first_set()
	{
		var w = new Widget();
		w.SetValue(Widget.CallbackProp, "initial");
		Assert.Equal((null, (object?)"initial"), w.LastChange);
	}

	[Fact]
	public void PropertyChangedCallback_does_not_fire_when_value_unchanged()
	{
		var w = new Widget();
		w.SetValue(Widget.CallbackProp, "same");
		w.LastChange = default;
		w.SetValue(Widget.CallbackProp, "same"); // no change
		Assert.Equal(default, w.LastChange);
	}

	[Fact]
	public void CoerceValueCallback_clamps_negative_to_zero()
	{
		var w = new Widget();
		w.SetValue(Widget.CoercedProp, -5);
		Assert.Equal(0, w.GetValue(Widget.CoercedProp));
	}

	[Fact]
	public void CoerceValueCallback_passes_positive_through()
	{
		var w = new Widget();
		w.SetValue(Widget.CoercedProp, 10);
		Assert.Equal(10, w.GetValue(Widget.CoercedProp));
	}

	[Fact]
	public void RegisterAttached_default_value_readable_on_any_object()
	{
		var target = new DependencyObject();
		Assert.Equal(12.0, Widget.GetAttachedFontSize(target));
	}

	[Fact]
	public void RegisterAttached_set_and_get_round_trip()
	{
		var target = new DependencyObject();
		Widget.SetAttachedFontSize(target, 24.0);
		Assert.Equal(24.0, Widget.GetAttachedFontSize(target));
	}

	[Fact]
	public void Register_duplicate_name_on_same_owner_throws()
	{
		// nameof is a constant, so run the type initializer first: Widget.NameProp must exist already.
		System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(Widget).TypeHandle);
		Assert.Throws<InvalidOperationException>(() =>
			DependencyProperty.Register(nameof(Widget.NameProp), typeof(string), typeof(Widget)));
	}
}