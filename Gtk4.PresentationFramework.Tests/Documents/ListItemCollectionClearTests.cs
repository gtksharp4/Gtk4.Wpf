using System.Windows.Documents;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>ListItemCollection.Clear, ported from the held detached-List fix.</summary>
public class ListItemCollectionClearTests : WpfGtkTestBase
{
	[Fact]
	public void ListItemCollection_Clear_removes_every_item_and_releases_it() => Run(() =>
	{
		var list = new List();
		var first = new ListItem();
		var second = new ListItem();
		list.ListItems.Add(first);
		list.ListItems.Add(second);
		list.ListItems.Clear();
		Assert.Equal(0, list.ListItems.Count);
		Assert.Null(first.Parent);
		Assert.Null(second.Parent);
		list.ListItems.Add(first);
		Assert.Equal(1, list.ListItems.Count);
	});
}
