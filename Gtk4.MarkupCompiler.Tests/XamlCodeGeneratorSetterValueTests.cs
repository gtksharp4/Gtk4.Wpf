using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class XamlCodeGeneratorSetterValueTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	<Window.Resources>
{body}
	</Window.Resources>
</Window>";

	static string Style(string children) =>
		Window($@"<Style x:Key=""s"" TargetType=""Button"">{children}</Style>");

	string Gen(string xaml) => _gen.Generate(xaml, out _);

	[Fact]
	public void Plain_string_setter_is_unchanged()
	{
		string code = Gen(Style(@"<Setter Property=""Background"" Value=""Red""/>"));
		Assert.Contains(@"PropertyName = ""Background"", Value = ""Red""", code);
	}

	[Fact]
	public void Missing_value_is_null()
	{
		string code = Gen(Style(@"<Setter Property=""Background""/>"));
		Assert.Contains(@"PropertyName = ""Background"", Value = null", code);
	}

	[Fact]
	public void Style_setter_static_resource_uses_FindResource_on_owner()
	{
		string code = Gen(Style(@"<Setter Property=""Background"" Value=""{StaticResource brush}""/>"));
		Assert.Contains(@"Value = this.FindResource(""brush"")", code);
	}

	[Fact]
	public void Style_setter_dynamic_resource_is_extension()
	{
		string code = Gen(Style(@"<Setter Property=""Background"" Value=""{DynamicResource brush}""/>"));
		Assert.Contains(@"Value = new System.Windows.Markup.DynamicResourceExtension(""brush"")", code);
	}

	[Fact]
	public void Style_setter_x_null_is_null()
	{
		string code = Gen(Style(@"<Setter Property=""Tag"" Value=""{x:Null}""/>"));
		Assert.Contains(@"PropertyName = ""Tag"", Value = null", code);
	}

	[Fact]
	public void Style_setter_x_Static_is_static_member()
	{
		string code = Gen(Style(@"<Setter Property=""Width"" Value=""{x:Static Button.ClickEvent}""/>"));
		Assert.Contains("Value = global::System.Windows.Controls.Button.ClickEvent", code);
	}

	[Fact]
	public void Style_setter_binding_is_Binding_object()
	{
		string code = Gen(Style(@"<Setter Property=""Tag"" Value=""{Binding Name}""/>"));
		Assert.Contains("Value = new System.Windows.Data.Binding", code);
	}

	[Fact]
	public void Trigger_setter_markup_values()
	{
		string code = Gen(Style(@"<Style.Triggers><Trigger Property=""IsEnabled"" Value=""False"">
<Setter Property=""Background"" Value=""{DynamicResource d}""/>
<Setter Property=""Foreground"" Value=""{StaticResource s}""/></Trigger></Style.Triggers>"));
		Assert.Contains(@"DynamicResourceExtension(""d"")", code);
		Assert.Contains(@"Value = this.FindResource(""s"")", code);
	}

	[Fact]
	public void DataTrigger_setter_markup_values()
	{
		string code = Gen(Style(@"<Style.Triggers><DataTrigger Binding=""{Binding A}"" Value=""1"">
<Setter Property=""Tag"" Value=""{Binding B}""/>
<Setter Property=""Foreground"" Value=""{x:Null}""/></DataTrigger></Style.Triggers>"));
		Assert.Contains("PropertyName = \"Tag\", Value = new System.Windows.Data.Binding", code);
		Assert.Contains(@"PropertyName = ""Foreground"", Value = null", code);
	}

	[Fact]
	public void MultiTrigger_setter_markup_values()
	{
		string code = Gen(Style(@"<Style.Triggers><MultiTrigger><MultiTrigger.Conditions>
<Condition Property=""IsEnabled"" Value=""True""/></MultiTrigger.Conditions>
<Setter Property=""Background"" Value=""{DynamicResource d}""/></MultiTrigger></Style.Triggers>"));
		Assert.Contains(@"DynamicResourceExtension(""d"")", code);
	}

	[Fact]
	public void TemplateBinding_setter_uses_template_binding_path()
	{
		string code = Gen(Style(@"<Style.Triggers><Trigger Property=""IsEnabled"" Value=""False"">
<Setter Property=""Tag"" Value=""{TemplateBinding Width}""/></Trigger></Style.Triggers>"));
		Assert.DoesNotContain(@"Value = ""{TemplateBinding", code);
	}
}
