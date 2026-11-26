using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>Selector.IsSynchronizedWithCurrentItem against the default collection view.</summary>
public class SelectorSynchronizationTests : WpfGtkTestBase
{
	static System.ComponentModel.ICollectionView View(object source) => CollectionViewSource.GetDefaultView(source)!;

	static ObservableCollection<string> Names() => new ObservableCollection<string> { "a", "b", "c" };

	[Fact]
	public void A_string_ItemsSource_has_a_view_over_its_characters_and_does_not_throw() => Run(() =>
	{
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = "abc" };
		Assert.Equal(3, lb.Items.Count);
		Assert.Equal('a', lb.SelectedItem);
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
	public void A_selection_the_view_filters_out_moves_to_the_views_current_item() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		lb.SelectedItem = "b";
		View(src).Filter = o => (string)o != "b";
		Assert.NotNull(View(src).CurrentItem);
		Assert.Equal(View(src).CurrentItem, lb.SelectedItem);
		Assert.NotEqual("b", lb.SelectedItem);
	});

	[Fact]
	public void Removing_the_selected_item_moves_to_the_views_current_item() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = src };
		lb.SelectedItem = "b";
		src.Remove("b");
		Assert.NotNull(View(src).CurrentItem);
		Assert.Equal(View(src).CurrentItem, lb.SelectedItem);
	});

	[Fact]
	public void Switching_ItemsSource_leaves_the_old_view_current_item_alone() => Run(() =>
	{
		var list1 = Names();
		var list2 = new ObservableCollection<string> { "x", "y" };
		var lb = new ListBox { IsSynchronizedWithCurrentItem = true, ItemsSource = list1 };
		lb.SelectedItem = "b";
		Assert.Equal("b", View(list1).CurrentItem);
		lb.ItemsSource = list2;
		Assert.Equal("b", View(list1).CurrentItem);
		Assert.Equal("x", lb.SelectedItem);
	});

	[Fact]
	public void Selector_has_no_abstract_non_public_members()
	{
		const System.Reflection.BindingFlags all = System.Reflection.BindingFlags.Instance
			| System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic
			| System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly;
		Assert.DoesNotContain(typeof(Selector).GetMethods(all), m => m.IsAbstract && !m.IsPublic);
		Assert.DoesNotContain(typeof(Selector).GetProperties(all),
			p => (p.GetMethod?.IsAbstract ?? false) && !p.GetMethod!.IsPublic);
	}

	[Fact]
	public void An_unset_flag_does_not_sync_the_default_view_of_a_plain_list() => Run(() =>
	{
		var src = Names();
		var lb = new ListBox { ItemsSource = src };
		lb.SelectedItem = "c";
		Assert.Equal("a", View(src).CurrentItem);
	});

	[Fact]
	public void An_unset_flag_syncs_an_explicit_collection_view() => Run(() =>
	{
		var view = new ListCollectionView(Names());
		var lb = new ListBox { ItemsSource = view };
		lb.SelectedItem = "c";
		Assert.Equal("c", view.CurrentItem);
	});

	[Fact]
	public void An_unset_flag_does_not_sync_a_multiple_selection_ListBox() => Run(() =>
	{
		var view = new ListCollectionView(Names());
		var lb = new ListBox { SelectionMode = SelectionMode.Extended, ItemsSource = view };
		lb.SelectedItem = "c";
		Assert.Equal("a", view.CurrentItem);
	});
}
