using System.Windows;
using Xunit;
using WpfGrid = System.Windows.Controls.Grid;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfContentControl = System.Windows.Controls.ContentControl;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests (no GTK) verifying that adding a child to Content or Children
/// sets the child's logical parent so DP inheritance works.
/// </summary>
public class ContentChildParentingTests
{
	[Fact]
	public void Panel_Children_Add_sets_LogicalParent()
	{
		var parent = new WpfGrid();
		var child = new WpfTextBlock();

		parent.Children.Add(child);

		parent.DataContext = "ctx";
		Assert.Equal("ctx", child.DataContext);
	}

	[Fact]
	public void Panel_Children_Remove_clears_LogicalParent()
	{
		var parent = new WpfGrid();
		var child = new WpfTextBlock();
		parent.DataContext = "ctx";
		parent.Children.Add(child);

		parent.Children.Remove(child);

		Assert.Null(child.DataContext);
	}

	[Fact]
	public void ContentControl_Content_UIElement_sets_LogicalParent()
	{
		var cc = new TestContentControl();
		var child = new WpfTextBlock();
		cc.DataContext = "inherited";

		cc.Content = child;

		Assert.Equal("inherited", child.DataContext);
	}

	[Fact]
	public void ContentControl_Content_replaced_clears_old_parent()
	{
		var cc = new TestContentControl();
		var child1 = new WpfTextBlock();
		var child2 = new WpfTextBlock();
		cc.DataContext = "ctx";

		cc.Content = child1;
		Assert.Equal("ctx", child1.DataContext);

		cc.Content = child2;
		Assert.Null(child1.DataContext);
		Assert.Equal("ctx", child2.DataContext);
	}

	/// <summary>ContentControl subclass whose CreateWidget is never called in pure tests.</summary>
	sealed class TestContentControl : WpfContentControl
	{
		protected override Gtk.Widget CreateWidget() =>
			throw new System.NotSupportedException("No GTK in pure tests.");
	}
}