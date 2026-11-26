using System;
using System.Windows;
using Xunit;
using WpfComboBox = System.Windows.Controls.ComboBox;
using WpfListBox = System.Windows.Controls.ListBox;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pinning tests for D3 (SelectionChangedEventArgs.Source type),
/// D4 (SelectionChanged fires exactly once), and D5 (SelectedItem syncs GTK row).
/// D5 GTK-sync part is covered by SelectionDefectGtkTests (runs on GTK thread).
/// </summary>
public class SelectionDefectTests
{
	// ── D3: Source is typed object; non-null for ListBox ─────────────────────

	[Fact]
	public void D3_ListBox_SelectionChanged_Source_is_the_listbox()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };

		object? capturedSource = null;
		lb.SelectionChanged += (s, e) => { capturedSource = e.Source; };

		lb.SelectedIndex = 1;

		Assert.NotNull(capturedSource);
		Assert.Same(lb, capturedSource);
	}

	[Fact]
	public void D3_ComboBox_SelectionChanged_Source_is_the_combobox()
	{
		var cb = new WpfComboBox();
		cb.ItemsSource = new[] { "x", "y" };

		object? capturedSource = null;
		cb.SelectionChanged += (s, e) => { capturedSource = e.Source; };

		cb.SelectedItem = "y";

		Assert.NotNull(capturedSource);
		Assert.Same(cb, capturedSource);
	}

	// ── D4: SelectionChanged fires exactly once per logical change ─────────

	[Fact]
	public void D4_SelectedIndex_set_once_raises_exactly_once()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };

		int count = 0;
		lb.SelectionChanged += (s, e) => { count++; };

		lb.SelectedIndex = 2;

		Assert.Equal(1, count);
	}

	[Fact]
	public void D4_SelectedItem_set_once_raises_exactly_once()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };

		int count = 0;
		lb.SelectionChanged += (s, e) => { count++; };

		lb.SelectedItem = "b";

		Assert.Equal(1, count);
	}

	[Fact]
	public void D4_SelectedIndex_event_has_correct_old_and_new_items()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		lb.SelectedIndex = 0; // pre-select

		object? oldVal = null;
		object? newVal = null;
		lb.SelectionChanged += (s, e) =>
		{
			oldVal = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
			newVal = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
		};

		lb.SelectedIndex = 2;

		Assert.Equal("a", oldVal);
		Assert.Equal("c", newVal);
	}

	[Fact]
	public void D4_SelectedItem_event_has_correct_old_and_new_items()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		lb.SelectedItem = "a"; // pre-select

		object? oldVal = null;
		object? newVal = null;
		lb.SelectionChanged += (s, e) =>
		{
			oldVal = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
			newVal = e.AddedItems.Count > 0 ? e.AddedItems[0] : null;
		};

		lb.SelectedItem = "c";

		Assert.Equal("a", oldVal);
		Assert.Equal("c", newVal);
	}

	// ── D5: SelectedItem programmatic set cross-syncs SelectedIndex ──────────

	[Fact]
	public void D5_SelectedItem_set_updates_SelectedIndex()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };

		lb.SelectedItem = "c";

		Assert.Equal(2, lb.SelectedIndex);
	}

	[Fact]
	public void D5_SelectedItem_not_in_list_sets_index_minus_one()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		lb.SelectedIndex = 0; // pre-select

		int eventCount = 0;
		lb.SelectionChanged += (s, e) => { eventCount++; };

		lb.SelectedItem = "notInList";

		Assert.Equal(-1, lb.SelectedIndex);
		// One event for the deselect.
		Assert.Equal(1, eventCount);
	}

	[Fact]
	public void D5_SelectedIndex_set_updates_SelectedItem()
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };

		lb.SelectedIndex = 2;

		Assert.Equal("c", lb.SelectedItem);
	}
}

/// <summary>
/// D5 GTK smoke: after SelectedItem set, the backing Gtk.ListBox row is selected.
/// Skipped when native GTK is unavailable.
/// </summary>
public class SelectionDefectGtkTests : WpfGtkTestBase
{
	[Fact]
	public void D5_SelectedItem_selects_gtk_row() => Run(() =>
	{
		var lb = new System.Windows.Controls.ListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		_ = lb.Widget; // materialise

		lb.SelectedItem = "c"; // index 2

		Assert.Equal(2, lb.SelectedIndex);
		// Verify GTK row: cast Widget to Gtk.ListBox and inspect selected rows.
		var gtkLb = (Gtk.ListBox)lb.Widget;
		var selectedRow = gtkLb.SelectedRow;
		Assert.NotNull(selectedRow);
	});
}