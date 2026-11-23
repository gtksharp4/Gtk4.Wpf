using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

public class GeneratorV2Tests : GeneratorV2TestBase
{
	const string PageStub = "namespace A { public partial class P : System.Windows.Controls.Page { } }";
	const string WindowStub = "namespace A { public partial class W : System.Windows.Window { } }";

	[Fact]
	public void G1_element_triggers_compile_against_the_layer()
	{
		string code = Gen($@"<Page x:Class=""A.P"" {Head}><Button><Button.Triggers>
<EventTrigger RoutedEvent=""Button.Click""><BeginStoryboard><Storyboard/></BeginStoryboard></EventTrigger>
</Button.Triggers></Button></Page>");
		Assert.Contains(".AttachElementTriggers();", code);
		AssertCompiles(code, PageStub);
	}

	[Fact]
	public void G2_Uri_clr_property_is_converted()
	{
		string code = Gen($@"<Page x:Class=""A.P"" {Head}><TextBlock>
<Hyperlink NavigateUri=""Page2.xaml"">go</Hyperlink></TextBlock></Page>");
		Assert.Contains(".NavigateUri = new System.Uri(\"Page2.xaml\", System.UriKind.RelativeOrAbsolute);", code);
		AssertCompiles(code, PageStub);
	}

	[Fact]
	public void G2_PointCollection_goes_through_the_runtime_converter()
	{
		string code = Gen($@"<Page x:Class=""A.P"" {Head}><Polygon Points=""0,0 10,0 5,8""/></Page>");
		Assert.Contains(".Points = (global::System.Windows.Shapes.PointCollection)System.Windows.Markup" +
			".XamlValueConverter.ConvertByType(typeof(global::System.Windows.Shapes.PointCollection), " +
			"\"0,0 10,0 5,8\");", code);
		AssertCompiles(code, PageStub);
	}

	[Fact]
	public void G2_plain_string_property_keeps_its_literal()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head} Title=""T""/>");
		Assert.Contains("this.Title = \"T\";", code);
	}

	[Fact]
	public void G3_without_code_behind_the_partial_declares_the_root_base_and_a_constructor()
	{
		string code = Gen($@"<Page x:Class=""A.P"" {Head}><Button x:Name=""b""/></Page>", hasCodeBehind: false);
		Assert.Contains("public partial class P : global::System.Windows.Controls.Page {", code);
		Assert.Contains("public P() { InitializeComponent(); }", code);
		AssertCompiles(code);
	}

	[Fact]
	public void G3_with_code_behind_the_declaration_is_unchanged()
	{
		string code = Gen($@"<Page x:Class=""A.P"" {Head}/>");
		Assert.Contains("public partial class P : global::System.Windows.Controls.Page {", code);
		Assert.DoesNotContain("public P()", code);
	}

	[Fact]
	public void G3_code_behind_is_detected_from_the_project_sources_by_class_name()
	{
		string xaml = $@"<Page x:Class=""A.P"" {Head}/>";
		string with = Gen(xaml, sources: new[]
		{ "namespace A { public partial class P : System.Windows.Controls.Page { } }" });
		Assert.Contains("public partial class P : global::System.Windows.Controls.Page {", with);
		string without = Gen(xaml, sources: new[] { "namespace A { public partial class Other { } }" });
		Assert.Contains("public partial class P : global::System.Windows.Controls.Page {", without);
	}

	[Fact]
	public void G3_Application_root_gets_a_base_but_no_constructor()
	{
		string code = Gen($@"<Application x:Class=""A.App"" {Head}/>", hasCodeBehind: false);
		Assert.Contains("public partial class App : global::System.Windows.Application {", code);
		Assert.DoesNotContain("public App()", code);
		AssertCompiles(code);
	}

	const string Local = "xmlns:local=\"clr-namespace:A\" xmlns:sys=\"clr-namespace:System;assembly=mscorlib\"";

	const string UserTypes = @"namespace A { public partial class W : System.Windows.Window { }
public class Conv : System.Windows.Data.IValueConverter {
public object Convert(object v, System.Type t, object p, System.Globalization.CultureInfo c) => v;
public object ConvertBack(object v, System.Type t, object p, System.Globalization.CultureInfo c) => v; }
public class Data { public string Name { get; set; } public int Count { get; set; } }
public class Scale { public static double Conv(double d, int t) => d; } }";

	[Fact]
	public void G4_user_type_resource_is_instantiated_with_its_attributes()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head} {Local}><Window.Resources>
<local:Data x:Key=""d"" Name=""n"" Count=""3""/><local:Conv x:Key=""c""/></Window.Resources></Window>");
		Assert.DoesNotContain("Resources[\"d\"] = \"\"", code);
		Assert.Contains("new global::A.Data();", code);
		Assert.Contains("XamlValueConverter.SetClrProperty(__e0, \"Count\", \"3\");", code);
		AssertCompiles(code, UserTypes);
	}

	[Fact]
	public void G4_XmlDataProvider_gets_its_inline_document()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Window.Resources>
<XmlDataProvider x:Key=""p"" XPath=""Inv/Books""><x:XData>
<Inv xmlns=""""><Books><Book Id=""1""/></Books></Inv></x:XData></XmlDataProvider>
</Window.Resources></Window>");
		Assert.Contains(".XPath = \"Inv/Books\";", code);
		Assert.Contains("LoadXml(\"<Inv xmlns=\\\"\\\"><Books><Book Id=\\\"1\\\" /></Books></Inv>\")", code);
		AssertCompiles(code, UserTypes);
	}

	[Fact]
	public void G4_provider_with_an_attribute_the_layer_lacks_gets_no_entry()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Window.Resources>
<XmlDataProvider x:Key=""p"" XmlNamespaceManager=""{{StaticResource m}}"" XPath=""a""/></Window.Resources></Window>");
		Assert.DoesNotContain("Resources[\"p\"]", code);
	}

	[Fact]
	public void G4_SolidColorBrush_resource_is_instantiated()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Window.Resources>
<SolidColorBrush x:Key=""B"" Color=""Gold""/></Window.Resources></Window>");
		Assert.DoesNotContain("Resources[\"B\"] = \"\"", code);
		Assert.Contains("SetValue(System.Windows.Media.SolidColorBrush.ColorProperty, ", code);
		Assert.Contains("\"Gold\"", code);
		AssertCompiles(code, UserTypes);
	}

	[Fact]
	public void G4_ObjectDataProvider_keeps_type_method_and_parameters()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head} {Local}><Window.Resources>
<ObjectDataProvider x:Key=""o"" ObjectType=""{{x:Type local:Scale}}"" MethodName=""Conv"">
<ObjectDataProvider.MethodParameters><sys:Double>2.5</sys:Double><sys:Int32>1</sys:Int32>
</ObjectDataProvider.MethodParameters>
</ObjectDataProvider></Window.Resources></Window>");
		Assert.Contains(".MethodName = \"Conv\";", code);
		Assert.Contains("MethodParameters.Add(System.Windows.Markup.XamlValueConverter" +
			".ConvertByType(typeof(global::System.Double), \"2.5\"))", code);
		AssertCompiles(code, UserTypes);
	}

	[Fact]
	public void G4_System_clr_namespace_values_keep_the_plain_emission()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head} {Local}><Window.Resources>
<sys:String x:Key=""s"">hi</sys:String></Window.Resources></Window>");
		Assert.Contains("Resources[\"s\"] = \"hi\";", code);
	}

	[Fact]
	public void G5_converter_in_an_inner_dictionary_resolves_through_the_enclosing_scopes()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head} {Local}><StackPanel>
<StackPanel.Resources><local:Conv x:Key=""c""/></StackPanel.Resources>
<TextBox Text=""{{Binding Name, Converter={{StaticResource c}}}}""/></StackPanel></Window>");
		Assert.Matches(@"Converter = \(System\.Windows\.Data\.IValueConverter\)System\.Windows\.Markup" +
			@"\.StaticResourceScope\.Find\(""c"", this, __e\d+(, __e\d+)*\)", code);
		AssertCompiles(code, UserTypes);
	}

	[Fact]
	public void G5_attribute_StaticResource_on_an_unparented_element_probes_its_ancestors()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Grid>
<Grid.Resources><SolidColorBrush x:Key=""B""/></Grid.Resources>
<Border Background=""{{StaticResource B}}""/></Grid></Window>");
		Assert.Matches(@"BackgroundProperty, System\.Windows\.Markup\.StaticResourceScope" +
			@"\.Find\(""B"", this, __e\d+, __e\d+\)", code);
		Assert.DoesNotMatch(@"__e\d+\.FindResource", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G5_DynamicResource_on_a_DP_is_a_deferred_reference()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Border Background=""{{DynamicResource B}}""/></Window>");
		Assert.Matches(@"SetLocalResourceReference\(__e\d+, .*BackgroundProperty, ""B""\)", code);
		Assert.DoesNotContain("FindResource(\"B\")", code);
	}

	[Fact]
	public void G5_root_level_lookup_is_unchanged()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head} Style=""{{StaticResource S}}""/>");
		Assert.Contains("this.Style = (System.Windows.Style)this.FindResource(\"S\");", code);
	}
}
