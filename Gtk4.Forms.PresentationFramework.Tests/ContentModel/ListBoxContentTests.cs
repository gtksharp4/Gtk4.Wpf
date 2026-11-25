using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.ContentModel;

/// <summary>ListBox items go through the shared resolution, including the implicit DataType template.</summary>
public class ListBoxContentTests : ContentModelTestBase
{
	static ListBoxItem Item(ListBox lb, int i) => (ListBoxItem)lb.ContainerAt(i)!;

	[Fact]
	public void ListBoxItem_hosts_a_UIElement_child() => Run(() =>
	{
		var text = new TextBlock { Text = "el" };
		var item = new ListBoxItem { Content = text };
		Assert.True(IsInside(item.Widget, text.Widget));
		Assert.Equal("el", Assert.Single(Texts(item)));
	});

	[Fact]
	public void ListBoxItem_applies_its_ContentTemplate() => Run(() =>
	{
		var item = new ListBoxItem { Content = new Place { Name = "A" }, ContentTemplate = Template("t:") };
		Assert.Equal("t:A", Assert.Single(Texts(item)));
	});

	[Fact]
	public void ListBoxItem_shows_the_text_of_a_Hyperlink_content_not_its_type_name() => Run(() =>
	{
		var link = new Hyperlink();
		link.Inlines.Add(new Run("Animation"));
		var item = new ListBoxItem { Content = link };
		Assert.Equal("Animation", Assert.Single(Texts(item)));
	});

	[Fact]
	public void ListBoxItem_keeps_string_content_and_follows_content_changes() => Run(() =>
	{
		var item = new ListBoxItem { Content = "one" };
		_ = item.Widget;
		Assert.Equal("one", Assert.Single(Texts(item)));
		item.Content = "two";
		Assert.Equal("two", Assert.Single(Texts(item)));
	});

	[Fact]
	public void Generated_items_use_an_implicit_template_from_the_window_resources() => Run(() =>
	{
		var window = new Window();
		window.Resources[new DataTemplateKey(typeof(Place))] = Template("imp:");
		var lb = new ListBox { ItemsSource = new[] { new Place { Name = "Seattle" }, new Place { Name = "Portland" } } };
		window.Content = lb;
		_ = lb.Widget;
		Assert.Equal("imp:Seattle", Assert.Single(Texts(Item(lb, 0))));
		Assert.Equal("imp:Portland", Assert.Single(Texts(Item(lb, 1))));
	});

	[Fact]
	public void Generated_containers_get_the_item_as_DataContext() => Run(() =>
	{
		var place = new Place { Name = "A" };
		var lb = new ListBox { ItemsSource = new[] { place } };
		_ = lb.Widget;
		Assert.Same(place, Item(lb, 0).DataContext);
	});

	[Fact]
	public void DataTrigger_and_MultiDataTrigger_setters_apply_to_generated_items() => Run(() =>
	{
		var style = new Style(typeof(ListBoxItem));
		var wa = new DataTrigger { Binding = new System.Windows.Data.Binding("State"), Value = "WA" };
		wa.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = Brushes.Red });
		style.Triggers.Add(wa);
		var multi = new MultiDataTrigger();
		multi.Conditions.Add(new Condition { Binding = new System.Windows.Data.Binding("Name"), Value = "Portland" });
		multi.Conditions.Add(new Condition { Binding = new System.Windows.Data.Binding("State"), Value = "OR" });
		multi.Setters.Add(new Setter { Property = Control.BackgroundProperty, Value = Brushes.Cyan });
		style.Triggers.Add(multi);

		var window = new Window();
		window.Resources[typeof(ListBoxItem)] = style;
		window.Resources[new DataTemplateKey(typeof(Place))] = Template("p:");
		var lb = new ListBox
		{
			ItemsSource = new[]
			{
				new Place { Name = "Seattle", State = "WA" },
				new Place { Name = "Portland", State = "OR" },
				new Place { Name = "Salem", State = "OR" },
			},
		};
		window.Content = lb;
		_ = lb.Widget;

		Assert.Equal(Colors.Red, ColorOf(Item(lb, 0).Foreground));
		Assert.NotEqual(Colors.Red, ColorOf(Item(lb, 1).Foreground));
		Assert.Equal(Colors.Cyan, ColorOf(Item(lb, 1).Background));
		Assert.NotEqual(Colors.Cyan, ColorOf(Item(lb, 2).Background));
		var text = (TextBlock)Item(lb, 0).Presenter!.VisualChildren.Single();
		Assert.Equal(Colors.Red, ColorOf(text.Foreground));
	});

	static Color? ColorOf(Brush? b) => (b as SolidColorBrush)?.Color;
}
