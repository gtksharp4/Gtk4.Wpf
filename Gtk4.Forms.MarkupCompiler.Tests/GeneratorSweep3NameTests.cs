using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>Sweep III: a CLR property named Name is a field only where WPF treats it as the element name.</summary>
public class GeneratorSweep3NameTests : Sweep3Base
{
	const string Strings = "xmlns:sys=\"clr-namespace:System;assembly=mscorlib\"";

	[Fact]
	public void Name_with_a_markup_extension_on_a_non_element_is_a_plain_property_set()
	{
		string xaml = Page("<Button/>", "<sys:String x:Key=\"NameStr\">n</sys:String>"
			+ "<DoubleAnimation x:Key=\"A\" Name=\"{StaticResource NameStr}\"/>", Strings);

		string code = Compile(xaml);

		Lacks(code, "internal ");
		Has(code, "DoubleAnimation.NameProperty");
	}

	[Fact]
	public void Plain_Name_on_a_non_element_creates_no_field()
	{
		string code = Compile(Page("<Button/>", "<DoubleAnimation x:Key=\"A\" Name=\"anim\"/>"));

		Lacks(code, "internal System.Windows.Media.Animation.DoubleAnimation anim");
		Lacks(code, "RegisterName(\"anim\"");
	}

	[Fact]
	public void Plain_Name_on_an_element_still_creates_the_field()
	{
		string code = Compile(Page("<Button Name=\"b\"/>"));

		Has(code, "internal ");
		Has(code, "RegisterName(\"b\"");
	}

	[Fact]
	public void X_Name_always_creates_the_field()
	{
		string code = Compile(Page("<Button/>", "<DoubleAnimation x:Key=\"A\" x:Name=\"anim\"/>"));

		Has(code, "anim");
	}

	[Fact]
	public void Plain_Name_on_a_type_without_a_Name_property_still_names_the_object()
	{
		string code = Compile(Page("<Grid><Grid.ColumnDefinitions><ColumnDefinition Name=\"c0\"/>"
			+ "</Grid.ColumnDefinitions></Grid>"));

		Has(code, "RegisterName(\"c0\"");
		Lacks(code, ".Name = ");
	}
}
