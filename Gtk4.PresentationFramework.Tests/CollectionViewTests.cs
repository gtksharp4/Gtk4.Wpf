using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for Round-9 CollectionView: ListCollectionView, CollectionViewSource,
/// and ItemsControl integration with filtered/sorted views.
/// Each test fails without the feature and passes with it.
/// </summary>
public class CollectionViewTests
{
	// ── Helpers ──────────────────────────────────────────────────────────────

	record Person(string Name, int Age);

	static ObservableCollection<Person> MakeList() => new ObservableCollection<Person>
	{
		new Person("Charlie", 30),
		new Person("Alice", 25),
		new Person("Bob", 25),
	};

	// ── Filter ───────────────────────────────────────────────────────────────

	[Fact]
	public void Filter_hides_non_matching_items()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		view.Filter = o => ((Person)o).Age == 25;
		view.Refresh();

		var items = new List<object>();
		foreach (var item in view) items.Add(item);

		Assert.Equal(2, items.Count);
		Assert.All(items, i => Assert.Equal(25, ((Person)i).Age));
	}

	[Fact]
	public void Filter_null_shows_all_items()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		view.Filter = null;
		view.Refresh();

		var items = new List<object>();
		foreach (var item in view) items.Add(item);

		Assert.Equal(3, items.Count);
	}

	// ── Sort ─────────────────────────────────────────────────────────────────

	[Fact]
	public void SortDescriptions_ascending_by_name()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		view.SortDescriptions.Add(new SortDescription(nameof(Person.Name), ListSortDirection.Ascending));
		view.Refresh();

		var names = new List<string>();
		foreach (Person item in view) names.Add(item.Name);

		Assert.Equal(new[] { "Alice", "Bob", "Charlie" }, names);
	}

	[Fact]
	public void SortDescriptions_descending_by_age()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		view.SortDescriptions.Add(new SortDescription(nameof(Person.Age), ListSortDirection.Descending));
		view.Refresh();

		var items = new List<Person>();
		foreach (Person item in view) items.Add(item);

		Assert.Equal(30, items[0].Age); // Charlie first
	}

	// ── Group ────────────────────────────────────────────────────────────────

	[Fact]
	public void PropertyGroupDescription_groups_by_age()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		view.GroupDescriptions.Add(new PropertyGroupDescription(nameof(Person.Age)));
		view.Refresh();

		Assert.NotNull(view.Groups);
		Assert.Equal(2, view.Groups!.Count); // age 30 and age 25

		var groups = new List<CollectionViewGroup>();
		foreach (var g in view.Groups) groups.Add((CollectionViewGroup)g);

		var age25Group = groups.Find(g => Equals(g.Name, 25))!;
		Assert.NotNull(age25Group);
		Assert.Equal(2, age25Group.ItemCount);
	}

	// ── Current navigation ───────────────────────────────────────────────────

	[Fact]
	public void MoveCurrentToNext_advances_CurrentItem()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		view.MoveCurrentToFirst();
		Assert.Equal(0, view.CurrentPosition);

		view.MoveCurrentToNext();
		Assert.Equal(1, view.CurrentPosition);
		Assert.NotNull(view.CurrentItem);
	}

	[Fact]
	public void MoveCurrentTo_selects_item()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		var target = src[1];

		bool ok = view.MoveCurrentTo(target);

		Assert.True(ok);
		Assert.Same(target, view.CurrentItem);
	}

	[Fact]
	public void CurrentChanged_fires_on_navigation()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		int count = 0;
		view.CurrentChanged += (_, _) => count++;

		// WPF starts a non-empty view on its first item, so the first move is a no-op.
		view.MoveCurrentToFirst();
		view.MoveCurrentToNext();

		Assert.Equal(1, count);
	}

}
