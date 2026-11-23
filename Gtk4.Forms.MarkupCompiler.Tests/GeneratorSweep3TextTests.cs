using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>Sweep III: text-only property elements (TabItem.Header, MenuItem.Header, TextBlock.Text).</summary>
public class GeneratorSweep3TextTests : Sweep3Base
{
	[Fact]
	public void TabItem_header_text_is_assigned_with_whitespace_collapsed()
	{
		string xaml = Page("<TabControl><TabItem><TabItem.Header>\n  Add   Control\n</TabItem.Header></TabItem></TabControl>");

		string code = Compile(xaml);

		Has(code, "\"Add Control\"");
		Assert.DoesNotContain(Messages(xaml), m => m.Contains("Skipped"));
	}

	[Fact]
	public void MenuItem_header_text_and_nested_item_are_emitted()
	{
		string xaml = Page("<Menu><MenuItem><MenuItem.Header>File</MenuItem.Header>"
			+ "<MenuItem><MenuItem.Header>Load</MenuItem.Header></MenuItem></MenuItem></Menu>");

		string code = Compile(xaml);

		Has(code, "\"File\"");
		Has(code, "\"Load\"");
		Assert.DoesNotContain(Messages(xaml), m => m.Contains("Skipped"));
	}

	[Fact]
	public void TextBlock_text_property_element_is_emitted()
	{
		string xaml = Page("<TextBlock><TextBlock.Text>hello</TextBlock.Text></TextBlock>");

		Has(Compile(xaml), "\"hello\"");
	}

	[Fact]
	public void Typed_text_property_element_uses_the_attribute_conversion()
	{
		string xaml = Page("<Border><Border.Margin>12pt</Border.Margin></Border>");

		Has(Compile(xaml), "\"12pt\"");
	}

	[Fact]
	public void Empty_text_property_element_is_still_reported_as_skipped_or_ignored()
	{
		string code = Compile(Page("<TabItem><TabItem.Header>   </TabItem.Header></TabItem>"));

		Lacks(code, "HeaderProperty");
	}
}
