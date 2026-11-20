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
	public void TargetType_resolves_in_the_additional_namespaces(string name, string clr)
		=> Assert.Contains($"typeof(global::{clr})", Gen(name));
}
