using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>A CollectionViewSource used as Binding.Source stands for its View.</summary>
public class CollectionViewSourceBindingTests : WpfGtkTestBase
{
	static ListBox BoundTo(CollectionViewSource cvs, Window window)
	{
		window.Resources["cvs"] = cvs;
		var list = new ListBox();
		window.Content = list;
		list.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = cvs });
		return list;
	}

	[Fact]
	public void ItemsSource_binds_to_the_view_of_the_source() => Run(() =>
	{
		var cvs = new CollectionViewSource { Source = new[] { "a", "b", "c" } };
		var list = BoundTo(cvs, new Window());

		Assert.Equal(3, list.Items.Count);
		Assert.IsAssignableFrom<ICollectionView>(list.ItemsSource);
		Assert.Same(cvs.View, list.ItemsSource);
	});

	[Fact]
	public void The_displayed_order_follows_the_SortDescriptions_of_the_source() => Run(() =>
	{
		var cvs = new CollectionViewSource { Source = new[] { "b", "c", "a" } };
		cvs.SortDescriptions.Add(new SortDescription("", ListSortDirection.Ascending));
		var list = BoundTo(cvs, new Window());

		Assert.Equal(new[] { "a", "b", "c" }, list.Items.Cast<string>().ToArray());
	});

	[Fact]
	public void A_source_without_a_view_binds_the_CollectionViewSource_itself() => Run(() =>
	{
		var cvs = new CollectionViewSource();
		var text = new ContentControl();
		text.SetBinding(ContentControl.ContentProperty, new Binding { Source = cvs });

		Assert.Same(cvs, text.Content);
	});
}
