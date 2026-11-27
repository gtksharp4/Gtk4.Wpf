using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Compile-checked corpus of the sweep V "Skipped property element" backlog shapes (from the samples).</summary>
public class GeneratorSweep5CorpusTests : Sweep3Base
{
	static readonly Dictionary<string, string> Snippets = new()
	{
		["DataGridColumns"] =
			"<DataGrid AutoGenerateColumns=\"False\"><DataGrid.Columns>"
			+ "<DataGridTextColumn Header=\"Name\" Binding=\"{Binding Name}\" Width=\"100\"/>"
			+ "<DataGridTextColumn Header=\"Cost\"><DataGridTextColumn.Binding><Binding Path=\"Cost\""
			+ " UpdateSourceTrigger=\"PropertyChanged\"/></DataGridTextColumn.Binding></DataGridTextColumn>"
			+ "</DataGrid.Columns></DataGrid>",
		["ListBoxItemsSource"] =
			"<ListBox><ListBox.ItemsSource><Binding Path=\"Items\"/></ListBox.ItemsSource></ListBox>",
		["TextBlockText"] = "<TextBlock><TextBlock.Text><Binding Path=\"Name\"/></TextBlock.Text></TextBlock>",
		["PathData"] =
			"<Path Stroke=\"Red\"><Path.Data><PathGeometry><PathFigure StartPoint=\"0,0\"><LineSegment Point=\"5,5\"/>"
			+ "</PathFigure></PathGeometry></Path.Data></Path>",
		["PathDataCombined"] =
			"<Path Stroke=\"Red\"><Path.Data><CombinedGeometry GeometryCombineMode=\"Xor\"><CombinedGeometry.Geometry1>"
			+ "<EllipseGeometry Center=\"50,50\" RadiusX=\"50\" RadiusY=\"50\"/></CombinedGeometry.Geometry1>"
			+ "<CombinedGeometry.Geometry2><RectangleGeometry Rect=\"0,0,40,40\"/></CombinedGeometry.Geometry2>"
			+ "</CombinedGeometry></Path.Data></Path>",
		["RenderTransform"] =
			"<Button><Button.RenderTransform><RotateTransform Angle=\"5\"/></Button.RenderTransform></Button>",
		["RenderTransformGroup"] =
			"<Button><Button.RenderTransform><TransformGroup><ScaleTransform ScaleX=\"2\"/>"
			+ "<RotateTransform Angle=\"5\"/></TransformGroup></Button.RenderTransform></Button>",
		["InputBindings"] =
			"<Grid><Grid.InputBindings><KeyBinding Key=\"F5\" Command=\"ApplicationCommands.Open\"/>"
			+ "<MouseBinding MouseAction=\"LeftDoubleClick\" Command=\"ApplicationCommands.Open\"/></Grid.InputBindings></Grid>",
		["ItemsPanel"] =
			"<ListBox><ListBox.ItemsPanel><ItemsPanelTemplate><WrapPanel/></ItemsPanelTemplate></ListBox.ItemsPanel></ListBox>",
		["NamedBeginStoryboard"] =
			"<Button x:Name=\"B\"><Button.Triggers><EventTrigger RoutedEvent=\"Button.Click\"><BeginStoryboard"
			+ " x:Name=\"BS\"><Storyboard><DoubleAnimation Storyboard.TargetName=\"B\""
			+ " Storyboard.TargetProperty=\"Opacity\" To=\"0\" Duration=\"0:0:1\"/></Storyboard></BeginStoryboard>"
			+ "</EventTrigger></Button.Triggers></Button>",
	};

	public static IEnumerable<object[]> Names() => Snippets.Keys.Select(k => new object[] { k });

	static string Doc(string name) => "<Window " + Head + " x:Class=\"App.C\">" + Snippets[name] + "</Window>";

	[Theory]
	[MemberData(nameof(Names))]
	public void Generated_code_compiles(string name) => Compile(Doc(name));

	[Theory]
	[MemberData(nameof(Names))]
	public void Nothing_is_skipped(string name)
		=> Assert.DoesNotContain(Messages(Doc(name)), m => m.StartsWith("Skipped", System.StringComparison.Ordinal));

	// The layer has no GridView, GroupStyle or CompositeCollection yet: these stay reported, not guessed.
	[Theory]
	[InlineData("<ListView><ListView.View><GridView><GridViewColumn Header=\"N\"/></GridView></ListView.View></ListView>",
		"ListView.View")]
	[InlineData("<ListBox><ListBox.GroupStyle><GroupStyle/></ListBox.GroupStyle></ListBox>", "ListBox.GroupStyle")]
	[InlineData("<ListBox><ListBox.ItemsSource><CompositeCollection><ListBoxItem>a</ListBoxItem>"
		+ "</CompositeCollection></ListBox.ItemsSource></ListBox>", "ListBox.ItemsSource")]
	public void Types_the_layer_lacks_are_still_reported(string body, string element)
		=> Assert.Contains(Messages("<Window " + Head + " x:Class=\"App.C\">" + body + "</Window>"),
			m => m.StartsWith("Skipped property element " + element, System.StringComparison.Ordinal));
}
