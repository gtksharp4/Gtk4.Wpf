using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Xunit;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfSelectionMode = System.Windows.Controls.SelectionMode;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Headless unit tests for SelectionChanged AddedItems / RemovedItems
/// and UnselectAll on ListBox.
/// </summary>
public class ListBoxMultiSelectEventTests
{
	// ── SelectionChanged with AddedItems / RemovedItems ───────────────────────

	[Fact]
	public void SelectionChanged_AddedItems_has_new_item()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		IList<object?>? added = null;
		lb.SelectionChanged += (s, e) =>
		{
			added = new List<object?>();
			foreach (var item in e.AddedItems) added.Add(item);
		};
		lb.SelectedIndex = 2;
		Assert.NotNull(added);
		Assert.Equal(1, added!.Count);
		Assert.Equal("c", added[0]);
	}

	[Fact]
	public void SelectionChanged_RemovedItems_has_old_item()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		lb.SelectedIndex = 0;
		IList<object?>? removed = null;
		lb.SelectionChanged += (s, e) =>
		{
			removed = new List<object?>();
			foreach (var item in e.RemovedItems) removed.Add(item);
		};
		lb.SelectedIndex = 2;
		Assert.NotNull(removed);
		Assert.Equal(1, removed!.Count);
		Assert.Equal("a", removed[0]);
	}

	[Fact]
	public void SelectionChanged_fires_once_on_SelectedIndex_change()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		int count = 0;
		lb.SelectionChanged += (s, e) => count++;
		lb.SelectedIndex = 1;
		Assert.Equal(1, count);
	}

	[Fact]
	public void SelectionChanged_on_deselect_has_empty_added_and_old_in_removed()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		lb.SelectedIndex = 0;
		IList<object?>? added = null;
		IList<object?>? removed = null;
		lb.SelectionChanged += (s, e) =>
		{
			added = new List<object?>();
			removed = new List<object?>();
			foreach (var item in e.AddedItems) added.Add(item);
			foreach (var item in e.RemovedItems) removed.Add(item);
		};
		lb.SelectedIndex = -1;
		Assert.NotNull(added);
		Assert.Equal(0, added!.Count);
		Assert.Equal(1, removed!.Count);
		Assert.Equal("a", removed[0]);
	}

	// ── UnselectAll (headless) ────────────────────────────────────────────────

	[Fact]
	public void UnselectAll_headless_clears_selection_and_raises_event()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		lb.SelectedIndex = 0;
		int count = 0;
		IList<object?>? removed = null;
		lb.SelectionChanged += (s, e) =>
		{
			count++;
			removed = new List<object?>();
			foreach (var item in e.RemovedItems) removed.Add(item);
		};
		lb.UnselectAll();
		Assert.Equal(1, count);
		Assert.Equal(-1, lb.SelectedIndex);
		Assert.Null(lb.SelectedItem);
		Assert.Equal(0, lb.SelectedItems.Count);
		Assert.NotNull(removed);
		Assert.Equal(1, removed!.Count);
		Assert.Equal("a", removed[0]);
	}

	[Fact]
	public void UnselectAll_headless_no_event_when_nothing_selected()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		int count = 0;
		lb.SelectionChanged += (s, e) => count++;
		lb.UnselectAll();
		Assert.Equal(0, count);
	}
}
