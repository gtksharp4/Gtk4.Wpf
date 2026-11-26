using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure (no GTK) tests for <see cref="VisualTreeHelper"/>.
/// Children are added via Panel.Children which sets logical parent; VisualTreeHelper
/// walks Visual.VisualChildrenCount / GetVisualChild / VisualParent, all wired from
/// UIElement.VisualChildren / LogicalParent.
/// </summary>
public class VisualTreeHelperTests
{
	[Fact]
	public void GetChildrenCount_empty_panel_returns_zero()
	{
		var panel = new TestPanel();
		Assert.Equal(0, VisualTreeHelper.GetChildrenCount(panel));
	}

	[Fact]
	public void GetChildrenCount_panel_with_children_returns_count()
	{
		var panel = new TestPanel();
		panel.Children.Add(new TextBlock());
		panel.Children.Add(new TextBlock());
		panel.Children.Add(new TextBlock());

		Assert.Equal(3, VisualTreeHelper.GetChildrenCount(panel));
	}

	[Fact]
	public void GetChild_returns_correct_child()
	{
		var panel = new TestPanel();
		var c0 = new TextBlock();
		var c1 = new TextBlock();
		panel.Children.Add(c0);
		panel.Children.Add(c1);

		Assert.Same(c0, VisualTreeHelper.GetChild(panel, 0));
		Assert.Same(c1, VisualTreeHelper.GetChild(panel, 1));
	}

	[Fact]
	public void GetChild_out_of_range_throws()
	{
		var panel = new TestPanel();
		Assert.Throws<ArgumentOutOfRangeException>(() => VisualTreeHelper.GetChild(panel, 0));
	}

	[Fact]
	public void GetParent_returns_null_for_root()
	{
		var panel = new TestPanel();
		Assert.Null(VisualTreeHelper.GetParent(panel));
	}

	[Fact]
	public void GetParent_returns_panel_after_child_added()
	{
		var panel = new TestPanel();
		var child = new TextBlock();
		panel.Children.Add(child);

		// LogicalParent is set → VisualParent is set → GetParent returns panel.
		Assert.Same(panel, VisualTreeHelper.GetParent(child));
	}

	[Fact]
	public void GetParent_clears_after_child_removed()
	{
		var panel = new TestPanel();
		var child = new TextBlock();
		panel.Children.Add(child);
		panel.Children.Remove(child);

		Assert.Null(VisualTreeHelper.GetParent(child));
	}

	[Fact]
	public void GetChildrenCount_non_Visual_throws()
	{
		var dobj = new System.Windows.DependencyObject();
		Assert.Throws<ArgumentException>(() => VisualTreeHelper.GetChildrenCount(dobj));
	}

	// ── GTK-free test panel ──────────────────────────────────────────────────────

	sealed class TestPanel : Panel
	{
		internal override bool IsWidgetCreated => false;
		protected override Gtk.Widget CreateWidget() =>
			throw new NotSupportedException("No GTK in pure tests.");
		protected override void AddChildWidget(UIElement child) { }
		protected override void RemoveChildWidget(UIElement child) { }
	}
}
