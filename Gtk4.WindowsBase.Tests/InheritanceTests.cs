using System;
using System.Collections.Generic;
using System.Windows;
using Xunit;

namespace Gtk4.WindowsBase.Tests;
// ── Domain types ─────────────────────────────────────────────────────────────────

/// <summary>A node that exposes InheritanceParent publicly for test wiring.</summary>
class TreeNode : DependencyObject
{
	public DependencyObject? Parent
	{
		get => InheritanceParent;
		set => InheritanceParent = value;
	}

	/// <summary>Inherited font-size property (FrameworkPropertyMetadata.Inherits = true).</summary>
	public static readonly DependencyProperty FontSizeProp =
		DependencyProperty.Register(nameof(FontSizeProp), typeof(double), typeof(TreeNode),
			new FrameworkPropertyMetadata(12.0, inherits: true));

	/// <summary>Non-inherited property for contrast.</summary>
	public static readonly DependencyProperty BackgroundProp =
		DependencyProperty.Register(nameof(BackgroundProp), typeof(string), typeof(TreeNode),
			new PropertyMetadata("transparent"));

	/// <summary>Inherited property with a change callback so we can record notifications.</summary>
	public static readonly DependencyProperty ThemeProp =
		DependencyProperty.Register(nameof(ThemeProp), typeof(string), typeof(TreeNode),
			new FrameworkPropertyMetadata(null, inherits: true, affectsMeasure: false,
				affectsArrange: false, affectsRender: false, OnThemeChanged));

	static void OnThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
		((TreeNode)d).ThemeChanges.Add((e.OldValue, e.NewValue));

	public List<(object? Old, object? New)> ThemeChanges = new List<(object?, object?)>();
}

// ── Tests ────────────────────────────────────────────────────────────────────────

public class InheritanceTests
{
	// ── #3: values flow into subtrees attached AFTER the value is set ────────────

	[Fact]
	public void Child_attached_after_value_set_sees_inherited_value()
	{
		var parent = new TreeNode();
		parent.SetValue(TreeNode.FontSizeProp, 24.0);

		// Attach AFTER value is set.
		var child = new TreeNode { Parent = parent };

		Assert.Equal(24.0, child.GetValue(TreeNode.FontSizeProp));
	}

	[Fact]
	public void Grandchild_attached_after_value_set_sees_inherited_value()
	{
		var root = new TreeNode();
		root.SetValue(TreeNode.FontSizeProp, 32.0);

		var middle = new TreeNode { Parent = root };
		var leaf = new TreeNode { Parent = middle };

		Assert.Equal(32.0, leaf.GetValue(TreeNode.FontSizeProp));
	}

	[Fact]
	public void Child_attached_after_value_set_fires_callback()
	{
		var parent = new TreeNode();
		parent.SetValue(TreeNode.ThemeProp, "dark");

		// Attaching fires a change from default (null) → "dark".
		var child = new TreeNode { Parent = parent };

		Assert.Single(child.ThemeChanges);
		Assert.Equal((null, (object?)"dark"), child.ThemeChanges[0]);
	}

	[Fact]
	public void Child_local_value_not_overwritten_when_attached_after_value_set()
	{
		var parent = new TreeNode();
		parent.SetValue(TreeNode.FontSizeProp, 24.0);

		var child = new TreeNode();
		child.SetValue(TreeNode.FontSizeProp, 10.0);
		child.Parent = parent;

		// Local value must win; no spurious notification.
		Assert.Equal(10.0, child.GetValue(TreeNode.FontSizeProp));
	}

	// ── existing tests ────────────────────────────────────────────────────────────

	[Fact]
	public void Child_with_no_local_value_sees_parent_value()
	{
		var parent = new TreeNode();
		var child = new TreeNode { Parent = parent };

		parent.SetValue(TreeNode.FontSizeProp, 20.0);

		Assert.Equal(20.0, child.GetValue(TreeNode.FontSizeProp));
	}

	[Fact]
	public void Child_local_value_shadows_parent()
	{
		var parent = new TreeNode();
		var child = new TreeNode { Parent = parent };

		parent.SetValue(TreeNode.FontSizeProp, 20.0);
		child.SetValue(TreeNode.FontSizeProp, 14.0);

		Assert.Equal(14.0, child.GetValue(TreeNode.FontSizeProp));
	}

	[Fact]
	public void Clearing_local_value_resumes_inheritance()
	{
		var parent = new TreeNode();
		var child = new TreeNode { Parent = parent };

		parent.SetValue(TreeNode.FontSizeProp, 20.0);
		child.SetValue(TreeNode.FontSizeProp, 14.0);
		child.ClearValue(TreeNode.FontSizeProp);

		Assert.Equal(20.0, child.GetValue(TreeNode.FontSizeProp));
	}

	[Fact]
	public void Grandchild_inherits_through_chain()
	{
		var root = new TreeNode();
		var middle = new TreeNode { Parent = root };
		var leaf = new TreeNode { Parent = middle };

		root.SetValue(TreeNode.FontSizeProp, 18.0);

		Assert.Equal(18.0, leaf.GetValue(TreeNode.FontSizeProp));
	}

	[Fact]
	public void Non_inherited_property_does_not_propagate()
	{
		var parent = new TreeNode();
		var child = new TreeNode { Parent = parent };

		parent.SetValue(TreeNode.BackgroundProp, "blue");

		// Child has no local value and the property does not inherit → falls back to default.
		Assert.Equal("transparent", child.GetValue(TreeNode.BackgroundProp));
	}

	[Fact]
	public void Changing_parent_inherited_value_re_notifies_child()
	{
		var parent = new TreeNode();
		var child = new TreeNode { Parent = parent };

		parent.SetValue(TreeNode.ThemeProp, "dark");
		child.ThemeChanges.Clear(); // ignore the initial propagation

		parent.SetValue(TreeNode.ThemeProp, "light");

		Assert.Single(child.ThemeChanges);
		Assert.Equal(("dark", (object?)"light"), child.ThemeChanges[0]);
	}

	[Fact]
	public void Child_with_local_value_is_not_re_notified_on_parent_change()
	{
		var parent = new TreeNode();
		var child = new TreeNode { Parent = parent };

		child.SetValue(TreeNode.ThemeProp, "custom");
		child.ThemeChanges.Clear();

		parent.SetValue(TreeNode.ThemeProp, "dark");

		// Child's local value shadows the inherited one — no notification expected.
		Assert.Empty(child.ThemeChanges);
	}

	[Fact]
	public void Default_value_seen_when_no_parent()
	{
		var node = new TreeNode();
		Assert.Equal(12.0, node.GetValue(TreeNode.FontSizeProp));
	}
}