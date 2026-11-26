using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Attribute literals on System.Object properties stay strings, as in WPF's XAML parser.</summary>
public class GeneratorSweep4ObjectLiteralTests : Sweep3Base
{
	static string Win(string body) => "<Window " + Head + " x:Class=\"App.C\">" + body + "</Window>";

	[Theory]
	[InlineData("<ComboBoxItem Tag=\"1\"/>", "\"1\"")]
	[InlineData("<Button Tag=\"2.5\"/>", "\"2.5\"")]
	[InlineData("<Button Content=\"3\"/>", "\"3\"")]
	[InlineData("<Button CommandParameter=\"2\"/>", "\"2\"")]
	[InlineData("<Button ToolTip=\"true\"/>", "\"true\"")]
	[InlineData("<Border Tag=\"false\"/>", "\"false\"")]
	public void Object_typed_literal_is_a_string(string xaml, string literal)
	{
		string code = Compile(Win(xaml));

		Assert.Contains(literal, code);
		Assert.DoesNotMatch(@"\b(Tag|Content|CommandParameter|ToolTip)\s*=\s*(\d|true|false)", code);
	}

	[Fact]
	public void Root_object_property_literal_is_a_string()
	{
		string code = Compile("<Window " + Head + " x:Class=\"App.C\" Tag=\"7\"/>");

		Assert.DoesNotMatch(@"Tag\s*=\s*7", code);
	}

	[Fact]
	public void Markup_extensions_on_object_properties_are_unchanged()
	{
		string code = Compile(Win("<Button Tag=\"{x:Null}\" Content=\"{x:Static SystemColors.ControlBrush}\"/>"));

		Assert.Contains("null", code);
		Assert.Contains("SystemColors", code);
	}

	[Fact]
	public void Setter_and_trigger_values_keep_their_raw_text_conversion()
	{
		string code = Compile(Win("<Window.Resources><Style x:Key=\"s\" TargetType=\"Button\">"
			+ "<Setter Property=\"Width\" Value=\"5\"/><Style.Triggers>"
			+ "<Trigger Property=\"IsEnabled\" Value=\"true\"><Setter Property=\"Tag\" Value=\"2\"/></Trigger>"
			+ "<DataTrigger Binding=\"{Binding A}\" Value=\"3\"/></Style.Triggers></Style></Window.Resources>"));

		Has(code, "PropertyName = \"Width\", Value = \"5\"");
		Has(code, "PropertyName = \"IsEnabled\", Value = \"true\"");
		Has(code, "PropertyName = \"Tag\", Value = \"2\"");
		Has(code, "Value = \"3\"");
	}
}
