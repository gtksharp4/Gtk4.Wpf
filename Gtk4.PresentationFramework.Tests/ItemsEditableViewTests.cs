using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ItemsEditableViewTests : WpfGtkTestBase
{
	public class Item
	{
		public string Name { get; set; } = "";
	}

	static ItemsControl Realised(ObservableCollection<Item> source)
	{
		var ic = new ItemsControl { ItemsSource = source };
		_ = ic.Widget;
		return ic;
	}

	[Fact]
	public void AddNew_and_CommitNew_add_to_the_source_and_the_containers() => Run(() =>
	{
		var source = new ObservableCollection<Item> { new Item { Name = "a" } };
		var ic = Realised(source);
		var view = (IEditableCollectionView)ic.Items;
		Assert.True(view.CanAddNew);
		var added = (Item)view.AddNew();
		Assert.True(view.IsAddingNew);
		Assert.Same(added, view.CurrentAddItem);
		view.CommitNew();
		Assert.Equal(2, source.Count);
		Assert.Same(added, source[1]);
		Assert.Equal(2, ic.Items.Count);
		Assert.NotNull(ic.ContainerAt(0));
		Assert.NotNull(ic.ContainerAt(1));
		Assert.Null(ic.ContainerAt(2));
	});

	[Fact]
	public void EditItem_CommitEdit_and_Remove_forward_to_the_view() => Run(() =>
	{
		var first = new Item { Name = "a" };
		var source = new ObservableCollection<Item> { first, new Item { Name = "b" } };
		var ic = Realised(source);
		var view = (IEditableCollectionView)ic.Items;
		view.EditItem(first);
		Assert.True(view.IsEditingItem);
		Assert.Same(first, view.CurrentEditItem);
		view.CommitEdit();
		Assert.False(view.IsEditingItem);
		Assert.True(view.CanRemove);
		view.Remove(first);
		Assert.Single(source);
		view.RemoveAt(0);
		Assert.Empty(source);
		Assert.Empty(ic.Items);
	});

	[Fact]
	public void Inline_items_are_not_editable() => Run(() =>
	{
		var ic = new ItemsControl();
		ic.Items.Add("x");
		var view = (IEditableCollectionView)ic.Items;
		Assert.False(view.CanAddNew);
		Assert.False(view.CanRemove);
		Assert.False(view.IsAddingNew);
		Assert.Throws<InvalidOperationException>(() => view.AddNew());
	});
}
