using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public partial class EditableCollectionViewTests
{
	public class Row : IEditableObject
	{
		public string Name { get; set; } = "";
		public List<string> Calls { get; } = new List<string>();

		public void BeginEdit() => Calls.Add("begin");

		public void EndEdit() => Calls.Add("end");

		public void CancelEdit() => Calls.Add("cancel");
	}

	public class PlainRow
	{
		public string Name { get; set; } = "";
	}

	public class NoDefaultCtor
	{
		public NoDefaultCtor(int x)
		{
		}
	}

	static IEditableCollectionView Edit(IEnumerable source) => (IEditableCollectionView)new ListCollectionView((IList)source);

	[Fact]
	public void ListCollectionView_implements_the_interface()
	{
		Assert.IsAssignableFrom<IEditableCollectionView>(new ListCollectionView(new List<Row>()));
	}

	[Fact]
	public void CanAddNew_needs_a_growable_list_and_a_default_constructor()
	{
		Assert.True(Edit(new ObservableCollection<Row>()).CanAddNew);
		Assert.True(Edit(new List<PlainRow>()).CanAddNew);
		Assert.False(Edit(new PlainRow[1]).CanAddNew);
		Assert.False(Edit(new List<NoDefaultCtor>()).CanAddNew);
		Assert.False(Edit(new ArrayList()).CanAddNew);
		var untyped = new ArrayList { new PlainRow() };
		Assert.True(Edit(untyped).CanAddNew);
	}

	[Fact]
	public void AddNew_appends_an_item_and_makes_it_current()
	{
		var list = new List<Row> { new Row() };
		var view = new ListCollectionView(list);
		var editable = (IEditableCollectionView)view;
		var item = (Row)editable.AddNew();
		Assert.Equal(2, list.Count);
		Assert.Same(item, list[1]);
		Assert.True(editable.IsAddingNew);
		Assert.Same(item, editable.CurrentAddItem);
		Assert.Same(item, view.CurrentItem);
		Assert.Equal(new[] { "begin" }, item.Calls);
		Assert.Equal(2, view.Count);
	}

	[Fact]
	public void CommitNew_ends_the_edit_and_keeps_the_item()
	{
		var list = new List<Row>();
		var editable = Edit(list);
		var item = (Row)editable.AddNew();
		editable.CommitNew();
		Assert.False(editable.IsAddingNew);
		Assert.Null(editable.CurrentAddItem);
		Assert.Single(list);
		Assert.Equal(new[] { "begin", "end" }, item.Calls);
		editable.CommitNew();
	}

	[Fact]
	public void CancelNew_removes_the_item()
	{
		var list = new List<Row>();
		var view = new ListCollectionView(list);
		var editable = (IEditableCollectionView)view;
		var item = (Row)editable.AddNew();
		editable.CancelNew();
		Assert.Empty(list);
		Assert.Equal(0, view.Count);
		Assert.Equal(new[] { "begin", "cancel" }, item.Calls);
		Assert.False(editable.IsAddingNew);
		editable.CancelNew();
	}

	[Fact]
	public void AddNew_commits_the_previous_new_item()
	{
		var list = new List<Row>();
		var editable = Edit(list);
		var first = (Row)editable.AddNew();
		var second = (Row)editable.AddNew();
		Assert.Equal(new[] { "begin", "end" }, first.Calls);
		Assert.Equal(2, list.Count);
		Assert.Same(second, editable.CurrentAddItem);
	}

	[Fact]
	public void AddNew_when_it_cannot_add_throws()
	{
		Assert.Throws<InvalidOperationException>(() => Edit(new PlainRow[1]).AddNew());
	}
}
