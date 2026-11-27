using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

public class CompositeCollectionTests : WpfGtkTestBase
{
	static CompositeCollection Make(ObservableCollection<string> a, params object[] more)
	{
		var composite = new CompositeCollection { new CollectionContainer { Collection = a } };
		foreach (var m in more) composite.Add(m);
		return composite;
	}

	[Fact]
	public void The_list_members_work_on_entries_and_the_flattened_items_are_enumerated_by_a_view()
	{
		var first = new ObservableCollection<string> { "a", "b" };
		var composite = Make(first, "single");
		composite.Add(new CollectionContainer { Collection = new[] { "c", "d" } });

		Assert.Equal(3, composite.Count);
		Assert.IsType<CollectionContainer>(composite[0]);
		Assert.Equal("single", composite[1]);
		Assert.Equal(1, composite.IndexOf("single"));
		Assert.True(composite.Contains("single"));
		Assert.False(composite.Contains("d"));
		Assert.Equal(new object?[] { "a", "b", "single", "c", "d" },
			CollectionViewSource.GetDefaultView(composite)!.Cast<object>());
	}

	[Fact]
	public void A_container_without_a_collection_contributes_nothing()
	{
		var composite = new CompositeCollection { new CollectionContainer(), "x" };

		Assert.Equal(new object[] { "x" }, CollectionViewSource.GetDefaultView(composite)!.Cast<object>());
	}

	[Fact]
	public void The_indexer_replaces_an_entry_and_raises_Replace()
	{
		var composite = new CompositeCollection { "x" };
		NotifyCollectionChangedEventArgs? args = null;
		((INotifyCollectionChanged)composite).CollectionChanged += (_, e) => args = e;

		composite[0] = "y";

		Assert.Equal("y", composite[0]);
		Assert.Equal(NotifyCollectionChangedAction.Replace, args!.Action);
		Assert.Equal(0, args.NewStartingIndex);
	}

	[Fact]
	public void Insert_and_RemoveAt_take_entry_indexes()
	{
		var composite = Make(new ObservableCollection<string> { "a", "b" }, "s");

		composite.Insert(1, "mid");
		Assert.IsType<CollectionContainer>(composite[0]);
		Assert.Equal(new object?[] { composite[0], "mid", "s" }, composite.Cast<object>());
		Assert.Equal(new object[] { "a", "b", "mid", "s" },
			CollectionViewSource.GetDefaultView(composite)!.Cast<object>());
		composite.RemoveAt(1);
		Assert.Equal(2, composite.Count);
	}

	[Fact]
	public void Entry_changes_raise_Add_and_Remove_at_entry_indexes_and_container_changes_stay_with_the_view()
	{
		var source = new ObservableCollection<string> { "a" };
		var composite = Make(source);
		var seen = new System.Collections.Generic.List<NotifyCollectionChangedEventArgs>();
		((INotifyCollectionChanged)composite).CollectionChanged += (_, e) => seen.Add(e);

		composite.Add("c");
		composite.Remove("c");
		source.Add("b");

		Assert.Equal(2, seen.Count);
		Assert.Equal(NotifyCollectionChangedAction.Add, seen[0].Action);
		Assert.Equal(1, seen[0].NewStartingIndex);
		Assert.Equal(NotifyCollectionChangedAction.Remove, seen[1].Action);
		Assert.Equal(1, seen[1].OldStartingIndex);
	}

	[Fact]
	public void A_container_collection_can_come_from_a_binding()
	{
		var source = new[] { "p", "q" };
		var container = new CollectionContainer();
		BindingOperations.SetBinding(container, CollectionContainer.CollectionProperty,
			new Binding { Source = source });
		var composite = new CompositeCollection { container };

		Assert.Equal(new object[] { "p", "q" }, CollectionViewSource.GetDefaultView(composite)!.Cast<object>());
	}

	[Fact]
	public void A_view_over_a_composite_refreshes_when_a_container_changes()
	{
		var source = new ObservableCollection<string> { "a" };
		var view = CollectionViewSource.GetDefaultView(Make(source, "s"))!;

		source.Add("b");

		Assert.Equal(new object[] { "a", "b", "s" }, view.Cast<object>());
	}

	[Fact]
	public void A_ListBox_shows_the_flattened_items_and_a_ListBoxItem_is_its_own_container() => Run(() =>
	{
		var source = new ObservableCollection<string> { "a", "b" };
		var own = new ListBoxItem { Content = "own" };
		var lb = new ListBox { ItemsSource = Make(source, own) };
		_ = lb.Widget;

		Assert.Equal(3, lb.Items.Count);
		Assert.Same(own, lb.ContainerAt(2));
		Assert.Equal("a", ((ListBoxItem)lb.ContainerAt(0)!).Content);

		source.Add("c");

		Assert.Equal(4, lb.Items.Count);
		Assert.Same(own, lb.ContainerAt(3));
		lb.SelectedIndex = 3;
		Assert.Same(own, lb.SelectedItem);
	});
}
