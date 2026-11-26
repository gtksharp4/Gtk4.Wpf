using System;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Plain-string DataType, {x:TypeExtension}, unparseable type refs and global:: layer names.</summary>
public class XamlCodeGeneratorTypeRefDiagnosticsTests
{
	const string Pres = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	static readonly string Dir =
		Path.GetDirectoryName(typeof(XamlCodeGeneratorTypeRefDiagnosticsTests).Assembly.Location)!;

	static string Window(string body, string extraNs = "") =>
		$@"<Window x:Class=""MyApp.MainWindow"" xmlns=""{Pres}""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" {extraNs}>
	{body}
</Window>";

	static string Gen(string xaml)
	{
		string code = new XamlCodeGenerator().Generate(xaml, out _);
		Assert.NotNull(code);
		return code;
	}

	static string DataTemplate(string dataType) => Window(
		$@"<Window.Resources><DataTemplate x:Key=""d"" DataType=""{dataType}""><Border /></DataTemplate>"
		+ "</Window.Resources>", @"xmlns:local=""clr-namespace:My.Ns""");

	[Fact]
	public void DataTemplate_plain_string_DataType_is_an_xml_name_not_a_type()
	{
		string code = Gen(DataTemplate("Hero"));
		Assert.DoesNotContain("typeof(Hero)", code);
		Assert.Contains(".DataType = \"Hero\";", code);
	}

	[Fact]
	public void DataTemplate_prefixed_plain_DataType_is_a_string()
		=> Assert.Contains(".DataType = \"local:Place\";", Gen(DataTemplate("local:Place")));

	[Theory]
	[InlineData("{x:Type local:Place}")]
	[InlineData("{x:TypeExtension local:Place}")]
	public void DataTemplate_x_Type_forms_emit_typeof(string form)
		=> Assert.Contains(".DataType = typeof(global::My.Ns.Place);", Gen(DataTemplate(form)));

	[Fact]
	public void DataTemplate_plain_DataType_compiles()
	{
		string code = Gen(DataTemplate("Hero"));
		const string stub = "namespace MyApp { public partial class MainWindow : System.Windows.Window { } }";
		CancellationToken ct = TestContext.Current.CancellationToken;
		var refs = Directory.GetFiles(Dir, "*.dll").Where(f => Path.GetFileName(f).StartsWith("Gtk4.P")
				|| Path.GetFileName(f).StartsWith("Gtk4.WindowsBase") || Path.GetFileName(f).StartsWith("GtkSharp")
				|| Path.GetFileName(f).StartsWith("GLibSharp"))
			.Concat(((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
				.Where(p => !Path.GetFileName(p).StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase)
					&& (Path.GetFileName(p).StartsWith("System.") || Path.GetFileName(p) == "netstandard.dll")))
			.Select(f => (MetadataReference)MetadataReference.CreateFromFile(f)).ToList();
		var compilation = CSharpCompilation.Create("g",
			new[]
			{
				CSharpSyntaxTree.ParseText(code, cancellationToken: ct),
				CSharpSyntaxTree.ParseText(stub, cancellationToken: ct),
			},
			refs, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var hero = compilation.GetDiagnostics(ct).Where(d => d.GetMessage().Contains("Hero")).ToList();
		Assert.True(hero.Count == 0, string.Join("; ", hero));
	}

	[Theory]
	[InlineData("{x:TypeExtension Button}")]
	[InlineData("{x:TypeExtension TypeName=Button}")]
	public void TargetType_accepts_x_TypeExtension(string form)
		=> Assert.Contains("new System.Windows.Style(typeof(global::System.Windows.Controls.Button))",
			Gen(Window($@"<Window.Resources><Style TargetType=""{form}"" /></Window.Resources>")));

	[Theory]
	[InlineData("{x:Type Button, }")]
	[InlineData("{x:Type }")]
	[InlineData("{Binding Foo}")]
	public void Unparseable_TargetType_throws_instead_of_emitting_null(string form)
		=> Assert.Throws<XamlCodeGeneratorException>(() => new XamlCodeGenerator().Generate(
			Window($@"<Window.Resources><Style x:Key=""s"" TargetType=""{form}"" /></Window.Resources>"), out _));

	[Fact]
	public void Unparseable_DataType_x_Type_throws()
		=> Assert.Throws<XamlCodeGeneratorException>(() => new XamlCodeGenerator().Generate(
			DataTemplate("{x:Type Button, }"), out _));

	[Fact]
	public void DataTemplate_brace_escaped_DataType_is_the_plain_string()
		=> Assert.Contains(".DataType = \"Hero\";", Gen(DataTemplate("{}Hero")));

	[Theory]
	[InlineData("{Binding Foo}")]
	[InlineData("{DynamicResource t}")]
	public void DataTemplate_markup_extension_DataType_throws(string form)
		=> Assert.Throws<XamlCodeGeneratorException>(() => new XamlCodeGenerator().Generate(
			DataTemplate(form), out _));

	[Fact]
	public void DataTemplate_StaticResource_DataType_emits_the_lookup()
		=> Assert.Contains(".DataType = this.FindResource(\"t\");", Gen(DataTemplate("{StaticResource t}")));

	[Fact]
	public void Layer_types_are_global_qualified_in_every_position()
	{
		string code = Gen(Window(@"<Button Grid.Row=""1"" TextElement.FontSize=""4"">
<Button.Template><ControlTemplate TargetType=""Button""><Border /></ControlTemplate></Button.Template>
</Button>"));
		Assert.Contains("global::System.Windows.Controls.Grid.SetRow(", code);
		Assert.Contains("typeof(global::System.Windows.Controls.Grid)", code);
		Assert.Contains("ControlTemplate(typeof(global::System.Windows.Controls.Button))", code);
	}
}
