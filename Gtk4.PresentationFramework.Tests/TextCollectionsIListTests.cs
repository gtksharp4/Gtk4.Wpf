using System;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>The non-generic IList view of the text collections, plus List.MarkerOffset and TextBox.IsReadOnlyCaretVisible
/// (the members EditingExaminerDemo reads through IList and sets from XAML).</summary>
public class TextCollectionsIListTests : WpfGtkTestBase
{
	[Fact]
	public void BlockCollection_is_an_IList_that_adds_removes_and_enumerates()
	{
		var section = new Section();
		IList list = section.Blocks;
		var paragraph = new Paragraph();

		Assert.Equal(0, list.Add(paragraph));
		Assert.Equal(1, list.Count);
		Assert.True(list.Contains(paragraph));
		Assert.Equal(0, list.IndexOf(paragraph));
		Assert.Same(paragraph, list[0]);
		Assert.Same(section, paragraph.Parent);

		list.Remove(paragraph);
		Assert.Empty(section.Blocks);
	}

	[Fact]
	public void InlineCollection_and_ListItemCollection_are_ILists()
	{
		IList inlines = new Paragraph().Inlines;
		IList items = new List().ListItems;

		inlines.Add(new Run("a"));
		items.Add(new ListItem());

		Assert.Single(inlines);
		Assert.Single(items);
		inlines.RemoveAt(0);
		items.RemoveAt(0);
		Assert.Empty(inlines);
		Assert.Empty(items);
	}

	[Fact]
	public void IList_rejects_a_wrong_element_type_and_unsupported_edits()
	{
		IList list = new Section().Blocks;

		Assert.Throws<ArgumentException>(() => list.Add("text"));
		Assert.Throws<NotSupportedException>(() => list.Insert(0, new Paragraph()));
		Assert.False(list.Contains("text"));
		Assert.Equal(-1, list.IndexOf("text"));
	}

	[Fact]
	public void List_MarkerOffset_defaults_to_NaN_and_is_validated() => Run(() =>
	{
		var list = new List();

		Assert.True(double.IsNaN(list.MarkerOffset));
		list.MarkerOffset = 12;
		Assert.Equal(12, list.MarkerOffset);
		Assert.Throws<ArgumentException>(() => list.MarkerOffset = 2000000);
	});

	[Fact]
	public void TextBox_IsReadOnlyCaretVisible_defaults_to_false_and_round_trips() => Run(() =>
	{
		var box = new TextBox();

		Assert.False(box.IsReadOnlyCaretVisible);
		box.IsReadOnlyCaretVisible = true;
		Assert.True(box.IsReadOnlyCaretVisible);
	});
}
