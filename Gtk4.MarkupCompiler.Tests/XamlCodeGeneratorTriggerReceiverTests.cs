using System.Linq;
using System.Text.RegularExpressions;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

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

		Assert.Matches(@"Value = System\.Windows\.Markup\.StaticResourceScope\.Find\(""B"", this, __e\d+\)", code);
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

		Assert.Equal(2, Regex.Matches(code,
			@"Value = System\.Windows\.Markup\.StaticResourceScope\.Find\(""W"", this, __e\d+(, __e\d+)*\)").Count);
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

	[Fact]
	public void Nested_style_setter_probes_every_enclosing_scope_innermost_first()
	{
		string code = Gen(Window(@"<Grid><Grid.Resources><SolidColorBrush x:Key=""K""/></Grid.Resources>
<Border><Border.Resources><Style TargetType=""Border""><Setter Property=""Background"" Value=""{StaticResource K}""/>
</Style></Border.Resources></Border></Grid>"));

		var m = Regex.Match(code, @"StaticResourceScope\.Find\(""K"", this, (__e\d+), (__e\d+)\)");
		Assert.True(m.Success, code);
		Assert.Contains($"var {m.Groups[1].Value} = new System.Windows.Controls.Border", code);
		Assert.Contains($"var {m.Groups[2].Value} = new System.Windows.Controls.Grid", code);
	}
}
