using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>A Page/Window root without x:Class gets a class named after its file, plus a registry entry.</summary>
public class XamlCodeGeneratorImplicitClassTests
{
	const string Pres = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	static readonly string Dir = Path.GetDirectoryName(typeof(XamlCodeGeneratorImplicitClassTests).Assembly.Location)!;

	static string Gen(string xaml, string path, string rootNs, out string className)
	{
		string pf = Path.Combine(Dir, "Gtk4.PresentationFramework.dll");
		string wb = Path.Combine(Dir, "Gtk4.WindowsBase.dll");
		using var gen = new XamlCodeGenerator();
		gen.SetReferencePaths(new[] { pf, wb }.Concat(Directory.GetFiles(Dir, "*Sharp.dll")).ToArray());
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		return gen.Generate(xaml, out className, path, rootNs);
	}

	[Fact]
	public void Page_without_class_generates_a_class_and_a_registry_entry()
	{
		string code = Gen($"<Page xmlns=\"{Pres}\"><Button x:Name=\"b\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"/></Page>",
			"Page1.xaml", "MyApp", out string className);
		Assert.Equal("MyApp.Page1", className);
		Assert.Contains("internal sealed partial class Page1 : global::System.Windows.Controls.Page", code);
		Assert.Contains("public Page1() { InitializeComponent(); }", code);
		Assert.Contains("GtkFormsXamlRegistration(\"Page1.xaml\", typeof(global::MyApp.Page1))", code);
	}

	[Fact]
	public void Generated_class_compiles_against_the_layer()
	{
		string code = Gen($"<Page xmlns=\"{Pres}\"><Button/></Page>", "Views\\Page-1.xaml", "MyApp", out _);
		Assert.Contains("\"Views/Page-1.xaml\"", code);
		Assert.Contains("class Page_1 :", code);
		var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
			.Where(p => !Path.GetFileName(p).StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase))
			.Concat(Directory.GetFiles(Dir, "*.dll").Where(f =>
				Path.GetFileName(f).StartsWith("Gtk4.PresentationFramework") || Path.GetFileName(f).StartsWith("Gtk4.WindowsBase")
				|| Path.GetFileName(f).StartsWith("GtkSharp") || Path.GetFileName(f).StartsWith("GLibSharp")))
			.Select(f => (MetadataReference)MetadataReference.CreateFromFile(f));
		var ct = TestContext.Current.CancellationToken;
		var errors = CSharpCompilation.Create("g", new[] { CSharpSyntaxTree.ParseText(code, cancellationToken: ct) }, refs,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.GetDiagnostics(ct).Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.True(errors.Count == 0, string.Join("; ", errors));
	}

	[Fact]
	public void Without_a_path_or_with_another_root_nothing_is_generated()
	{
		Assert.Null(Gen($"<Page xmlns=\"{Pres}\"/>", null!, "MyApp", out string noPath));
		Assert.Null(noPath);
		Assert.Null(Gen($"<StackPanel xmlns=\"{Pres}\"/>", "Panel.xaml", "MyApp", out _));
	}

	[Fact]
	public void Explicit_x_Class_still_wins_and_is_registered()
	{
		string code = Gen($"<Page x:Class=\"A.B\" xmlns=\"{Pres}\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"/>",
			"Page1.xaml", "MyApp", out string className);
		Assert.Equal("A.B", className);
		Assert.Contains("GtkFormsXamlRegistration(\"Page1.xaml\", typeof(global::A.B))", code);
	}
}
