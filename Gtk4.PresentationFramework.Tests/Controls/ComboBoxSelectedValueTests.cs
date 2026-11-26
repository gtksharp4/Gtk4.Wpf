using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

file sealed class Item
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
}

/// <summary>Headless tests for ComboBox SelectedValue / SelectedValuePath.</summary>
public class ComboBoxSelectedValueTests
{
	[Fact]
	public void SelectedValuePath_default_is_empty()
	{
		var cb = new ComboBox();
		Assert.Equal(string.Empty, cb.SelectedValuePath);
	}

	[Fact]
	public void SelectedValue_null_when_nothing_selected()
	{
		var cb = new ComboBox();
		cb.ItemsSource = new[] { "a", "b" };
		Assert.Null(cb.SelectedValue);
	}

	[Fact]
	public void SelectedValue_equals_item_when_path_empty()
	{
		var cb = new ComboBox();
		cb.ItemsSource = new[] { "alpha", "beta" };
		cb.SelectedItem = "beta";
		Assert.Equal("beta", cb.SelectedValue);
	}

	[Fact]
	public void SelectedValue_reflects_property_path()
	{
		var items = new[] { new Item { Id = 10 }, new Item { Id = 20 } };
		var cb = new ComboBox();
		cb.ItemsSource = items;
		cb.SelectedValuePath = "Id";
		cb.SelectedItem = items[0];
		Assert.Equal(10, cb.SelectedValue);
	}

	[Fact]
	public void SelectedValue_updates_when_SelectedItem_changes()
	{
		var items = new[] { new Item { Id = 1 }, new Item { Id = 2 } };
		var cb = new ComboBox();
		cb.ItemsSource = items;
		cb.SelectedValuePath = "Id";
		cb.SelectedItem = items[0];
		Assert.Equal(1, cb.SelectedValue);
		cb.SelectedItem = items[1];
		Assert.Equal(2, cb.SelectedValue);
	}

	[Fact]
	public void SelectedValue_clears_when_selection_cleared()
	{
		var cb = new ComboBox();
		cb.ItemsSource = new[] { "a", "b" };
		cb.SelectedItem = "a";
		Assert.Equal("a", cb.SelectedValue);
		cb.SelectedItem = null;
		Assert.Null(cb.SelectedValue);
	}

	[Fact]
	public void SelectedValuePath_change_re_resolves_SelectedValue()
	{
		var items = new[] { new Item { Id = 7, Name = "bar" } };
		var cb = new ComboBox();
		cb.ItemsSource = items;
		cb.SelectedItem = items[0];
		cb.SelectedValuePath = "Id";
		Assert.Equal(7, cb.SelectedValue);
		cb.SelectedValuePath = "Name";
		Assert.Equal("bar", cb.SelectedValue);
	}

	[Fact]
	public void Setting_SelectedValue_selects_matching_item()
	{
		var items = new[] { new Item { Id = 1 }, new Item { Id = 2 }, new Item { Id = 3 } };
		var cb = new ComboBox();
		cb.ItemsSource = items;
		cb.SelectedValuePath = "Id";
		cb.SelectedValue = 3;
		Assert.Equal(items[2], cb.SelectedItem);
	}

	[Fact]
	public void Setting_SelectedValue_no_match_clears_selection()
	{
		var items = new[] { new Item { Id = 1 }, new Item { Id = 2 } };
		var cb = new ComboBox();
		cb.ItemsSource = items;
		cb.SelectedValuePath = "Id";
		cb.SelectedItem = items[0];
		cb.SelectedValue = 99;
		Assert.Null(cb.SelectedItem);
	}

	[Fact]
	public void Setting_SelectedValue_empty_path_selects_by_identity()
	{
		var cb = new ComboBox();
		cb.ItemsSource = new[] { "alpha", "beta", "gamma" };
		cb.SelectedValue = "gamma";
		Assert.Equal("gamma", cb.SelectedItem);
		Assert.Equal("gamma", cb.SelectedValue);
	}
}
