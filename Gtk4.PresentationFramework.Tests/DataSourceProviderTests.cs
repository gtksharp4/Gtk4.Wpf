using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>
/// Pure tests for <see cref="ObjectDataProvider"/> and <see cref="XmlDataProvider"/>.
/// Each test fails without the feature and passes with it.
/// </summary>
public class DataSourceProviderTests
{
	// ── ObjectDataProvider ───────────────────────────────────────────────────

	public class Counter
	{
		public int Value { get; set; } = 42;

		public string GetLabel() => "label-from-method";

		public string Greet(string name) => $"Hello {name}";
	}

	[Fact]
	public void ObjectDataProvider_instantiates_type_as_Data()
	{
		var odp = new ObjectDataProvider { ObjectType = typeof(Counter) };
		odp.InitialLoad();

		Assert.NotNull(odp.Data);
		Assert.IsType<Counter>(odp.Data);
	}

	[Fact]
	public void ObjectDataProvider_invokes_MethodName_as_Data()
	{
		var odp = new ObjectDataProvider
		{
			ObjectType = typeof(Counter),
			MethodName = "GetLabel"
		};
		odp.InitialLoad();

		Assert.Equal("label-from-method", odp.Data);
	}

	[Fact]
	public void ObjectDataProvider_MethodParameters_passed_to_method()
	{
		var odp = new ObjectDataProvider
		{
			ObjectType = typeof(Counter),
			MethodName = "Greet"
		};
		odp.MethodParameters.Add("World");
		odp.InitialLoad();

		Assert.Equal("Hello World", odp.Data);
	}

	[Fact]
	public void ObjectDataProvider_ObjectInstance_used_directly()
	{
		var instance = new Counter { Value = 99 };
		var odp = new ObjectDataProvider { ObjectInstance = instance };
		odp.InitialLoad();

		Assert.Same(instance, odp.Data);
	}

	[Fact]
	public void ObjectDataProvider_DataChanged_fires_on_Refresh()
	{
		var odp = new ObjectDataProvider { ObjectType = typeof(Counter) };
		odp.InitialLoad();

		int fireCount = 0;
		odp.DataChanged += (_, _) => fireCount++;

		odp.Refresh();
		Assert.Equal(1, fireCount);
	}

	[Fact]
	public void ObjectDataProvider_IsInitialLoadEnabled_false_defers_load()
	{
		var odp = new ObjectDataProvider
		{
			ObjectType = typeof(Counter),
			IsInitialLoadEnabled = false
		};
		odp.InitialLoad(); // should not load
		Assert.Null(odp.Data);

		odp.Refresh(); // explicit Refresh bypasses IsInitialLoadEnabled
		Assert.NotNull(odp.Data);
	}

	// ── XmlDataProvider ──────────────────────────────────────────────────────

	const string SampleXml = "<Root><Item name=\"A\"/><Item name=\"B\"/></Root>";

	static List<XmlNode> GetNodes(object? data)
	{
		var list = Assert.IsType<List<XmlNode>>(data);
		return list;
	}

	[Fact]
	public void XmlDataProvider_inline_xml_no_xpath_returns_root_node()
	{
		var xdp = new XmlDataProvider { XmlSerializer = SampleXml };
		xdp.InitialLoad();

		var list = GetNodes(xdp.Data);
		Assert.Single(list);
		Assert.Equal("Root", list[0].Name);
	}

	[Fact]
	public void XmlDataProvider_xpath_selects_matching_nodes()
	{
		var xdp = new XmlDataProvider
		{
			XmlSerializer = SampleXml,
			XPath = "/Root/Item"
		};
		xdp.InitialLoad();

		var list = GetNodes(xdp.Data);
		Assert.Equal(2, list.Count);
	}

	[Fact]
	public void XmlDataProvider_xpath_attribute_filter_selects_subset()
	{
		var xdp = new XmlDataProvider
		{
			XmlSerializer = SampleXml,
			XPath = "/Root/Item[@name='A']"
		};
		xdp.InitialLoad();

		var list = GetNodes(xdp.Data);
		Assert.Single(list);
		Assert.Equal("A", list[0].Attributes!["name"]!.Value);
	}

	[Fact]
	public void XmlDataProvider_Document_property_used_as_source()
	{
		var doc = new XmlDocument();
		doc.LoadXml(SampleXml);
		var xdp = new XmlDataProvider { Document = doc, XPath = "/Root/Item" };
		xdp.InitialLoad();

		var list = GetNodes(xdp.Data);
		Assert.Equal(2, list.Count);
	}

	[Fact]
	public void XmlDataProvider_DataChanged_fires_on_Refresh()
	{
		var xdp = new XmlDataProvider { XmlSerializer = SampleXml, XPath = "/Root/Item" };
		xdp.InitialLoad();

		int fires = 0;
		xdp.DataChanged += (_, _) => fires++;

		xdp.Refresh();
		Assert.Equal(1, fires);
	}
}

public class DataSourceProviderLazyLoadTests
{
	public class Item { public int Value { get; set; } = 7; }

	[Fact]
	public void Data_loads_the_provider_on_first_read()
	{
		var odp = new ObjectDataProvider { ObjectType = typeof(Item) };
		var item = Assert.IsType<Item>(odp.Data);
		Assert.Same(item, odp.Data);
	}

	[Fact]
	public void Data_does_not_load_when_the_initial_load_is_disabled()
	{
		var odp = new ObjectDataProvider { ObjectType = typeof(Item), IsInitialLoadEnabled = false };
		Assert.Null(odp.Data);
		odp.Refresh();
		Assert.IsType<Item>(odp.Data);
	}
}
