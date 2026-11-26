using System.Linq;
using System.Reflection;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class GeneratorV2FixTests : GeneratorV2TestBase
{
	const string WindowStub = "namespace A { public partial class W : System.Windows.Window { } }";
	const string Local = "xmlns:local=\"clr-namespace:A\" xmlns:sys=\"clr-namespace:System;assembly=mscorlib\"";

	static string Window(string resources, string ns = "")
		=> $@"<Window x:Class=""A.W"" {Head} {ns}><Window.Resources>{resources}</Window.Resources></Window>";


	[Fact]
	public void F1_StaticResource_on_a_Freezable_coerces_the_resource_and_never_calls_FindResource_on_it()
	{
		string code = Gen(Window(
			"<Color x:Key=\"c\">Red</Color><SolidColorBrush x:Key=\"b\" Color=\"{StaticResource c}\"/>"));
		Assert.Contains("SetValue(System.Windows.Media.SolidColorBrush.ColorProperty, (global::System.Windows.Media.Color)" +
			"System.Windows.Markup.XamlValueConverter.CoerceResource(typeof(global::System.Windows.Media.Color), " +
			"System.Windows.Markup.StaticResourceScope.Find(\"c\"", code);
		Assert.DoesNotContain("__e0.FindResource", code);
		AssertCompiles(code, WindowStub);
	}


	[Fact]
	public void F1_StaticResource_on_a_real_DP_with_a_CLR_only_name_keeps_SetValue()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head} {Local}><Window.Resources>
<sys:String x:Key=""s"">v</sys:String></Window.Resources><Button Content=""{{StaticResource s}}""/></Window>");
		Assert.Contains("SetValue(System.Windows.Controls.Button.ContentProperty, ", code);
		Assert.DoesNotContain(".Content = (", code);
	}

	[Fact]
	public void A_Run_with_Background_and_Foreground_uses_the_dependency_properties_and_compiles()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><TextBlock>"
			+ @"<Run Background=""Yellow"" Foreground=""Red"" Text=""x""/></TextBlock></Window>");
		Assert.Contains("SetValue(System.Windows.Documents.Run.BackgroundProperty,", code);
		Assert.Contains("SetValue(System.Windows.Documents.Run.ForegroundProperty,", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void F1_DynamicResource_on_a_Freezable_resolves_against_the_enclosing_element_and_compiles()
	{
		string code = Gen(Window(
			"<Color x:Key=\"c\">Red</Color><SolidColorBrush x:Key=\"b\" Color=\"{DynamicResource c}\"/>"));
		Assert.Contains("XamlValueConverter.SetResourceValue(this, __e0, System.Windows.Media.SolidColorBrush" +
			".ColorProperty, \"c\");", code);
		Assert.DoesNotContain("SetLocalResourceReference(__e0", code);
		AssertCompiles(code, WindowStub);
	}

	const string UserR = @"namespace A { public partial class W : System.Windows.Window { }
public class R { public string EmployeeNumber { get; set; } public int Count { get; set; }
public bool Flag { get; set; } } }";

	[Fact]
	public void F2_unresolved_user_type_attributes_go_through_the_runtime_hook()
	{
		string code = Gen(Window(
			"<local:R x:Key=\"r\" EmployeeNumber=\"57304\" Count=\"3\" Flag=\"True\"/>", Local));
		Assert.Contains("XamlValueConverter.SetClrProperty(__e0, \"EmployeeNumber\", \"57304\");", code);
		Assert.Contains("XamlValueConverter.SetClrProperty(__e0, \"Count\", \"3\");", code);
		AssertCompiles(code, UserR);
	}

	[Fact]
	public void F2_unresolved_user_type_with_a_StaticResource_attribute_compiles()
	{
		string code = Gen(Window("<sys:String x:Key=\"s\">v</sys:String>"
			+ "<local:R x:Key=\"r\" EmployeeNumber=\"{StaticResource s}\"/>", Local));
		Assert.Contains("XamlValueConverter.SetClrValue(__e0, \"EmployeeNumber\", ", code);
		AssertCompiles(code, UserR);
	}

	const string RegionType =
		"namespace A { public partial class W : System.Windows.Window { } public enum Region { N, S } }";

	[Fact]
	public void F3_x_Type_parameter_is_emitted_as_typeof()
	{
		string code = Gen(Window(@"<ObjectDataProvider x:Key=""p"" ObjectType=""{x:Type sys:Enum}""
MethodName=""GetValues"">
<ObjectDataProvider.MethodParameters><x:Type TypeName=""local:Region""/></ObjectDataProvider.MethodParameters>
</ObjectDataProvider>", Local));
		Assert.Contains(".MethodParameters.Add(typeof(global::A.Region));", code);
		AssertCompiles(code, RegionType);
	}

	[Fact]
	public void F3_x_Static_and_x_Null_parameters_are_emitted()
	{
		string code = Gen(Window(@"<ObjectDataProvider x:Key=""p"" ObjectType=""{x:Type sys:Math}"" MethodName=""Max"">
<ObjectDataProvider.MethodParameters><x:Static Member=""sys:Int32.MaxValue""/><x:Null/>
</ObjectDataProvider.MethodParameters>
</ObjectDataProvider>", Local));
		Assert.Contains(".MethodParameters.Add(global::System.Int32.MaxValue);", code);
		Assert.Contains(".MethodParameters.Add(null);", code);
	}

	[Fact]
	public void F3_a_parameter_that_cannot_be_emitted_gives_the_whole_provider_no_entry()
	{
		string code = Gen(Window(@"<ObjectDataProvider x:Key=""p"" ObjectType=""{x:Type sys:Math}"" MethodName=""Max"">
<ObjectDataProvider.MethodParameters><sys:Int32>1</sys:Int32><Button/></ObjectDataProvider.MethodParameters>
</ObjectDataProvider>", Local));
		Assert.DoesNotContain("Resources[\"p\"]", code);
		Assert.DoesNotContain("ObjectDataProvider()", code);
		Assert.DoesNotContain("MethodParameters", code);
	}

	[Fact]
	public void F7_an_ancestor_resource_written_after_the_referencing_child_resolves()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Border Background=""{{StaticResource B}}""/>
<Window.Resources><SolidColorBrush x:Key=""B"" Color=""Gold""/></Window.Resources></Window>");
		Assert.True(code.IndexOf("Resources[\"B\"]") < code.IndexOf("StaticResourceScope.Find(\"B\""), code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void F7_a_forward_reference_inside_one_dictionary_stays_in_document_order()
	{
		string code = Gen(Window(@"<Style x:Key=""a"" TargetType=""Button"" BasedOn=""{StaticResource b}""/>
<Style x:Key=""b"" TargetType=""Button""/>"));
		Assert.True(code.IndexOf("StaticResourceScope.Find(\"b\"") < code.IndexOf("Resources[\"b\"]"), code);
	}

	[Fact]
	public void F8_generators_keep_no_shared_static_state_and_interleave_cleanly()
	{
		var fields = typeof(XamlCodeGenerator).GetFields(
			BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			.Where(f => f.IsStatic && !f.IsInitOnly && !f.IsLiteral).Select(f => f.Name).ToList();
		Assert.Empty(fields);

		string a = Window("<SolidColorBrush x:Key=\"B\"/>").Replace("<Window.Resources>",
			"<Window.Resources><Style x:Key=\"s\" TargetType=\"Button\">"
			+ "<Setter Property=\"Background\" Value=\"{StaticResource B}\"/></Style>");
		string b = $@"<Window x:Class=""A.W"" {Head}><Grid>
<Grid.Resources><SolidColorBrush x:Key=""G""/></Grid.Resources>
<Border Background=""{{StaticResource G}}""/></Grid></Window>";
		string a1 = Gen(a), b1 = Gen(b);
		Assert.Equal(a1, Gen(a));
		Assert.Equal(b1, Gen(b));
		Assert.Equal(a1, Gen(a));
	}
}
