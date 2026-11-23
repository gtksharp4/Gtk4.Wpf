using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

public class ItemsControlItemsPanelTests : WpfGtkTestBase
{
	static ItemsPanelTemplate Wrap() => new ItemsPanelTemplate(() => new WrapPanel());

	[Fact]
	public void ItemsPanelProperty_is_registered_with_a_sealed_StackPanel_default()
	{
		var p = ItemsControl.ItemsPanelProperty;
		Assert.Equal("ItemsPanel", p.Name);
		Assert.Equal(typeof(ItemsPanelTemplate), p.PropertyType);
		Assert.Equal(typeof(ItemsControl), p.OwnerType);
		Assert.IsAssignableFrom<FrameworkPropertyMetadata>(p.DefaultMetadata);
		var template = Assert.IsType<ItemsPanelTemplate>(p.DefaultMetadata.DefaultValue);
		Assert.True(template.IsSealed);
		Assert.IsType<StackPanel>(template.CreateVisualTree());
		Assert.Same(template, new ItemsControl().ItemsPanel);
	}

	[Fact]
	public void ItemsPanelTemplate_is_a_FrameworkTemplate_with_two_constructors()
	{
		Assert.IsAssignableFrom<FrameworkTemplate>(new ItemsPanelTemplate());
		Assert.False(new ItemsPanelTemplate().HasContent);
		Assert.True(Wrap().HasContent);
		Assert.False(Wrap().IsSealed);
	}

	[Fact]
	public void A_template_whose_root_is_not_a_panel_is_rejected_when_the_widget_is_created() => Run(() =>
	{
		var ic = new ItemsControl { ItemsPanel = new ItemsPanelTemplate(() => new Border()) };
		Assert.Throws<InvalidOperationException>(() => ic.Widget);
	});

	[Fact]
	public void The_default_panel_keeps_the_items_in_the_vertical_box() => Run(() =>
	{
		var ic = new ItemsControl { ItemsSource = new[] { "a", "b" } };
		var box = (Gtk.Box)ic.Widget;
		Assert.Null(ic.HostedItemsPanel);
		Assert.Equal(ic.ContainerAt(0)!.Widget, box.FirstChild);
		Assert.Equal(ic.ContainerAt(1)!.Widget, box.FirstChild.NextSibling);
	});

	[Fact]
	public void A_wrap_panel_template_hosts_the_item_containers() => Run(() =>
	{
		var ic = new ItemsControl { ItemsPanel = Wrap(), ItemsSource = new[] { "a", "b", "c" } };
		var box = (Gtk.Box)ic.Widget;
		var wrap = Assert.IsType<WrapPanel>(ic.HostedItemsPanel);
		Assert.Same(wrap.Widget, box.FirstChild);
		Assert.Null(box.FirstChild.NextSibling);
		Assert.Equal(3, wrap.Children.Count);
		Assert.Same(ic.ContainerAt(1), wrap.Children[1]);
		Assert.Same(wrap, ic.ContainerAt(0)!.LogicalParent);
		Assert.Same(ic, wrap.LogicalParent);
	});

	[Fact]
	public void Changing_the_template_after_creation_moves_the_containers() => Run(() =>
	{
		var ic = new ItemsControl { ItemsSource = new[] { "a", "b" } };
		var box = (Gtk.Box)ic.Widget;
		var first = ic.ContainerAt(0);

		ic.ItemsPanel = Wrap();
		var wrap = Assert.IsType<WrapPanel>(ic.HostedItemsPanel);
		Assert.Equal(2, wrap.Children.Count);
		Assert.Same(first, wrap.Children[0]);
		Assert.Same(wrap.Widget, box.FirstChild);
		Assert.Null(box.FirstChild.NextSibling);

		ic.ClearValue(ItemsControl.ItemsPanelProperty);
		Assert.Null(ic.HostedItemsPanel);
		Assert.Equal(0, wrap.Children.Count);
		Assert.Equal(first!.Widget, box.FirstChild);
		Assert.Same(ic, first.LogicalParent);
	});

	[Fact]
	public void Items_added_and_removed_after_creation_follow_the_panel() => Run(() =>
	{
		var source = new ObservableCollection<string> { "a" };
		var ic = new ItemsControl { ItemsPanel = Wrap(), ItemsSource = source };
		_ = ic.Widget;
		var wrap = (WrapPanel)ic.HostedItemsPanel!;
		source.Add("b");
		Assert.Equal(2, wrap.Children.Count);
		source.Clear();
		Assert.Equal(0, wrap.Children.Count);
	});

	[Fact]
	public void A_listbox_keeps_its_native_widget_and_stores_the_value() => Run(() =>
	{
		var list = new ListBox { ItemsPanel = Wrap(), ItemsSource = new[] { "a" } };
		Assert.IsType<Gtk.ListBox>(list.Widget);
		Assert.NotNull(list.ItemsPanel);
		Assert.Null(list.HostedItemsPanel);
	});
}

public class ItemsPanelTemplateContentTests : WpfGtkTestBase
{
	[Fact]
	public void Content_assigned_by_generated_code_backs_the_template_factory() => Run(() =>
	{
		var panel = new WrapPanel();
		var template = new ItemsPanelTemplate { Content = panel };
		Assert.True(template.HasContent);
		Assert.Same(panel, template.CreateVisualTree());
		var ic = new ItemsControl { ItemsPanel = template, ItemsSource = new[] { "a" } };
		_ = ic.Widget;
		Assert.Same(panel, ic.HostedItemsPanel);
		template.Content = null;
		Assert.False(template.HasContent);
	});

	[Fact]
	public void A_shared_Content_panel_backs_only_the_first_control_the_second_uses_the_default_stack() => Run(() =>
	{
		var shared = new ItemsPanelTemplate { Content = new WrapPanel() };
		var a = new ItemsControl { ItemsPanel = shared, ItemsSource = new[] { "a1", "a2" } };
		var b = new ItemsControl { ItemsPanel = shared, ItemsSource = new[] { "b1", "b2" } };
		var host = new StackPanel();
		host.Children.Add(a);
		host.Children.Add(b);
		_ = host.Widget;
		var wrap = Assert.IsType<WrapPanel>(a.HostedItemsPanel);
		Assert.Equal(2, wrap.Children.Count);
		Assert.Same(a, wrap.LogicalParent);
		Assert.Null(b.HostedItemsPanel);
		var container = b.ItemContainerGenerator.ContainerFromIndex(1);
		Assert.NotNull(container);
		Assert.Same(b, ((FrameworkElement)container!).LogicalParent);
	});

	[Fact]
	public void Seal_and_IsSealed_live_on_FrameworkTemplate()
	{
		var isSealed = typeof(FrameworkTemplate).GetProperty("IsSealed");
		Assert.NotNull(isSealed);
		Assert.True(isSealed!.GetMethod!.IsPublic);
		Assert.True(typeof(FrameworkTemplate).GetMethod("Seal", Type.EmptyTypes)!.IsPublic);
		var template = new ControlTemplate();
		Assert.False(template.IsSealed);
		template.Seal();
		Assert.True(template.IsSealed);
	}
}
