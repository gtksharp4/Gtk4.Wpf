using System.Linq;
using System.Text.RegularExpressions;
using Gtk4.Forms.MarkupCompiler;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

public class XamlCodeGeneratorTriggerReceiverTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body) =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
{body}
</Window>";

	string Gen(string xaml) => _gen.Generate(xaml, out _);

	[Fact]
	public void Style_trigger_StaticResource_resolves_on_the_owning_element()
	{
		string code = Gen(Window(@"<Grid><Grid.Resources><SolidColorBrush x:Key=""B""/>
<Style TargetType=""Button""><Style.Triggers><Trigger Property=""IsMouseOver"" Value=""True"">
<Setter Property=""Background"" Value=""{StaticResource B}""/></Trigger></Style.Triggers></Style>
</Grid.Resources></Grid>"));

		Assert.Matches(@"Value = \(__e\d+\.TryFindResource\(""B""\) \?\? this\.FindResource\(""B""\)\)", code);
	}

	[Fact]
	public void Outer_scope_key_falls_back_to_this_for_unparented_style_and_element_triggers()
	{
		// Elements are emitted before they are attached, so their own chain cannot see Window.Resources yet.
		string code = Gen(Window(@"<Window.Resources><SolidColorBrush x:Key=""W""/></Window.Resources><Grid>
<Grid.Resources><Style TargetType=""Button""><Style.Triggers><Trigger Property=""IsMouseOver"" Value=""True"">
<Setter Property=""Background"" Value=""{StaticResource W}""/></Trigger></Style.Triggers></Style></Grid.Resources>
<Button><Button.Triggers><Trigger Property=""IsMouseOver"" Value=""True"">
<Setter Property=""Background"" Value=""{StaticResource W}""/></Trigger></Button.Triggers></Button></Grid>"));

		Assert.Equal(2, Regex.Matches(code, @"Value = \(__e\d+\.TryFindResource\(""W""\) \?\? this\.FindResource\(""W""\)\)").Count);
	}

	[Fact]
	public void Nested_Setter_Value_emissions_use_distinct_local_names()
	{
		string code = Gen(Window(@"<Window.Resources><Style TargetType=""Button""><Setter Property=""Template""><Setter.Value>
<ControlTemplate TargetType=""Button""><Border><Border.Resources><Style TargetType=""Border"">
<Setter Property=""Template""><Setter.Value><ControlTemplate TargetType=""Button""><Border/></ControlTemplate></Setter.Value></Setter>
</Style></Border.Resources></Border></ControlTemplate></Setter.Value></Setter></Style></Window.Resources>"));

		var names = Regex.Matches(code, @"var (__sv\d+|__e\d+) ").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
		Assert.True(names.Count >= 4, code);
		Assert.True(names.Count == names.Distinct().Count(), code);
	}
}
