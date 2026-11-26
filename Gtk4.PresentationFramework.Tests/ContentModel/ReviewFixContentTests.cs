using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.ContentModel;

/// <summary>Round 72 review fixes: ContentSource scope, TabItem hosting, TabControl templates, rebuilds.</summary>
public class ReviewFixContentTests : ContentModelTestBase
{
	[Fact]
	public void A_bare_ContentPresenter_in_a_DataTemplate_does_not_alias_its_owner() => Run(() =>
	{
		var template = new DataTemplate
		{
			ItemFactory = item =>
			{
				var panel = new StackPanel();
				panel.Children.Add(new TextBlock { Text = "in:" + item });
				panel.Children.Add(new ContentPresenter());
				return panel;
			},
		};
		var cc = new ContentControl { ContentTemplate = template, Content = "x" };
		Assert.Equal(new[] { "in:x" }, Texts(cc));
		var presenter = new ContentPresenter { ContentTemplate = template, Content = "y" };
		Assert.Equal(new[] { "in:y" }, Texts(presenter));
	});

	[Fact]
	public void A_ContentPresenter_in_a_ControlTemplate_still_aliases_its_owner() => Run(() =>
	{
		var cc = new ContentControl
		{
			Template = new ControlTemplate { Factory = () => new Border { Child = new ContentPresenter() } },
			Content = "kept",
		};
		Assert.Equal(new[] { "kept" }, Texts(cc));
	});

	[Fact]
	public void TabItem_removes_the_text_label_when_the_content_becomes_an_element_and_back() => Run(() =>
	{
		var item = new TabItem { Content = "OLDTEXT" };
		_ = item.Widget;
		Assert.Equal(new[] { "OLDTEXT" }, Texts(item));
		item.Content = new TextBlock { Text = "elem" };
		Assert.Equal(new[] { "elem" }, Texts(item));
		item.Content = "NEW";
		Assert.Equal(new[] { "NEW" }, Texts(item));
	});

	[Fact]
	public void TabControl_ItemTemplate_is_the_header_template_and_ContentTemplate_the_content_template() => Run(() =>
	{
		var tc = new TabControl
		{
			ItemsSource = new[] { new Place { Name = "One" } },
			ItemTemplate = Template("head:"),
			ContentTemplate = Template("body:"),
		};
		_ = tc.Widget;
		var item = (TabItem)tc.ContainerAt(0)!;
		Assert.Equal(new[] { "body:One" }, Texts(item));
		Assert.Contains("head:One", Texts(item.CreateTabWidget()));
		Assert.Same(tc.ItemTemplate, item.HeaderTemplate);
		Assert.Null(item.ContentTemplate);
	});

	[Fact]
	public void TabControl_item_selectors_split_header_and_content_the_same_way() => Run(() =>
	{
		var headers = new FixedSelector(Template("hs:"));
		var bodies = new FixedSelector(Template("bs:"));
		var tc = new TabControl
		{
			ItemsSource = new[] { new Place { Name = "One" } },
			ItemTemplateSelector = headers,
			ContentTemplateSelector = bodies,
		};
		_ = tc.Widget;
		var item = (TabItem)tc.ContainerAt(0)!;
		Assert.Same(headers, item.HeaderTemplateSelector);
		Assert.Null(item.ContentTemplateSelector);
		Assert.Equal(new[] { "bs:One" }, Texts(item));
		Assert.Contains("hs:One", Texts(item.CreateTabWidget()));
	});

	[Fact]
	public void A_Visual_item_gets_no_generated_header() => Run(() =>
	{
		var tc = new TabControl { ItemsSource = new UIElement[] { new TextBlock { Text = "page" } } };
		_ = tc.Widget;
		Assert.Null(((TabItem)tc.ContainerAt(0)!).Header);
	});

	[Fact]
	public void DataTemplateKey_is_not_sealed_and_TemplateType_follows_WPF_order()
	{
		Assert.False(typeof(DataTemplateKey).IsSealed);
		var type = typeof(TemplateKey).GetNestedType("TemplateType", System.Reflection.BindingFlags.NonPublic)!;
		Assert.Equal(0, (int)System.Enum.Parse(type, "DataTemplate"));
		Assert.Equal(1, (int)System.Enum.Parse(type, "TableTemplate"));
	}

	[Fact]
	public void A_parent_change_keeps_plain_string_content_and_its_styled_label() => Run(() =>
	{
		var button = new Button { Content = "plain" };
		var native = (Gtk.Button)button.Widget;
		var before = native.FirstChild;
		var panel = new StackPanel();
		panel.Children.Add(button);
		panel.Children.Remove(button);
		Assert.Same(before, native.FirstChild);
		Assert.Equal("plain", Assert.Single(Texts(button)));
	});

	[Fact]
	public void Joining_a_tree_re_chooses_when_an_ancestor_supplies_the_implicit_template() => Run(() =>
	{
		var holder = new StackPanel();
		holder.Resources[new DataTemplateKey(typeof(Place))] = Template("imp:");
		var inner = new StackPanel();
		var item = new ListBoxItem { Content = new Place { Name = "Q" } };
		inner.Children.Add(item);
		_ = item.Widget;
		Assert.Equal("Place:Q", Assert.Single(Texts(item)));
		holder.Children.Add(inner);
		Assert.Equal("imp:Q", Assert.Single(Texts(item)));
	});

	sealed class FixedSelector : DataTemplateSelector
	{
		readonly DataTemplate _template;

		public FixedSelector(DataTemplate template) => _template = template;

		public override DataTemplate? SelectTemplate(object? item, DependencyObject? container) => _template;
	}
}
