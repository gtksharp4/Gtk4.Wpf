using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>
/// Pinning tests for D6 (style precedence, un-apply) and D7 (BasedOn cycle guard,
/// implicit-style guard uses ReadLocalValue).
/// </summary>
public class StylePrecedenceDefectTests
{
	sealed class TestEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("TagD67", typeof(string), typeof(TestEl),
				new PropertyMetadata(null));
		public string? Tag2
		{
			get => (string?)GetValue(TagProperty);
			set => SetValue(TagProperty, value);
		}
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── D6a: local value beats style value regardless of order ────────────────

	/// <summary>D6 — local value set BEFORE style wins over setter.</summary>
	[Fact]
	public void D6_local_value_before_style_wins()
	{
		var el = new TestEl();
		el.Tag2 = "local"; // local set first

		var style = new Style(typeof(TestEl));
		style.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "styled" });
		el.Style = style;

		Assert.Equal("local", el.Tag2);
	}

	/// <summary>D6 — local value set AFTER style wins over setter.</summary>
	[Fact]
	public void D6_local_value_after_style_wins()
	{
		var el = new TestEl();
		var style = new Style(typeof(TestEl));
		style.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "styled" });
		el.Style = style;

		el.Tag2 = "local"; // local set after style

		Assert.Equal("local", el.Tag2);
	}

	// ── D6b: replacing Style removes old setters ───────────────────────────────

	/// <summary>D6 — replacing a style reverts properties only the old style set.</summary>
	[Fact]
	public void D6_replacing_style_reverts_old_setter()
	{
		var el = new TestEl();
		var style1 = new Style(typeof(TestEl));
		style1.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "style1" });
		el.Style = style1;
		Assert.Equal("style1", el.Tag2);

		var style2 = new Style(typeof(TestEl));
		// style2 does NOT set TagProperty
		el.Style = style2;

		// Old setter removed; new style has no setter → reverts to default (null).
		Assert.Null(el.Tag2);
	}

	/// <summary>D6 — setting Style = null removes the style setters.</summary>
	[Fact]
	public void D6_null_style_reverts_setter()
	{
		var el = new TestEl();
		var style = new Style(typeof(TestEl));
		style.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "styled" });
		el.Style = style;
		Assert.Equal("styled", el.Tag2);

		el.Style = null;

		// Style removed → property reverts to default.
		Assert.Null(el.Tag2);
	}

	/// <summary>D6 — replacing a style does not revert a locally-set value.</summary>
	[Fact]
	public void D6_replacing_style_does_not_revert_local_value()
	{
		var el = new TestEl();
		el.Tag2 = "local";

		var style1 = new Style(typeof(TestEl));
		style1.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "styled" });
		el.Style = style1;
		Assert.Equal("local", el.Tag2); // local still wins

		var style2 = new Style(typeof(TestEl));
		el.Style = style2;

		// local value must survive the style swap.
		Assert.Equal("local", el.Tag2);
	}

	// ── D7: BasedOn cycle guard ────────────────────────────────────────────────

	/// <summary>D7 — a BasedOn cycle does not hang; each style's setters applied once.</summary>
	[Fact]
	public void D7_BasedOn_cycle_does_not_hang()
	{
		var s1 = new Style(typeof(TestEl));
		var s2 = new Style(typeof(TestEl)) { BasedOn = s1 };
		// Create cycle: s1.BasedOn = s2.
		s1.BasedOn = s2;

		var el = new TestEl();
		s2.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "cycled" });

		// Should not hang.
		var ex = Record.Exception(() => el.Style = s2);
		Assert.Null(ex);
	}

	// ── D7: implicit style guard uses ReadLocalValue ───────────────────────────

	/// <summary>D7 — element with an EXPLICIT Style ignores implicit TargetType style.</summary>
	[Fact]
	public void D7_explicit_Style_wins_over_implicit()
	{
		var parent = new TestEl();
		var implicitStyle = new Style(typeof(TestEl));
		implicitStyle.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "implicit" });
		parent.Resources[typeof(TestEl)] = implicitStyle;

		var child = new TestEl();
		child.LogicalParent = parent;

		// Set an explicit Style BEFORE calling ApplyImplicitStyle.
		var explicitStyle = new Style(typeof(TestEl));
		explicitStyle.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "explicit" });
		child.Style = explicitStyle;

		child.ApplyImplicitStyle();

		// Explicit style must win; implicit must not overwrite.
		Assert.Equal("explicit", child.Tag2);
	}

	/// <summary>D7 — element without an explicit Style gets the implicit one.</summary>
	[Fact]
	public void D7_no_explicit_Style_gets_implicit()
	{
		var parent = new TestEl();
		var implicitStyle = new Style(typeof(TestEl));
		implicitStyle.Setters.Add(new Setter { Property = TestEl.TagProperty, Value = "implicit" });
		parent.Resources[typeof(TestEl)] = implicitStyle;

		var child = new TestEl();
		child.LogicalParent = parent;
		// No explicit Style set.
		child.ApplyImplicitStyle();

		Assert.Equal("implicit", child.Tag2);
	}
}