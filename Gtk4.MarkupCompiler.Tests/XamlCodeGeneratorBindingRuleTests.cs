using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Rule attribute typing, Binding.Converter element, diagnostics and binding-argument markup.</summary>
public class XamlCodeGeneratorBindingRuleTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:local=""clr-namespace:MyApp"">
	{body}
</Window>";

	string Gen(string body) => _gen.Generate(Window(body), out _);

	string GenRules(string rules) => Gen(@"<TextBox><TextBox.Text><Binding Path=""Age""><Binding.ValidationRules>"
		+ rules + "</Binding.ValidationRules></Binding></TextBox.Text></TextBox>");

	string GenArg(string binding) => Gen($@"<TextBlock Text=""{binding}"" />");

	[Fact]
	public void ValidationStep_attribute_emits_the_enum()
		=> Assert.Contains(".ValidationStep = System.Windows.Controls.ValidationStep.UpdatedValue;",
			GenRules(@"<local:R ValidationStep=""UpdatedValue""/>"));

	[Fact]
	public void Builtin_ExceptionValidationRule_resolves_and_emits_the_enum()
	{
		string code = GenRules(@"<ExceptionValidationRule ValidationStep=""ConvertedProposedValue""/>");
		Assert.Contains("new System.Windows.Controls.ExceptionValidationRule()", code);
		Assert.Contains(".ValidationStep = System.Windows.Controls.ValidationStep.ConvertedProposedValue;", code);
	}

	[Fact]
	public void Builtin_rules_resolve_to_the_Controls_types()
	{
		string code = GenRules("<ExceptionValidationRule/><DataErrorValidationRule/>");
		Assert.Contains("new System.Windows.Controls.ExceptionValidationRule()", code);
		Assert.Contains("new System.Windows.Controls.DataErrorValidationRule()", code);
	}

	[Fact]
	public void Unresolved_rule_type_is_a_build_error()
		=> Assert.Throws<XamlCodeGeneratorException>(() => GenRules("<Nope:R xmlns:Nope=\"urn:nope\"/>"));

	[Fact]
	public void Unhandled_Binding_property_element_is_a_build_error()
		=> Assert.Throws<XamlCodeGeneratorException>(() => Gen(
			@"<TextBox><TextBox.Text><Binding Path=""Age""><Binding.Source><x:Null/></Binding.Source>"
			+ "</Binding></TextBox.Text></TextBox>"));

	[Fact]
	public void Binding_Converter_element_emits_the_converter_assignment()
	{
		string code = Gen(@"<TextBox><TextBox.Text><Binding Path=""Age""><Binding.Converter><local:Conv/>"
			+ "</Binding.Converter></Binding></TextBox.Text></TextBox>");
		Assert.Contains("new MyApp.Conv()", code);
		Assert.Matches(@"\.Converter = \(System\.Windows\.Data\.IValueConverter\)__cv\d+;", code);
	}

	[Fact]
	public void ConverterParameter_XNull_is_null()
		=> Assert.Contains("ConverterParameter = null", GenArg("{Binding Age, ConverterParameter={x:Null}}"));

	[Fact]
	public void ConverterParameter_brace_escape_is_stripped()
		=> Assert.Contains(@"ConverterParameter = ""{0}""", GenArg("{Binding Age, ConverterParameter='{}{0}'}"));

	[Fact]
	public void ConverterParameter_XType_emits_typeof()
		=> Assert.Contains("ConverterParameter = typeof(global::System.Windows.Controls.Button)",
			GenArg("{Binding Age, ConverterParameter={x:Type Button}}"));

	[Fact]
	public void DynamicResource_on_a_binding_argument_is_a_build_error()
		=> Assert.Throws<XamlCodeGeneratorException>(
			() => GenArg("{Binding Age, ConverterParameter={DynamicResource p}}"));

	[Fact]
	public void Unknown_markup_extension_on_a_binding_argument_is_a_build_error()
		=> Assert.Throws<XamlCodeGeneratorException>(
			() => GenArg("{Binding Age, ConverterParameter={x:Reference foo}}"));
}
