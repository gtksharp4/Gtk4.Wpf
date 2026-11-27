using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class GroupStyleTests : WpfGtkTestBase
{
	sealed record City(string Name, string State);

	static readonly City[] Cities =
	{
		new("Seattle", "WA"), new("Portland", "OR"), new("Tacoma", "WA"), new("Salem", "OR"), new("Boise", "ID"),
	};

	static ICollectionView GroupedByState()
	{
		var view = new CollectionViewSource { Source = Cities }.View!;
		view.GroupDescriptions.Add(new PropertyGroupDescription("State"));
		return view;
	}

	static DataTemplate NameHeader() => new DataTemplate { ItemFactory = g => new TextBlock { Text = ((CollectionViewGroup)g!).Name?.ToString() ?? "" } };

	static List<Gtk.Widget> Children(Gtk.Widget parent)
	{
		var result = new List<Gtk.Widget>();
		for (var c = parent.FirstChild; c != null; c = c.NextSibling) result.Add(c);
		return result;
	}

	[Fact]
	public void A_grouped_view_enumerates_its_items_group_by_group()
	{
		var view = GroupedByState();

		Assert.Equal(new[] { "Seattle", "Tacoma", "Portland", "Salem", "Boise" },
			view.Cast<City>().Select(c => c.Name));
		var groups = view.Groups!.Cast<CollectionViewGroup>().ToList();
		Assert.Equal(new object?[] { "WA", "OR", "ID" }, groups.Select(g => g.Name));
		Assert.Equal(new[] { 2, 2, 1 }, groups.Select(g => g.ItemCount));
	}

	[Fact]
	public void GroupStyle_is_an_empty_collection_by_default_and_Default_is_shared()
	{
		Assert.Empty(new ItemsControl().GroupStyle);
		Assert.Same(GroupStyle.Default, GroupStyle.Default);
		Assert.Null(GroupStyle.Default.HeaderTemplate);
	}

	[Fact]
	public void Without_a_GroupStyle_a_grouped_source_shows_no_headers() => Run(() =>
	{
		var ic = new ItemsControl { ItemsSource = GroupedByState() };
		_ = ic.Widget;

		Assert.True(ic.IsGrouping);
		Assert.Empty(ic.GroupHeaders);
		Assert.Equal(5, Children(ic.Widget).Count);
	});

	[Fact]
	public void A_GroupStyle_adds_one_header_ahead_of_each_group_in_an_ItemsControl() => Run(() =>
	{
		var ic = new ItemsControl { ItemsSource = GroupedByState() };
		ic.GroupStyle.Add(new GroupStyle { HeaderTemplate = NameHeader() });
		_ = ic.Widget;

		Assert.Equal(3, ic.GroupHeaders.Count);
		var kids = Children(ic.Widget);
		Assert.Equal(8, kids.Count);
		// header, 2 items, header, 2 items, header, 1 item
		Assert.Equal(ic.GroupHeaders[0].Widget.Handle, kids[0].Handle);
		Assert.Equal(ic.GroupHeaders[1].Widget.Handle, kids[3].Handle);
		Assert.Equal(ic.GroupHeaders[2].Widget.Handle, kids[6].Handle);
		Assert.Equal(ic.ContainerAt(0)!.Widget.Handle, kids[1].Handle);
	});

	[Fact]
	public void The_header_template_sees_the_group_as_its_data_context() => Run(() =>
	{
		var ic = new ItemsControl { ItemsSource = GroupedByState() };
		ic.GroupStyle.Add(new GroupStyle
		{
			HeaderTemplate = new DataTemplate
			{
				ItemFactory = g =>
				{
					var t = new TextBlock();
					t.SetBinding(TextBlock.TextProperty, new Binding("Name"));
					return t;
				}
			}
		});
		_ = ic.Widget;

		var header = (ContentPresenter)ic.GroupHeaders[1];
		Assert.IsType<CollectionViewGroup>(header.Content);
		Assert.Equal("OR", ((CollectionViewGroup)header.Content!).Name);
		Assert.Equal(2, ((CollectionViewGroup)header.Content!).ItemCount);
	});

	[Fact]
	public void A_GroupStyle_without_a_template_shows_the_group_name() => Run(() =>
	{
		var ic = new ItemsControl { ItemsSource = GroupedByState() };
		ic.GroupStyle.Add(GroupStyle.Default);
		_ = ic.Widget;

		Assert.Equal("WA", ((ContentPresenter)ic.GroupHeaders[0]).Content);
	});

	[Fact]
	public void Changing_the_grouping_rebuilds_the_headers() => Run(() =>
	{
		var view = GroupedByState();
		var ic = new ItemsControl { ItemsSource = view };
		ic.GroupStyle.Add(new GroupStyle { HeaderTemplate = NameHeader() });
		_ = ic.Widget;
		Assert.Equal(3, ic.GroupHeaders.Count);

		view.GroupDescriptions.Clear();
		Assert.Empty(ic.GroupHeaders);
		Assert.Equal(5, Children(ic.Widget).Count);

		ic.GroupStyle.Clear();
		view.GroupDescriptions.Add(new PropertyGroupDescription("State"));
		Assert.Empty(ic.GroupHeaders);
	});

	[Fact]
	public void The_headers_survive_an_ItemsPanel_change() => Run(() =>
	{
		var ic = new ItemsControl { ItemsSource = GroupedByState() };
		ic.GroupStyle.Add(new GroupStyle { HeaderTemplate = NameHeader() });
		_ = ic.Widget;

		ic.ItemsPanel = new ItemsPanelTemplate(() => new StackPanel());

		Assert.Equal(3, ic.GroupHeaders.Count);
		Assert.Equal(3, ic.HostedItemsPanel!.Children.OfType<ContentPresenter>()
			.Count(p => p.Content is CollectionViewGroup));
		Assert.Equal(8, ic.HostedItemsPanel.Children.Count);
	});

	[Fact]
	public void A_ListBox_keeps_one_row_per_item_and_puts_the_header_on_the_first_row_of_a_group() => Run(() =>
	{
		var lb = new ListBox { ItemsSource = GroupedByState() };
		lb.GroupStyle.Add(new GroupStyle { HeaderTemplate = NameHeader() });
		_ = lb.Widget;

		Assert.Equal(5, Children(lb._gtkListBox!).Count(c => c is Gtk.ListBoxRow));
		var rows = Enumerable.Range(0, 5).Select(i => (Gtk.ListBoxRow)lb.ContainerAt(i)!.Widget).ToList();
		Assert.NotNull(rows[0].Header);
		Assert.Null(rows[1].Header);
		Assert.NotNull(rows[2].Header);
		Assert.Null(rows[3].Header);
		Assert.NotNull(rows[4].Header);
		Assert.Equal(3, lb.GroupHeaders.Count);
	});

	[Fact]
	public void A_ListBox_selects_by_item_index_across_groups() => Run(() =>
	{
		var lb = new ListBox { ItemsSource = GroupedByState() };
		lb.GroupStyle.Add(new GroupStyle { HeaderTemplate = NameHeader() });
		_ = lb.Widget;

		lb.SelectedIndex = 2;

		Assert.Equal("Portland", ((City)lb.SelectedItem!).Name);
	});
}
