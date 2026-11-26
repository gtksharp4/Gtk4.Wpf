using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

file sealed class Exposed : ComboBox
{
	public bool Own(object? o) => IsItemItsOwnContainerOverride(o);
	public DependencyObject Make() => GetContainerForItemOverride();
}

/// <summary>Headless object-model tests for ComboBox as an ItemsControl.</summary>
public class ComboBoxItemsControlTests
{
	[Fact]
	public void ComboBox_is_ItemsControl() => Assert.IsAssignableFrom<ItemsControl>(new ComboBox());

	[Fact]
	public void ItemsSourceProperty_is_inherited() =>
		Assert.Same(ItemsControl.ItemsSourceProperty, ComboBox.ItemsSourceProperty);

	[Fact]
	public void ItemsControl_has_ContentProperty_Items()
	{
		var attr = typeof(ItemsControl).GetCustomAttribute<System.Windows.Markup.ContentPropertyAttribute>();
		Assert.Equal("Items", attr!.Name);
	}

	[Fact]
	public void Inline_items_are_selectable()
	{
		var cb = new ComboBox();
		var a = new ComboBoxItem { Content = "A" };
		var b = new ComboBoxItem { Content = "B" };
		cb.Items.Add(a);
		cb.Items.Add(b);
		cb.SelectedItem = b;
		Assert.Equal(1, cb.SelectedIndex);
		Assert.Same(b, cb.SelectedItem);
	}

	[Fact]
	public void SelectedValuePath_Content_yields_content()
	{
		var cb = new ComboBox { SelectedValuePath = "Content" };
		var b = new ComboBoxItem { Content = "B" };
		cb.Items.Add(new ComboBoxItem { Content = "A" });
		cb.Items.Add(b);
		cb.SelectedItem = b;
		Assert.Equal("B", cb.SelectedValue);
	}

	[Fact]
	public void Container_hooks()
	{
		var cb = new Exposed();
		Assert.True(cb.Own(new ComboBoxItem()));
		Assert.False(cb.Own("x"));
		Assert.IsType<ComboBoxItem>(cb.Make());
	}

	[Fact]
	public void ItemsSource_with_inline_items_throws()
	{
		var cb = new ComboBox();
		cb.Items.Add("x");
		var ex = Assert.Throws<InvalidOperationException>(() => cb.ItemsSource = new[] { "a" });
		Assert.Equal("Items collection must be empty before using ItemsSource.", ex.Message);
	}

	[Fact]
	public void Items_Add_with_ItemsSource_throws()
	{
		var cb = new ComboBox { ItemsSource = new[] { "a" } };
		Assert.Throws<InvalidOperationException>(() => cb.Items.Add("b"));
	}

	[Fact]
	public void Clearing_ItemsSource_empties_Items_then_inline_allowed()
	{
		var cb = new ComboBox { ItemsSource = new[] { "a" } };
		cb.ItemsSource = null;
		Assert.Empty(cb.Items);
		cb.Items.Add("z");
		Assert.Single(cb.Items);
	}

	[Fact]
	public void ItemsSource_populates_Items()
	{
		var cb = new ComboBox { ItemsSource = new[] { "a", "b" } };
		Assert.Equal(new object?[] { "a", "b" }, cb.Items.Cast<object?>().ToArray());
	}

	[Fact]
	public void DisplayMemberPath_is_inherited() =>
		Assert.Same(ItemsControl.DisplayMemberPathProperty, ComboBox.DisplayMemberPathProperty);

	[Fact]
	public void Selected_properties_keep_two_way_defaults()
	{
		foreach (var dp in new[] { ComboBox.SelectedIndexProperty, ComboBox.SelectedItemProperty,
			ComboBox.SelectedValueProperty })
		{
			var m = (FrameworkPropertyMetadata)dp.GetMetadata(typeof(ComboBox));
			Assert.True(m.BindsTwoWayByDefault);
		}
	}

	[Fact]
	public void TextSearch_defaults()
	{
		var cb = new ComboBox();
		Assert.True(cb.IsTextSearchEnabled);
		Assert.False(cb.IsTextSearchCaseSensitive);
	}
}
