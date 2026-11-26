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
	[Fact]
	public void Edit_cycle_calls_IEditableObject_in_order()
	{
		var row = new Row();
		var editable = Edit(new List<Row> { row });
		editable.EditItem(row);
		Assert.True(editable.IsEditingItem);
		Assert.Same(row, editable.CurrentEditItem);
		Assert.True(editable.CanCancelEdit);
		editable.CommitEdit();
		Assert.False(editable.IsEditingItem);
		editable.EditItem(row);
		editable.CancelEdit();
		Assert.Equal(new[] { "begin", "end", "begin", "cancel" }, row.Calls);
		Assert.Null(editable.CurrentEditItem);
	}

	[Fact]
	public void CancelEdit_throws_for_an_item_that_is_not_IEditableObject()
	{
		var row = new PlainRow();
		var editable = Edit(new List<PlainRow> { row });
		editable.EditItem(row);
		Assert.False(editable.CanCancelEdit);
		Assert.Throws<InvalidOperationException>(() => editable.CancelEdit());
		editable.CommitEdit();
	}

	[Fact]
	public void EditItem_commits_an_earlier_edit()
	{
		var a = new Row();
		var b = new Row();
		var editable = Edit(new List<Row> { a, b });
		editable.EditItem(a);
		editable.EditItem(b);
		Assert.Equal(new[] { "begin", "end" }, a.Calls);
		Assert.Same(b, editable.CurrentEditItem);
	}

	[Fact]
	public void Add_and_edit_are_mutually_exclusive_where_wpf_throws()
	{
		var row = new Row();
		var editable = Edit(new List<Row> { row });
		editable.EditItem(row);
		Assert.False(editable.CanAddNew);
		Assert.False(editable.CanRemove);
		Assert.Throws<InvalidOperationException>(() => editable.CommitNew());
		Assert.Throws<InvalidOperationException>(() => editable.CancelNew());
		Assert.Throws<InvalidOperationException>(() => editable.RemoveAt(0));
		editable.CommitEdit();
		var added = editable.AddNew();
		Assert.Throws<InvalidOperationException>(() => editable.CommitEdit());
		Assert.Throws<InvalidOperationException>(() => editable.CancelEdit());
		Assert.False(editable.CanRemove);
		editable.EditItem(added);
		Assert.True(editable.IsAddingNew);
		Assert.False(editable.IsEditingItem);
	}

	[Fact]
	public void Remove_and_RemoveAt_delete_from_the_source()
	{
		var a = new PlainRow();
		var b = new PlainRow();
		var c = new PlainRow();
		var list = new List<PlainRow> { a, b, c };
		var view = new ListCollectionView(list);
		var editable = (IEditableCollectionView)view;
		Assert.True(editable.CanRemove);
		editable.Remove(b);
		Assert.Equal(new[] { a, c }, list);
		Assert.Equal(2, view.Count);
		editable.RemoveAt(0);
		Assert.Equal(new[] { c }, list);
		editable.Remove(new PlainRow());
		Assert.Single(list);
		Assert.Throws<ArgumentOutOfRangeException>(() => editable.RemoveAt(5));
		Assert.False(Edit(new PlainRow[1]).CanRemove);
	}

	[Fact]
	public void RemoveAt_follows_the_view_order_when_filtered()
	{
		var list = new List<PlainRow> { new PlainRow { Name = "x" }, new PlainRow { Name = "y" } };
		var view = new ListCollectionView(list) { Filter = o => ((PlainRow)o).Name == "y" };
		((IEditableCollectionView)view).RemoveAt(0);
		Assert.Equal("x", list[0].Name);
		Assert.Single(list);
	}

	[Fact]
	public void NewItemPlaceholderPosition_defaults_to_none_and_is_settable_when_idle()
	{
		var editable = Edit(new List<Row>());
		Assert.Equal(NewItemPlaceholderPosition.None, editable.NewItemPlaceholderPosition);
		editable.NewItemPlaceholderPosition = NewItemPlaceholderPosition.AtEnd;
		Assert.Equal(NewItemPlaceholderPosition.AtEnd, editable.NewItemPlaceholderPosition);
		Assert.Throws<InvalidEnumArgumentException>(
			() => editable.NewItemPlaceholderPosition = (NewItemPlaceholderPosition)9);
		editable.AddNew();
		Assert.Throws<InvalidOperationException>(
			() => editable.NewItemPlaceholderPosition = NewItemPlaceholderPosition.None);
	}
}
