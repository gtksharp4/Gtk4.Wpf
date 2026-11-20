using System.Text.RegularExpressions;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

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
	public void G3_an_implicit_page_class_that_the_code_already_declares_emits_only_the_registration()
	{
		string code = Gen($@"<Page {Head}><Border/></Page>", out string name,
			sources: new[] { "namespace A { public class P : System.Windows.Controls.Page { } }" },
			xamlPath: "P.xaml", rootNamespace: "A");
		Assert.Equal("A.P", name);
		Assert.Contains("GtkFormsXamlRegistration(\"P.xaml\", typeof(global::A.P))", code);
		Assert.DoesNotContain("class P", code);
	}
}
