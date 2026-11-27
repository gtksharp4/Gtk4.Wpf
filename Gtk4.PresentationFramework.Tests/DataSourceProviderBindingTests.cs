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


	public sealed class Row
	{
		public string Data { get; set; } = "row data";
		public string Name { get; set; } = "n";
	}

	sealed class RowProvider : DataSourceProvider
	{
		public string Marker { get; set; } = "provider";

		public void Set(object? data) => OnQueryFinished(data);

		protected override void QueryData() => OnQueryFinished(new Row());
	}

	[Fact]
	public void A_path_is_evaluated_against_the_Data_even_when_the_provider_has_a_property_of_that_name() => Run(() =>
	{
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, new Binding("Data") { Source = new RowProvider() });
		var xpath = new TextBlock();
		xpath.SetBinding(TextBlock.TextProperty, new Binding("Name") { Source = new RowProvider() });

		Assert.Equal("row data", text.Text);
		Assert.Equal("n", xpath.Text);
	});

	[Fact]
	public void BindsDirectlyToSource_evaluates_the_path_against_the_provider() => Run(() =>
	{
		var provider = Tasks();
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty,
			new Binding("XPath") { Source = provider, BindsDirectlyToSource = true });

		Assert.Equal("Tasks/Task", text.Text);
	});

	[Fact]
	public void A_binding_to_a_provider_follows_a_change_of_its_Data() => Run(() =>
	{
		var provider = new RowProvider();
		var text = new TextBlock();
		text.SetBinding(TextBlock.TextProperty, new Binding("Name") { Source = provider });
		Assert.Equal("n", text.Text);

		provider.Set(new Row { Name = "second" });

		Assert.Equal("second", text.Text);
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
