using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Gtk4.PresentationFramework.Tests.Lifecycle;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class ListBoxScrollIntoViewTests : WpfGtkTestBase
{
	[Fact]
	public void Null_item_throws() => Run(() =>
	{
		Assert.Throws<ArgumentNullException>(() => new ListBox().ScrollIntoView(null!));
	});

	[Fact]
	public void An_item_that_is_not_in_Items_is_ignored() => Run(() =>
	{
		var list = new ListBox();
		list.Items.Add("a");
		list.ScrollIntoView("missing");
		list.ScrollIntoView("a");
	});

	[Fact]
	public void Scrolls_the_row_into_the_viewport_after_load() => Run(() =>
	{
		using var gtk = new GtkDispatcherScope();
		var list = new ListBox();
		var items = Enumerable.Range(0, 200).Select(i => "Item " + i).ToList();
		foreach (var s in items) list.Items.Add(s);
		var viewer = new ScrollViewer { Content = list };
		var win = new Window { Width = 300, Height = 200, Content = viewer };
		var sw = (Gtk.ScrolledWindow)viewer.Widget;
		list.ScrollIntoView(items[150]); // deferred: not loaded yet
		win.Show();
		try
		{
			Assert.True(GtkPump.Until(() => list.IsLoaded && sw.Vadjustment.Upper > sw.Vadjustment.PageSize + 50));
			var adjustment = sw.Vadjustment;
			GtkPump.Until(() => adjustment.Value > 0);
			Assert.True(adjustment.Value > 0, "the deferred scroll moved the viewport");

			list.ScrollIntoView(items[0]);
			Assert.Equal(0, adjustment.Value, 3);

			list.ScrollIntoView(items[199]);
			Assert.True(adjustment.Value > 0);
			var row = list.ContainerAt(199)!.Widget;
			var content = ((Gtk.Viewport)sw.Child!).Child!;
			Assert.True(row.ComputeBounds(content, out var bounds));
			Assert.True(bounds.Y + bounds.Height <= adjustment.Value + adjustment.PageSize + 0.5);
			Assert.True(bounds.Y >= adjustment.Value - 0.5);
		}
		finally { win.Close(); }
	});
}
