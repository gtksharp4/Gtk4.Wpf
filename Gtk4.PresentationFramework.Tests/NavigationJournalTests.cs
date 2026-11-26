using System;
using System.Windows.Navigation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="NavigationJournal"/>. No GTK required.
/// </summary>
public class NavigationJournalTests
{
	[Fact]
	public void Empty_journal_cannot_go_back_or_forward()
	{
		var j = new NavigationJournal();
		Assert.False(j.CanGoBack);
		Assert.False(j.CanGoForward);
	}

	[Fact]
	public void Push_first_entry_sets_current()
	{
		var j = new NavigationJournal();
		j.Push("page1");
		Assert.Equal("page1", j.Current);
		Assert.False(j.CanGoBack);
		Assert.False(j.CanGoForward);
	}

	[Fact]
	public void Push_second_entry_enables_GoBack()
	{
		var j = new NavigationJournal();
		j.Push("page1");
		j.Push("page2");
		Assert.True(j.CanGoBack);
		Assert.False(j.CanGoForward);
	}

	[Fact]
	public void GoBack_moves_cursor_back()
	{
		var j = new NavigationJournal();
		j.Push("page1");
		j.Push("page2");
		var prev = j.GoBack();
		Assert.Equal("page1", prev);
		Assert.Equal("page1", j.Current);
	}

	[Fact]
	public void GoBack_then_GoForward_restores_position()
	{
		var j = new NavigationJournal();
		j.Push("page1");
		j.Push("page2");
		j.GoBack();
		Assert.True(j.CanGoForward);
		var fwd = j.GoForward();
		Assert.Equal("page2", fwd);
		Assert.False(j.CanGoForward);
	}

	[Fact]
	public void Push_after_GoBack_trims_forward_entries()
	{
		var j = new NavigationJournal();
		j.Push("page1");
		j.Push("page2");
		j.GoBack(); // cursor at page1
		j.Push("page3"); // forward stack (page2) is discarded
		Assert.False(j.CanGoForward);
		Assert.Equal("page3", j.Current);
	}

	[Fact]
	public void GoBack_on_empty_journal_throws()
	{
		var j = new NavigationJournal();
		Assert.Throws<InvalidOperationException>(() => j.GoBack());
	}

	[Fact]
	public void GoForward_when_no_forward_entries_throws()
	{
		var j = new NavigationJournal();
		j.Push("page1");
		Assert.Throws<InvalidOperationException>(() => j.GoForward());
	}

	[Fact]
	public void Clear_resets_journal()
	{
		var j = new NavigationJournal();
		j.Push("page1");
		j.Push("page2");
		j.Clear();
		Assert.False(j.CanGoBack);
		Assert.False(j.CanGoForward);
		Assert.Null(j.Current);
	}

	[Fact]
	public void CanGoBack_and_CanGoForward_correct_across_three_entries()
	{
		var j = new NavigationJournal();
		j.Push("a");
		j.Push("b");
		j.Push("c");
		Assert.True(j.CanGoBack);
		Assert.False(j.CanGoForward);
		j.GoBack();
		Assert.True(j.CanGoBack);
		Assert.True(j.CanGoForward);
		j.GoBack();
		Assert.False(j.CanGoBack);
		Assert.True(j.CanGoForward);
	}
}