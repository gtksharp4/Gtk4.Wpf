using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.VisualLayer;

public class LogicalChildrenTests : WpfGtkTestBase
{
	[Fact]
	public void Default_LogicalChildren_is_empty() => Run(() =>
	{
		var host = new VisualHost();
		Assert.Empty(LogicalTreeHelper.GetChildren(host).Cast<object>());
	});

	[Fact]
	public void AddLogicalChild_sets_LogicalParent_and_Remove_clears_it() => Run(() =>
	{
		var host = new VisualHost();
		var child = new VisualHost();
		host.AddL(child);
		Assert.Same(host, LogicalTreeHelper.GetParent(child));
		host.RemoveL(child);
		Assert.Null(LogicalTreeHelper.GetParent(child));
	});

	[Fact]
	public void A_child_with_another_logical_parent_throws() => Run(() =>
	{
		var a = new VisualHost();
		var b = new VisualHost();
		var child = new VisualHost();
		a.AddL(child);
		Assert.Throws<InvalidOperationException>(() => b.AddL(child));
		a.AddL(child);
	});

	[Fact]
	public void AddLogicalChild_null_is_ignored_like_WPF() => Run(() =>
	{
		var a = new VisualHost();
		var child = new VisualHost();
		a.AddL(child);
		var before = LogicalTreeHelper.GetChildren(a).Cast<object>().ToArray();
		a.AddL(null!);
		var after = LogicalTreeHelper.GetChildren(a).Cast<object>().ToArray();
		Assert.Equal(before, after);
		Assert.Same(a, child.LogicalParent);
		Assert.Throws<InvalidOperationException>(() => new VisualHost().AddL(child));
	});

	[Fact]
	public void RemoveLogicalChild_ignores_an_element_it_does_not_own() => Run(() =>
	{
		var a = new VisualHost();
		var b = new VisualHost();
		var child = new VisualHost();
		a.AddL(child);
		b.RemoveL(child);
		Assert.Same(a, LogicalTreeHelper.GetParent(child));
	});

	[Fact]
	public void LogicalTreeHelper_enumerates_the_overridden_LogicalChildren() => Run(() =>
	{
		var panel = new LogicalPanel();
		var item = new VisualHost();
		panel.Own.Add(item);
		Assert.Equal(new object[] { item }, LogicalTreeHelper.GetChildren(panel).Cast<object>());
		Assert.Equal(new object[] { item }, LogicalTreeHelper.GetChildren(panel).Cast<object>());
	});

	[Fact]
	public void Panel_ContentControl_and_ItemsControl_keep_their_logical_children() => Run(() =>
	{
		var panel = new StackPanel();
		var b = new Button();
		panel.Children.Add(b);
		Assert.Equal(new object[] { b }, LogicalTreeHelper.GetChildren(panel).Cast<object>());

		var holder = new ContentControl { Content = b };
		Assert.Equal(new object[] { b }, LogicalTreeHelper.GetChildren(holder).Cast<object>());
		Assert.Empty(LogicalTreeHelper.GetChildren(new ContentControl { Content = "text" }).Cast<object>());

		var items = new ItemsControl();
		items.Items.Add(new Button());
		items.Items.Add(new Button());
		Assert.NotNull(LogicalTreeHelper.GetChildren(items));
	});

	[Fact]
	public void FindLogicalNode_walks_the_overridden_tree() => Run(() =>
	{
		var panel = new LogicalPanel();
		var item = new VisualHost { Name = "target" };
		panel.Own.Add(item);
		Assert.Same(item, LogicalTreeHelper.FindLogicalNode(panel, "target"));
	});
}
