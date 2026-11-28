using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using Gtk4.PresentationFramework.Tests.ContentModel;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Data;

public class XPathBindingTests : ContentModelTestBase
{
	const string Books = "<Books><Book Number=\"1\" Stock=\"in\"><Title>One</Title></Book>"
		+ "<Book Number=\"2\" Stock=\"out\"><Title>Two</Title></Book>"
		+ "<Book Number=\"9\" Stock=\"in\"><Title>Nine</Title></Book></Books>";

	static XmlDocument Doc(string xml = Books)
	{
		var doc = new XmlDocument();
		doc.LoadXml(xml);
		return doc;
	}

	static XmlElement FirstBook() => (XmlElement)Doc().SelectSingleNode("Books/Book")!;

	[Fact]
	public void XPath_to_an_attribute_shows_its_value() => Run(() =>
	{
		var text = new TextBlock { DataContext = FirstBook() };
		text.SetBinding(TextBlock.TextProperty, new Binding { XPath = "@Number" });
		Assert.Equal("1", text.Text);
	});

	[Fact]
	public void XPath_to_an_element_shows_its_text() => Run(() =>
	{
		var text = new TextBlock { DataContext = FirstBook() };
		text.SetBinding(TextBlock.TextProperty, new Binding { XPath = "Title" });
		Assert.Equal("One", text.Text);
	});

	[Fact]
	public void A_Path_applies_to_the_XPath_result() => Run(() =>
	{
		var text = new TextBlock { DataContext = FirstBook() };
		text.SetBinding(TextBlock.TextProperty, new Binding("Name") { XPath = "Title" });
		Assert.Equal("Title", text.Text);
	});

	[Fact]
	public void An_unmatched_XPath_uses_the_FallbackValue() => Run(() =>
	{
		var text = new TextBlock { DataContext = FirstBook() };
		text.SetBinding(TextBlock.TextProperty, new Binding { XPath = "Missing", FallbackValue = "none" });
		Assert.Equal("none", text.Text);
	});

	[Fact]
	public void An_invalid_XPath_does_not_throw() => Run(() =>
	{
		var text = new TextBlock { DataContext = FirstBook() };
		text.SetBinding(TextBlock.TextProperty, new Binding { XPath = "[[", FallbackValue = "bad" });
		Assert.Equal("bad", text.Text);
	});

	[Fact]
	public void StringFormat_applies_to_the_XPath_text() => Run(() =>
	{
		var text = new TextBlock { DataContext = FirstBook() };
		text.SetBinding(TextBlock.TextProperty, new Binding { XPath = "Title", StringFormat = "{0} Page" });
		Assert.Equal("One Page", text.Text);
	});

	[Fact]
	public void ItemsSource_with_a_provider_and_XPath_gets_the_matching_nodes_in_document_order() => Run(() =>
	{
		var provider = new XmlDataProvider { Document = Doc(), XPath = "Books" };
		var list = new ListBox();
		list.SetBinding(ItemsControl.ItemsSourceProperty, new Binding
		{
			Source = provider,
			XPath = "*[@Stock='out'] | *[@Number>=8]",
		});
		_ = list.Widget;

		Assert.Equal(new[] { "2", "9" }, list.Items.Cast<XmlElement>().Select(e => e.GetAttribute("Number")));
	});

	[Fact]
	public void A_namespace_manager_resolves_prefixes() => Run(() =>
	{
		var doc = Doc("<r xmlns:a=\"urn:a\"><a:i>x</a:i></r>");
		var manager = new XmlNamespaceManager(doc.NameTable);
		manager.AddNamespace("q", "urn:a");
		var text = new TextBlock { DataContext = doc.DocumentElement };
		Binding.SetXmlNamespaceManager(text, manager);
		text.SetBinding(TextBlock.TextProperty, new Binding { XPath = "q:i" });
		Assert.Equal("x", text.Text);
	});

	[Fact]
	public void A_string_DataType_template_applies_to_an_XmlElement_and_a_CLR_template_to_CLR_items() => Run(() =>
	{
		var window = new Window();
		window.Resources[new DataTemplateKey("Hero")] = Template("hero:");
		window.Resources[new DataTemplateKey(typeof(Place))] = Template("place:");
		var hero = (XmlElement)Doc("<Heroes><Hero Name=\"Zed\">Zed</Hero></Heroes>").DocumentElement!.FirstChild!;
		var lb = new ListBox { ItemsSource = new object[] { hero, new Place { Name = "Rome" } } };
		window.Content = lb;
		_ = lb.Widget;

		Assert.Equal("hero:" + hero, Assert.Single(Texts((ListBoxItem)lb.ContainerAt(0)!)));
		Assert.Equal("place:Rome", Assert.Single(Texts((ListBoxItem)lb.ContainerAt(1)!)));
	});
}
