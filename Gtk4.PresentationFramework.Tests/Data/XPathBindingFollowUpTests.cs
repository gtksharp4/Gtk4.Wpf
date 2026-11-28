using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using Gtk4.PresentationFramework.Tests.ContentModel;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Data;

/// <summary>XPath bindings beyond a plain text target: object targets, master/detail, triggers, multi-bindings.</summary>
public class XPathBindingFollowUpTests : ContentModelTestBase
{
	const string Books = "<Books><Book Number=\"1\" Stock=\"in\"><Title>One</Title></Book>"
		+ "<Book Number=\"2\" Stock=\"out\"><Title>Two</Title></Book></Books>";

	sealed class JoinConverter : IMultiValueConverter
	{
		public object Convert(object?[] values, System.Type targetType, object? parameter, CultureInfo culture)
			=> string.Join("+", values);

		public object?[]? ConvertBack(object? value, System.Type[] targetTypes, object? parameter, CultureInfo culture)
			=> throw new System.NotSupportedException();
	}

	static XmlDocument Doc()
	{
		var doc = new XmlDocument();
		doc.LoadXml(Books);
		return doc;
	}

	static XmlElement Book(int index) => (XmlElement)Doc().DocumentElement!.ChildNodes[index]!;

	[Fact]
	public void A_Label_Content_bound_to_an_attribute_shows_the_attribute_value() => Run(() =>
	{
		var label = new Label { DataContext = Book(0) };
		label.SetBinding(ContentControl.ContentProperty, new Binding { XPath = "@Number" });
		_ = label.Widget;
		Assert.Equal("1", Assert.Single(Texts(label)));
	});

	[Fact]
	public void A_ListBox_of_XmlElements_without_a_template_shows_the_inner_text() => Run(() =>
	{
		var lb = new ListBox { ItemsSource = Doc().DocumentElement!.ChildNodes.Cast<XmlNode>().ToList() };
		_ = lb.Widget;
		Assert.Equal("One", Assert.Single(Texts((ListBoxItem)lb.ContainerAt(0)!)));
		Assert.Equal("Two", Assert.Single(Texts((ListBoxItem)lb.ContainerAt(1)!)));
	});

	[Fact]
	public void Moving_the_current_item_of_a_provider_re_evaluates_an_XPath_binding() => Run(() =>
	{
		var provider = new XmlDataProvider { Document = Doc(), XPath = "Books/Book" };
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, new Binding { Source = provider, XPath = "@Number" });
		Assert.Equal("1", text.Text);

		var view = CollectionViewSource.GetDefaultView(provider.Data!);
		Assert.True(view!.MoveCurrentToNext());
		Assert.Equal("2", text.Text);
	});

	[Fact]
	public void A_DataTrigger_with_an_XPath_binding_fires_on_the_matching_node_text() => Run(() =>
	{
		var style = new Style(typeof(TextBlock));
		var trigger = new DataTrigger { Binding = new Binding { XPath = "@Stock" }, Value = "out" };
		trigger.Setters.Add(new Setter(TextBlock.TextProperty, "sold out"));
		style.Triggers.Add(trigger);

		var inStock = new TextBlock { Style = style, DataContext = Book(0) };
		var outOfStock = new TextBlock { Style = style, DataContext = Book(1) };
		Assert.NotEqual("sold out", inStock.Text);
		Assert.Equal("sold out", outOfStock.Text);
	});

	[Fact]
	public void MultiBinding_children_accept_XPath() => Run(() =>
	{
		var multi = new MultiBinding { Converter = new JoinConverter() };
		multi.Bindings.Add(new Binding { XPath = "@Number" });
		multi.Bindings.Add(new Binding { XPath = "Title" });
		var text = new TextBlock { DataContext = Book(1) };
		text.SetBinding(TextBlock.TextProperty, multi);
		Assert.Equal("2+Two", text.Text);
	});

	[Fact]
	public void A_hierarchical_template_ItemsSource_XPath_filters_the_children() => Run(() =>
	{
		var root = (XmlElement)Doc().DocumentElement!;
		var template = new HierarchicalDataTemplate { ItemsSource = new Binding { XPath = "Book[@Stock='out']" } };
		var children = template.ResolveChildren(root)!.Cast<XmlElement>().ToList();
		Assert.Equal("2", Assert.Single(children).GetAttribute("Number"));
	});
}
