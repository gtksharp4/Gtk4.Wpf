using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Constructor detection must survive parentheses inside the parameter list.</summary>
public class ConstructorDetectionTests : GeneratorV2TestBase
{
	const string Dict = $@"<ResourceDictionary x:Class=""P1.Dict"" {Head}><SolidColorBrush x:Key=""B"" Color=""Red""/>
</ResourceDictionary>";

	[Theory]
	[InlineData("public Dict(int x = default(int)) { }")]
	[InlineData("public Dict([Foo(1)] int x) { }")]
	[InlineData("public Dict(int x = default(int)) : this() { }")]
	public void A_constructor_with_nested_parentheses_is_a_declared_non_default_constructor(string ctor)
	{
		string src = "namespace P1 { public partial class Dict { " + ctor + " } }";
		string code = Gen(Dict, out _, sources: new[] { src }, xamlPath: "Dict.xaml", rootNamespace: "P1");

		Assert.DoesNotContain("public Dict() { InitializeComponent(); }", code);
		Assert.DoesNotContain("GtkFormsXamlRegistration", code);
	}
}
