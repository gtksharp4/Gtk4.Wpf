using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

public class GeneratorImplicitDataTemplateTests : GeneratorV2TestBase
{
	const string Stub = "namespace A { public partial class W : System.Windows.Window { } "
		+ "public partial class App : System.Windows.Application { } public class Place { } }";

	static string Win(string resources)
		=> $@"<Window x:Class=""A.W"" xmlns:local=""clr-namespace:A"" {Head}><Window.Resources>{resources}</Window.Resources></Window>";

	[Fact]
	public void A_keyed_DataTemplate_keeps_its_string_key()
	{
		string code = Gen(Win(@"<DataTemplate x:Key=""k"" DataType=""{x:Type local:Place}""><TextBlock/></DataTemplate>"));
		Assert.Contains("Resources[\"k\"] = __dt", code);
		Assert.DoesNotContain("DataTemplateKey", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void A_keyless_DataTemplate_with_a_local_type_is_stored_under_its_DataTemplateKey()
	{
		string code = Gen(Win(@"<DataTemplate DataType=""{x:Type local:Place}""><TextBlock Text=""{Binding Name}""/></DataTemplate>"));
		Assert.Contains("Resources[new System.Windows.DataTemplateKey(typeof(", code);
		Assert.Contains(".ItemFactory = ", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void A_keyless_DataTemplate_with_a_framework_type_uses_that_type()
	{
		string code = Gen(Win(@"<DataTemplate DataType=""{x:Type Button}""><TextBlock/></DataTemplate>"));
		Assert.Contains("new System.Windows.DataTemplateKey(typeof(", code);
		Assert.Contains("Button))", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void A_keyless_HierarchicalDataTemplate_is_stored_under_its_DataTemplateKey()
	{
		string code = Gen(Win(@"<HierarchicalDataTemplate DataType=""{x:Type local:Place}"" ItemsSource=""{Binding Kids}""><TextBlock/></HierarchicalDataTemplate>"));
		Assert.Contains("new System.Windows.HierarchicalDataTemplate()", code);
		Assert.Contains("Resources[new System.Windows.DataTemplateKey(typeof(", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void A_keyless_DataTemplate_without_a_DataType_is_not_emitted_under_a_key()
	{
		string code = Gen(Win(@"<DataTemplate><TextBlock/></DataTemplate>"));
		Assert.DoesNotContain("DataTemplateKey", code);
	}

	[Fact]
	public void Application_resources_also_get_the_implicit_key()
	{
		string code = Gen($@"<Application x:Class=""A.App"" xmlns:local=""clr-namespace:A"" {Head}><Application.Resources>
<DataTemplate DataType=""{{x:Type local:Place}}""><TextBlock/></DataTemplate></Application.Resources></Application>");
		Assert.Contains("Resources[new System.Windows.DataTemplateKey(typeof(", code);
		AssertCompiles(code, Stub);
	}
}

public class GeneratorImplicitStyleTests : GeneratorV2TestBase
{
	static string Win(string resources)
		=> $@"<Window x:Class=""A.W"" {Head}><Window.Resources>{resources}</Window.Resources></Window>";

	const string DataTriggers = @"<Style TargetType=""ListBoxItem""><Style.Triggers>
<DataTrigger Binding=""{Binding Path=State}"" Value=""WA""><Setter Property=""Foreground"" Value=""Red""/></DataTrigger>
<MultiDataTrigger><MultiDataTrigger.Conditions>
<Condition Binding=""{Binding Path=Name}"" Value=""Portland""/><Condition Binding=""{Binding Path=State}"" Value=""OR""/>
</MultiDataTrigger.Conditions><Setter Property=""Background"" Value=""Cyan""/></MultiDataTrigger></Style.Triggers></Style>";

	[Fact]
	public void A_keyless_ListBoxItem_style_is_registered_under_its_type_for_generated_containers()
	{
		string code = Gen(Win(DataTriggers));
		Assert.Contains("Resources[typeof(global::System.Windows.Controls.ListBoxItem)] = __style", code);
	}

	[Fact]
	public void A_MultiDataTrigger_in_Style_Triggers_is_emitted_with_binding_conditions()
	{
		string code = Gen(Win(DataTriggers));
		Assert.Contains("new System.Windows.MultiDataTrigger()", code);
		Assert.Contains("Conditions.Add(new System.Windows.Condition { Binding = new System.Windows.Data.Binding(\"Name\")", code);
		Assert.Contains("Value = \"OR\"", code);
		Assert.Contains("PropertyName = \"Background\"", code);
		AssertCompiles(code, "namespace A { public partial class W : System.Windows.Window { } }");
	}
}
