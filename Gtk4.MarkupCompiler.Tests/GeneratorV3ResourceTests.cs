using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class GeneratorV3ResourceTests : GeneratorV2TestBase
{
	const string AppStub = "namespace A { public partial class App : System.Windows.Application { } }";

	static string App(string resources)
		=> $@"<Application x:Class=""A.App"" {Head}><Application.Resources>{resources}</Application.Resources></Application>";

	const string Grid = @"<DrawingBrush x:Key=""g"" Viewport=""0,0,10,10"" ViewportUnits=""Absolute"" TileMode=""Tile"">
<DrawingBrush.Drawing><DrawingGroup><DrawingGroup.Children>
<GeometryDrawing Brush=""White""><GeometryDrawing.Geometry><RectangleGeometry Rect=""0 0 1 1""/>
</GeometryDrawing.Geometry></GeometryDrawing>
</DrawingGroup.Children></DrawingGroup></DrawingBrush.Drawing></DrawingBrush>";

	[Fact]
	public void G1_DrawingBrush_resource_is_instantiated_with_attributes_and_property_elements()
	{
		string code = Gen(App(Grid));
		Assert.DoesNotContain("Resources[\"g\"] = \"\"", code);
		Assert.Contains("new System.Windows.Media.DrawingBrush()", code);
		Assert.Contains("new System.Windows.Media.DrawingGroup()", code);
		Assert.Contains("new System.Windows.Media.GeometryDrawing()", code);
		Assert.Contains(".Children.Add(", code);
		Assert.Contains(".Drawing = ", code);
		Assert.Contains("Resources[\"g\"] = __e", code);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void G1_DrawingGroup_content_children_and_path_string_geometry()
	{
		string code = Gen(App(@"<DrawingBrush x:Key=""g""><DrawingBrush.Drawing><DrawingGroup>
<GeometryDrawing Geometry=""M0,0 L1,1"" Brush=""#44CCCCFF""/></DrawingGroup></DrawingBrush.Drawing></DrawingBrush>"));
		Assert.Contains("GeometryProperty, System.Windows.Markup.XamlValueConverter.Convert(typeof(System.Windows.Media" +
			".GeometryDrawing), System.Windows.Media.GeometryDrawing.GeometryProperty, \"M0,0 L1,1\")", code);
		Assert.Contains("DrawingGroup", code);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void G1_LinearGradientBrush_resource_gets_its_GradientStops()
	{
		string code = Gen(App(@"<LinearGradientBrush x:Key=""l"" StartPoint=""0,0"" EndPoint=""1,1"">
<GradientStop Color=""WhiteSmoke"" Offset=""0.2""/><GradientStop Color=""Transparent"" Offset=""1""/>
</LinearGradientBrush>"));
		Assert.Contains("new System.Windows.Media.LinearGradientBrush()", code);
		Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(code, @"\.GradientStops\.Add\(").Count);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void G1_a_nested_brush_in_a_property_element_is_attached()
	{
		string code = Gen(App(@"<DrawingBrush x:Key=""g""><DrawingBrush.Drawing><DrawingGroup>
<GeometryDrawing Geometry=""M0,0 L1,1""><GeometryDrawing.Brush>
<LinearGradientBrush StartPoint=""0,0.5"" EndPoint=""1,0.5""><GradientStop Offset=""0.0"" Color=""#9999FF""/>
</LinearGradientBrush></GeometryDrawing.Brush></GeometryDrawing></DrawingGroup></DrawingBrush.Drawing></DrawingBrush>"));
		Assert.Contains(".Brush = __e", code);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void G1_a_type_the_layer_lacks_is_skipped_not_an_error()
	{
		string code = Gen(App(@"<DrawingBrush x:Key=""g""><DrawingBrush.Drawing><DrawingGroup>
<GeometryDrawing Brush=""Red""><GeometryDrawing.Geometry><NoSuchGeometry/></GeometryDrawing.Geometry>
</GeometryDrawing></DrawingGroup></DrawingBrush.Drawing></DrawingBrush>"));
		Assert.DoesNotContain("NoSuchGeometry", code);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void G1_presentationOptions_Freeze_is_ignored()
	{
		string code = Gen($@"<Application x:Class=""A.App"" {Head}
xmlns:po=""http://schemas.microsoft.com/winfx/2006/xaml/presentation/options""><Application.Resources>
<LinearGradientBrush x:Key=""l"" po:Freeze=""True""><GradientStop Color=""Red"" Offset=""0""/>
</LinearGradientBrush></Application.Resources></Application>");
		Assert.DoesNotContain("Freeze", code);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void G1_an_attribute_with_no_safe_conversion_gets_no_entry()
	{
		string code = Gen(App(@"<DrawingBrush x:Key=""g"" Stretch=""{StaticResource t}""/>"));
		Assert.DoesNotContain("Resources[\"g\"]", code);
		AssertCompiles(code, AppStub);
	}

	[Fact]
	public void G1_text_value_of_a_converter_backed_struct_goes_through_the_converter()
	{
		string code = Gen(App(@"<Color x:Key=""c"">Red</Color>"));
		Assert.Contains("ConvertByType(typeof(global::System.Windows.Media.Color), \"Red\")", code);
	}
}
