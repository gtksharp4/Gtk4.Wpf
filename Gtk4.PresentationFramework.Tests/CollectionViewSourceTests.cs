using System.Collections.ObjectModel;
using System.ComponentModel;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>CollectionViewSource.GetDefaultView and ItemsControl integration with collection views.</summary>
public class CollectionViewSourceTests
{
	record Person(string Name, int Age);

	static ObservableCollection<Person> MakeList() => new ObservableCollection<Person>
	{
		new Person("Charlie", 30),
		new Person("Alice", 25),
		new Person("Bob", 25),
	};

	// ── CollectionViewSource ─────────────────────────────────────────────────

	[Fact]
	public void GetDefaultView_returns_stable_cached_view()
	{
		var src = MakeList();
		var v1 = CollectionViewSource.GetDefaultView(src);
		var v2 = CollectionViewSource.GetDefaultView(src);
		Assert.Same(v1, v2);
	}

	[Fact]
	public void GetDefaultView_returns_ICollectionView_itself()
	{
		var src = MakeList();
		var view = new ListCollectionView(src);
		var same = CollectionViewSource.GetDefaultView(view);
		Assert.Same(view, same);
	}

	// ── ItemsControl integration ──────────────────────────────────────────────

	sealed class TestItemsControl : ItemsControl
	{
		protected override Gtk.Widget CreateWidget() => new Gtk.Box(Gtk.Orientation.Vertical, 0);
		protected override void AddItemWidget(Gtk.Widget w) { }
		protected override void RemoveItemWidget(Gtk.Widget w) { }
		protected override Gtk.Widget CreateItemsWidget() => new Gtk.Box(Gtk.Orientation.Vertical, 0);
	}

	[Fact]
	public void ItemsControl_Items_reflects_filtered_view_order()
	{
		var src = new ObservableCollection<Person>
		{
			new Person("Z", 10),
			new Person("A", 20),
			new Person("M", 10),
		};

		// Set up a view with filter and sort, then assign as ItemsSource.
		var view = new ListCollectionView(src);
		view.Filter = o => ((Person)o).Age == 10;
		view.SortDescriptions.Add(new SortDescription(nameof(Person.Name), ListSortDirection.Ascending));
		view.Refresh();

		var ctrl = new TestItemsControl();
		ctrl.ItemsSource = view; // already an ICollectionView

		// Items should be M then Z (filtered to age=10, sorted ascending by name).
		Assert.Equal(2, ctrl.Items.Count);
		Assert.Equal("M", ((Person)ctrl.Items[0]!).Name);
		Assert.Equal("Z", ((Person)ctrl.Items[1]!).Name);
	}

	[Fact]
	public void ItemsControl_uses_GetDefaultView_when_given_plain_collection()
	{
		var src = new ObservableCollection<Person>
		{
			new Person("B", 1),
			new Person("A", 2),
		};

		var ctrl = new TestItemsControl { ItemsSource = src };
		// Default view has no filter/sort — same count as source.
		Assert.Equal(2, ctrl.Items.Count);
	}

	[Fact]
	public void GetDefaultView_of_null_is_null()
	{
		Assert.Null(CollectionViewSource.GetDefaultView(null));
	}

	[Fact]
	public void GetDefaultView_of_a_string_is_a_view_over_its_characters()
	{
		var view = CollectionViewSource.GetDefaultView("ab");
		Assert.NotNull(view);
		Assert.Equal(new object[] { 'a', 'b' }, view!.Cast<object>().ToArray());
	}

	[Fact]
	public void ItemsControl_with_a_string_ItemsSource_does_not_throw()
	{
		var ctrl = new TestItemsControl { ItemsSource = "ab" };
		Assert.Equal(2, ctrl.Items.Count);
	}

	[Fact]
	public void Group_and_sort_types_live_in_their_WPF_namespaces()
	{
		Assert.Equal("System.ComponentModel", typeof(GroupDescription).Namespace);
		Assert.Equal("System.Windows.Data", typeof(PropertyGroupDescription).Namespace);
		Assert.Equal("System.Windows.Data", typeof(DataTransferEventArgs).Namespace);
	}

	[Fact]
	public void SortDescriptionCollection_Empty_is_read_only()
	{
		var sort = new SortDescription("Name", ListSortDirection.Ascending);
		Assert.Throws<NotSupportedException>(() => SortDescriptionCollection.Empty.Add(sort));
		Assert.Empty(SortDescriptionCollection.Empty);
	}
}
