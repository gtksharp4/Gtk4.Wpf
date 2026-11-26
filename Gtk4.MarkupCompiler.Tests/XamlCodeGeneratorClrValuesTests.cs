using System.IO;
using System.Linq;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Plain attribute values on CLR (non-DP) properties: struct and enum conversion.</summary>
public class XamlCodeGeneratorClrValuesTests
{
	const string Pres = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	static readonly string Dir = Path.GetDirectoryName(typeof(XamlCodeGeneratorClrValuesTests).Assembly.Location)!;

	static string Gen(string element)
	{
		string pf = Path.Combine(Dir, "Gtk4.PresentationFramework.dll");
		string wb = Path.Combine(Dir, "Gtk4.WindowsBase.dll");
		using var gen = new XamlCodeGenerator();
		gen.SetReferencePaths(new[] { pf, wb }.Concat(Directory.GetFiles(Dir, "*Sharp.dll")).ToArray());
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		string code = gen.Generate($@"<Window x:Class=""MyApp.MainWindow"" xmlns=""{Pres}""
	xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
	<Window.Resources><Style x:Key=""s"" TargetType=""Border""><Setter Property=""Background""><Setter.Value>
	{element}</Setter.Value></Setter></Style></Window.Resources>
</Window>", out _);
		Assert.NotNull(code);
		return code;
	}

	[Fact]
	public void Struct_without_a_registered_converter_is_not_cast_from_ConvertByType()
	{
		string code = Gen(@"<MatrixTransform Matrix=""1,0,0,1,10,10"" />");
		Assert.Contains(".Matrix = ", code);
		Assert.DoesNotContain("ConvertByType(typeof(global::System.Windows.Media.Matrix)", code);
	}

	[Fact]
	public void Struct_with_a_registered_converter_is_converted()
		=> Assert.Contains("ConvertByType(typeof(global::System.Windows.Point), \"1,2\")",
			Gen(@"<LinearGradientBrush StartPoint=""1,2"" />"));

	[Theory]
	[InlineData("0", ".MappingMode = (global::System.Windows.Media.BrushMappingMode)(0);")]
	[InlineData("absolute", ".MappingMode = global::System.Windows.Media.BrushMappingMode.Absolute;")]
	[InlineData("relativetoboundingbox",
		".MappingMode = global::System.Windows.Media.BrushMappingMode.RelativeToBoundingBox;")]
	[InlineData("Bogus", ".MappingMode = \"Bogus\";")]
	public void Enum_values_accept_numbers_and_any_case(string value, string expected)
		=> Assert.Contains(expected, Gen($@"<LinearGradientBrush MappingMode=""{value}"" />"));

	[Fact]
	public void Flags_enum_values_are_resolved_per_member()
		=> Assert.Contains(".Effects = global::System.Windows.DragDropEffects.Copy | " +
			"global::System.Windows.DragDropEffects.Move | (global::System.Windows.DragDropEffects)(4);",
			Gen(@"<DragEventArgs Effects=""copy, MOVE, 4"" />"));
}
