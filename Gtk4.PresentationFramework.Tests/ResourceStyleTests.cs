using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests (no GTK) for Phase-1 Round-2: ResourceDictionary, resource lookup,
/// Style/Setter/BasedOn, implicit styles, and Application.Current.Resources.
/// </summary>
public class ResourceStyleTests
{
	// ── Minimal concrete FrameworkElement for testing ────────────────────────

	sealed class TestElement : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("Tag2", typeof(string), typeof(TestElement),
				new PropertyMetadata(null));

		public string? Tag2
		{
			get => (string?)GetValue(TagProperty);
			set => SetValue(TagProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── ResourceDictionary: own entries ─────────────────────────────────────

	[Fact]
	public void ResourceDictionary_stores_and_retrieves()
	{
		var rd = new ResourceDictionary();
		rd["key"] = "value";
		Assert.Equal("value", rd["key"]);
	}

	[Fact]
	public void ResourceDictionary_missing_key_returns_null()
	{
		var rd = new ResourceDictionary();
		Assert.Null(rd["missing"]);
	}

	// ── ResourceDictionary: merged dictionaries (last-wins) ──────────────────

	[Fact]
	public void MergedDictionaries_first_added_resolved_when_key_unique()
	{
		var merged = new ResourceDictionary();
		merged["x"] = "from-merged";
		var rd = new ResourceDictionary();
		rd.MergedDictionaries.Add(merged);
		Assert.Equal("from-merged", rd["x"]);
	}

	[Fact]
	public void MergedDictionaries_last_wins_on_duplicate_key()
	{
		var m1 = new ResourceDictionary(); m1["k"] = "first";
		var m2 = new ResourceDictionary(); m2["k"] = "second";
		var rd = new ResourceDictionary();
		rd.MergedDictionaries.Add(m1);
		rd.MergedDictionaries.Add(m2);
		// WPF: last merged wins.
		Assert.Equal("second", rd["k"]);
	}

	[Fact]
	public void Local_key_takes_precedence_over_merged()
	{
		var merged = new ResourceDictionary(); merged["k"] = "merged";
		var rd = new ResourceDictionary();
		rd.MergedDictionaries.Add(merged);
		rd["k"] = "local";
		Assert.Equal("local", rd["k"]);
	}

	// ── TryGetValue ──────────────────────────────────────────────────────────

	[Fact]
	public void TryGetValue_returns_true_for_merged_key()
	{
		var merged = new ResourceDictionary(); merged["m"] = 42;
		var rd = new ResourceDictionary();
		rd.MergedDictionaries.Add(merged);
		bool found = rd.TryGetValue("m", out var v);
		Assert.True(found);
		Assert.Equal(42, v);
	}

	// ── FindResource walks up the logical parent chain ───────────────────────

	[Fact]
	public void FindResource_finds_in_own_Resources()
	{
		var el = new TestElement();
		el.Resources["color"] = "Red";
		Assert.Equal("Red", el.FindResource("color"));
	}

	[Fact]
	public void FindResource_walks_up_to_parent()
	{
		var parent = new TestElement();
		parent.Resources["color"] = "Blue";
		var child = new TestElement();
		child.InheritanceParent = parent;

		Assert.Equal("Blue", child.FindResource("color"));
	}

	[Fact]
	public void FindResource_prefers_closer_ancestor()
	{
		var grandparent = new TestElement();
		grandparent.Resources["color"] = "Green";
		var parent = new TestElement();
		parent.Resources["color"] = "Blue";
		parent.InheritanceParent = grandparent;
		var child = new TestElement();
		child.InheritanceParent = parent;

		Assert.Equal("Blue", child.FindResource("color"));
	}

	[Fact]
	public void FindResource_throws_when_absent()
	{
		var el = new TestElement();
		Assert.Throws<InvalidOperationException>(() => el.FindResource("missing"));
	}

	[Fact]
	public void TryFindResource_returns_null_when_absent()
	{
		var el = new TestElement();
		Assert.Null(el.TryFindResource("missing"));
	}

	// ── Application.Current.Resources ────────────────────────────────────────

	[Fact]
	public void FindResource_falls_back_to_Application_Current_Resources()
	{
		// Arrange: create a fresh Application (sets Current).
		var app = new MockApplication();
		app.Resources["appKey"] = "appVal";

		var el = new TestElement(); // no parent, no local resource

		Assert.Equal("appVal", el.FindResource("appKey"));
	}

	// ── Style: setters applied ───────────────────────────────────────────────

	[Fact]
	public void Style_Apply_sets_property_via_dp()
	{
		var el = new TestElement();
		var style = new Style(typeof(TestElement));
		style.Setters.Add(new Setter { Property = TestElement.TagProperty, Value = "styled" });
		style.Apply(el);
		Assert.Equal("styled", el.Tag2);
	}

	[Fact]
	public void Style_Apply_sets_property_via_name()
	{
		var el = new TestElement();
		var style = new Style(typeof(TestElement));
		// "Tag2" is the DP name registered for TestElement.TagProperty
		style.Setters.Add(new Setter { PropertyName = "Tag2", Value = "by-name" });
		style.Apply(el);
		Assert.Equal("by-name", el.Tag2);
	}

	// ── Style: BasedOn chain ─────────────────────────────────────────────────

	[Fact]
	public void Style_BasedOn_base_setters_applied_first()
	{
		var base_ = new Style(typeof(TestElement));
		base_.Setters.Add(new Setter { Property = TestElement.TagProperty, Value = "base" });

		var derived = new Style(typeof(TestElement)) { BasedOn = base_ };
		derived.Setters.Add(new Setter { Property = TestElement.TagProperty, Value = "derived" });

		var el = new TestElement();
		derived.Apply(el);

		// Derived wins (applied last).
		Assert.Equal("derived", el.Tag2);
	}

	[Fact]
	public void Style_BasedOn_base_only_setter_applied_when_derived_does_not_override()
	{
		var el2 = new TestElement();
		var base_ = new Style(typeof(TestElement));
		base_.Setters.Add(new Setter { Property = TestElement.TagProperty, Value = "from-base" });

		var derived = new Style(typeof(TestElement)) { BasedOn = base_ };
		// no setter in derived for Tag2

		derived.Apply(el2);
		Assert.Equal("from-base", el2.Tag2);
	}

	// ── Style DP: assigned to FrameworkElement.Style ─────────────────────────

	[Fact]
	public void FrameworkElement_Style_applies_setters_on_set()
	{
		var el = new TestElement();
		var style = new Style(typeof(TestElement));
		style.Setters.Add(new Setter { Property = TestElement.TagProperty, Value = "auto" });
		el.Style = style;
		Assert.Equal("auto", el.Tag2);
	}

	// ── Implicit style by TargetType ─────────────────────────────────────────

	[Fact]
	public void ApplyImplicitStyle_finds_style_keyed_by_type()
	{
		var parent = new TestElement();
		var implicitStyle = new Style(typeof(TestElement));
		implicitStyle.Setters.Add(new Setter { Property = TestElement.TagProperty, Value = "implicit" });
		parent.Resources[typeof(TestElement)] = implicitStyle;

		var child = new TestElement();
		child.InheritanceParent = parent;

		child.ApplyImplicitStyle();

		Assert.Equal("implicit", child.Tag2);
	}

	[Fact]
	public void ApplyImplicitStyle_does_not_override_explicit_Style()
	{
		var parent = new TestElement();
		var implicitStyle = new Style(typeof(TestElement));
		implicitStyle.Setters.Add(new Setter { Property = TestElement.TagProperty, Value = "implicit" });
		parent.Resources[typeof(TestElement)] = implicitStyle;

		var child = new TestElement();
		child.InheritanceParent = parent;

		var explicitStyle = new Style(typeof(TestElement));
		explicitStyle.Setters.Add(new Setter { Property = TestElement.TagProperty, Value = "explicit" });
		child.Style = explicitStyle;

		child.ApplyImplicitStyle();

		Assert.Equal("explicit", child.Tag2);
	}
}

/// <summary>
/// Minimal stub Application that skips GTK initialisation for pure tests.
/// </summary>
class MockApplication : System.Windows.Application
{
	public MockApplication()
	{
		// Application() sets Current — no GTK needed for pure resource tests.
	}
}