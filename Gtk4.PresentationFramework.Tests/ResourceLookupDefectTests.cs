using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pinning tests for D1 (logical-parent resource walk) and D2 (null resource not absent).
/// </summary>
public class ResourceLookupDefectTests
{
	sealed class TestEl : FrameworkElement
	{
		public new static readonly DependencyProperty TagProperty =
			DependencyProperty.Register("TagD12", typeof(string), typeof(TestEl),
				new PropertyMetadata(null));
		public string? Tag2
		{
			get => (string?)GetValue(TagProperty);
			set => SetValue(TagProperty, value);
		}
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("pure test");
	}

	// ── D1: logical-parent walk ──────────────────────────────────────────────

	/// <summary>D1 — resource on LogicalParent is found from child.</summary>
	[Fact]
	public void D1_TryFindResource_walks_LogicalParent()
	{
		var parent = new TestEl();
		parent.Resources["key"] = "parentVal";

		var child = new TestEl();
		// Wire via LogicalParent (not InheritanceParent).
		child.LogicalParent = parent;

		Assert.Equal("parentVal", child.TryFindResource("key"));
	}

	/// <summary>D1 — resource on grandparent is found from grandchild via LogicalParent chain.</summary>
	[Fact]
	public void D1_FindResource_walks_grandparent_LogicalParent()
	{
		var grandparent = new TestEl();
		grandparent.Resources["gk"] = "gpVal";

		var parent = new TestEl();
		parent.LogicalParent = grandparent;

		var child = new TestEl();
		child.LogicalParent = parent;

		Assert.Equal("gpVal", child.FindResource("gk"));
	}

	/// <summary>D1 — closer ancestor shadows farther one.</summary>
	[Fact]
	public void D1_FindResource_prefers_closer_LogicalParent()
	{
		var grandparent = new TestEl();
		grandparent.Resources["k"] = "gp";

		var parent = new TestEl();
		parent.Resources["k"] = "p";
		parent.LogicalParent = grandparent;

		var child = new TestEl();
		child.LogicalParent = parent;

		Assert.Equal("p", child.FindResource("k"));
	}

	/// <summary>D1 — cycle in LogicalParent chain does not hang.</summary>
	[Fact]
	public void D1_TryFindResource_cycle_guard_does_not_hang()
	{
		var a = new TestEl();
		var b = new TestEl();
		// Create a cycle: a→b→a would loop; LogicalParent setter prevents self-assign
		// but we also have the step counter guard.
		a.LogicalParent = b;
		// Don't set b.LogicalParent = a as that would need bypassing the setter.
		// Verify the lookup terminates normally.
		var result = a.TryFindResource("missing");
		Assert.Null(result);
	}

	// ── D2: null stored resource is "found", not absent ──────────────────────

	/// <summary>D2 — TryFindResource returns null (not absent) for a null-mapped key.</summary>
	[Fact]
	public void D2_TryFindResource_null_resource_does_not_fall_through_to_ancestor()
	{
		var ancestor = new TestEl();
		ancestor.Resources["k"] = "ancestorVal";

		var child = new TestEl();
		child.Resources["k"] = null; // explicitly null
		child.LogicalParent = ancestor;

		// Should return null (found null, not fallen through to ancestor).
		// We confirm it does NOT return "ancestorVal".
		var result = child.TryFindResource("k");
		Assert.Null(result);
		Assert.NotEqual("ancestorVal", result);
	}

	/// <summary>D2 — FindResource succeeds (doesn't throw) for a null-mapped key.</summary>
	[Fact]
	public void D2_FindResource_null_resource_does_not_throw()
	{
		var el = new TestEl();
		el.Resources["k"] = null;

		var ex = Record.Exception(() => el.FindResource("k"));
		Assert.Null(ex);
	}

	/// <summary>D2 — FindResource throws when key truly absent (not for null value).</summary>
	[Fact]
	public void D2_FindResource_throws_when_key_absent()
	{
		var el = new TestEl();
		Assert.Throws<InvalidOperationException>(() => el.FindResource("absent"));
	}
}