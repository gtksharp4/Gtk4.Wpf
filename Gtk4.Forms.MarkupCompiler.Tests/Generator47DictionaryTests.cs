using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

public class Generator47DictionaryTests : GeneratorV2TestBase
{
	const string AppStub = "namespace A { public partial class App : System.Windows.Application { } }";

	const string Merged = $@"<Application x:Class=""A.App"" {Head}><Application.Resources><ResourceDictionary>
<ResourceDictionary.MergedDictionaries><ResourceDictionary Source=""Strings.xaml""/></ResourceDictionary.MergedDictionaries>
</ResourceDictionary></Application.Resources></Application>";

	const string Dict = $@"<ResourceDictionary {Head} xmlns:system=""clr-namespace:System;assembly=mscorlib"">
<system:String x:Key=""Msg"">Hello</system:String>
<Style x:Key=""s"" TargetType=""Button""><Setter Property=""Width"" Value=""5""/></Style>
<Style x:Key=""t"" TargetType=""Button"" BasedOn=""{{StaticResource s}}""/>
</ResourceDictionary>";

	[Fact]
	public void V4_merged_dictionary_with_Source_is_loaded_when_the_markup_runs()
	{
		string code = Gen(Merged);
		Assert.Contains("GtkFormsXamlRegistry.TryLoadDictionary(__d, \"Strings.xaml\"", code);
		Assert.Contains("this.Resources.MergedDictionaries.Add(__d)", code);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void V4_a_dictionary_file_compiles_to_a_registered_ResourceDictionary_class()
	{
		string code = Gen(Dict, out string cls, hasCodeBehind: false, xamlPath: "Strings.xaml", rootNamespace: "A");
		Assert.Equal("A.Strings", cls);
		Assert.Contains("GtkFormsXamlRegistration(\"Strings.xaml\", typeof(global::A.Strings))", code);
		Assert.Contains("partial class Strings : global::System.Windows.ResourceDictionary", code);
		Assert.Contains("__owner.Resources[\"Msg\"] = \"Hello\"", code);
		Assert.Contains("this.CopyEntriesFrom(__owner.Resources)", code);
		AssertCompiles(code);
	}

	[Fact]
	public void V4_a_dictionary_file_without_a_path_is_still_skipped()
	{
		Assert.Null(Gen(Dict, out _, hasCodeBehind: false));
	}

	[Fact]
	public void V4_a_Source_on_the_Resources_dictionary_itself_is_loaded()
	{
		string code = Gen($@"<Application x:Class=""A.App"" {Head}><Application.Resources>
<ResourceDictionary Source=""Strings.xaml""/></Application.Resources></Application>");
		Assert.Contains("TryLoadDictionary(this.Resources, \"Strings.xaml\"", code);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void V4_merged_and_dictionary_file_emit_no_messages()
	{
		Assert.Empty(Messages(Merged));
	}
}
