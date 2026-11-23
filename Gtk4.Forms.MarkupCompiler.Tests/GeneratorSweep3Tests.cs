using System.Linq;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>Sweep III: base-qualified property elements, resource objects, explicit Viewport3D.Children.</summary>
public class GeneratorSweep3Tests : Sweep3Base
{
	[Fact]
	public void Length_units_on_margin_go_through_the_runtime_converter()
	{
		string code = Compile(Page("<Border Margin=\"12pt\"/>"));

		Has(code, "12pt");
		Assert.DoesNotContain("Skipped", string.Join("|", Messages(Page("<Border Margin=\"12pt\"/>"))));
	}

	[Fact]
	public void Base_qualified_property_element_builds_the_gradient_stops()
	{
		string xaml = Page("<Button/>", "<RadialGradientBrush x:Key=\"G\"><GradientBrush.GradientStops>"
			+ "<GradientStopCollection><GradientStop Color=\"Red\" Offset=\"1\"/></GradientStopCollection>"
			+ "</GradientBrush.GradientStops></RadialGradientBrush>");

		string code = Compile(xaml);

		Has(code, "GradientStops.Add(");
		Lacks(code, "Resources[\"G\"] = \"\"");
		Assert.DoesNotContain(Messages(xaml), m => m.Contains("Skipped"));
	}

	[Fact]
	public void Base_qualified_resources_and_render_transform_are_own_properties()
	{
		string xaml = Page("<Button><FrameworkElement.Resources><SolidColorBrush x:Key=\"B\" Color=\"Red\"/>"
			+ "</FrameworkElement.Resources><UIElement.RenderTransform><RotateTransform Angle=\"5\"/>"
			+ "</UIElement.RenderTransform></Button>");

		string code = Compile(xaml);

		Has(code, "Resources[\"B\"]");
		Has(code, "RenderTransform");
	}

	[Fact]
	public void Attached_property_element_on_an_unrelated_owner_is_unchanged()
	{
		string code = Compile(Page("<Grid><Button><Grid.Row>1</Grid.Row></Button></Grid>"));

		Lacks(code, "Button.Row");
	}

	[Fact]
	public void Keyed_element_with_attributes_in_a_dictionary_is_built_not_stringified()
	{
		string xaml = Page("<Button/>", "<Border x:Key=\"Holder\" Margin=\"2\" Background=\"Red\"/>");

		string code = Compile(xaml);

		Lacks(code, "Resources[\"Holder\"] = \"\"");
		Has(code, "Resources[\"Holder\"] = __e");
	}

	[Fact]
	public void Explicit_viewport_children_property_element_adds_each_visual()
	{
		string xaml = Page("<Viewport3D x:Name=\"vp\"><Viewport3D.Camera><PerspectiveCamera Position=\"0,0,3\"/>"
			+ "</Viewport3D.Camera><Viewport3D.Children><ModelVisual3D><ModelVisual3D.Content>"
			+ "<Model3DGroup><GeometryModel3D x:Name=\"gm\"/></Model3DGroup></ModelVisual3D.Content>"
			+ "</ModelVisual3D></Viewport3D.Children></Viewport3D>");

		string code = Compile(xaml);

		Has(code, ".Children.Add(");
		Has(code, "RegisterName(\"gm\"");
		Assert.DoesNotContain(Messages(xaml), m => m.Contains("Skipped"));
	}

	[Fact]
	public void RenderOptions_attributes_are_emitted_now_that_the_owner_exists()
	{
		string code = Compile(Page("<Image RenderOptions.BitmapScalingMode=\"NearestNeighbor\" "
			+ "RenderOptions.EdgeMode=\"Aliased\"/>"));

		Has(code, "RenderOptions.SetBitmapScalingMode(");
		Has(code, "RenderOptions.SetEdgeMode(");
	}

	[Fact]
	public void Length_units_on_a_CLR_double_property_are_converted_not_emitted_as_text()
	{
		string xaml = Page("<Path><Path.Data><PathGeometry><PathFigure><ArcSegment Point=\"1,1\" Size=\"1,1\" "
			+ "RotationAngle=\"1in\"/></PathFigure></PathGeometry></Path.Data></Path>");

		string code = Compile(xaml);

		Has(code, "ConvertByType(typeof(double), \"1in\")");
		Lacks(code, "RotationAngle = \"1in\"");
	}

	[Fact]
	public void Media_graph_with_a_RenderOptions_attribute_is_still_built()
	{
		string xaml = Page("<Viewport3D><ModelVisual3D><ModelVisual3D.Content><GeometryModel3D>"
			+ "<GeometryModel3D.Geometry><MeshGeometry3D x:Name=\"geo\" Positions=\"0,0,0 1,0,0 0,1,0\"/>"
			+ "</GeometryModel3D.Geometry><GeometryModel3D.Material><DiffuseMaterial><DiffuseMaterial.Brush>"
			+ "<VisualBrush RenderOptions.CachingHint=\"Cache\" RenderOptions.CacheInvalidationThresholdMinimum=\"0\"/>"
			+ "</DiffuseMaterial.Brush></DiffuseMaterial></GeometryModel3D.Material></GeometryModel3D>"
			+ "</ModelVisual3D.Content></ModelVisual3D></Viewport3D>");

		string code = Compile(xaml);

		Has(code, "RegisterName(\"geo\"");
		Has(code, "RenderOptions.SetCachingHint(");
	}
}
