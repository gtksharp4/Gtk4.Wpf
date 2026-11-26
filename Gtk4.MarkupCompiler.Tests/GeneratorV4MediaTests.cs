using System.Linq;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

public class GeneratorV4MediaTests : GeneratorV2TestBase
{
	const string AppStub = "namespace A { public partial class App : System.Windows.Application { } }";

	static string Win(string resources)
		=> $@"<Window x:Class=""A.W"" {Head}><Window.Resources>{resources}</Window.Resources></Window>";

	const string WinStub = "namespace A { public partial class W : System.Windows.Window { } }";

	[Fact]
	public void F3_a_GradientStopCollection_resource_is_assigned_by_lookup()
	{
		string code = Gen(Win(@"<GradientStopCollection x:Key=""s""><GradientStop Color=""Red"" Offset=""0""/>
</GradientStopCollection><LinearGradientBrush x:Key=""b"" GradientStops=""{StaticResource s}""/>"));
		Assert.Contains("new System.Windows.Media.GradientStopCollection()", code.Replace("global::", ""));
		Assert.Contains(".GradientStops = ", code);
		Assert.Contains("as global::System.Windows.Media.GradientStopCollection;", code);
		Assert.DoesNotContain("Resources[\"b\"] = \"\"", code);
		AssertCompiles(code, WinStub);
	}

	[Fact]
	public void F3_a_DrawingImage_resource_with_a_brush_reference_is_an_object()
	{
		string code = Gen(Win(@"<SolidColorBrush x:Key=""b"" Color=""Red""/>
<DrawingImage x:Key=""d""><DrawingImage.Drawing><GeometryDrawing Brush=""{StaticResource b}"" Geometry=""M0,0 L1,1""/>
</DrawingImage.Drawing></DrawingImage>"));
		Assert.Contains("new System.Windows.Media.DrawingImage()", code.Replace("global::", ""));
		Assert.Contains(".Brush = ", code);
		AssertCompiles(code, WinStub);
	}

	[Fact]
	public void F3_a_DynamicResource_brush_reference_is_resolved_once()
	{
		string code = Gen(Win(@"<SolidColorBrush x:Key=""b"" Color=""Red""/>
<GeometryDrawing x:Key=""g"" Brush=""{DynamicResource b}""/>"));
		Assert.Contains(".Brush = ", code);
		AssertCompiles(code, WinStub);
	}


	[Theory]
	[InlineData("StaticResource")]
	[InlineData("DynamicResource")]
	public void F3_an_x_Static_key_on_a_media_property_keeps_the_fallback(string ext)
	{
		string code = Gen(Win($@"<GeometryDrawing x:Key=""g""
Brush=""{{{ext} {{x:Static SystemColors.ControlBrushKey}}}}""/>"));
		Assert.DoesNotContain("{x:Static", code);
		AssertCompiles(code, WinStub);
	}

	[Fact]
	public void F3_a_DynamicResource_media_reference_does_not_throw_for_a_missing_key()
	{
		string code = Gen(Win(@"<GeometryDrawing x:Key=""g"" Brush=""{DynamicResource later}""/>"));
		Assert.Contains("?? this.TryFindResource(\"later\")) as global::System.Windows.Media.Brush;", code);
		AssertCompiles(code, WinStub);
	}
	[Fact]
	public void F3_a_PathGeometry_resource_takes_Figures_markup_and_a_Transform_element()
	{
		string code = Gen(Win(@"<PathGeometry x:Key=""p"" Figures=""M0,0 L10,10"">
<PathGeometry.Transform><RotateTransform Angle=""45""/></PathGeometry.Transform></PathGeometry>"));
		Assert.Contains("PathFigureCollection.Parse(\"M0,0 L10,10\")", code);
		Assert.Contains(".Transform = ", code);
		AssertCompiles(code, WinStub);
	}

	[Fact]
	public void F4_gradient_mapping_and_spread_are_emitted_and_enums_are_case_insensitive()
	{
		string code = Gen(Win(@"<LinearGradientBrush x:Key=""l"" MappingMode=""absolute"" SpreadMethod=""Reflect""/>"));
		Assert.Contains(".MappingMode = ", code);
		Assert.Contains(".SpreadMethod = ", code);
		AssertCompiles(code, WinStub);
	}

	[Fact]
	public void F4_a_GeometryGroup_with_ellipses_is_emitted_inside_a_GeometryDrawing()
	{
		string code = Gen(Win(@"<DrawingGroup x:Key=""g""><GeometryDrawing Brush=""Red""><GeometryDrawing.Geometry>
<GeometryGroup FillRule=""Nonzero""><EllipseGeometry RadiusX=""1"" RadiusY=""2""/>
<EllipseGeometry RadiusX=""3"" RadiusY=""4""/>
</GeometryGroup></GeometryDrawing.Geometry></GeometryDrawing></DrawingGroup>"));
		Assert.Contains("new System.Windows.Media.GeometryGroup()", code.Replace("global::", ""));
		Assert.Equal(2, System.Text.RegularExpressions.Regex.Matches(code, @"\.Children\.Add\(").Count - 1);
		AssertCompiles(code, WinStub);
	}

	[Fact]
	public void F4_a_media_property_the_layer_lacks_is_reported_as_a_message()
	{
		string xaml = Win(@"<LinearGradientBrush x:Key=""l"" NoSuchProp=""1""><NoSuchStop/></LinearGradientBrush>");
		using var gen = new XamlCodeGenerator();
		string pf = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(
			typeof(GeneratorV4MediaTests).Assembly.Location)!, "Gtk4.PresentationFramework.dll");
		string wb = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pf)!, "Gtk4.WindowsBase.dll");
		string dir = System.IO.Path.GetDirectoryName(pf)!;
		gen.SetReferencePaths(new[] { pf, wb }.Concat(System.IO.Directory.GetFiles(dir, "*Sharp.dll")).ToArray());
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		gen.Generate(xaml, out _);
		Assert.Contains(gen.Messages, m => m.Contains("NoSuchProp"));
	}
}

public class GeneratorV4InitTests : GeneratorV2TestBase
{
	[Fact]
	public void The_body_between_BeginInit_and_EndInit_abandons_the_scope_on_failure()
	{
		string code = Gen($@"<Window x:Class=""A.W"" {Head}><Button/></Window>");
		int begin = code.IndexOf("__initBeginInit.BeginInit();", System.StringComparison.Ordinal);
		int tryAt = code.IndexOf("try", begin, System.StringComparison.Ordinal);
		int abandon = code.IndexOf("AbandonInitialization(__initRoot);", System.StringComparison.Ordinal);
		int end = code.IndexOf("__initEndInit.EndInit();", System.StringComparison.Ordinal);
		Assert.True(begin < tryAt && tryAt < abandon && abandon < end, code);
		Assert.Contains("throw;", code);
		AssertCompiles(code, "namespace A { public partial class W : System.Windows.Window { } }");
	}

	[Fact]
	public void An_Application_root_has_no_init_bracket()
	{
		string code = Gen($@"<Application x:Class=""A.App"" {Head}/>", hasCodeBehind: false);
		Assert.DoesNotContain("AbandonInitialization", code);
	}
}
