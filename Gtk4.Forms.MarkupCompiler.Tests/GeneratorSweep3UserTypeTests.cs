using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>Sweep III: content property of a type declared in the project, prefixed commands.</summary>
public class GeneratorSweep3UserTypeTests : Sweep3Base
{
	const string Local = "xmlns:l=\"clr-namespace:App\"";

	static string Source(string types) => "namespace App { " + types + " public partial class C { } }";

	[Fact]
	public void User_type_inherits_the_content_property_of_a_referenced_base()
	{
		string stub = Source("public class MagCanvas : System.Windows.Controls.Canvas { }");

		string code = Compile(Page("<l:MagCanvas><Button/></l:MagCanvas>", attrs: Local), stub);

		Has(code, ".Children.Add(");
		Lacks(code, "__e0.Content");
	}

	[Fact]
	public void User_type_content_property_from_nameof_is_assigned()
	{
		string stub = Source("[System.Windows.Markup.ContentProperty(nameof(Example))] "
			+ "public class Ex : System.Windows.Controls.Control { public object Example { get; set; } }");

		string code = Compile(Page("<l:Ex><Button/></l:Ex>", attrs: Local), stub);

		Has(code, ".Example = ");
	}

	[Fact]
	public void User_type_collection_content_property_from_a_string_adds_each_child()
	{
		string stub = Source("[System.Windows.Markup.ContentProperty(\"Parts\")] public class Ex : "
			+ "System.Windows.Controls.Control { public System.Collections.Generic.List<object> Parts { get; } "
			+ "= new System.Collections.Generic.List<object>(); }");

		string code = Compile(Page("<l:Ex><Button/><Button/></l:Ex>", attrs: Local), stub);

		Assert.Equal(2, code.Split(new[] { ".Parts.Add(" }, System.StringSplitOptions.None).Length - 1);
	}

	[Fact]
	public void User_type_inherits_the_content_property_of_a_user_base()
	{
		string stub = Source("[System.Windows.Markup.ContentProperty(\"Body\")] public class A : "
			+ "System.Windows.Controls.Control { public object Body { get; set; } } public class B : A { }");

		string code = Compile(Page("<l:B><Button/></l:B>", attrs: Local), stub);

		Has(code, ".Body = ");
	}

	[Fact]
	public void Prefixed_command_is_a_direct_static_reference()
	{
		string xaml = Page("<Button Command=\"a:AnnotationService.CreateTextStickyNoteCommand\"/>",
			attrs: "xmlns:a=\"clr-namespace:System.Windows.Annotations;assembly=PresentationFramework\"");

		string code = Compile(xaml);

		Has(code, "global::System.Windows.Annotations.AnnotationService.CreateTextStickyNoteCommand");
	}

	[Fact]
	public void Prefixed_command_member_without_the_Command_suffix_is_found()
	{
		string xaml = Page("<Button Command=\"a:AnnotationService.CreateTextStickyNote\"/>",
			attrs: "xmlns:a=\"clr-namespace:System.Windows.Annotations\"");

		Has(Compile(xaml), "AnnotationService.CreateTextStickyNoteCommand");
	}

	[Fact]
	public void Unknown_prefixed_command_keeps_the_runtime_converter()
	{
		string xaml = Page("<Button Command=\"a:AnnotationService.Nope\"/>",
			attrs: "xmlns:a=\"clr-namespace:System.Windows.Annotations\"");

		Has(Compile(xaml), "XamlValueConverter.Convert(");
	}
}
