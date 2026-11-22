using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

public class ItemCollectionTests : WpfGtkTestBase
{
	[Fact]
	public void Inline_edits_work_and_Add_returns_the_index() => Run(() =>
	{
		var list = new ListBox();
		Assert.Equal(0, list.Items.Add("a"));
		Assert.Equal(1, list.Items.Add("b"));
		list.Items.Insert(1, "x");
		Assert.Equal(new object[] { "a", "x", "b" }, list.Items.Cast<object>().ToArray());
		list.Items.Remove("x");
		list.Items.RemoveAt(0);
		Assert.Equal("b", list.Items[0]);
		Assert.Equal(1, list.Items.Count);
		Assert.True(list.Items.Contains("b"));
		Assert.Equal(0, list.Items.IndexOf("b"));
		list.Items.Clear();
		Assert.True(list.Items.IsEmpty);
	});

	[Fact]
	public void Items_is_a_non_generic_IList_and_CollectionView() => Run(() =>
	{
		var list = new ListBox();
		IList asList = list.Items;
		Assert.Equal(0, asList.Add("a"));
		Assert.False(asList.IsReadOnly);
		Assert.IsAssignableFrom<System.Windows.Data.CollectionView>(list.Items);
	});

	[Fact]
	public void ItemsSource_mode_reflects_the_source_and_rejects_edits() => Run(() =>
	{
		var source = new ObservableCollection<string> { "a", "b" };
		var list = new ListBox { ItemsSource = source };
		Assert.Equal(2, list.Items.Count);
		source.Add("c");
		Assert.Equal(3, list.Items.Count);
		Assert.Equal("c", list.Items[2]);
		Assert.Throws<InvalidOperationException>(() => list.Items.Add("d"));
		Assert.Throws<InvalidOperationException>(() => list.Items.Clear());
		Assert.Throws<InvalidOperationException>(() => list.Items.RemoveAt(0));
		list.ItemsSource = null;
		Assert.Equal(0, list.Items.Count);
		Assert.Equal(0, list.Items.Add("z"));
	});

	[Fact]
	public void Setting_ItemsSource_with_inline_items_throws() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		Assert.Throws<InvalidOperationException>(() => list.ItemsSource = new[] { "x" });
	});

	[Fact]
	public void Currency_is_shared_with_the_source_view() => Run(() =>
	{
		var source = new ObservableCollection<string> { "a", "b", "c" };
		var list = new ListBox { ItemsSource = source };
		var view = System.Windows.Data.CollectionViewSource.GetDefaultView(source)!;
		Assert.True(list.Items.MoveCurrentToPosition(1));
		Assert.Equal(1, view.CurrentPosition);
		Assert.Equal("b", list.Items.CurrentItem);
		view.MoveCurrentToLast();
		Assert.Equal(2, list.Items.CurrentPosition);
		Assert.False(list.Items.IsCurrentAfterLast);
		Assert.False(list.Items.IsEmpty);
	});

	[Fact]
	public void Inline_currency_follows_the_items() => Run(() =>
	{
		var list = new ListBox();
		Assert.True(list.Items.IsEmpty);
		list.Items.Add("a");
		list.Items.Add("b");
		Assert.True(list.Items.MoveCurrentToLast());
		Assert.Equal("b", list.Items.CurrentItem);
		Assert.Equal(1, list.Items.CurrentPosition);
	});

	[Fact]
	public void Filter_and_sort_apply_to_inline_items() => Run(() =>
	{
		var list = new ListBox();
		foreach (var s in new[] { "c", "a", "b" }) list.Items.Add(s);
		list.Items.Filter = o => (string)o != "a";
		Assert.Equal(new object[] { "c", "b" }, list.Items.Cast<object>().ToArray());
		list.Items.Filter = null;
		list.Items.SortDescriptions.Add(new SortDescription(string.Empty, ListSortDirection.Ascending));
		Assert.Equal(new object[] { "a", "b", "c" }, list.Items.Cast<object>().ToArray());
	});

	[Fact]
	public void Filtering_inline_items_rebuilds_the_realised_rows() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		list.Items.Add("b");
		_ = list.Widget;
		Assert.NotNull(list.ItemContainerGenerator.ContainerFromIndex(1));
		list.Items.Filter = o => (string)o == "a";
		Assert.NotNull(list.ItemContainerGenerator.ContainerFromIndex(0));
		Assert.Null(list.ItemContainerGenerator.ContainerFromIndex(1));
	});

	[Fact]
	public void CollectionChanged_is_raised_for_inline_and_source_changes() => Run(() =>
	{
		var list = new ListBox();
		int raised = 0;
		list.Items.CollectionChanged += (_, _) => raised++;
		list.Items.Add("a");
		Assert.True(raised > 0);
		list.Items.Clear();
		var source = new ObservableCollection<string>();
		list.ItemsSource = source;
		raised = 0;
		source.Add("x");
		Assert.Equal(1, raised);
	});

	[Fact]
	public void ComboBox_Items_is_an_ItemCollection() => Run(() =>
	{
		var combo = new ComboBox();
		combo.Items.Add("a");
		Assert.IsType<ItemCollection>(combo.Items);
		Assert.Equal(1, combo.Items.Count);
	});
}
