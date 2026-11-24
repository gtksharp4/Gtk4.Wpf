using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>The Round 46 generator fixes, checked on the real WPF-Samples XAML they were found in.</summary>
public class GeneratorV3SampleTests : GeneratorV2TestBase
{
	static string Sample(string file) => Gen(File.ReadAllText(
		Path.Combine(Path.GetDirectoryName(typeof(GeneratorV3SampleTests).Assembly.Location)!, "Samples", file)),
		hasCodeBehind: false);

	[Fact]
	public void TargetValues_drawing_resources_are_built_not_empty_strings()
	{
		string code = Sample("TargetValues.App.xaml");
		Assert.DoesNotContain("Resources[\"MyGridBrushResource\"] = \"\"", code);
		Assert.DoesNotContain("Resources[\"ArrowBackgroundBrush\"] = \"\"", code);
		Assert.DoesNotContain("Resources[\"MyGlassBrushResource\"] = \"\"", code);
		Assert.Equal(2, Regex.Matches(code, @"new System\.Windows\.Media\.DrawingBrush\(\)").Count);
		Assert.Equal(12, Regex.Matches(code, @"\.GradientStops\.Add\(").Count);
		Assert.Contains("GeometryProperty, System.Windows.Markup.XamlValueConverter.Convert(typeof(System.Windows.Media" +
			".GeometryDrawing), System.Windows.Media.GeometryDrawing.GeometryProperty, \"M0,0 L1,1\")", code);
		Assert.DoesNotContain("Freeze", code);
	}

	[Fact]
	public void OpacityAnimation_grid_brush_is_built()
	{
		string code = Sample("OpacityAnimation.App.xaml");
		Assert.DoesNotContain("Resources[\"MyGridBrushResource\"] = \"\"", code);
		Assert.Contains("new System.Windows.Media.RectangleGeometry()", code);
	}

	[Fact]
	public void RoutedEventHandling_wires_Button_Click_with_AddHandler()
	{
		string code = Sample("RoutedEventHandling.MainWindow.xaml");
		Assert.Contains("AddHandler(global::System.Windows.Controls.Button.ClickEvent, " +
			"new global::System.Windows.RoutedEventHandler(this.HandleClick))", code);
		Assert.DoesNotContain("SetClick", code);
	}

	[Fact]
	public void CaptureUnCaptureMouse_wires_attached_events_and_preview_mouse_events()
	{
		string code = Sample("CaptureUnCaptureMouse.MainWindow.xaml");
		Assert.Contains("RadioButton.CheckedEvent", code);
		Assert.Contains(".PreviewMouseDown += this.ButtonPreviewMouseDown;", code);
		Assert.DoesNotContain("SetChecked", code);
		Assert.DoesNotContain("SetClick", code);
	}

	[Fact]
	public void DataTemplatingIntro_template_names_do_not_become_root_fields()
	{
		string code = Sample("DataTemplatingIntro.MainWindow.xaml");
		Assert.DoesNotContain(" border;", code);
		Assert.DoesNotContain("this.border", code);
	}

	[Fact]
	public void IntroToStyling_BasedOn_x_Type_key_is_a_type_lookup()
	{
		string code = Sample("IntroToStylingAndTemplating.MainWindow.xaml");
		Assert.Contains("FindResource(typeof(global::System.Windows.Controls.TextBlock))", code);
		Assert.Contains("Resources[typeof(global::System.Windows.Controls.TextBlock)] = __style", code);
		Assert.DoesNotContain("{x:Type", code);
	}

	[Fact]
	public void AlternatingAppearance_nested_x_Type_names_are_qualified()
	{
		string code = Sample("AlternatingAppearanceOfItems.MainWindow.xaml");
		Assert.Contains("typeof(System.Windows.Controls.TreeViewItem)", code);
	}
}
