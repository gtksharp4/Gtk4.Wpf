using System.Windows.Automation;
using Auto = System.Windows.Automation.Automation;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.AutomationClient;

public class TreeWalkerTests : UiaTestBase
{
	[Fact]
	public void Raw_walker_walks_parent_children_and_siblings() => Run(() =>
	{
		OpenForm();
		var walker = TreeWalker.RawViewWalker;
		var panel = Find("Panel");
		var first = walker.GetFirstChild(panel);
		Assert.Equal("Name", first.Current.AutomationId);
		Assert.Equal("Items", walker.GetLastChild(panel).Current.AutomationId);
		var second = walker.GetNextSibling(first);
		Assert.Equal("Ok", second.Current.AutomationId);
		Assert.Equal(first, walker.GetPreviousSibling(second));
		Assert.Null(walker.GetPreviousSibling(first));
		Assert.Null(walker.GetNextSibling(walker.GetLastChild(panel)));
		Assert.Equal(panel, walker.GetParent(second));
	});

	[Fact]
	public void A_window_parent_is_the_root_element_and_the_root_has_none() => Run(() =>
	{
		OpenForm();
		var window = Find("MainWindow");
		Assert.Equal(AutomationElement.RootElement, TreeWalker.RawViewWalker.GetParent(window));
		Assert.Null(TreeWalker.RawViewWalker.GetParent(AutomationElement.RootElement));
		Assert.Equal(window, TreeWalker.RawViewWalker.GetFirstChild(AutomationElement.RootElement));
	});

	[Fact]
	public void Control_and_content_walkers_expose_their_conditions() => Run(() =>
	{
		Assert.Same(Auto.ControlViewCondition, TreeWalker.ControlViewWalker.Condition);
		Assert.Same(Auto.ContentViewCondition, TreeWalker.ContentViewWalker.Condition);
		Assert.Same(Auto.RawViewCondition, TreeWalker.RawViewWalker.Condition);
	});

	[Fact]
	public void A_custom_walker_filters_and_promotes_the_children_of_skipped_elements() => Run(() =>
	{
		OpenForm();
		var walker = new TreeWalker(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
		var window = Find("MainWindow");
		Assert.Equal("Ok", walker.GetFirstChild(window).Current.AutomationId);
		Assert.Equal("Off", walker.GetNextSibling(walker.GetFirstChild(window)).Current.AutomationId);
		Assert.Null(walker.GetParent(Find("Ok")));
	});

	[Fact]
	public void Normalize_returns_the_element_when_it_matches_else_its_nearest_matching_ancestor() => Run(() =>
	{
		OpenForm();
		var ok = Find("Ok");
		Assert.Equal(ok, TreeWalker.RawViewWalker.Normalize(ok));
		var buttons = new TreeWalker(new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Window));
		Assert.Equal(Find("MainWindow"), buttons.Normalize(ok));
	});

	[Fact]
	public void Walker_arguments_are_validated() => Run(() =>
	{
		Assert.Throws<System.ArgumentNullException>(() => new TreeWalker(null!));
		Assert.Throws<System.ArgumentNullException>(() => TreeWalker.RawViewWalker.GetParent(null!));
	});
}
