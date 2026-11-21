using System;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

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
		Assert.Throws<ArgumentNullException>(() => ItemsControl.ContainerFromElement(null!, new Button()));
	});
}
