using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Compile-checked corpus of the "Skipped property element" backlog shapes (taken from the real samples):
/// each snippet must compile and produce no "Skipped ..." message.
/// </summary>
public class GeneratorSweep4CorpusTests : Sweep3Base
{
	const string TextEffects =
		"<StackPanel><TextBlock Name=\"RealText\" Foreground=\"White\">Windows Vista"
		+ "<TextBlock.TextEffects><TextEffect PositionCount=\"1\" x:Name=\"MyTextEffect\"><TextEffect.Transform>"
		+ "<RotateTransform x:Name=\"TextEffectRotateTransform\" Angle=\"0\" CenterX=\"10\" CenterY=\"10\"/>"
		+ "</TextEffect.Transform></TextEffect></TextBlock.TextEffects></TextBlock></StackPanel>";

	static readonly Dictionary<string, string> Snippets = new()
	{
		["TextEffects"] = TextEffects,
		["ItemContainerStyle"] =
			"<ListBox><ListBox.ItemContainerStyle><Style><Setter Property=\"Control.Width\" Value=\"100\"/>"
			+ "<Setter Property=\"Control.Margin\" Value=\"5\"/></Style></ListBox.ItemContainerStyle></ListBox>",
		["ItemContainerStyleTargetType"] =
			"<ItemsControl><ItemsControl.ItemContainerStyle><Style TargetType=\"{x:Type ContentPresenter}\">"
			+ "<Setter Property=\"Width\" Value=\"1\"/></Style></ItemsControl.ItemContainerStyle></ItemsControl>",
		["ElementStyleProperty"] =
			"<TextBlock><TextBlock.Style><Style TargetType=\"TextBlock\"><Setter Property=\"Margin\" Value=\"2\"/>"
			+ "</Style></TextBlock.Style></TextBlock>",
		["OpacityMaskRadial"] =
			"<Rectangle Width=\"3\"><Rectangle.OpacityMask><RadialGradientBrush><RadialGradientBrush.GradientStops>"
			+ "<GradientStop Offset=\"0.0\" Color=\"#FF000000\"/><GradientStop Offset=\"1.0\" Color=\"#00000000\"/>"
			+ "</RadialGradientBrush.GradientStops></RadialGradientBrush></Rectangle.OpacityMask></Rectangle>",
		["OpacityMaskImageBrush"] =
			"<Image><Image.OpacityMask><ImageBrush ImageSource=\"a.png\"/></Image.OpacityMask></Image>",
		["OpacityMaskDrawingBrush"] =
			"<Image><Image.OpacityMask><DrawingBrush><DrawingBrush.Drawing><GeometryDrawing>"
			+ "<GeometryDrawing.Brush><RadialGradientBrush><RadialGradientBrush.GradientStops>"
			+ "<GradientStop Offset=\"0\" Color=\"Black\"/></RadialGradientBrush.GradientStops>"
			+ "</RadialGradientBrush></GeometryDrawing.Brush><GeometryDrawing.Geometry>"
			+ "<RectangleGeometry Rect=\"0.05,0.05 0.9,0.9\"/></GeometryDrawing.Geometry></GeometryDrawing>"
			+ "</DrawingBrush.Drawing></DrawingBrush></Image.OpacityMask></Image>",
		["OpacityMaskCanvasButton"] =
			"<Canvas><Canvas.OpacityMask><LinearGradientBrush><GradientStop Offset=\"0\" Color=\"Red\"/>"
			+ "</LinearGradientBrush></Canvas.OpacityMask><Button><Button.OpacityMask><SolidColorBrush Color=\"Red\"/>"
			+ "</Button.OpacityMask></Button></Canvas>",
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

	[Fact]
	public void TextEffect_names_are_registered_in_the_window_scope()
	{
		string code = Compile(Doc("TextEffects"));

		Has(code, "RegisterName(\"MyTextEffect\"");
		Has(code, "RegisterName(\"TextEffectRotateTransform\"");
		Has(code, "TextEffects.Add(");
	}

	const string CmdStub = "namespace App { public partial class C { public static readonly System.Windows.Input."
		+ "RoutedCommand Custom = new System.Windows.Input.RoutedCommand();"
		+ " void Ex(object s, System.Windows.Input.ExecutedRoutedEventArgs e) { }"
		+ " void Can(object s, System.Windows.Input.CanExecuteRoutedEventArgs e) { } } }";

	[Theory]
	[InlineData("Command=\"ApplicationCommands.Open\"", "ConvertByType(typeof(System.Windows.Input.ICommand)")]
	[InlineData("Command=\"{x:Static ApplicationCommands.Open}\"", "ApplicationCommands.Open")]
	[InlineData("Command=\"{x:Static local:C.Custom}\"", "C.Custom")]
	[InlineData("Command=\"local:C.Custom\"", "C.Custom")]
	public void Window_CommandBindings_wire_handlers_to_the_code_behind(string command, string expected)
	{
		string xaml = "<Window " + Head + " xmlns:local=\"clr-namespace:App\" x:Class=\"App.C\">"
			+ "<Window.CommandBindings><CommandBinding " + command + " Executed=\"Ex\" CanExecute=\"Can\"/>"
			+ "</Window.CommandBindings></Window>";

		string code = Compile(xaml, CmdStub);

		Has(code, expected);
		Has(code, ".Executed += this.Ex;");
		Has(code, ".CanExecute += this.Can;");
		Has(code, "this.CommandBindings.Add(");
		Assert.DoesNotContain(Messages(xaml), m => m.StartsWith("Skipped", System.StringComparison.Ordinal));
	}

	[Fact]
	public void Element_CommandBindings_are_added_to_that_element()
	{
		string code = Compile("<Window " + Head + " x:Class=\"App.C\"><StackPanel><StackPanel.CommandBindings>"
			+ "<CommandBinding Command=\"ApplicationCommands.Open\" Executed=\"Ex\"/></StackPanel.CommandBindings>"
			+ "</StackPanel></Window>", CmdStub);

		Has(code, ".CommandBindings.Add(");
		Lacks(code, "this.CommandBindings.Add(");
	}


	[Fact]
	public void Window_DataContext_takes_a_project_type_object()
	{
		string xaml = "<Window " + Head + " xmlns:local=\"clr-namespace:App\" x:Class=\"App.C\">"
			+ "<Window.DataContext><local:VM/></Window.DataContext></Window>";

		string code = Compile(xaml, "namespace App { public partial class C { } public class VM { } }");

		Has(code, "new global::App.VM()");
		Has(code, "DataContext");
	}

	[Theory]
	[InlineData("<CroppedBitmap SourceRect=\"1,2,3,4\"/>", "CroppedBitmap.SourceRectProperty, \"1,2,3,4\"")]
	[InlineData("<FormatConvertedBitmap DestinationFormat=\"Bgra32\"/>",
		"FormatConvertedBitmap.DestinationFormatProperty, \"Bgra32\"")]
	public void Imaging_values_go_through_the_runtime_converter(string element, string expected)
	{
		string code = Compile("<Window " + Head + " x:Class=\"App.C\"><Image><Image.Source>" + element
			+ "</Image.Source></Image></Window>");

		Has(code, expected);
	}
}
