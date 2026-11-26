using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Element-syntax Binding under a property element.</summary>
public class XamlCodeGeneratorBindingElementTests
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

	[Fact]
	public void Binding_element_sets_attributes_and_calls_SetBinding()
	{
		string code = Gen(@"<TextBox><TextBox.Text><Binding Path=""Age"" Mode=""TwoWay"" "
			+ @"UpdateSourceTrigger=""PropertyChanged"" ValidatesOnExceptions=""True"" /></TextBox.Text></TextBox>");
		Assert.Contains("new System.Windows.Data.Binding()", code);
		Assert.Contains(@".Path = ""Age"";", code);
		Assert.Contains(".Mode = System.Windows.Data.BindingMode.TwoWay;", code);
		Assert.Contains(".UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.PropertyChanged;", code);
		Assert.Contains(".ValidatesOnExceptions = true;", code);
		Assert.Contains("SetBinding(global::System.Windows.Controls.TextBox.TextProperty, __b", code);
	}

	[Fact]
	public void ValidationRules_children_are_constructed_and_added()
	{
		string code = Gen(@"<TextBox><TextBox.Text><Binding Path=""Age""><Binding.ValidationRules>"
			+ @"<local:AgeRule Min=""0"" Label=""x"" /></Binding.ValidationRules></Binding></TextBox.Text></TextBox>");
		Assert.Contains("new MyApp.AgeRule()", code);
		Assert.Contains(".Min = 0;", code);
		Assert.Contains(@".Label = ""x"";", code);
		Assert.Contains(".ValidationRules.Add(__vr", code);
		Assert.Contains("SetBinding(global::System.Windows.Controls.TextBox.TextProperty", code);
	}

	[Fact]
	public void Binding_element_supports_converter_parameter_and_culture()
	{
		string code = Gen(@"<TextBox><TextBox.Text><Binding Path=""Age"" Converter=""{StaticResource c}"" "
			+ @"ConverterParameter=""p"" ConverterCulture=""fr-FR"" /></TextBox.Text></TextBox>");
		Assert.Contains(@".Converter = (System.Windows.Data.IValueConverter)System.Windows.Markup.StaticResourceScope.Find(""c"", this", code);
		Assert.Contains(@".ConverterParameter = ""p"";", code);
		Assert.Contains(@"GetCultureInfo(""fr-FR"")", code);
	}

	[Fact]
	public void MultiBinding_property_element_still_works()
	{
		string code = Gen(@"<TextBlock><TextBlock.Text><MultiBinding><Binding Path=""A"" /></MultiBinding>"
			+ "</TextBlock.Text></TextBlock>");
		Assert.Contains("new System.Windows.Data.MultiBinding()", code);
	}
}
