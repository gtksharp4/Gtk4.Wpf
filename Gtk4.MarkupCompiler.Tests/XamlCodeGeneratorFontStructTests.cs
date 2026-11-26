using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Font structs live in System.Windows; a CLR property of one must use the converter.</summary>
public class XamlCodeGeneratorFontStructTests
{
	const string Pres = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	static readonly string Dir = Path.GetDirectoryName(typeof(XamlCodeGeneratorFontStructTests).Assembly.Location)!;

	// The test assembly cannot reference the layer at compile time, so build the owner type with Roslyn.
	static string BuildOwnerAssembly()
	{
		const string src = "[assembly: System.Windows.Markup.XmlnsDefinition(\"urn:font\", \"FontApp\")] "
			+ "namespace FontApp { public class FontHolder { public System.Windows.FontWeight Weight { get; set; } } }";
		var layer = new[] { "Gtk4.PresentationFramework.dll", "Gtk4.WindowsBase.dll" }
			.Select(n => Path.Combine(Dir, n));
		var refs = layer.Concat(Directory.GetFiles(Dir, "*Sharp.dll"))
			.Concat(((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
				.Where(p => !Path.GetFileName(p).StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase)))
			.Select(f => (MetadataReference)MetadataReference.CreateFromFile(f));
		string path = Path.Combine(Path.GetTempPath(), "FontApp-" + Guid.NewGuid().ToString("N") + ".dll");
		var result = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(path),
				new[] { CSharpSyntaxTree.ParseText(src) }, refs,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.Emit(path);
		Assert.True(result.Success, string.Join("; ", result.Diagnostics));
		return path;
	}

	[Fact]
	public void Non_nullable_CLR_FontWeight_property_is_converted_by_type()
	{
		string owner = BuildOwnerAssembly();
		string pf = Path.Combine(Dir, "Gtk4.PresentationFramework.dll");
		string wb = Path.Combine(Dir, "Gtk4.WindowsBase.dll");
		using var gen = new XamlCodeGenerator();
		gen.SetReferencePaths(new[] { pf, wb, owner }.Concat(Directory.GetFiles(Dir, "*Sharp.dll")).ToArray());
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf, owner }));
		string code = gen.Generate($@"<Window x:Class=""MyApp.MainWindow"" xmlns=""{Pres}""
	xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:f=""urn:font"">
	<f:FontHolder Weight=""Bold"" />
</Window>", out _);
		Assert.NotNull(code);
		Assert.Contains("ConvertByType(typeof(global::System.Windows.FontWeight)", code);
	}
}
