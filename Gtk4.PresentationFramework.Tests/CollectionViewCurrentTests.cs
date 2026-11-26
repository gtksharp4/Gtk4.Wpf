using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>WPF current-item semantics of CollectionView / ListCollectionView.</summary>
public class CollectionViewCurrentTests
{
	static ObservableCollection<string> Names() => new ObservableCollection<string> { "a", "b", "c" };

	static IEnumerable<string> Sequence()
	{
		yield return "x";
		yield return "y";
	}

	[Fact]
	public void Default_view_of_IList_is_ListCollectionView_and_of_a_sequence_a_CollectionView()
	{
		Assert.IsType<ListCollectionView>(CollectionViewSource.GetDefaultView(Names()));
		var plain = CollectionViewSource.GetDefaultView(Sequence());
		Assert.IsType<CollectionView>(plain);
	}

	[Fact]
	public void GetDefaultView_takes_object_and_returns_null_for_a_non_collection_or_null()
	{
		object src = Names();
		Assert.NotNull(CollectionViewSource.GetDefaultView(src));
		Assert.Null(CollectionViewSource.GetDefaultView(42));
		Assert.Null(CollectionViewSource.GetDefaultView(null));
	}

	[Fact]
	public void Current_starts_at_the_first_item_or_before_first_when_empty()
	{
		var view = new ListCollectionView(Names());
		Assert.Equal(0, view.CurrentPosition);
		Assert.Equal("a", view.CurrentItem);

		var empty = new ListCollectionView(new List<string>());
		Assert.Equal(-1, empty.CurrentPosition);
		Assert.True(empty.IsCurrentBeforeFirst);
		Assert.True(empty.IsEmpty);
	}

	[Fact]
	public void MoveCurrentToNext_past_the_end_is_after_last_and_returns_false()
	{
		var view = new ListCollectionView(Names());
		view.MoveCurrentToLast();
		Assert.False(view.MoveCurrentToNext());
		Assert.True(view.IsCurrentAfterLast);
		Assert.Null(view.CurrentItem);
		Assert.Equal(3, view.CurrentPosition);
	}

	[Fact]
	public void MoveCurrentToPrevious_before_first_returns_false()
	{
		var view = new ListCollectionView(Names());
		Assert.False(view.MoveCurrentToPrevious());
		Assert.True(view.IsCurrentBeforeFirst);
	}

	[Fact]
	public void MoveCurrentTo_unknown_item_or_null_moves_before_first()
	{
		var view = new ListCollectionView(Names());
		Assert.False(view.MoveCurrentTo("zzz"));
		Assert.Equal(-1, view.CurrentPosition);
		Assert.True(view.MoveCurrentTo("b"));
		Assert.False(view.MoveCurrentTo(null));
		Assert.Equal(-1, view.CurrentPosition);
	}

	[Fact]
	public void MoveCurrentToPosition_out_of_range_throws()
	{
		var view = new ListCollectionView(Names());
		Assert.Throws<ArgumentOutOfRangeException>(() => view.MoveCurrentToPosition(-2));
		Assert.Throws<ArgumentOutOfRangeException>(() => view.MoveCurrentToPosition(4));
	}

	[Fact]
	public void CurrentChanging_can_cancel_the_move()
	{
		var view = new ListCollectionView(Names());
		var changed = 0;
		view.CurrentChanging += (_, e) => e.Cancel = true;
		view.CurrentChanged += (_, _) => changed++;
		view.MoveCurrentToLast();
		Assert.Equal(0, view.CurrentPosition);
		Assert.Equal(0, changed);
	}

	[Fact]
	public void CurrentChangingEventArgs_not_cancelable_throws_on_cancel()
	{
		var args = new CurrentChangingEventArgs(false);
		Assert.False(args.IsCancelable);
		Assert.Throws<InvalidOperationException>(() => args.Cancel = true);
	}

	[Fact]
	public void Refresh_keeps_the_current_item_when_it_survives()
	{
		var src = Names();
		var view = new ListCollectionView(src);
		view.MoveCurrentTo("c");
		var changed = 0;
		view.CurrentChanged += (_, _) => changed++;
		src.Insert(0, "z");
		Assert.Equal("c", view.CurrentItem);
		Assert.Equal(3, view.CurrentPosition);
		Assert.Equal(0, changed);
	}

	[Fact]
	public void Removing_the_current_item_moves_current_to_the_neighbour_and_raises_CurrentChanged()
	{
		var src = Names();
		var view = new ListCollectionView(src);
		view.MoveCurrentTo("b");
		var changed = 0;
		view.CurrentChanged += (_, _) => changed++;
		src.Remove("b");
		Assert.Equal("c", view.CurrentItem);
		Assert.Equal(1, changed);
	}

	[Fact]
	public void Setting_Filter_and_SortDescriptions_refresh_without_an_explicit_Refresh()
	{
		var view = new ListCollectionView(Names());
		view.Filter = o => (string)o != "a";
		Assert.Equal(new[] { "b", "c" }, view.Cast<string>());
		view.SortDescriptions.Add(new SortDescription(null, ListSortDirection.Descending));
		Assert.Equal(new[] { "c", "b" }, view.Cast<string>());
	}

	[Fact]
	public void DeferRefresh_postpones_the_refresh_until_disposed()
	{
		var view = new ListCollectionView(Names());
		using (view.DeferRefresh())
		{
			view.Filter = o => (string)o == "a";
			Assert.Equal(3, view.Cast<object>().Count());
		}
		Assert.Single(view.Cast<object>());
	}

	[Fact]
	public void CollectionView_exposes_Count_Contains_IndexOf_GetItemAt_and_Can_flags()
	{
		var view = new ListCollectionView(Names());
		Assert.Equal(3, view.Count);
		Assert.True(view.Contains("b"));
		Assert.Equal(1, view.IndexOf("b"));
		Assert.Equal("c", view.GetItemAt(2));
		Assert.True(view.CanFilter && view.CanSort && view.CanGroup);
		Assert.Throws<ArgumentOutOfRangeException>(() => view.GetItemAt(3));
	}

	[Fact]
	public void CollectionViewSource_SortDescriptions_is_a_SortDescriptionCollection_applied_to_the_view()
	{
		var cvs = new CollectionViewSource { Source = Names() };
		cvs.SortDescriptions.Add(new SortDescription(null, ListSortDirection.Descending));
		Assert.Equal(new[] { "c", "b", "a" }, cvs.View!.Cast<string>());
	}

	[Fact]
	public void CurrentItem_change_raises_PropertyChanged()
	{
		var view = new ListCollectionView(Names());
		var names = new List<string>();
		((INotifyPropertyChanged)view).PropertyChanged += (_, e) => names.Add(e.PropertyName!);
		view.MoveCurrentToNext();
		Assert.Contains("CurrentItem", names);
		Assert.Contains("CurrentPosition", names);
	}
}
