using System.IO;
using System.Linq;
using Gtk4.Forms.MarkupCompiler;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>The presentation xmlns covers every populated System.Windows.* namespace of the real assembly.</summary>
public class XamlCodeGeneratorPresentationNamespacesTests
{
	const string Pres = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	static readonly string Dir =
		Path.GetDirectoryName(typeof(XamlCodeGeneratorPresentationNamespacesTests).Assembly.Location)!;

	static string Gen(string targetType)
	{
		string pf = Path.Combine(Dir, "Gtk4.Forms.PresentationFramework.dll");
		string wb = Path.Combine(Dir, "Gtk4.Forms.WindowsBase.dll");
		string[] gtk = Directory.GetFiles(Dir, "*Sharp.dll");
		using var gen = new XamlCodeGenerator();
		gen.SetReferencePaths(new[] { pf, wb }.Concat(gtk).ToArray());
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		string code = gen.Generate($@"<Window x:Class=""MyApp.MainWindow"" xmlns=""{Pres}""
	xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
	<Window.Resources><Style x:Key=""s"" TargetType=""{targetType}"" /></Window.Resources>
</Window>", out _);
		Assert.NotNull(code);
		return code;
	}

	[Theory]
	[InlineData("RepeatButton", "System.Windows.Controls.Primitives.RepeatButton")]
	[InlineData("Paragraph", "System.Windows.Documents.Paragraph")]
	[InlineData("Brush", "System.Windows.Media.Brush")]
	[InlineData("Storyboard", "System.Windows.Media.Animation.Storyboard")]
	[InlineData("AutomationProperties", "System.Windows.Automation.AutomationProperties")]
	public void TargetType_resolves_in_the_additional_namespaces(string name, string clr)
		=> Assert.Contains($"typeof(global::{clr})", Gen(name));

	[Fact]
	public void Struct_and_enum_CLR_properties_are_converted_not_emitted_as_strings()
	{
		string pf = Path.Combine(Dir, "Gtk4.Forms.PresentationFramework.dll");
		string wb = Path.Combine(Dir, "Gtk4.Forms.WindowsBase.dll");
		using var gen = new XamlCodeGenerator();
		gen.SetReferencePaths(new[] { pf, wb }.Concat(Directory.GetFiles(Dir, "*Sharp.dll")).ToArray());
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		string code = gen.Generate($@"<Window x:Class=""MyApp.MainWindow"" xmlns=""{Pres}""
	xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
	<Window.Resources><Style x:Key=""s"" TargetType=""Border""><Setter Property=""Background""><Setter.Value>
	<LinearGradientBrush EndPoint=""0,10"" MappingMode=""Absolute"" /></Setter.Value></Setter></Style></Window.Resources>
</Window>", out _);
		Assert.Contains("ConvertByType(typeof(global::System.Windows.Point), \"0,10\")", code);
		Assert.Contains(".MappingMode = global::System.Windows.Media.BrushMappingMode.Absolute;", code);
	}
}
