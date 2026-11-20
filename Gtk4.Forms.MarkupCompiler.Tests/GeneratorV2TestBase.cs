using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>Generates against the built layer and compiles the result with Roslyn.</summary>
public abstract class GeneratorV2TestBase
{
	protected const string Head = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" "
		+ "xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"";

	static readonly string Dir = Path.GetDirectoryName(typeof(GeneratorV2TestBase).Assembly.Location)!;

	protected static string Gen(string xaml, bool hasCodeBehind = true, string[] sources = null)
	{
		string pf = Path.Combine(Dir, "Gtk4.Forms.PresentationFramework.dll");
		string wb = Path.Combine(Dir, "Gtk4.Forms.WindowsBase.dll");
		using var gen = new XamlCodeGenerator { HasCodeBehind = hasCodeBehind, CodeBehindSources = sources };
		gen.SetReferencePaths(new[] { pf, wb }.Concat(Directory.GetFiles(Dir, "*Sharp.dll")).ToArray());
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		return gen.Generate(xaml, out _);
	}

	protected static void AssertCompiles(string code, string extra = "")
	{
		var refs = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
			.Where(p => !Path.GetFileName(p).StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase))
			.Concat(Directory.GetFiles(Dir, "*.dll").Where(f =>
				Path.GetFileName(f).StartsWith("Gtk4.Forms.PresentationFramework")
				|| Path.GetFileName(f).StartsWith("Gtk4.Forms.WindowsBase")
				|| Path.GetFileName(f).StartsWith("GtkSharp") || Path.GetFileName(f).StartsWith("GLibSharp")))
			.Select(f => (MetadataReference)MetadataReference.CreateFromFile(f));
		var ct = TestContext.Current.CancellationToken;
		var trees = new[] { CSharpSyntaxTree.ParseText(code, cancellationToken: ct),
			CSharpSyntaxTree.ParseText(extra, cancellationToken: ct) };
		var errors = CSharpCompilation.Create("g", trees, refs,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.GetDiagnostics(ct).Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.True(errors.Count == 0, string.Join("; ", errors) + "\n" + code);
	}
}
