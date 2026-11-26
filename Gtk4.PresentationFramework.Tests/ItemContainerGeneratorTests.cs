using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ItemContainerGeneratorTests : WpfGtkTestBase
{
	[Fact]
	public void Is_never_null_and_not_started_before_the_widget_exists() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		Assert.NotNull(list.ItemContainerGenerator);
		Assert.Same(list.ItemContainerGenerator, list.ItemContainerGenerator);
		Assert.Equal(GeneratorStatus.NotStarted, list.ItemContainerGenerator.Status);
		Assert.Null(list.ItemContainerGenerator.ContainerFromIndex(0));
		Assert.Null(list.ItemContainerGenerator.ContainerFromItem("a"));
	});

	[Fact]
	public void Maps_inline_items_to_their_containers_both_ways() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		list.Items.Add("b");
		var gen = list.ItemContainerGenerator;
		_ = list.Widget;
		Assert.Equal(GeneratorStatus.ContainersGenerated, gen.Status);
		var container = Assert.IsType<ListBoxItem>(gen.ContainerFromItem("b"));
		Assert.Same(container, gen.ContainerFromIndex(1));
		Assert.Equal(1, gen.IndexFromContainer(container));
		Assert.Equal("b", gen.ItemFromContainer(container));
		Assert.Equal("b", container.Content);
	});

	[Fact]
	public void Maps_ItemsSource_items() => Run(() =>
	{
		var list = new ListBox { ItemsSource = new ObservableCollection<string> { "x", "y", "z" } };
		_ = list.Widget;
		var container = list.ItemContainerGenerator.ContainerFromItem("z");
		Assert.NotNull(container);
		Assert.Equal(2, list.ItemContainerGenerator.IndexFromContainer(container!));
	});

	[Fact]
	public void Absent_lookups_report_minus_one_and_UnsetValue() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		_ = list.Widget;
		var gen = list.ItemContainerGenerator;
		Assert.Null(gen.ContainerFromItem("missing"));
		Assert.Null(gen.ContainerFromIndex(5));
		Assert.Null(gen.ContainerFromIndex(-1));
		var stranger = new ListBoxItem();
		Assert.Equal(-1, gen.IndexFromContainer(stranger));
		Assert.Same(DependencyProperty.UnsetValue, gen.ItemFromContainer(stranger));
	});

	[Fact]
	public void An_item_that_is_its_own_container_maps_to_itself() => Run(() =>
	{
		var item = new ListBoxItem { Content = "own" };
		var list = new ListBox();
		list.Items.Add(item);
		_ = list.Widget;
		var gen = list.ItemContainerGenerator;
		Assert.Same(item, gen.ContainerFromItem(item));
		Assert.Same(item, gen.ContainerFromIndex(0));
		Assert.Equal(0, gen.IndexFromContainer(item));
		Assert.Same(item, gen.ItemFromContainer(item));
	});

	[Fact]
	public void Lookups_follow_a_rebuild() => Run(() =>
	{
		var source = new ObservableCollection<string> { "a", "b" };
		var list = new ListBox { ItemsSource = source };
		_ = list.Widget;
		var gen = list.ItemContainerGenerator;
		var before = gen.ContainerFromItem("b");
		source.Insert(0, "new");
		var after = gen.ContainerFromItem("b");
		Assert.NotNull(after);
		Assert.NotSame(before, after);
		Assert.Equal(2, gen.IndexFromContainer(after!));
		Assert.Equal(GeneratorStatus.ContainersGenerated, gen.Status);
	});

	[Fact]
	public void StatusChanged_is_raised_around_generation() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		var seen = new List<GeneratorStatus>();
		list.ItemContainerGenerator.StatusChanged += (s, _) => seen.Add(((ItemContainerGenerator)s!).Status);
		_ = list.Widget;
		Assert.Equal(new[] { GeneratorStatus.GeneratingContainers, GeneratorStatus.ContainersGenerated }, seen);
	});

	[Fact]
	public void Templated_items_are_hosted_through_a_ContentPresenter() => Run(() =>
	{
		var list = new ListBox
		{
			ItemTemplate = new DataTemplate { ItemFactory = i => new TextBlock { Text = (string)i! } },
		};
		list.Items.Add("one");
		_ = list.Widget;
		var container = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem("one")!;
		Assert.Equal(1, VisualTreeHelper.GetChildrenCount(container));
		var presenter = Assert.IsType<ContentPresenter>(VisualTreeHelper.GetChild(container, 0));
		Assert.Same(list.ItemTemplate, presenter.ContentTemplate);
		Assert.Equal("one", presenter.Content);
	});

	[Fact]
	public void DisplayMemberPath_items_are_hosted_through_a_ContentPresenter() => Run(() =>
	{
		var list = new ListBox { DisplayMemberPath = "Length" };
		list.Items.Add("abc");
		_ = list.Widget;
		var container = (ListBoxItem)list.ItemContainerGenerator.ContainerFromItem("abc")!;
		var presenter = Assert.IsType<ContentPresenter>(VisualTreeHelper.GetChild(container, 0));
		Assert.Equal("3", presenter.Content);
		Assert.Equal("abc", container.Content);
	});

	[Fact]
	public void Null_containers_throw_and_foreign_ones_are_unknown() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		_ = list.Widget;
		var gen = list.ItemContainerGenerator;
		var ex1 = Assert.Throws<ArgumentNullException>(() => gen.IndexFromContainer(null!));
		var ex2 = Assert.Throws<ArgumentNullException>(() => gen.ItemFromContainer(null!));
		Assert.Equal("container", ex1.ParamName);
		Assert.Equal("container", ex2.ParamName);
		var foreign = new ListBoxItem();
		Assert.Equal(-1, gen.IndexFromContainer(foreign));
		Assert.Same(DependencyProperty.UnsetValue, gen.ItemFromContainer(foreign));
	});
}
