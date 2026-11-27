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
	public void Enumeration_and_Count_flatten_containers_in_order()
	{
		var composite = Make(new ObservableCollection<string> { "a", "b" }, "single");
		composite.Add(new CollectionContainer { Collection = new[] { "c", "d" } });

		Assert.Equal(5, composite.Count);
		Assert.Equal(new object[] { "a", "b", "single", "c", "d" }, composite.Cast<object>());
		Assert.Equal("single", composite[2]);
		Assert.Equal(3, composite.IndexOf("c"));
		Assert.True(composite.Contains("d"));
	}

	[Fact]
	public void A_container_without_a_collection_contributes_nothing()
	{
		var composite = new CompositeCollection { new CollectionContainer(), "x" };

		Assert.Equal(new object[] { "x" }, composite.Cast<object>());
	}

	[Fact]
	public void The_indexer_cannot_be_assigned()
	{
		var composite = new CompositeCollection { "x" };

		Assert.Throws<NotSupportedException>(() => composite[0] = "y");
	}

	[Fact]
	public void Insert_and_RemoveAt_work_on_entry_boundaries_only()
	{
		var composite = Make(new ObservableCollection<string> { "a", "b" }, "s");

		composite.Insert(2, "mid");
		Assert.Equal(new object[] { "a", "b", "mid", "s" }, composite.Cast<object>());
		Assert.Throws<NotSupportedException>(() => composite.Insert(1, "bad"));
		composite.RemoveAt(2);
		Assert.Equal(new object[] { "a", "b", "s" }, composite.Cast<object>());
	}

	[Fact]
	public void A_change_in_a_container_collection_or_the_composite_raises_CollectionChanged()
	{
		var source = new ObservableCollection<string> { "a" };
		var composite = Make(source);
		int raised = 0;
		((INotifyCollectionChanged)composite).CollectionChanged += (_, _) => raised++;

		source.Add("b");
		composite.Add("c");
		composite.Remove("c");

		Assert.Equal(3, raised);
	}

	[Fact]
	public void A_container_collection_can_come_from_a_binding()
	{
		var source = new[] { "p", "q" };
		var container = new CollectionContainer();
		BindingOperations.SetBinding(container, CollectionContainer.CollectionProperty,
			new Binding { Source = source });
		var composite = new CompositeCollection { container };

		Assert.Equal(new object[] { "p", "q" }, composite.Cast<object>());
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
