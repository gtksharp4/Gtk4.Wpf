using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>InlineUIContainer wrapping, whitespace around LineBreak, xml:lang and [ContentProperty(Name=)].</summary>
public class XamlCodeGeneratorContentModelInlineTests
{
	const string Pres = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
	const string XNs = "http://schemas.microsoft.com/winfx/2006/xaml";

	static readonly string Dir = Path.GetDirectoryName(typeof(XamlCodeGeneratorContentModelInlineTests).Assembly.Location)!;

	static string[] LayerPaths() => new[] { "Gtk4.PresentationFramework.dll", "Gtk4.WindowsBase.dll" }
		.Select(n => Path.Combine(Dir, n)).ToArray();

	static string Gen(string xaml, params string[] extra)
	{
		string pf = Path.Combine(Dir, "Gtk4.PresentationFramework.dll");
		using var gen = new XamlCodeGenerator();
		gen.SetReferencePaths(LayerPaths().Concat(extra).Concat(Directory.GetFiles(Dir, "*Sharp.dll")).ToArray());
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }.Concat(extra).ToArray()));
		string code = gen.Generate(xaml, out _);
		Assert.NotNull(code);
		return code;
	}

	static string Window(string body, string rootAttrs = "") => $@"<Window x:Class=""MyApp.MainWindow"" xmlns=""{Pres}""
	xmlns:x=""{XNs}"" {rootAttrs}>{body}</Window>";

	[Fact]
	public void UIElement_in_Inlines_is_wrapped_in_InlineUIContainer()
	{
		string code = Gen(Window("<TextBlock>a <Button x:Name=\"b\"/></TextBlock>"));
		Assert.Contains(".Inlines.Add(new global::System.Windows.Documents.InlineUIContainer(", code);
		Assert.DoesNotContain(".Inlines.Add(b)", code);
		Assert.DoesNotContain(".Inlines.Add(__e", code.Replace(".Inlines.Add(new", ""));
	}

	[Fact]
	public void Whitespace_around_LineBreak_is_trimmed()
	{
		string code = Gen(Window("<TextBlock>First <LineBreak/> second</TextBlock>"));
		int first = code.IndexOf("Text = \"First\"", StringComparison.Ordinal);
		int br = code.IndexOf("LineBreak", StringComparison.Ordinal);
		int second = code.IndexOf("Text = \"second\"", StringComparison.Ordinal);
		Assert.True(first >= 0 && br > first && second > br, code);
		Assert.DoesNotContain("Text = \"First \"", code);
		Assert.DoesNotContain("Text = \" second\"", code);
	}

	[Fact]
	public void Whitespace_is_kept_around_ordinary_inline_elements()
	{
		string code = Gen(Window("<TextBlock>a <Bold>b</Bold> c</TextBlock>"));
		Assert.Contains("Text = \"a \"", code);
		Assert.Contains("Text = \" c\"", code);
	}

	[Fact]
	public void Xml_lang_is_a_directive_on_root_and_children()
	{
		string code = Gen(Window("<TextBlock xml:lang=\"de-DE\"/>",
			"xmlns:xml=\"http://www.w3.org/XML/1998/namespace\" xml:lang=\"en-US\""));
		Assert.DoesNotContain(".lang", code);
		Assert.DoesNotContain("Language", code);
		Assert.DoesNotContain("en-US", code);
		Assert.DoesNotContain("de-DE", code);
	}

	// The owner assembly declares its own ContentProperty with a settable Name (as the 2006 XAML language does).
	static string BuildHolderAssembly()
	{
		const string src = "[assembly: System.Windows.Markup.XmlnsDefinition(\"urn:hold\", \"Hold\")] "
			+ "namespace Hold { public sealed class ContentPropertyAttribute : System.Attribute "
			+ "{ public string Name { get; set; } } "
			+ "[ContentProperty(Name = \"Body\")] public class Holder : System.Windows.Controls.ContentControl "
			+ "{ public object Body { get; set; } } }";
		var refs = LayerPaths().Concat(Directory.GetFiles(Dir, "*Sharp.dll"))
			.Concat(((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
				.Where(p => !Path.GetFileName(p).StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase)))
			.Select(f => (MetadataReference)MetadataReference.CreateFromFile(f));
		string path = Path.Combine(Path.GetTempPath(), "Hold-" + Guid.NewGuid().ToString("N") + ".dll");
		var result = CSharpCompilation.Create(Path.GetFileNameWithoutExtension(path),
				new[] { CSharpSyntaxTree.ParseText(src) }, refs,
				new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
			.Emit(path);
		Assert.True(result.Success, string.Join("; ", result.Diagnostics));
		return path;
	}

	[Fact]
	public void ContentProperty_named_argument_is_honoured()
	{
		string holder = BuildHolderAssembly();
		string code = Gen(Window("<local:Holder><Button/></local:Holder>", "xmlns:local=\"urn:hold\""), holder);
		Assert.True(code.Contains(".Body = "), code);
		Assert.DoesNotContain("__e0.Content", code);
	}
}
