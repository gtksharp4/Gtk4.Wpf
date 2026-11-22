using System;
using System.Windows;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>
/// Pure tests (no GTK) for live <c>{DynamicResource}</c> tracking via
/// <see cref="FrameworkElement.SetResourceReference"/>.
/// </summary>
public class DynamicResourceTests
{
	// ── Minimal concrete FrameworkElement ────────────────────────────────────

	sealed class El : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register(
				"DynResTag", typeof(string), typeof(El), new PropertyMetadata(null));

		public new string? Tag
		{
			get => (string?)GetValue(TagProperty);
			set => SetValue(TagProperty, value);
		}

		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── Test 1: immediate resolution ────────────────────────────────────────

	/// <summary>SetResourceReference resolves the key immediately.</summary>
	[Fact]
	public void SetResourceReference_resolves_current_value_immediately()
	{
		var el = new El();
		el.Resources["color"] = "Red";

		el.SetResourceReference(El.TagProperty, "color");

		Assert.Equal("Red", el.Tag);
	}

	// ── Test 2: live update on dict replace ──────────────────────────────────

	/// <summary>Replacing the resource in the same dict pushes the new value.</summary>
	[Fact]
	public void SetResourceReference_updates_when_dict_entry_replaced()
	{
		var el = new El();
		el.Resources["color"] = "Red";
		el.SetResourceReference(El.TagProperty, "color");

		el.Resources["color"] = "Blue";

		Assert.Equal("Blue", el.Tag);
	}

	// ── Test 3: live update from ancestor dict ───────────────────────────────

	/// <summary>Adding a closer-scope resource shadows the farther one and updates the DP.</summary>
	[Fact]
	public void SetResourceReference_updates_when_ancestor_dict_adds_closer_key()
	{
		var parent = new El();
		parent.Resources["color"] = "Green";

		var child = new El();
		child.InheritanceParent = parent;
		child.SetResourceReference(El.TagProperty, "color");

		Assert.Equal("Green", child.Tag);  // initial resolution from parent

		// Now the child's own dict gets the key — should shadow parent.
		child.Resources["color"] = "Yellow";

		Assert.Equal("Yellow", child.Tag);
	}

	// ── Test 4: local value beats dynamic resource; clearing re-resolves ─────

	/// <summary>Local SetValue overrides the dynamic resource; ClearValue re-resolves it.</summary>
	[Fact]
	public void Local_value_beats_dynamic_resource_and_ClearValue_re_resolves()
	{
		var el = new El();
		el.Resources["color"] = "Red";
		el.SetResourceReference(El.TagProperty, "color");

		Assert.Equal("Red", el.Tag);

		// Local value wins.
		el.SetValue(El.TagProperty, "LocalOverride");
		Assert.Equal("LocalOverride", el.Tag);

		// Clearing local re-surfaces the dynamic resource.
		el.ClearValue(El.TagProperty);
		Assert.Equal("Red", el.Tag);
	}

	// ── Test 5: ClearResourceReference stops updates ─────────────────────────

	/// <summary>After ClearResourceReference, dict changes do not update the DP.</summary>
	[Fact]
	public void ClearResourceReference_detaches_live_link()
	{
		var el = new El();
		el.Resources["color"] = "Red";
		el.SetResourceReference(El.TagProperty, "color");
		Assert.Equal("Red", el.Tag);

		el.ClearResourceReference(El.TagProperty);

		el.Resources["color"] = "Purple";

		// DP must NOT have been updated after detach.
		Assert.NotEqual("Purple", el.Tag);
	}

	// ── Test 6: unrelated key does not update the DP ─────────────────────────

	/// <summary>Changing an unrelated key leaves the DP value unchanged.</summary>
	[Fact]
	public void Changing_unrelated_key_does_not_update_dp()
	{
		var el = new El();
		el.Resources["color"] = "Red";
		el.Resources["font"] = "Arial";
		el.SetResourceReference(El.TagProperty, "color");

		Assert.Equal("Red", el.Tag);

		el.Resources["font"] = "Verdana";

		Assert.Equal("Red", el.Tag);  // unchanged
	}
}
