using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Xunit;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfSelectionMode = System.Windows.Controls.SelectionMode;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for ListBox multi-selection. Skipped when GTK is unavailable.
/// All bodies run on the GTK thread via Run(…).
/// </summary>
public class ListBoxMultiSelectGtkTests : WpfGtkTestBase
{
	// ── GTK SelectionMode applied on realize ──────────────────────────────────

	[Fact]
	public void SelectionMode_Multiple_applied_to_GtkListBox() => Run(() =>
	{
		var lb = new WpfListBox { SelectionMode = WpfSelectionMode.Multiple };
		lb.ItemsSource = new[] { "a", "b", "c" };
		var gtkLb = (Gtk.ListBox)lb.Widget;
		Assert.Equal(Gtk.SelectionMode.Multiple, gtkLb.SelectionMode);
	});

	[Fact]
	public void SelectionMode_Extended_applied_to_GtkListBox() => Run(() =>
	{
		var lb = new WpfListBox { SelectionMode = WpfSelectionMode.Extended };
		lb.ItemsSource = new[] { "a", "b", "c" };
		var gtkLb = (Gtk.ListBox)lb.Widget;
		Assert.Equal(Gtk.SelectionMode.Multiple, gtkLb.SelectionMode);
	});

	[Fact]
	public void SelectionMode_Single_applied_to_GtkListBox() => Run(() =>
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		var gtkLb = (Gtk.ListBox)lb.Widget;
		Assert.Equal(Gtk.SelectionMode.Single, gtkLb.SelectionMode);
	});

	[Fact]
	public void SelectionMode_changed_after_realize_updates_GtkListBox() => Run(() =>
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		_ = lb.Widget; // materialize in Single
		lb.SelectionMode = WpfSelectionMode.Multiple;
		var gtkLb = (Gtk.ListBox)lb.Widget;
		Assert.Equal(Gtk.SelectionMode.Multiple, gtkLb.SelectionMode);
	});

	// ── Multi-select: two rows selected → SelectedItems has both ─────────────

	[Fact]
	public void Multiple_mode_selecting_two_rows_via_gtk_gives_both_in_SelectedItems() => Run(() =>
	{
		var lb = new WpfListBox { SelectionMode = WpfSelectionMode.Multiple };
		lb.ItemsSource = new[] { "alpha", "beta", "gamma" };
		var gtkLb = (Gtk.ListBox)lb.Widget;

		var row0 = gtkLb.GetRowAtIndex(0);
		var row1 = gtkLb.GetRowAtIndex(1);
		Assert.NotNull(row0);
		Assert.NotNull(row1);

		gtkLb.SelectRow(row0!);
		gtkLb.SelectRow(row1!);

		Assert.Equal(2, lb.SelectedItems.Count);
		Assert.True(lb.SelectedItems.Contains("alpha"), "SelectedItems should contain 'alpha'");
		Assert.True(lb.SelectedItems.Contains("beta"), "SelectedItems should contain 'beta'");
	});

	[Fact]
	public void Multiple_mode_SelectedItem_is_first_selected() => Run(() =>
	{
		var lb = new WpfListBox { SelectionMode = WpfSelectionMode.Multiple };
		lb.ItemsSource = new[] { "alpha", "beta", "gamma" };
		var gtkLb = (Gtk.ListBox)lb.Widget;

		gtkLb.SelectRow(gtkLb.GetRowAtIndex(0)!);
		gtkLb.SelectRow(gtkLb.GetRowAtIndex(2)!);

		// SelectedItem = first selected (lowest index in SelectedRows array).
		Assert.Equal("alpha", lb.SelectedItem);
		Assert.Equal(0, lb.SelectedIndex);
	});

	// ── SelectionChanged fires with correct AddedItems in multi-select ────────

	[Fact]
	public void Multiple_mode_SelectionChanged_reports_added_items() => Run(() =>
	{
		var lb = new WpfListBox { SelectionMode = WpfSelectionMode.Multiple };
		lb.ItemsSource = new[] { "a", "b", "c" };
		var gtkLb = (Gtk.ListBox)lb.Widget;

		var allAdded = new List<object?>();
		lb.SelectionChanged += (s, e) =>
		{
			foreach (var item in e.AddedItems) allAdded.Add(item);
		};

		gtkLb.SelectRow(gtkLb.GetRowAtIndex(0)!);
		gtkLb.SelectRow(gtkLb.GetRowAtIndex(2)!);

		Assert.True(allAdded.Contains("a"), "AddedItems should contain 'a'");
		Assert.True(allAdded.Contains("c"), "AddedItems should contain 'c'");
	});

	// ── Single mode: second GTK row selection replaces first ──────────────────

	[Fact]
	public void Single_mode_gtk_select_row1_then_row2_leaves_one_item() => Run(() =>
	{
		var lb = new WpfListBox(); // Single
		lb.ItemsSource = new[] { "x", "y", "z" };
		var gtkLb = (Gtk.ListBox)lb.Widget;

		gtkLb.SelectRow(gtkLb.GetRowAtIndex(0)!);
		gtkLb.SelectRow(gtkLb.GetRowAtIndex(1)!);

		Assert.Equal(1, lb.SelectedItems.Count);
		Assert.Equal("y", lb.SelectedItem);
	});

	// ── SelectAll / UnselectAll with GTK ─────────────────────────────────────

	[Fact]
	public void SelectAll_in_Multiple_mode_selects_all_items() => Run(() =>
	{
		var lb = new WpfListBox { SelectionMode = WpfSelectionMode.Multiple };
		lb.ItemsSource = new[] { "a", "b", "c" };
		_ = lb.Widget;
		lb.SelectAll();
		Assert.Equal(3, lb.SelectedItems.Count);
	});

	[Fact]
	public void UnselectAll_clears_selection() => Run(() =>
	{
		var lb = new WpfListBox { SelectionMode = WpfSelectionMode.Multiple };
		lb.ItemsSource = new[] { "a", "b", "c" };
		var gtkLb = (Gtk.ListBox)lb.Widget;
		gtkLb.SelectAll();
		lb.UnselectAll();
		Assert.Equal(0, lb.SelectedItems.Count);
		Assert.Equal(-1, lb.SelectedIndex);
		Assert.Null(lb.SelectedItem);
	});
}
