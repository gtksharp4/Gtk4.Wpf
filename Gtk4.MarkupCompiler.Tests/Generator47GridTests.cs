using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class Generator47GridTests : GeneratorV2TestBase
{
	const string PageStub = "namespace A { public partial class P : System.Windows.Controls.Page { } }";

	static string Page(string body)
		=> $@"<Page x:Class=""A.P"" {Head}>{body}</Page>";

	[Fact]
	public void V1_RowDefinitions_are_added_with_converted_heights_and_limits()
	{
		string code = Gen(Page(@"<Grid><Grid.RowDefinitions>
<RowDefinition Height=""Auto"" MinHeight=""5"" MaxHeight=""50""/>
<RowDefinition Height=""2*"" SharedSizeGroup=""g""/>
<RowDefinition Height=""100""/><RowDefinition/>
</Grid.RowDefinitions><Button/></Grid>"));
		Assert.Contains("new System.Windows.Controls.RowDefinition()", code);
		Assert.Equal(4, System.Text.RegularExpressions.Regex.Matches(code, @"\.RowDefinitions\.Add\(").Count);
		Assert.Contains("ConvertByType(typeof(global::System.Windows.GridLength), \"Auto\")", code);
		Assert.Contains("\"2*\"", code);
		Assert.Contains(".SharedSizeGroup = \"g\"", code);
		Assert.Contains(".MinHeight = ", code);
		AssertCompiles(code, PageStub);
	}

	[Fact]
	public void V1_ColumnDefinitions_are_added()
	{
		string code = Gen(Page(@"<Grid><Grid.ColumnDefinitions>
<ColumnDefinition Width=""*"" MinWidth=""3"" MaxWidth=""9""/><ColumnDefinition Width=""Auto""/>
</Grid.ColumnDefinitions></Grid>"));
		Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(code, @"\.ColumnDefinitions\.Add\(").Count);
		Assert.Contains(".MaxWidth = ", code);
		AssertCompiles(code, PageStub);
	}

	[Fact]
	public void V1_definitions_after_the_children_are_still_added()
	{
		string code = Gen(Page(@"<Grid><Button Grid.Row=""1""/><Grid.RowDefinitions><RowDefinition/>
<RowDefinition/></Grid.RowDefinitions></Grid>"));
		Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(code, @"\.RowDefinitions\.Add\(").Count);
		AssertCompiles(code, PageStub);
	}

	[Fact]
	public void V2_an_unsupported_property_element_is_reported_with_its_line()
	{
		var msgs = Messages(Page("<Grid>\n<Grid.Bogus><Button/></Grid.Bogus></Grid>"));
		Assert.Contains(msgs, m => m.Contains("Grid.Bogus") && m.Contains("line 2"));
	}

	[Fact]
	public void V2_supported_property_elements_are_not_reported()
	{
		string xaml = Page(@"<Grid><Grid.RowDefinitions><RowDefinition/></Grid.RowDefinitions>
<Grid.Resources><Style x:Key=""s"" TargetType=""Button""/></Grid.Resources>
<Button><Button.Template><ControlTemplate TargetType=""Button""><Border/></ControlTemplate></Button.Template></Button></Grid>");
		Assert.Empty(Messages(xaml));
	}
}
