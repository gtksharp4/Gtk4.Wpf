using System;
using System.Windows;
using Xunit;
using WpfContentPresenter = System.Windows.Controls.ContentPresenter;
using WpfDataTemplate = System.Windows.Controls.DataTemplate;
using WpfItemsControl = System.Windows.Controls.ItemsControl;
using WpfListBox = System.Windows.Controls.ListBox;
using WpfListView = System.Windows.Controls.ListView;
using WpfTextBlock = System.Windows.Controls.TextBlock;

namespace Gtk4.PresentationFramework.Tests.Controls;

/// <summary>
/// GTK smoke tests for Phase-2 templating + items controls.
/// All bodies run on the GTK thread via Run(…).
/// Skipped automatically when native GTK is unavailable.
/// </summary>
public class TemplatingGtkSmokeTests : WpfGtkTestBase
{
	// ── ContentPresenter ─────────────────────────────────────────────────────

	[Fact]
	public void ContentPresenter_string_content_produces_GtkBox() => Run(() =>
	{
		var cp = new WpfContentPresenter { Content = "hello" };
		Assert.IsType<Gtk.Box>(cp.Widget);
	});

	[Fact]
	public void ContentPresenter_UIElement_content_hosts_its_widget() => Run(() =>
	{
		var tb = new WpfTextBlock { Text = "inner" };
		var cp = new WpfContentPresenter { Content = tb };
		var box = (Gtk.Box)cp.Widget;
		Assert.Same(tb.Widget, box.FirstChild);
	});

	[Fact]
	public void ContentPresenter_DataTemplate_produces_templated_widget() => Run(() =>
	{
		var dt = new WpfDataTemplate
		{
			ItemFactory = item => new WpfTextBlock { Text = item?.ToString() ?? string.Empty }
		};
		var cp = new WpfContentPresenter { Content = "world", ContentTemplate = dt };
		var box = (Gtk.Box)cp.Widget;
		var child = box.FirstChild;
		Assert.NotNull(child);
		// DataTemplate factory returns a TextBlock backed by Gtk.Label.
		Assert.IsType<Gtk.Label>(child);
	});

	[Fact]
	public void ContentPresenter_content_switch_does_not_throw() => Run(() =>
	{
		var cp = new WpfContentPresenter { Content = "initial" };
		_ = cp.Widget;
		var ex = Record.Exception(() => cp.Content = "updated");
		Assert.Null(ex);
	});

	// ── ItemsControl ─────────────────────────────────────────────────────────

	[Fact]
	public void ItemsControl_three_items_backs_GtkBox() => Run(() =>
	{
		var ic = new WpfItemsControl();
		ic.ItemsSource = new[] { "a", "b", "c" };
		Assert.IsType<Gtk.Box>(ic.Widget);
	});

	[Fact]
	public void ItemsControl_three_items_packs_three_child_widgets() => Run(() =>
	{
		var ic = new WpfItemsControl();
		ic.ItemsSource = new[] { "a", "b", "c" };
		var box = (Gtk.Box)ic.Widget;
		int count = CountChildren(box);
		Assert.Equal(3, count);
	});

	[Fact]
	public void ItemsControl_DataTemplate_factory_produces_widgets() => Run(() =>
	{
		var ic = new WpfItemsControl
		{
			ItemTemplate = new WpfDataTemplate
			{
				ItemFactory = item => new WpfTextBlock { Text = item?.ToString() ?? string.Empty }
			}
		};
		ic.ItemsSource = new[] { "x", "y", "z" };
		var box = (Gtk.Box)ic.Widget;
		Assert.Equal(3, CountChildren(box));
	});

	// ── ListBox ───────────────────────────────────────────────────────────────

	[Fact]
	public void ListBox_Widget_is_GtkListBox() => Run(() =>
	{
		var lb = new WpfListBox();
		Assert.IsType<Gtk.ListBox>(lb.Widget);
	});

	[Fact]
	public void ListBox_three_items_produce_three_GtkListBoxRows() => Run(() =>
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "one", "two", "three" };
		var gtkLb = (Gtk.ListBox)lb.Widget;
		int count = 0;
		var child = gtkLb.FirstChild;
		while (child != null)
		{
			if (child is Gtk.ListBoxRow) count++;
			child = child.NextSibling;
		}
		Assert.Equal(3, count);
	});

	[Fact]
	public void ListBox_SelectedIndex_set_selects_correct_item() => Run(() =>
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b", "c" };
		_ = lb.Widget;
		lb.SelectedIndex = 1;
		Assert.Equal(1, lb.SelectedIndex);
		Assert.Equal("b", lb.SelectedItem);
	});

	[Fact]
	public void ListBox_GtkRowSelected_updates_SelectedIndex() => Run(() =>
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "p", "q", "r" };
		var gtkLb = (Gtk.ListBox)lb.Widget;
		var row = gtkLb.GetRowAtIndex(2);
		if (row != null) gtkLb.SelectRow(row);
		Assert.Equal(2, lb.SelectedIndex);
		Assert.Equal("r", lb.SelectedItem);
	});

	[Fact]
	public void ListBox_SelectionChanged_fires_on_GtkRowSelected() => Run(() =>
	{
		var lb = new WpfListBox();
		lb.ItemsSource = new[] { "a", "b" };
		var gtkLb = (Gtk.ListBox)lb.Widget;
		bool fired = false;
		lb.SelectionChanged += (s, e) => { fired = true; };
		var row = gtkLb.GetRowAtIndex(0);
		if (row != null) gtkLb.SelectRow(row);
		Assert.True(fired);
	});

	// ── ListView ──────────────────────────────────────────────────────────────

	[Fact]
	public void ListView_Widget_is_GtkListBox() => Run(() =>
	{
		var lv = new WpfListView();
		Assert.IsType<Gtk.ListBox>(lv.Widget);
	});

	[Fact]
	public void ListView_three_items_produce_three_rows() => Run(() =>
	{
		var lv = new WpfListView();
		lv.ItemsSource = new[] { "1", "2", "3" };
		var gtkLb = (Gtk.ListBox)lv.Widget;
		int count = 0;
		var child = gtkLb.FirstChild;
		while (child != null)
		{
			if (child is Gtk.ListBoxRow) count++;
			child = child.NextSibling;
		}
		Assert.Equal(3, count);
	});

	// ── Helpers ───────────────────────────────────────────────────────────────

	static int CountChildren(Gtk.Widget container)
	{
		int count = 0;
		var child = container.FirstChild;
		while (child != null) { count++; child = child.NextSibling; }
		return count;
	}
}