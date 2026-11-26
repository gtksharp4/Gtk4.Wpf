using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class GeneratorV3TypeKeyTests : GeneratorV2TestBase
{
	const string WindowStub = "namespace A { public partial class W : System.Windows.Window { } public class Foo { } }";
	const string Local = "xmlns:local=\"clr-namespace:A\"";

	static string Window(string resources, string body = "")
		=> $@"<Window x:Class=""A.W"" {Head} {Local}><Window.Resources>{resources}</Window.Resources>{body}</Window>";

	const string Chain = @"<Style TargetType=""TextBlock""><Setter Property=""FontSize"" Value=""12""/></Style>
<Style x:Key=""t"" TargetType=""TextBlock"" BasedOn=""{StaticResource {x:Type TextBlock}}""/>";

	[Fact]
	public void G2_StaticResource_with_an_x_Type_key_looks_up_the_type()
	{
		string code = Gen(Window(Chain));
		Assert.DoesNotContain("{x:Type", code);
		Assert.Contains("FindResource(typeof(global::System.Windows.Controls.TextBlock))", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G2_an_implicit_style_referenced_by_type_key_is_registered_under_its_type()
	{
		string code = Gen(Window(Chain));
		Assert.Contains("Resources[typeof(global::System.Windows.Controls.TextBlock)] = __style", code);
	}

	[Fact]
	public void G2_an_unreferenced_implicit_style_is_not_registered()
	{
		string code = Gen(Window("<Style TargetType=\"TextBlock\"><Setter Property=\"FontSize\" Value=\"12\"/></Style>"));
		Assert.DoesNotContain("Resources[typeof(", code);
	}

	[Fact]
	public void G2_a_style_keyed_by_an_x_Type_is_registered_under_the_type()
	{
		string code = Gen(Window(@"<Style x:Key=""{x:Type Button}"" TargetType=""Button""/>"));
		Assert.Contains("Resources[typeof(global::System.Windows.Controls.Button)] = __style", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G2_StaticResource_x_Type_on_an_element_attribute()
	{
		string code = Gen(Window(Chain, "<Button Style=\"{StaticResource {x:Type TextBlock}}\"/>"));
		Assert.Contains("typeof(global::System.Windows.Controls.TextBlock)", code);
		Assert.DoesNotContain("{x:Type", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G2_string_keys_are_unchanged()
	{
		string code = Gen(Window("<Style x:Key=\"k\" TargetType=\"Button\"/>", "<Button Style=\"{StaticResource k}\"/>"));
		Assert.Contains("Find(\"k\"", code);
		Assert.Contains("Resources[\"k\"] = __style", code);
	}

	[Fact]
	public void G2_nested_x_Type_with_a_prefix_inside_a_RelativeSource()
	{
		string code = Gen(Window("", @"<TextBlock Text=""{Binding Path=X, RelativeSource={RelativeSource FindAncestor,
AncestorType={x:Type local:Foo}}}""/>"));
		Assert.Contains("typeof(global::A.Foo)", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G2_nested_x_Type_of_a_presentation_type_outside_the_built_in_map_is_qualified()
	{
		string code = Gen(Window("", @"<TextBlock Text=""{Binding Path=X, RelativeSource={RelativeSource FindAncestor,
AncestorType={x:Type TreeViewItem}}}""/>"));
		Assert.Contains("typeof(System.Windows.Controls.TreeViewItem)", code);
		AssertCompiles(code, WindowStub);
	}
}
