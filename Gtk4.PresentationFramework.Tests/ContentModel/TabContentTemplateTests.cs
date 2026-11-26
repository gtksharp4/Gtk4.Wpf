using System.Linq;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ContentModel;

/// <summary>TabControl.ContentTemplate and friends apply live to the selected tab without its own (WPF).</summary>
public class TabContentTemplateTests : ContentModelTestBase
{
	[Fact]
	public void An_explicit_TabItem_without_its_own_template_shows_the_TabControl_ContentTemplate() => Run(() =>
	{
		var tab = new TabItem { Header = "h", Content = new Place { Name = "One" } };
		var tc = new TabControl { ContentTemplate = Template("body:") };
		tc.Items.Add(tab);
		_ = tc.Widget;
		Assert.Equal(new[] { "body:One" }, Texts(tab));
	});

	[Fact]
	public void Changing_the_TabControl_ContentTemplate_updates_the_generated_content() => Run(() =>
	{
		var tc = new TabControl
		{
			ItemsSource = new[] { new Place { Name = "One" } },
			ContentTemplate = Template("a:"),
		};
		_ = tc.Widget;
		var item = (TabItem)tc.ContainerAt(0)!;
		Assert.Equal(new[] { "a:One" }, Texts(item));
		tc.ContentTemplate = Template("b:");
		Assert.Equal(new[] { "b:One" }, Texts(item));
		tc.ContentTemplate = null;
		Assert.Equal(new[] { "Place:One" }, Texts(item));
	});

	[Fact]
	public void A_TabItem_with_its_own_template_keeps_it() => Run(() =>
	{
		var tab = new TabItem { Content = new Place { Name = "One" }, ContentTemplate = Template("own:") };
		var tc = new TabControl { ContentTemplate = Template("body:") };
		tc.Items.Add(tab);
		_ = tc.Widget;
		Assert.Equal(new[] { "own:One" }, Texts(tab));
	});

	[Fact]
	public void The_template_follows_the_selection_and_the_string_format_applies() => Run(() =>
	{
		var first = new TabItem { Content = new Place { Name = "One" } };
		var second = new TabItem { Content = 5 };
		var tc = new TabControl { ContentTemplate = Template("body:") };
		tc.Items.Add(first);
		tc.Items.Add(second);
		_ = tc.Widget;
		Assert.Equal(new[] { "body:One" }, Texts(first));
		Assert.Equal(new[] { "5" }, Texts(second));
		tc.SelectedIndex = 1;
		Assert.Equal(new[] { "body:5" }, Texts(second));
		Assert.Equal(new[] { "Place:One" }, Texts(first));
		tc.ContentTemplate = null;
		tc.ContentStringFormat = "n{0}";
		Assert.Equal(new[] { "n5" }, Texts(second));
	});
}
