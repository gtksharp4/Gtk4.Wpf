using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.Controls;

// ── POCO helpers ─────────────────────────────────────────────────────────────

file sealed class Item
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public Address? Address { get; set; }
}

file sealed class Address
{
	public string City { get; set; } = string.Empty;
}

// ── SelectorValueSupport ─────────────────────────────────────────────────────

/// <summary>Pure unit tests for <see cref="SelectorValueSupport"/>.</summary>
public class SelectorValueSupportTests
{
	[Fact]
	public void GetValue_null_path_returns_item()
	{
		var item = new Item { Id = 1 };
		Assert.Same(item, SelectorValueSupport.GetValue(item, null));
	}

	[Fact]
	public void GetValue_empty_path_returns_item()
	{
		var item = new Item { Id = 2 };
		Assert.Same(item, SelectorValueSupport.GetValue(item, string.Empty));
	}

	[Fact]
	public void GetValue_single_segment_reads_property()
	{
		var item = new Item { Id = 42, Name = "foo" };
		Assert.Equal(42, SelectorValueSupport.GetValue(item, "Id"));
		Assert.Equal("foo", SelectorValueSupport.GetValue(item, "Name"));
	}

	[Fact]
	public void GetValue_dotted_path_recurses()
	{
		var item = new Item { Address = new Address { City = "Rome" } };
		Assert.Equal("Rome", SelectorValueSupport.GetValue(item, "Address.City"));
	}

	[Fact]
	public void GetValue_missing_segment_returns_null()
	{
		var item = new Item { Id = 1 };
		Assert.Null(SelectorValueSupport.GetValue(item, "Missing"));
	}

	[Fact]
	public void GetValue_null_intermediate_returns_null()
	{
		// Address is null; City segment cannot be resolved.
		var item = new Item { Address = null };
		Assert.Null(SelectorValueSupport.GetValue(item, "Address.City"));
	}

	[Fact]
	public void GetValue_null_item_returns_null()
	{
		Assert.Null(SelectorValueSupport.GetValue(null, "Id"));
	}

	[Fact]
	public void FindByValue_returns_first_match()
	{
		var items = new object[] { new Item { Id = 1 }, new Item { Id = 2 }, new Item { Id = 3 } };
		var list = new System.Collections.ArrayList();
		foreach (var i in items) list.Add(i);

		var result = SelectorValueSupport.FindByValue(list, "Id", 2);
		Assert.Equal(2, ((Item)result!).Id);
	}

	[Fact]
	public void FindByValue_no_match_returns_null()
	{
		var list = new System.Collections.ArrayList();
		list.Add(new Item { Id = 1 });

		Assert.Null(SelectorValueSupport.FindByValue(list, "Id", 99));
	}

	[Fact]
	public void FindByValue_empty_path_matches_by_object_equality()
	{
		var a = "alpha";
		var b = "beta";
		var list = new System.Collections.ArrayList();
		list.Add(a);
		list.Add(b);

		Assert.Same(b, SelectorValueSupport.FindByValue(list, string.Empty, "beta"));
	}
}

// ── ListBox SelectedValue / SelectedValuePath ─────────────────────────────────

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

// ── ComboBox SelectedValue / SelectedValuePath ────────────────────────────────

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
