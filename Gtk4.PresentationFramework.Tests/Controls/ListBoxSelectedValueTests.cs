using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

file sealed class Item
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
}

/// <summary>Headless tests for ListBox SelectedValue / SelectedValuePath.</summary>
public class ListBoxSelectedValueTests
{
	[Fact]
	public void SelectedValuePath_default_is_empty()
	{
		var lb = new ListBox();
		Assert.Equal(string.Empty, lb.SelectedValuePath);
	}

	[Fact]
	public void SelectedValue_null_when_nothing_selected()
	{
		var lb = new ListBox();
		lb.ItemsSource = new[] { "a", "b" };
		Assert.Null(lb.SelectedValue);
	}

	[Fact]
	public void SelectedValue_equals_item_when_path_empty()
	{
		var lb = new ListBox();
		lb.ItemsSource = new[] { "alpha", "beta" };
		lb.SelectedIndex = 1;
		Assert.Equal("beta", lb.SelectedValue);
	}

	[Fact]
	public void SelectedValue_reflects_property_path()
	{
		var items = new[] { new Item { Id = 10, Name = "x" }, new Item { Id = 20, Name = "y" } };
		var lb = new ListBox();
		lb.ItemsSource = items;
		lb.SelectedValuePath = "Id";
		lb.SelectedIndex = 0;
		Assert.Equal(10, lb.SelectedValue);
	}

	[Fact]
	public void SelectedValue_updates_when_SelectedItem_changes()
	{
		var items = new[] { new Item { Id = 1 }, new Item { Id = 2 } };
		var lb = new ListBox();
		lb.ItemsSource = items;
		lb.SelectedValuePath = "Id";
		lb.SelectedIndex = 0;
		Assert.Equal(1, lb.SelectedValue);
		lb.SelectedIndex = 1;
		Assert.Equal(2, lb.SelectedValue);
	}

	[Fact]
	public void SelectedValue_clears_when_selection_cleared()
	{
		var lb = new ListBox();
		lb.ItemsSource = new[] { "a", "b" };
		lb.SelectedIndex = 0;
		Assert.Equal("a", lb.SelectedValue);
		lb.SelectedIndex = -1;
		Assert.Null(lb.SelectedValue);
	}

	[Fact]
	public void SelectedValuePath_change_re_resolves_SelectedValue()
	{
		var items = new[] { new Item { Id = 7, Name = "foo" } };
		var lb = new ListBox();
		lb.ItemsSource = items;
		lb.SelectedIndex = 0;
		lb.SelectedValuePath = "Id";
		Assert.Equal(7, lb.SelectedValue);
		lb.SelectedValuePath = "Name";
		Assert.Equal("foo", lb.SelectedValue);
	}

	[Fact]
	public void Setting_SelectedValue_selects_matching_item()
	{
		var items = new[] { new Item { Id = 1 }, new Item { Id = 2 }, new Item { Id = 3 } };
		var lb = new ListBox();
		lb.ItemsSource = items;
		lb.SelectedValuePath = "Id";
		lb.SelectedValue = 2;
		Assert.Equal(items[1], lb.SelectedItem);
	}

	[Fact]
	public void Setting_SelectedValue_no_match_clears_selection()
	{
		var items = new[] { new Item { Id = 1 }, new Item { Id = 2 } };
		var lb = new ListBox();
		lb.ItemsSource = items;
		lb.SelectedValuePath = "Id";
		lb.SelectedIndex = 0;
		lb.SelectedValue = 99;
		Assert.Null(lb.SelectedItem);
	}

	[Fact]
	public void Setting_SelectedValue_empty_path_selects_by_identity()
	{
		var lb = new ListBox();
		lb.ItemsSource = new[] { "alpha", "beta", "gamma" };
		lb.SelectedValue = "beta";
		Assert.Equal("beta", lb.SelectedItem);
		Assert.Equal("beta", lb.SelectedValue);
	}
}
