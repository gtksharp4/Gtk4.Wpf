using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>RemoveAt and the indexer setter address the view position even for equal duplicates.</summary>
public class ItemCollectionViewPositionTests : WpfGtkTestBase
{
	sealed class Tag
	{
		public string Name { get; set; } = "";
		public int Rank { get; set; }
		public override bool Equals(object? obj) => obj is Tag t && t.Name == Name;
		public override int GetHashCode() => Name.GetHashCode();
	}

	// Inline order: first(rank 2), other(rank 1), second(rank 0), the two "dup" items Equals-equal.
	static (ListBox list, Tag first, Tag second) Build(bool sort)
	{
		var list = new ListBox();
		var first = new Tag { Name = "dup", Rank = 2 };
		var second = new Tag { Name = "dup", Rank = 0 };
		list.Items.Add(first);
		list.Items.Add(new Tag { Name = "other", Rank = 1 });
		list.Items.Add(second);
		if (sort) list.Items.SortDescriptions.Add(new SortDescription("Rank", ListSortDirection.Ascending));
		return (list, first, second);
	}

	static Tag[] Inline(ListBox list)
	{
		list.Items.SortDescriptions.Clear();
		list.Items.Filter = null;
		return list.Items.Cast<Tag>().ToArray();
	}

	[Fact]
	public void RemoveAt_under_a_sort_removes_the_second_occurrence_that_sorts_first() => Run(() =>
	{
		var (list, first, second) = Build(sort: true);
		Assert.Same(second, list.Items[0]);

		list.Items.RemoveAt(0);

		Assert.Equal(new[] { first.Name, "other" }, Inline(list).Select(t => t.Name).ToArray());
		Assert.Same(first, list.Items[0]);
	});

	[Fact]
	public void Indexer_set_under_a_sort_replaces_the_right_slot() => Run(() =>
	{
		var (list, first, second) = Build(sort: true);
		var replacement = new Tag { Name = "new", Rank = 9 };

		list.Items[0] = replacement;

		var inline = Inline(list);
		Assert.Same(first, inline[0]);
		Assert.Same(replacement, inline[2]);
	});

	[Fact]
	public void Reference_equal_duplicates_resolve_by_position() => Run(() =>
	{
		var list = new ListBox();
		var shared = new Tag { Name = "s", Rank = 1 };
		var low = new Tag { Name = "low", Rank = 0 };
		list.Items.Add(shared);
		list.Items.Add(low);
		list.Items.Add(shared);
		list.Items.SortDescriptions.Add(new SortDescription("Rank", ListSortDirection.Descending));

		list.Items.RemoveAt(1);

		Assert.Equal(new object[] { shared, low }, Inline(list));
	});

	[Fact]
	public void Filter_hiding_the_first_duplicate_maps_to_the_visible_one() => Run(() =>
	{
		var (list, first, second) = Build(sort: false);
		list.Items.Filter = o => ReferenceEquals(o, second) || ((Tag)o).Name != "dup";
		Assert.Same(second, list.Items[1]);

		list.Items.RemoveAt(1);

		var inline = Inline(list);
		Assert.Equal(2, inline.Length);
		Assert.Same(first, inline[0]);
		Assert.DoesNotContain(inline, t => ReferenceEquals(t, second));
	});

	[Fact]
	public void Filter_hiding_the_first_duplicate_replaces_the_visible_slot() => Run(() =>
	{
		var (list, first, second) = Build(sort: false);
		list.Items.Filter = o => ReferenceEquals(o, second) || ((Tag)o).Name != "dup";
		var replacement = new Tag { Name = "r" };

		list.Items[1] = replacement;

		var inline = Inline(list);
		Assert.Same(first, inline[0]);
		Assert.Same(replacement, inline[2]);
	});

	[Fact]
	public void Insert_under_a_sort_goes_before_the_item_at_the_view_position() => Run(() =>
	{
		var (list, first, second) = Build(sort: true);
		var added = new Tag { Name = "ins", Rank = -1 };

		list.Items.Insert(0, added);

		Assert.Same(added, Inline(list)[2]);
	});

	[Fact]
	public void CollectionChanged_never_reports_an_inline_index_for_a_sorted_removal() => Run(() =>
	{
		var (list, first, _) = Build(sort: true);
		var events = new List<NotifyCollectionChangedEventArgs>();
		list.Items.CollectionChanged += (_, e) => events.Add(e);

		list.Items.RemoveAt(0);

		Assert.NotEmpty(events);
		Assert.Equal(new[] { "dup", "other" }, Inline(list).Select(t => t.Name).ToArray());
		Assert.Same(first, Inline(list)[0]);
		Assert.All(events, e =>
			Assert.True(e.Action == NotifyCollectionChangedAction.Reset || e.OldStartingIndex == 0));
	});
}
