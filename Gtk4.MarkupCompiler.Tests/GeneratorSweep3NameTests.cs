using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

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
		string code = Compile(Page("<Button/>", "<AnnotationResource x:Key=\"A\" Name=\"anim\"/>"), Stub);

		Lacks(code, "AnnotationResource anim");
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

	[Fact]
	public void Plain_Name_on_a_Timeline_creates_the_field_and_registers_it()
	{
		string xaml = Page("<Button/>", "<Storyboard x:Key=\"k\" Name=\"sb\"/>");

		string code = Gen(xaml, sources: new[] { Stub });

		Has(code, "Storyboard sb");
		Has(code, "RegisterName(\"sb\"");
	}

	[Fact]
	public void A_named_Storyboard_field_is_usable_from_the_code_behind()
	{
		const string stub = "namespace App { public partial class C { void Go() { sb.Begin(this); } } }";

		string code = Compile(Page("<Button/>", "<Storyboard x:Key=\"k\" Name=\"sb\"/>"), stub);

		Has(code, "Storyboard sb");
	}

	[Fact]
	public void A_named_animation_in_a_Storyboard_creates_its_field()
	{
		string code = Compile(Page("<Button/>", "<Storyboard x:Key=\"k\"><DoubleAnimation Name=\"anim\" To=\"1\"/>"
			+ "</Storyboard>"));

		Has(code, "DoubleAnimation anim");
		Has(code, "RegisterName(\"anim\"");
	}
}
