using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

public class Generator47ResourceTests : GeneratorV2TestBase
{
	const string Stub = "namespace A { public partial class W : System.Windows.Window { } "
		+ "public class Data : System.Collections.ArrayList { } }";

	static string Win(string resources, string body = "")
		=> $@"<Window x:Class=""A.W"" xmlns:local=""clr-namespace:A"" {Head}><Window.Resources>{resources}</Window.Resources>{body}</Window>";

	[Fact]
	public void V3_HierarchicalDataTemplate_resource_is_an_object_with_items_binding_and_child_template()
	{
		string code = Gen(Win(@"<DataTemplate x:Key=""L3""><TextBlock Text=""{Binding Name}""/></DataTemplate>
<HierarchicalDataTemplate x:Key=""L2"" ItemsSource=""{Binding Path=Teams}"" ItemTemplate=""{StaticResource L3}"">
<TextBlock Text=""{Binding Path=Name}""/></HierarchicalDataTemplate>"));
		Assert.DoesNotContain("Resources[\"L2\"] = \"\"", code);
		Assert.Contains("new System.Windows.HierarchicalDataTemplate()", code);
		Assert.Contains(".ItemsSource = ", code);
		Assert.Contains(".ItemTemplate = (System.Windows.Controls.DataTemplate)", code);
		Assert.Contains(".ItemFactory = ", code);
		Assert.Contains("Resources.SetDeferred(\"L2\", () =>", code);
		Assert.Contains("return __dt", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void V3_HierarchicalDataTemplate_keeps_DataType()
	{
		string code = Gen(Win(@"<HierarchicalDataTemplate x:Key=""h"" DataType=""{x:Type local:Data}""
 ItemsSource=""{Binding}""><TextBlock/></HierarchicalDataTemplate>"));
		Assert.Contains(".DataType = ", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void V3_HierarchicalDataTemplate_as_an_ItemTemplate_property_element()
	{
		string code = Gen(Win("", @"<TreeView><TreeView.ItemTemplate><HierarchicalDataTemplate ItemsSource=""{Binding Kids}"">
<TextBlock/></HierarchicalDataTemplate></TreeView.ItemTemplate></TreeView>"));
		Assert.Contains("new System.Windows.HierarchicalDataTemplate()", code);
		Assert.Contains(".ItemTemplate = __dt", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void V3_CollectionViewSource_resource_gets_source_and_group_descriptions()
	{
		string code = Gen(Win(@"<local:Data x:Key=""d""/>
<CollectionViewSource x:Key=""g"" Source=""{StaticResource d}"">
<CollectionViewSource.GroupDescriptions><PropertyGroupDescription PropertyName=""State""/>
</CollectionViewSource.GroupDescriptions></CollectionViewSource>"));
		Assert.DoesNotContain("Resources[\"g\"] = \"\"", code);
		Assert.Contains("new System.Windows.Data.CollectionViewSource()", code);
		Assert.Contains(".GroupDescriptions.Add(", code);
		Assert.Contains(".PropertyName = \"State\"", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void V3_AlternationConverter_resource_collects_converted_values()
	{
		string code = Gen(Win(@"<AlternationConverter x:Key=""b""><SolidColorBrush>Silver</SolidColorBrush>
<SolidColorBrush>LightGray</SolidColorBrush></AlternationConverter>
<AlternationConverter x:Key=""f""><FontStyle>Italic</FontStyle><FontStyle>Normal</FontStyle></AlternationConverter>"));
		Assert.Contains("new System.Windows.Controls.AlternationConverter()", code);
		Assert.Equal(4, System.Text.RegularExpressions.Regex.Matches(code, @"\.Values\.Add\(").Count);
		Assert.DoesNotContain("Resources[\"b\"] = \"\"", code);
		AssertCompiles(code, Stub);
	}

	[Fact]
	public void V3_no_message_for_the_handled_resource_property_elements()
	{
		Assert.Empty(Messages(Win(@"<CollectionViewSource x:Key=""g"">
<CollectionViewSource.GroupDescriptions><PropertyGroupDescription PropertyName=""S""/>
</CollectionViewSource.GroupDescriptions></CollectionViewSource>")));
	}
}
