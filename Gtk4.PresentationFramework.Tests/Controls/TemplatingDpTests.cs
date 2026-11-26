using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Xunit;
using WpfButton = System.Windows.Controls.Button;
using WpfContentPresenter = System.Windows.Controls.ContentPresenter;
using WpfDataTemplate = System.Windows.Controls.DataTemplate;
using WpfItemsControl = System.Windows.Controls.ItemsControl;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfListView = System.Windows.Controls.ListView;
using WpfTextBlock = System.Windows.Controls.TextBlock;
using WpfControlTemplate = System.Windows.Controls.ControlTemplate;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Pure (no GTK) unit tests for Phase-2 templating + items controls.
/// </summary>
public class TemplatingDpTests
{
	// ── FrameworkTemplate / ControlTemplate / DataTemplate ──────────────────

	[Fact]
	public void ControlTemplate_HasContent_false_when_no_factory()
	{
		var ct = new WpfControlTemplate();
		Assert.False(ct.HasContent);
	}

	[Fact]
	public void ControlTemplate_HasContent_true_when_factory_set()
	{
		var ct = new WpfControlTemplate { Factory = () => new WpfTextBlock() };
		Assert.True(ct.HasContent);
	}

	[Fact]
	public void ControlTemplate_CreateVisualTree_returns_null_when_no_factory()
	{
		var ct = new WpfControlTemplate();
		Assert.Null(ct.CreateVisualTree());
	}

	[Fact]
	public void ControlTemplate_CreateVisualTree_invokes_factory()
	{
		var ct = new WpfControlTemplate { Factory = () => new WpfTextBlock { Text = "hi" } };
		var visual = ct.CreateVisualTree();
		Assert.IsType<WpfTextBlock>(visual);
		Assert.Equal("hi", ((WpfTextBlock)visual!).Text);
	}

	[Fact]
	public void ControlTemplate_CreateVisualTree_calls_factory_each_time()
	{
		int calls = 0;
		var ct = new WpfControlTemplate { Factory = () => { calls++; return new WpfTextBlock(); } };
		ct.CreateVisualTree();
		ct.CreateVisualTree();
		Assert.Equal(2, calls);
	}

	[Fact]
	public void ControlTemplate_TargetType_roundtrip()
	{
		var ct = new WpfControlTemplate(typeof(WpfButton));
		Assert.Equal(typeof(WpfButton), ct.TargetType);
	}

	[Fact]
	public void DataTemplate_ItemFactory_called_with_item()
	{
		object? received = null;
		var dt = new WpfDataTemplate
		{
			ItemFactory = item =>
			{
				received = item;
				return new WpfTextBlock { Text = item?.ToString() ?? string.Empty };
			}
		};
		var visual = dt.CreateVisualTreeForItem("hello");
		Assert.IsType<WpfTextBlock>(visual);
		Assert.Equal("hello", received);
	}

	[Fact]
	public void DataTemplate_falls_back_to_Factory_when_ItemFactory_null()
	{
		var dt = new WpfDataTemplate { Factory = () => new WpfTextBlock { Text = "fallback" } };
		var visual = dt.CreateVisualTreeForItem("ignored");
		Assert.IsType<WpfTextBlock>(visual);
		Assert.Equal("fallback", ((WpfTextBlock)visual!).Text);
	}

	[Fact]
	public void DataTemplate_HasContent_false_when_no_factory()
	{
		var dt = new WpfDataTemplate();
		Assert.False(dt.HasContent);
	}

	[Fact]
	public void DataTemplate_HasContent_true_when_ItemFactory_set()
	{
		var dt = new WpfDataTemplate { ItemFactory = _ => new WpfTextBlock() };
		Assert.True(dt.HasContent);
	}

	// ── L1: DataTemplate.HasContent via FrameworkTemplate base reference ─────

	[Fact]
	public void DataTemplate_HasContent_via_base_reference_true_when_ItemFactory_set()
	{
		// L1: HasContent must be virtual/override so a FrameworkTemplate-typed reference
		// sees DataTemplate's value (ItemFactory != null || Factory != null).
		FrameworkTemplate ft = new WpfDataTemplate { ItemFactory = _ => new WpfTextBlock() };
		Assert.True(ft.HasContent);
	}

	[Fact]
	public void DataTemplate_HasContent_via_base_reference_false_when_no_factory()
	{
		FrameworkTemplate ft = new WpfDataTemplate();
		Assert.False(ft.HasContent);
	}

	// ── ContentPresenter ─────────────────────────────────────────────────────

	[Fact]
	public void ContentPresenter_Content_default_is_null()
	{
		var cp = new WpfContentPresenter();
		Assert.Null(cp.Content);
	}

	[Fact]
	public void ContentPresenter_Content_roundtrip_string()
	{
		var cp = new WpfContentPresenter { Content = "hello" };
		Assert.Equal("hello", cp.Content);
	}

	[Fact]
	public void ContentPresenter_Content_roundtrip_UIElement()
	{
		var tb = new WpfTextBlock { Text = "hi" };
		var cp = new WpfContentPresenter { Content = tb };
		Assert.Same(tb, cp.Content);
	}

	[Fact]
	public void ContentPresenter_ContentTemplate_roundtrip()
	{
		var dt = new WpfDataTemplate { ItemFactory = _ => new WpfTextBlock() };
		var cp = new WpfContentPresenter { ContentTemplate = dt };
		Assert.Same(dt, cp.ContentTemplate);
	}

	// ── ItemsControl ─────────────────────────────────────────────────────────

	[Fact]
	public void ItemsControl_ItemsSource_null_gives_empty_Items()
	{
		var ic = new WpfItemsControl();
		Assert.Empty(ic.Items);
	}

	[Fact]
	public void ItemsControl_three_strings_produce_three_items()
	{
		var ic = new WpfItemsControl();
		ic.ItemsSource = new[] { "a", "b", "c" };
		Assert.Equal(3, ic.Items.Count);
	}

	[Fact]
	public void ItemsControl_Items_contains_the_source_values()
	{
		var ic = new WpfItemsControl();
		ic.ItemsSource = new[] { "x", "y" };
		Assert.Equal("x", ic.Items[0]);
		Assert.Equal("y", ic.Items[1]);
	}

	[Fact]
	public void ItemsControl_ItemTemplate_items_populate()
	{
		var ic = new WpfItemsControl
		{
			ItemTemplate = new WpfDataTemplate
			{
				ItemFactory = item => new WpfTextBlock { Text = item?.ToString() ?? string.Empty }
			}
		};
		ic.ItemsSource = new[] { "one", "two", "three" };
		Assert.Equal(3, ic.Items.Count);
	}

	[Fact]
	public void ItemsControl_reacts_to_ObservableCollection_add()
	{
		var coll = new ObservableCollection<string> { "a", "b" };
		var ic = new WpfItemsControl { ItemsSource = coll };
		Assert.Equal(2, ic.Items.Count);
		coll.Add("c");
		Assert.Equal(3, ic.Items.Count);
	}

	[Fact]
	public void ItemsControl_reacts_to_ObservableCollection_remove()
	{
		var coll = new ObservableCollection<string> { "a", "b", "c" };
		var ic = new WpfItemsControl { ItemsSource = coll };
		coll.RemoveAt(1);
		Assert.Equal(2, ic.Items.Count);
	}

	[Fact]
	public void ItemsControl_snapshot_on_plain_IEnumerable()
	{
		var list = new List<string> { "p", "q" };
		var ic = new WpfItemsControl { ItemsSource = list };
		Assert.Equal(2, ic.Items.Count);
		// No INCC — snapshot is not live:
		list.Add("r");
		Assert.Equal(2, ic.Items.Count);
	}

	// ── ListBox DP ────────────────────────────────────────────────────────────

	[Fact]
	public void ListBox_SelectedIndex_default_is_minus_one()
	{
		var lb = new WpfListBox();
		Assert.Equal(-1, lb.SelectedIndex);
	}

	[Fact]
	public void ListBox_SelectedItem_default_is_null()
	{
		var lb = new WpfListBox();
		Assert.Null(lb.SelectedItem);
	}

	[Fact]
	public void ListBox_SelectedItem_set_updates_SelectedIndex()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		lb.SelectedItem = "b";
		Assert.Equal(1, lb.SelectedIndex);
	}

	[Fact]
	public void ListBox_SelectedIndex_set_updates_SelectedItem()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "x", "y", "z" };
		lb.SelectedIndex = 2;
		Assert.Equal("z", lb.SelectedItem);
	}

	[Fact]
	public void ListBox_SelectionChanged_fires_when_SelectedItem_set()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		bool fired = false;
		lb.SelectionChanged += (s, e) => { fired = true; };
		lb.SelectedItem = "a";
		Assert.True(fired);
	}

	[Fact]
	public void ListBox_SelectionChanged_carries_old_and_new_item()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		lb.SelectedItem = "a";

		object? capturedOld = "sentinel";
		object? capturedNew = "sentinel";
		lb.SelectionChanged += (s, e) =>
		{
			capturedOld = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
			capturedNew = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
		};
		lb.SelectedItem = "b";
		Assert.Equal("a", capturedOld);
		Assert.Equal("b", capturedNew);
	}

	[Fact]
	public void ListBox_SelectedIndex_out_of_range_clears_SelectedItem()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		lb.SelectedIndex = 1;
		lb.SelectedIndex = -1;
		Assert.Null(lb.SelectedItem);
	}

	[Fact]
	public void ListBox_items_rebuilt_resets_selection()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		lb.SelectedIndex = 1;
		// Replace ItemsSource
		lb.ItemsSource = new[] { "x" };
		Assert.Equal(-1, lb.SelectedIndex);
		Assert.Null(lb.SelectedItem);
	}

	// ── ListView ──────────────────────────────────────────────────────────────

	[Fact]
	public void ListView_inherits_ListBox_items()
	{
		var lv = new WpfListView();
		lv.ItemsSource = new[] { "1", "2", "3" };
		Assert.Equal(3, lv.Items.Count);
	}

	[Fact]
	public void ListView_SelectionChanged_fires()
	{
		var lv = new WpfListView();
		lv.ItemsSource = new[] { "a", "b" };
		bool fired = false;
		lv.SelectionChanged += (s, e) => { fired = true; };
		lv.SelectedItem = "b";
		Assert.True(fired);
	}

	// ── L2: UIElement.IsWidgetCreated consistency via base reference ─────────

	[Fact]
	public void ContentControl_IsWidgetCreated_false_before_Widget_access()
	{
		// Before Widget is accessed, both base and derived views must agree: false.
		var cc = new WpfButton();
		UIElement ue = cc;
		Assert.False(cc.IsWidgetCreated);
		Assert.False(ue.IsWidgetCreated);
	}
}