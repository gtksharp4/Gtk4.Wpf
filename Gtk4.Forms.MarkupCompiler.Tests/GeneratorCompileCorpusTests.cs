using System.Collections.Generic;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>
/// Compile-checked generator corpus: each XAML is generated against the real layer assemblies and the output must
/// compile with Roslyn (any error diagnostic fails). Cases cover the shapes the sample sweep fixed.
/// </summary>
public class GeneratorCompileCorpusTests : GeneratorV2TestBase
{
	const string Stub = "namespace App { public partial class C { } }";

	static string Page(string body, string resources = "")
		=> "<Page " + Head + " x:Class=\"App.C\">" + (resources.Length == 0 ? ""
			: "<Page.Resources>" + resources + "</Page.Resources>") + body + "</Page>";

	public static IEnumerable<object[]> Cases()
	{
		yield return new object[] { "media3d-scene", GeneratorMedia3DTests.Scene };
		yield return new object[] { "context-menu-and-expander-header", Page(
			"<StackPanel><Button Name=\"b\"><Button.ContextMenu><ContextMenu><MenuItem Header=\"a\"/></ContextMenu>"
			+ "</Button.ContextMenu></Button><Expander><Expander.Header><StackPanel><TextBlock Text=\"h\"/>"
			+ "</StackPanel></Expander.Header><TextBlock Text=\"b\"/></Expander></StackPanel>") };
		yield return new object[] { "key-frames-and-bound-animation", Page("<Button Name=\"b\"/>",
			"<Storyboard x:Key=\"sb\"><DoubleAnimationUsingKeyFrames Storyboard.TargetName=\"b\" "
			+ "Storyboard.TargetProperty=\"Width\"><LinearDoubleKeyFrame Value=\"1\" KeyTime=\"0:0:1\"/>"
			+ "<SplineDoubleKeyFrame Value=\"2\" KeyTime=\"0:0:2\" KeySpline=\"0.1,0.2 0.3,0.4\"/>"
			+ "</DoubleAnimationUsingKeyFrames><DoubleAnimation Storyboard.TargetName=\"b\" "
			+ "Storyboard.TargetProperty=\"Height\" To=\"{Binding ElementName=b, Path=ActualWidth}\"/></Storyboard>") };
		yield return new object[] { "non-framework-element-binding", Page("<Button/>",
			"<CollectionViewSource x:Key=\"cvs\" Source=\"{Binding Path=Items}\"/>") };
		yield return new object[] { "explicit-collection-element", Page("<Path><Path.Data><GeometryGroup><PathGeometry>"
			+ "<PathGeometry.Figures><PathFigureCollection><PathFigure StartPoint=\"1,1\"><PathFigure.Segments>"
			+ "<PathSegmentCollection><BezierSegment Point1=\"1,1\" Point2=\"2,2\" Point3=\"3,3\"/>"
			+ "</PathSegmentCollection></PathFigure.Segments></PathFigure></PathFigureCollection></PathGeometry.Figures>"
			+ "</PathGeometry></GeometryGroup></Path.Data></Path>") };
		yield return new object[] { "deferred-static-resource", Page("<Button Style=\"{StaticResource S}\"/>",
			"<Style x:Key=\"S\" TargetType=\"Button\"><Setter Property=\"FocusVisualStyle\" "
			+ "Value=\"{StaticResource Later}\"/></Style><Style x:Key=\"Later\"/>") };
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public void Generated_code_compiles(string name, string xaml)
	{
		Assert.NotNull(name);
		string code = Gen(xaml, sources: new[] { Stub });

		AssertCompiles(code, Stub);
	}
}
