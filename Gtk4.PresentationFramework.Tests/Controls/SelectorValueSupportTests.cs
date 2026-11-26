using System.Windows.Controls;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Controls;

file sealed class Item
{
	public int Id { get; set; }
	public string Name { get; set; } = string.Empty;
	public Address? Address { get; set; }
}

file sealed class Address
{
	public string City { get; set; } = string.Empty;
}

/// <summary>Pure unit tests for <see cref="SelectorValueSupport"/>.</summary>
public class SelectorValueSupportTests
{
	[Fact]
	public void GetValue_null_path_returns_item()
	{
		var item = new Item { Id = 1 };
		Assert.Same(item, SelectorValueSupport.GetValue(item, null));
	}

	[Fact]
	public void GetValue_empty_path_returns_item()
	{
		var item = new Item { Id = 2 };
		Assert.Same(item, SelectorValueSupport.GetValue(item, string.Empty));
	}

	[Fact]
	public void GetValue_single_segment_reads_property()
	{
		var item = new Item { Id = 42, Name = "foo" };
		Assert.Equal(42, SelectorValueSupport.GetValue(item, "Id"));
		Assert.Equal("foo", SelectorValueSupport.GetValue(item, "Name"));
	}

	[Fact]
	public void GetValue_dotted_path_recurses()
	{
		var item = new Item { Address = new Address { City = "Rome" } };
		Assert.Equal("Rome", SelectorValueSupport.GetValue(item, "Address.City"));
	}

	[Fact]
	public void GetValue_missing_segment_returns_null()
	{
		var item = new Item { Id = 1 };
		Assert.Null(SelectorValueSupport.GetValue(item, "Missing"));
	}

	[Fact]
	public void GetValue_null_intermediate_returns_null()
	{
		// Address is null; City segment cannot be resolved.
		var item = new Item { Address = null };
		Assert.Null(SelectorValueSupport.GetValue(item, "Address.City"));
	}

	[Fact]
	public void GetValue_null_item_returns_null()
	{
		Assert.Null(SelectorValueSupport.GetValue(null, "Id"));
	}

	[Fact]
	public void FindByValue_returns_first_match()
	{
		var items = new object[] { new Item { Id = 1 }, new Item { Id = 2 }, new Item { Id = 3 } };
		var list = new System.Collections.ArrayList();
		foreach (var i in items) list.Add(i);

		var result = SelectorValueSupport.FindByValue(list, "Id", 2);
		Assert.Equal(2, ((Item)result!).Id);
	}

	[Fact]
	public void FindByValue_no_match_returns_null()
	{
		var list = new System.Collections.ArrayList();
		list.Add(new Item { Id = 1 });

		Assert.Null(SelectorValueSupport.FindByValue(list, "Id", 99));
	}

	[Fact]
	public void FindByValue_empty_path_matches_by_object_equality()
	{
		var a = "alpha";
		var b = "beta";
		var list = new System.Collections.ArrayList();
		list.Add(a);
		list.Add(b);

		Assert.Same(b, SelectorValueSupport.FindByValue(list, string.Empty, "beta"));
	}
}
