using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>The selection follows its item across Items edits and a pending index waits for its items.</summary>
public class ComboBoxSelectionFollowTests : WpfGtkTestBase
{
	static List<SelectionChangedEventArgs> Track(ComboBox cb)
	{
		var log = new List<SelectionChangedEventArgs>();
		cb.SelectionChanged += (_, e) => log.Add(e);
		return log;
	}

	static ComboBox Abc(int selected)
	{
		var cb = new ComboBox();
		cb.Items.Add("A");
		cb.Items.Add("B");
		cb.Items.Add("C");
		cb.SelectedIndex = selected;
		return cb;
	}

	[Fact]
	public void Pending_index_is_applied_when_items_arrive_headless()
	{
		var cb = new ComboBox { SelectedIndex = 0 };
		var log = Track(cb);
		var a = new ComboBoxItem { Content = "A" };
		cb.Items.Add(a);
		cb.Items.Add(new ComboBoxItem { Content = "B" });
		Assert.Same(a, cb.SelectedItem);
		Assert.Equal("A", cb.Text);
		Assert.Single(log);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Pending_index_is_applied_when_items_arrive_realized(bool widgetFirst) => Run(() =>
	{
		var cb = new ComboBox { SelectedIndex = 0 };
		if (widgetFirst) _ = cb.Widget;
		var log = Track(cb);
		var a = new ComboBoxItem { Content = "A" };
		cb.Items.Add(a);
		cb.Items.Add(new ComboBoxItem { Content = "B" });
		var dd = (Gtk.DropDown)cb.Widget;
		Assert.Same(a, cb.SelectedItem);
		Assert.Equal(0u, dd.Selected);
		Assert.Equal("A", cb.Text);
		Assert.Single(log);
	});

	[Fact]
	public void Xaml_order_index_then_inline_items_then_widget_raises_once() => Run(() =>
	{
		var cb = new ComboBox { SelectedIndex = 0 };
		var log = Track(cb);
		cb.Items.Add("A");
		cb.Items.Add("B");
		_ = cb.Widget;
		Assert.Equal("A", cb.SelectedItem);
		Assert.Single(log);
	});

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Insert_before_selection_keeps_item_and_shifts_index(bool realized) => Run(() =>
	{
		var cb = Abc(1);
		if (realized) _ = cb.Widget;
		var log = Track(cb);
		cb.Items.Insert(0, "Z");
		Assert.Equal("B", cb.SelectedItem);
		Assert.Equal(2, cb.SelectedIndex);
		Assert.Equal("B", cb.Text);
		Assert.Empty(log);
		if (realized) Assert.Equal(2u, ((Gtk.DropDown)cb.Widget).Selected);
	});

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Remove_unrelated_item_keeps_selection(bool realized) => Run(() =>
	{
		var cb = Abc(2);
		if (realized) _ = cb.Widget;
		var log = Track(cb);
		cb.Items.RemoveAt(0);
		Assert.Equal("C", cb.SelectedItem);
		Assert.Equal(1, cb.SelectedIndex);
		Assert.Empty(log);
		if (realized) Assert.Equal(1u, ((Gtk.DropDown)cb.Widget).Selected);
	});

	[Fact]
	public void Clear_headless_resets_selection_and_raises_once()
	{
		var cb = new ComboBox();
		cb.Items.Add("A");
		cb.Items.Add("B");
		cb.SelectedIndex = 1;
		var log = Track(cb);
		cb.Items.Clear();
		Assert.Equal(-1, cb.SelectedIndex);
		Assert.Null(cb.SelectedItem);
		Assert.Equal(string.Empty, cb.Text);
		var e = Assert.Single(log);
		Assert.Equal(new object?[] { "B" }, e.RemovedItems);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Removing_the_selected_item_clears_selection(bool realized) => Run(() =>
	{
		var cb = Abc(1);
		if (realized) _ = cb.Widget;
		var log = Track(cb);
		cb.Items.RemoveAt(1);
		Assert.Equal(-1, cb.SelectedIndex);
		Assert.Null(cb.SelectedItem);
		Assert.Equal(string.Empty, cb.Text);
		Assert.Single(log);
	});

	[Fact]
	public void ItemsSource_insert_before_selection_follows_item() => Run(() =>
	{
		var src = new ObservableCollection<string> { "A", "B", "C" };
		var cb = new ComboBox { ItemsSource = src };
		_ = cb.Widget;
		cb.SelectedIndex = 1;
		var log = Track(cb);
		src.Insert(0, "Z");
		Assert.Equal("B", cb.SelectedItem);
		Assert.Equal(2, cb.SelectedIndex);
		Assert.Empty(log);
	});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Cleared_SelectedItem_is_not_resurrected_by_items_edit(bool realized) => Run(() =>
	{
		var cb = Abc(1);
		if (realized) _ = cb.Widget;
		cb.SelectedItem = null;
		var log = Track(cb);
		cb.Items.Add("D");
		Assert.Equal(-1, cb.SelectedIndex);
		Assert.Null(cb.SelectedItem);
		Assert.Empty(log);
	});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Out_of_range_SelectedIndex_is_ignored(bool realized) => Run(() =>
	{
		var cb = Abc(1);
		if (realized) _ = cb.Widget;
		var log = Track(cb);
		cb.SelectedIndex = 7;
		Assert.Equal(1, cb.SelectedIndex);
		Assert.Equal("B", cb.SelectedItem);
		Assert.Empty(log);
		if (realized) Assert.Equal(1u, ((Gtk.DropDown)cb.Widget).Selected);
	});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Out_of_range_SelectedIndex_without_selection_stays_minus_one(bool realized) => Run(() =>
	{
		var cb = Abc(-1);
		if (realized) _ = cb.Widget;
		var log = Track(cb);
		cb.SelectedIndex = 7;
		Assert.Equal(-1, cb.SelectedIndex);
		Assert.Null(cb.SelectedItem);
		Assert.Empty(log);
	});

	[Fact]
	public void SelectedIndex_below_minus_one_throws_ArgumentException()
	{
		var cb = Abc(1);
		Assert.Throws<ArgumentException>(() => cb.SelectedIndex = -2);
		Assert.Equal(1, cb.SelectedIndex);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Foreign_SelectedItem_is_rejected_and_keeps_selection(bool realized) => Run(() =>
	{
		var cb = Abc(1);
		if (realized) _ = cb.Widget;
		var log = Track(cb);
		cb.SelectedItem = "Z";
		Assert.Equal("B", cb.SelectedItem);
		Assert.Equal(1, cb.SelectedIndex);
		Assert.Empty(log);
		Assert.Equal("B", cb.Text);
		if (realized) Assert.Equal(1u, ((Gtk.DropDown)cb.Widget).Selected);
	});

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Foreign_SelectedItem_without_selection_stays_unselected(bool realized) => Run(() =>
	{
		var cb = Abc(-1);
		if (realized) _ = cb.Widget;
		var log = Track(cb);
		cb.SelectedItem = "Z";
		Assert.Null(cb.SelectedItem);
		Assert.Equal(-1, cb.SelectedIndex);
		Assert.Empty(log);
	});
}
