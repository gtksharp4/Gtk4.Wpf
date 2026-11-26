using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class CollectionViewSourceFilterTests : WpfGtkTestBase
{
	static string[] Shown(CollectionViewSource cvs) => cvs.View!.Cast<string>().ToArray();

	[Fact]
	public void Adding_and_removing_a_handler_turns_the_filter_on_and_off() => Run(() =>
	{
		var cvs = new CollectionViewSource { Source = new[] { "a", "bb", "ccc" } };
		Assert.Null(cvs.View!.Filter);
		FilterEventHandler handler = (_, e) => e.Accepted = ((string)e.Item).Length > 1;
		cvs.Filter += handler;
		Assert.NotNull(cvs.View.Filter);
		Assert.Equal(new[] { "bb", "ccc" }, Shown(cvs));
		cvs.Filter -= handler;
		Assert.Null(cvs.View.Filter);
		Assert.Equal(new[] { "a", "bb", "ccc" }, Shown(cvs));
	});

	[Fact]
	public void A_handler_added_before_the_view_exists_applies_to_it() => Run(() =>
	{
		var cvs = new CollectionViewSource();
		cvs.Filter += (_, e) => e.Accepted = (string)e.Item != "a";
		cvs.Source = new[] { "a", "b" };
		Assert.Equal(new[] { "b" }, Shown(cvs));
	});

	[Fact]
	public void Every_handler_runs_and_the_final_Accepted_wins() => Run(() =>
	{
		var cvs = new CollectionViewSource { Source = new[] { "a", "b" } };
		int first = 0, second = 0;
		cvs.Filter += (_, e) => { first++; e.Accepted = false; };
		cvs.Filter += (_, e) => { second++; e.Accepted = (string)e.Item == "b"; };
		Assert.Equal(new[] { "b" }, Shown(cvs));
		Assert.Equal(first, second);
		Assert.True(first >= 2);
	});

	[Fact]
	public void Accepted_defaults_to_true_and_Item_is_the_item() => Run(() =>
	{
		var cvs = new CollectionViewSource { Source = new[] { "a" } };
		object? seen = null;
		bool? accepted = null;
		cvs.Filter += (_, e) => { seen = e.Item; accepted = e.Accepted; };
		Assert.Equal(new[] { "a" }, Shown(cvs));
		Assert.Equal("a", seen);
		Assert.True(accepted);
	});

	[Fact]
	public void The_filter_survives_a_later_Source_change() => Run(() =>
	{
		var cvs = new CollectionViewSource { Source = new[] { "a", "bb" } };
		cvs.Filter += (_, e) => e.Accepted = ((string)e.Item).Length > 1;
		Assert.Equal(new[] { "bb" }, Shown(cvs));
		cvs.Source = new[] { "x", "yy", "zzz" };
		Assert.Equal(new[] { "yy", "zzz" }, Shown(cvs));
	});

	[Fact]
	public void The_handler_sees_items_added_to_an_observable_source() => Run(() =>
	{
		var source = new ObservableCollection<string> { "a" };
		var cvs = new CollectionViewSource { Source = source };
		cvs.Filter += (_, e) => e.Accepted = (string)e.Item != "drop";
		source.Add("drop");
		source.Add("keep");
		Assert.Equal(new[] { "a", "keep" }, Shown(cvs));
	});

	[Fact]
	public void Source_is_a_dependency_property() => Run(() =>
	{
		var cvs = new CollectionViewSource();
		cvs.SetValue(CollectionViewSource.SourceProperty, new[] { "h1" });
		Assert.Equal(new[] { "h1" }, Shown(cvs));
		Assert.Same(cvs.Source, cvs.GetValue(CollectionViewSource.SourceProperty));
	});
}
