using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>Selector.IsSynchronizedWithCurrentItem against the default collection view.</summary>
public class SelectorSynchronizationTests : WpfGtkTestBase
{
	static System.ComponentModel.ICollectionView View(object source) => CollectionViewSource.GetDefaultView(source)!;

	static ObservableCollection<string> Names() => new ObservableCollection<string> { "a", "b", "c" };

	[Fact]
	public void A_string_ItemsSource_has_no_view_and_does_not_throw() => Run(() =>
	{
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = "abc" };
		Assert.Equal(3, lb.Items.Count);
		Assert.Null(lb.SelectedItem);
	});

	[Fact]
	public void Default_is_null_and_ListBox_ComboBox_TabControl_are_Selectors() => Run(() =>
	{
		var lb = new ListBox();
		Assert.Null(lb.IsSynchronizedWithCurrentItem);
		Assert.IsAssignableFrom<Selector>(lb);
		Assert.IsAssignableFrom<Selector>(new ComboBox());
		Assert.IsAssignableFrom<Selector>(new TabControl());
	});

	[Fact]
	public void True_selects_the_view_current_item_initially() => Run(() =>
	{
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = Names() };
		Assert.Equal("a", lb.SelectedItem);
	});

	[Fact]
	public void True_selection_follows_the_view_current_item() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		var view = View(src);
		view.MoveCurrentTo("c");
		Assert.Equal("c", lb.SelectedItem);
		Assert.Equal(2, lb.SelectedIndex);
	});

	[Fact]
	public void True_view_current_item_follows_the_selection() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		lb.SelectedItem = "b";
		Assert.Equal("b", View(src).CurrentItem);
	});

	[Fact]
	public void Clearing_the_selection_moves_the_view_before_first() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		lb.SelectedItem = null;
		Assert.True(View(src).IsCurrentBeforeFirst);
	});

	[Fact]
	public void Two_synchronized_selectors_over_one_source_stay_aligned() => Run(() =>
	{
		var src = Names();
		var master = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		var detail = new ComboBox { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		master.SelectedItem = "c";
		Assert.Equal("c", detail.SelectedItem);
		detail.SelectedItem = "b";
		Assert.Equal("b", master.SelectedItem);
	});

	[Fact]
	public void Null_does_not_synchronize_an_ordinary_collection() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { ItemsSource = src };
		Assert.Null(lb.SelectedItem);
		lb.SelectedItem = "b";
		Assert.Equal("a", View(src).CurrentItem);
	});

	[Fact]
	public void False_never_synchronizes() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { IsSynchronizedWithCurrentItem = false, ItemsSource = src };
		View(src).MoveCurrentTo("c");
		Assert.Null(lb.SelectedItem);
	});

	[Fact]
	public void Null_synchronizes_when_ItemsSource_is_itself_a_collection_view() => Run(() =>
	{
		var view = new ListCollectionView(Names());
		var lb = new ListBox { ItemsSource = view };
		Assert.Equal("a", lb.SelectedItem);
		view.MoveCurrentToLast();
		Assert.Equal("c", lb.SelectedItem);
	});

	[Fact]
	public void Turning_synchronization_on_adopts_the_view_current_item() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { ItemsSource = src };
		View(src).MoveCurrentTo("b");
		lb.IsSynchronizedWithCurrentItem = true;
		Assert.Equal("b", lb.SelectedItem);
	});

	[Fact]
	public void TabControl_is_synchronized() => Run(() =>
	{
		var src = Names();
		var tc = new TabControl { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		View(src).MoveCurrentTo("b");
		Assert.Equal("b", tc.SelectedItem);
		tc.SelectedItem = "c";
		Assert.Equal("c", View(src).CurrentItem);
	});

	[Fact]
	public void A_selection_the_view_filters_out_is_cleared() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		lb.SelectedItem = "b";
		View(src).Filter = o => (string)o != "b";
		Assert.NotEqual("b", lb.SelectedItem);
	});
}
