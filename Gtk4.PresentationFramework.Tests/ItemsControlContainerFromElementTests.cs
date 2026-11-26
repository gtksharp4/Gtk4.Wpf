using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ItemsControlContainerFromElementTests : WpfGtkTestBase
{
	[Fact]
	public void Finds_the_container_of_a_nested_element() => Run(() =>
	{
		var button = new Button();
		var panel = new StackPanel();
		panel.Children.Add(button);
		var other = new StackPanel();
		var list = new ItemsControl();
		list.Items.Add(other);
		list.Items.Add(panel);
		_ = list.Widget;

		var container = Assert.IsType<ContentPresenter>(list.ContainerFromElement(button));
		Assert.Same(panel, container.Content);
		Assert.Same(container, ItemsControl.ContainerFromElement(list, panel));
		Assert.Same(container, list.ContainerFromElement(container));
		Assert.NotSame(container, list.ContainerFromElement(other));
	});

	[Fact]
	public void Returns_null_outside_the_container_chain() => Run(() =>
	{
		var list = new ItemsControl();
		list.Items.Add(new StackPanel());
		_ = list.Widget;
		Assert.Null(list.ContainerFromElement(new Button()));
		Assert.Null(list.ContainerFromElement(list));
	});

	[Fact]
	public void Rejects_null_arguments() => Run(() =>
	{
		var list = new ItemsControl();
		Assert.Throws<ArgumentNullException>(() => list.ContainerFromElement(null!));
		Assert.Throws<ArgumentNullException>(() => ItemsControl.ContainerFromElement(list, null!));
	});

	[Fact]
	public void A_null_itemsControl_finds_the_nearest_container() => Run(() =>
	{
		var button = new Button();
		var panel = new StackPanel();
		panel.Children.Add(button);
		var list = new ItemsControl();
		list.Items.Add(panel);
		_ = list.Widget;
		var container = Assert.IsType<ContentPresenter>(ItemsControl.ContainerFromElement(null, button));
		Assert.Same(panel, container.Content);
		Assert.Null(ItemsControl.ContainerFromElement(null, new Button()));
	});

	[Fact]
	public void A_direct_child_that_is_not_a_container_returns_null() => Run(() =>
	{
		var list = new ItemsControl();
		list.Items.Add(new StackPanel());
		_ = list.Widget;
		var stray = new Button { LogicalParent = list };
		Assert.Null(ItemsControl.ContainerFromElement(list, stray));
	});
}
