using System.Text.RegularExpressions;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class GeneratorV3NameScopeTests : GeneratorV2TestBase
{
	const string WindowStub = "namespace A { public partial class W : System.Windows.Window { } }";

	static string Window(string content, string attrs = "")
		=> $@"<Window x:Class=""A.W"" {Head} {attrs}>{content}</Window>";

	[Fact]
	public void G3_a_name_in_a_DataTemplate_and_in_the_tree_is_one_root_field()
	{
		string code = Gen(Window(@"<Window.Resources><DataTemplate x:Key=""d""><Border x:Name=""border""/></DataTemplate>
</Window.Resources><Border x:Name=""border""/>"));
		Assert.Single(Regex.Matches(code, @"internal [\w.]*Border border;"));
		Assert.Single(Regex.Matches(code, @"this\.border = "));
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G3_names_inside_templates_get_no_root_field_and_no_assignment()
	{
		string code = Gen(Window(@"<Window.Resources>
<ControlTemplate x:Key=""c"" TargetType=""Button""><Grid><Border x:Name=""PART_a""/></Grid></ControlTemplate>
<DataTemplate x:Key=""d""><TextBlock x:Name=""PART_b""/></DataTemplate></Window.Resources><Button x:Name=""ok""/>"));
		Assert.DoesNotContain("PART_a;", code);
		Assert.DoesNotContain("this.PART_b", code);
		Assert.Contains("internal System.Windows.Controls.Button ok;", code);
		Assert.Contains("this.ok = ", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G3_the_same_name_in_two_templates_does_not_collide()
	{
		string code = Gen(Window(@"<Window.Resources>
<DataTemplate x:Key=""a""><Border x:Name=""b""/></DataTemplate>
<DataTemplate x:Key=""b""><Border x:Name=""b""/></DataTemplate></Window.Resources>"));
		Assert.DoesNotContain(" b;", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G3_a_name_in_a_nested_template_of_a_style_setter_is_scoped()
	{
		string code = Gen(Window(@"<Window.Resources><Style TargetType=""Button""><Setter Property=""Template""><Setter.Value>
<ControlTemplate TargetType=""Button""><Border x:Name=""bd""/></ControlTemplate></Setter.Value></Setter></Style>
</Window.Resources><Border x:Name=""bd""/>"));
		Assert.Single(Regex.Matches(code, @"Border bd;"));
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G3_a_root_x_Name_gets_a_field_bound_to_this()
	{
		string code = Gen(Window("", "x:Name=\"root\""));
		Assert.Contains("internal System.Windows.Window root;", code);
		Assert.Contains("this.root = this;", code);
		AssertCompiles(code, WindowStub);
	}

	[Fact]
	public void G3_a_root_name_equal_to_the_class_name_gets_no_field()
	{
		string code = Gen(Window("", "x:Name=\"W\""));
		Assert.DoesNotContain("internal System.Windows.Window W;", code);
	}

	[Fact]
	public void F5_an_implicit_page_whose_name_a_user_class_owns_gets_an_internal_class()
	{
		const string user = "namespace A { public class P : System.Windows.Controls.Page { } }";
		string code = Gen($@"<Page {Head}><Border/></Page>", out string name,
			sources: new[] { user }, xamlPath: "P.xaml", rootNamespace: "A");
		Assert.Equal("A.__Xaml_P", name);
		Assert.Contains("GtkFormsXamlRegistration(\"P.xaml\", typeof(global::A.__Xaml_P))", code);
		Assert.Contains("partial class __Xaml_P :", code);
		AssertCompiles(code, user);
	}

	[Theory]
	[InlineData("// class P")]
	[InlineData("/* class P */ namespace A { }")]
	[InlineData("namespace A { class Q { string s = \"class P\"; } }")]
	[InlineData("namespace B { public class P { } }")]
	[InlineData("namespace B { } namespace A { }")]
	public void F5_comments_strings_and_other_namespaces_do_not_collide(string source)
	{
		string code = Gen($@"<Page {Head}><Border/></Page>", out string name,
			sources: new[] { source }, xamlPath: "P.xaml", rootNamespace: "A");
		Assert.Equal("A.P", name);
		Assert.Contains("partial class P :", code);
	}

	[Theory]
	[InlineData("namespace A; public class P { }")]
	[InlineData("namespace A { namespace C { } public class P { } }")]
	[InlineData("namespace Z { namespace A { public class P { } } }")]
	public void F5_file_scoped_and_nested_namespaces_are_matched(string source)
	{
		string expected = source.StartsWith("namespace Z") ? "A.P" : "A.__Xaml_P";
		Gen($@"<Page {Head}><Border/></Page>", out string name,
			sources: new[] { source }, xamlPath: "P.xaml", rootNamespace: "A");
		Assert.Equal(expected, name);
	}

	[Fact]
	public void F4_x_Name_also_sets_FrameworkElement_Name_but_only_on_framework_elements()
	{
		string code = Gen(Window(@"<Border x:Name=""b""/><TextBlock Name=""t""/>
<Window.Resources><SolidColorBrush x:Key=""k"" x:Name=""brush""/></Window.Resources>", "x:Name=\"root\""));
		Assert.Contains("this.b = ", code);
		Assert.Matches(@"__e\d+\.Name = ""b"";", code);
		Assert.Matches(@"__e\d+\.Name = ""t"";", code);
		Assert.Contains("this.Name = \"root\";", code);
		Assert.DoesNotMatch(@"brush\w*\.Name =", code);
		AssertCompiles(code, WindowStub);
	}
}
