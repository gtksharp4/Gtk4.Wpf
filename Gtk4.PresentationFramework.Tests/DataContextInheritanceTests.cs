using System.ComponentModel;
using Xunit;
using WpfGrid = System.Windows.Controls.Grid;
using WpfStackPanel = System.Windows.Controls.StackPanel;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests (no GTK) verifying that DataContext inherits from parent to children
/// through the DP inheritance system.
/// </summary>
public class DataContextInheritanceTests
{
	[Fact]
	public void DataContext_propagates_to_child_via_LogicalParent()
	{
		var parent = new WpfGrid();
		var child = new WpfTextBlock();
		var context = new object();

		parent.DataContext = context;
		parent.Children.Add(child);

		Assert.Same(context, child.DataContext);
	}

	[Fact]
	public void DataContext_propagates_before_child_added()
	{
		var parent = new WpfStackPanel();
		var child = new WpfTextBlock();
		parent.DataContext = "hello";

		parent.Children.Add(child);

		Assert.Equal("hello", child.DataContext);
	}

	[Fact]
	public void DataContext_clears_when_child_removed()
	{
		var parent = new WpfGrid();
		var child = new WpfTextBlock();
		parent.DataContext = "ctx";
		parent.Children.Add(child);

		parent.Children.Remove(child);

		// After removal the child has no parent; its DataContext reverts to default (null).
		Assert.Null(child.DataContext);
	}

	[Fact]
	public void DataContext_propagates_through_two_levels()
	{
		var root = new WpfGrid();
		var mid = new WpfStackPanel();
		var leaf = new WpfTextBlock();

		root.Children.Add(mid);
		mid.Children.Add(leaf);

		root.DataContext = "inherited";

		Assert.Equal("inherited", leaf.DataContext);
	}

	[Fact]
	public void Local_DataContext_shadows_inherited()
	{
		var parent = new WpfGrid();
		var child = new WpfTextBlock();

		parent.DataContext = "parent";
		child.DataContext = "child";
		parent.Children.Add(child);

		// Local value wins over inherited.
		Assert.Equal("child", child.DataContext);
	}
}