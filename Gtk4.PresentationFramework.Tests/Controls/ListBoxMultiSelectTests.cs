using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Xunit;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfSelectionMode = System.Windows.Controls.SelectionMode;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// Headless unit tests for ListBox SelectionMode, mapping, and SelectedItems.
/// </summary>
public class ListBoxMultiSelectTests
{
	// ── SelectionMode DP ──────────────────────────────────────────────────────

	[Fact]
	public void SelectionMode_default_is_Single()
	{
		var lb = new WpfListBox();
		Assert.Equal(WpfSelectionMode.Single, lb.SelectionMode);
	}

	[Fact]
	public void SelectionMode_can_be_set_to_Multiple()
	{
		var lb = new WpfListBox();
		lb.SelectionMode = WpfSelectionMode.Multiple;
		Assert.Equal(WpfSelectionMode.Multiple, lb.SelectionMode);
	}

	[Fact]
	public void SelectionMode_can_be_set_to_Extended()
	{
		var lb = new WpfListBox();
		lb.SelectionMode = WpfSelectionMode.Extended;
		Assert.Equal(WpfSelectionMode.Extended, lb.SelectionMode);
	}

	[Fact]
	public void SelectionMode_set_before_realize_does_not_throw()
	{
		var lb = new WpfListBox();
		lb.SelectionMode = WpfSelectionMode.Multiple;
		lb.ItemsSource = new[] { "a", "b" };
		Assert.Equal(WpfSelectionMode.Multiple, lb.SelectionMode);
	}

	// ── GTK SelectionMode mapping ─────────────────────────────────────────────

	[Fact]
	public void GtkMapping_Single_maps_to_GtkSingle()
	{
		Assert.Equal(Gtk.SelectionMode.Single, WpfListBox.ToGtkSelectionMode(WpfSelectionMode.Single));
	}

	[Fact]
	public void GtkMapping_Multiple_maps_to_GtkMultiple()
	{
		Assert.Equal(Gtk.SelectionMode.Multiple, WpfListBox.ToGtkSelectionMode(WpfSelectionMode.Multiple));
	}

	[Fact]
	public void GtkMapping_Extended_maps_to_GtkMultiple()
	{
		Assert.Equal(Gtk.SelectionMode.Multiple, WpfListBox.ToGtkSelectionMode(WpfSelectionMode.Extended));
	}

	// ── SelectedItems ─────────────────────────────────────────────────────────

	[Fact]
	public void SelectedItems_empty_by_default()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		Assert.Equal(0, lb.SelectedItems.Count);
	}

	[Fact]
	public void SelectedItems_has_one_after_single_SelectedIndex_set()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		lb.SelectedIndex = 1;
		Assert.Equal(1, lb.SelectedItems.Count);
		Assert.Equal("b", lb.SelectedItems[0]);
	}

	[Fact]
	public void SelectedItem_equals_first_in_SelectedItems()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "x", "y", "z" };
		lb.SelectedIndex = 2;
		Assert.Equal(lb.SelectedItem, lb.SelectedItems[0]);
	}

	[Fact]
	public void SelectedItems_cleared_when_deselected()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		lb.SelectedIndex = 0;
		lb.SelectedIndex = -1;
		Assert.Equal(0, lb.SelectedItems.Count);
	}

	// ── Single mode: selecting second item deselects first ───────────────────

	[Fact]
	public void Single_mode_selecting_second_item_replaces_first()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		lb.SelectedIndex = 0;
		lb.SelectedIndex = 1;
		Assert.Equal(1, lb.SelectedItems.Count);
		Assert.Equal("b", lb.SelectedItems[0]);
		Assert.Equal("b", lb.SelectedItem);
	}
}
