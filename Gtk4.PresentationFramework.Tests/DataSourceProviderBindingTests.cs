using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class DataSourceProviderBindingTests : WpfGtkTestBase
{
	static XmlDataProvider Tasks()
	{
		var provider = new XmlDataProvider { XPath = "Tasks/Task", Document = new XmlDocument() };
		provider.Document!.LoadXml("<Tasks xmlns=\"\"><Task Name=\"a\" Type=\"Home\"/><Task Name=\"b\" Type=\"Work\"/>"
			+ "<Task Name=\"c\" Type=\"Home\"/></Tasks>");
		return provider;
	}

	[Fact]
	public void A_binding_whose_Source_is_a_provider_binds_to_its_Data() => Run(() =>
	{
		var provider = Tasks();
		var ic = new ItemsControl();
		ic.SetBinding(ItemsControl.ItemsSourceProperty, new Binding { Source = provider });
		_ = ic.Widget;

		Assert.Equal(3, ic.Items.Count);
		Assert.IsAssignableFrom<XmlNode>(ic.Items[0]);
	});

	[Fact]
	public void A_path_naming_a_provider_property_binds_to_the_provider_itself() => Run(() =>
	{
		var provider = Tasks();
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, new Binding("XPath") { Source = provider });

		Assert.Equal("Tasks/Task", text.Text);
	});

	[Fact]
	public void PropertyGroupDescription_groups_XML_nodes_by_an_attribute()
	{
		var view = new CollectionViewSource { Source = Tasks() }.View!;
		view.GroupDescriptions.Add(new PropertyGroupDescription("@Type"));

		var groups = view.Groups!.Cast<CollectionViewGroup>().ToList();
		Assert.Equal(new object?[] { "Home", "Work" }, groups.Select(g => g.Name));
		Assert.Equal(new[] { 2, 1 }, groups.Select(g => g.ItemCount));
	}
}
