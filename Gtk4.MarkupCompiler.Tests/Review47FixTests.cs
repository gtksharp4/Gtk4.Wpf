using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Round 47 review fixes: dictionary registration, same-stem dictionaries, provider parameters, Main.</summary>
public class Review47FixTests : GeneratorV2TestBase
{
	const string Dict = $@"<ResourceDictionary x:Class=""P1.Dict"" {Head}><SolidColorBrush x:Key=""B"" Color=""Red""/>
</ResourceDictionary>";

	const string AppXaml = $@"<Application x:Class=""A.App"" {Head}/>";

	[Fact]
	public void An_x_Class_dictionary_is_registered_for_Source_lookup()
	{
		string code = Gen(Dict, out _, hasCodeBehind: false, xamlPath: "Dict.xaml", rootNamespace: "P1");

		Assert.Contains("GtkFormsXamlRegistration(\"Dict.xaml\", typeof(global::P1.Dict))", code);
		AssertCompiles(code);
	}

	[Fact]
	public void An_x_Class_dictionary_without_a_public_default_constructor_is_not_registered()
	{
		string src = "namespace P1 { public partial class Dict { public Dict(int x) { InitializeComponent(); } } }";
		string code = Gen(Dict, out _, sources: new[] { src }, xamlPath: "Dict.xaml", rootNamespace: "P1");

		Assert.DoesNotContain("GtkFormsXamlRegistration", code);
	}

	[Fact]
	public void An_x_Class_dictionary_with_a_public_default_constructor_in_code_behind_is_registered()
	{
		string src = "namespace P1 { public partial class Dict { public Dict() { InitializeComponent(); } } }";
		string code = Gen(Dict, out _, sources: new[] { src }, xamlPath: "Dict.xaml", rootNamespace: "P1");

		Assert.Contains("GtkFormsXamlRegistration(\"Dict.xaml\"", code);
	}

	[Fact]
	public void A_dictionary_whose_code_behind_declares_no_constructor_gets_the_generated_one()
	{
		string src = "namespace P1 { public partial class Dict { void OnClick(object o) { } } }";
		string code = Gen(Dict, out _, sources: new[] { src }, xamlPath: "Dict.xaml", rootNamespace: "P1");

		Assert.Contains("public Dict() { InitializeComponent(); }", code);
		Assert.Contains("GtkFormsXamlRegistration(\"Dict.xaml\"", code);
	}

	[Fact]
	public void A_dictionary_with_its_own_public_constructor_does_not_get_a_second_one()
	{
		string src = "namespace P1 { public partial class Dict { public Dict() { InitializeComponent(); } } }";
		string code = Gen(Dict, out _, sources: new[] { src }, xamlPath: "Dict.xaml", rootNamespace: "P1");

		Assert.DoesNotContain("public Dict() { InitializeComponent(); }", code);
	}

	[Fact]
	public void A_dictionary_with_an_implicitly_private_constructor_is_not_registered()
	{
		string src = "namespace P1 { public partial class Dict { Dict() { InitializeComponent(); } } }";
		string code = Gen(Dict, out _, sources: new[] { src }, xamlPath: "Dict.xaml", rootNamespace: "P1");

		Assert.DoesNotContain("GtkFormsXamlRegistration", code);
	}

	[Fact]
	public void Merged_dictionaries_load_through_the_non_throwing_path()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Window.Resources><ResourceDictionary>
<ResourceDictionary.MergedDictionaries><ResourceDictionary Source=""/Missing;component/X.xaml""/>
</ResourceDictionary.MergedDictionaries></ResourceDictionary></Window.Resources></Window>");

		Assert.Contains("GtkFormsXamlRegistry.TryLoadDictionary(", code);
		Assert.DoesNotContain("{ Source =", code);
	}

	[Fact]
	public void Same_stem_dictionaries_in_different_folders_get_distinct_classes_and_paths()
	{
		const string xaml = $@"<ResourceDictionary {Head}><SolidColorBrush x:Key=""B"" Color=""Red""/></ResourceDictionary>";
		string light = Gen(xaml, out string lightCls, false, xamlPath: "Themes/Light/Colors.xaml", rootNamespace: "A");
		string dark = Gen(xaml, out string darkCls, false, xamlPath: "Themes\\Dark\\Colors.xaml", rootNamespace: "A");

		Assert.Equal("A.Themes_S_Light_S_Colors", lightCls);
		Assert.Equal("A.Themes_S_Dark_S_Colors", darkCls);
		Assert.Contains("GtkFormsXamlRegistration(\"Themes/Light/Colors.xaml\"", light);
		Assert.Contains("GtkFormsXamlRegistration(\"Themes/Dark/Colors.xaml\"", dark);
		AssertCompiles(light, dark);
	}

	[Fact]
	public void A_Page_keeps_its_stem_class_name()
	{
		Gen($@"<Page {Head}/>", out string cls, false, xamlPath: "Views/Main.xaml", rootNamespace: "A");

		Assert.Equal("A.Main", cls);
	}

	[Theory]
	[InlineData("MethodParameters")]
	[InlineData("ConstructorParameters")]
	public void ObjectDataProvider_parameter_lists_are_emitted_not_reported_as_dropped(string list)
	{
		string xaml = $@"<Window x:Class=""A.W"" {Head} xmlns:sys=""clr-namespace:System;assembly=mscorlib"">
<Window.Resources><ObjectDataProvider x:Key=""p"" ObjectType=""{{x:Type sys:Math}}"" MethodName=""Abs"">
<ObjectDataProvider.{list}><sys:Double>-1</sys:Double></ObjectDataProvider.{list}></ObjectDataProvider>
</Window.Resources></Window>";

		Assert.DoesNotContain(Messages(xaml), m => m.Contains(list));
		Assert.Contains("." + list + ".Add(", Gen(xaml));
	}

	[Fact]
	public void Main_comes_from_an_ApplicationDefinition_even_when_an_unrelated_class_declares_Main()
	{
		string src = "namespace A { class Tool { static void Main(string[] a) { } } "
			+ "public partial class App : System.Windows.Application { } }";

		Assert.Contains("static void Main()", Gen(AppXaml, sources: new[] { src }, itemType: "ApplicationDefinition"));
	}

	[Fact]
	public void A_Page_typed_App_xaml_gets_no_Main()
	{
		Assert.DoesNotContain("Main", Gen(AppXaml, itemType: "Page"));
	}

	[Fact]
	public void Main_is_skipped_when_the_application_class_declares_it()
	{
		string src = "namespace A { public partial class App : System.Windows.Application "
			+ "{ static void Main(string[] args) { } } }";
		string code = Gen(AppXaml, sources: new[] { src }, itemType: "ApplicationDefinition");

		Assert.DoesNotContain("static void Main()", code);
		AssertCompiles(code, src);
	}
}

public class Review47LanguageTests : GeneratorV2TestBase
{
	[Fact]
	public void A_Span_Language_is_converted_to_an_XmlLanguage_not_emitted_as_a_string()
	{
		string xaml = $@"<Window x:Class=""A.W"" {Head}><TextBlock><Span Language=""ar-SA"">x</Span></TextBlock></Window>";

		string code = Gen(xaml);

		Assert.DoesNotContain("Language = \"ar-SA\"", code);
		Assert.Contains("Span.LanguageProperty, System.Windows.Markup.XamlValueConverter.Convert(", code);
		AssertCompiles(code, "namespace A { public partial class W : System.Windows.Window { } }");
	}
}

public class Review47DynamicResourceTests : GeneratorV2TestBase
{
	[Fact]
	public void A_DynamicResource_on_a_media_target_probes_the_enclosing_scope_first()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Grid><Grid.Resources>
<SolidColorBrush x:Key=""b"" Color=""Red""/><GeometryDrawing x:Key=""g"" Brush=""{{DynamicResource b}}""/>
</Grid.Resources></Grid></Window>");

		Assert.Contains("StaticResourceScope.TryFind(\"b\", this, __", code);
		Assert.Contains("?? this.TryFindResource(\"b\")", code);
		AssertCompiles(code, "namespace A { public partial class W : System.Windows.Window { } }");
	}
}
