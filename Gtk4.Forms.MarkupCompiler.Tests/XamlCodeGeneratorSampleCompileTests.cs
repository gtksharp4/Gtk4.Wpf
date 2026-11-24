using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>
/// Generates code from real WPF-Samples XAML (copied under Samples/) and runs it through Roslyn
/// against the built layer assemblies: it must parse cleanly and never name a layer type unqualified.
/// </summary>
public class XamlCodeGeneratorSampleCompileTests
{
	readonly ITestOutputHelper _output;

	public XamlCodeGeneratorSampleCompileTests(ITestOutputHelper output) => _output = output;

	static readonly string Dir = Path.GetDirectoryName(typeof(XamlCodeGeneratorSampleCompileTests).Assembly.Location)!;

	public static IEnumerable<object[]> Samples()
		=> Directory.GetFiles(Path.Combine(Dir, "Samples"), "*.xaml")
			.Select(f => new object[] { Path.GetFileName(f) });

	// Samples whose generated code compiles with no errors at all.
	static readonly HashSet<string> Clean = new()
	{
		"OpacityAnimation.App.xaml", "BitmapEffectsGallery.App.xaml", "PropertyAnimation.App.xaml",
		"Converter.App.xaml", "DataTrigger.MainWindow.xaml", "AnimationTiming.HandoffVersusComposeExample.xaml",
		"InlineContainers.MainWindow.xaml", "MenuSeparator.MainWindow.xaml",
		"SimpleBinding.MainWindow.xaml", "InvokeTarget.MainWindow.xaml", "XmlLang.MainWindow.xaml",
		"TargetValues.App.xaml", "RoutedEventHandling.MainWindow.xaml", "CaptureUnCaptureMouse.MainWindow.xaml",
		"IntroToStylingAndTemplating.MainWindow.xaml", "DataTemplatingIntro.MainWindow.xaml",
		"AlternatingAppearanceOfItems.MainWindow.xaml",
	};

	// Samples whose handlers are stubbed from the generated code, so the compile check covers the rest.
	static readonly HashSet<string> StubHandlers = new()
	{
		"RoutedEventHandling.MainWindow.xaml", "CaptureUnCaptureMouse.MainWindow.xaml",
		"IntroToStylingAndTemplating.MainWindow.xaml", "InvokeTarget.MainWindow.xaml",
	};

	// Samples that miss only their code-behind handlers (the stub declares none): the exact diagnostics.
	static readonly Dictionary<string, string[]> Residual = new()
	{
		["HeightProperties.MainWindow.xaml"] = new[]
		{
			"CS1061:ChangeHeight", "CS1061:ChangeMaxHeight", "CS1061:ChangeMinHeight",
			"CS1061:ClipRect", "CS1061:UnclipRect",
		},
	};

	// User types the samples define in code-behind files that are not copied here.
	static readonly Dictionary<string, string> Stubs = new()
	{
		["DataTrigger.MainWindow.xaml"] = "namespace DataTrigger { public class Places : System.Collections.ObjectModel.ObservableCollection<object> { } public class Place { } }",
		["SimpleBinding.MainWindow.xaml"] = "namespace SimpleBinding { public class Person { public string PersonName { get; set; } } }",
		["IntroToStylingAndTemplating.MainWindow.xaml"] = "namespace IntroToStylingAndTemplating { public class PhotoList { } public class Photo { } }",
		["DataTemplatingIntro.MainWindow.xaml"] = "namespace DataTemplatingIntro { public class Tasks { } "
			+ "public class TaskListDataTemplateSelector : System.Windows.Controls.DataTemplateSelector { } }",
		["AlternatingAppearanceOfItems.MainWindow.xaml"] = "namespace AlternatingAppearanceOfItems { "
			+ "public class Places { } public class ListLeagueList { } }",
	};

	static readonly string[] Unqualified =
		{ "ListBoxItem", "Ellipse", "Rectangle", "Polygon", "Shape", "ScrollViewer", "SpellCheck" };

	static List<MetadataReference> References()
	{
		var refs = new List<MetadataReference>();
		string tpa = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!;
		foreach (string p in tpa.Split(Path.PathSeparator))
		{
			string n = Path.GetFileName(p);
			if (n.StartsWith("WindowsBase", StringComparison.OrdinalIgnoreCase)) continue;
			if (n.StartsWith("System.", StringComparison.Ordinal) || n == "mscorlib.dll" || n == "netstandard.dll")
				refs.Add(MetadataReference.CreateFromFile(p));
		}
		foreach (string f in Directory.GetFiles(Dir, "*.dll"))
		{
			string n = Path.GetFileName(f);
			if (n.StartsWith("Gtk4.Forms.PresentationFramework") || n.StartsWith("Gtk4.Forms.WindowsBase")
				|| n.StartsWith("GtkSharp") || n.StartsWith("GLibSharp") || n.StartsWith("Gtk4.Forms.Gtk"))
				refs.Add(MetadataReference.CreateFromFile(f));
		}
		return refs;
	}

	static string Generate(string file)
	{
		string pf = Path.Combine(Dir, "Gtk4.Forms.PresentationFramework.dll");
		string wb = Path.Combine(Dir, "Gtk4.Forms.WindowsBase.dll");
		using var gen = new XamlCodeGenerator();
		gen.SetReferencePaths(new[] { pf, wb });
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		string code = gen.Generate(File.ReadAllText(Path.Combine(Dir, "Samples", file)), out _);
		Assert.NotNull(code);
		return code;
	}

	// Stands in for the code-behind: declares the partial class with the root element's base type.
	static string CodeBehind(string file, string code)
	{
		XElement root = XDocument.Load(Path.Combine(Dir, "Samples", file)).Root!;
		string cls = (string)root.Attributes().First(a => a.Name.LocalName == "Class");
		string baseType = root.Name.LocalName switch
		{
			"Window" => "System.Windows.Window",
			"Application" => "System.Windows.Application",
			var n => "System.Windows.Controls." + n,
		};
		int dot = cls.LastIndexOf('.');
		string ns = dot < 0 ? "" : cls.Substring(0, dot);
		string name = cls.Substring(dot + 1);
		string handlers = StubHandlers.Contains(file) ? HandlerStubs(code) : "";
		return $"namespace {ns} {{ public partial class {name} : {baseType} {{ {handlers} }} }}"
			+ (Stubs.TryGetValue(file, out string s) ? s : "");
	}

	static string HandlerStubs(string code)
		=> string.Concat(System.Text.RegularExpressions.Regex
			.Matches(code, @"this\.(\w+)\)*;").Cast<System.Text.RegularExpressions.Match>()
			.Select(m => m.Groups[1].Value).Where(n => char.IsUpper(n[0]) && n != "InitializeComponent").Distinct()
			.Select(n => $"void {n}(object s, System.Windows.RoutedEventArgs e) {{ }} "));

	[Theory]
	[MemberData(nameof(Samples))]
	public void Sample_generated_code_parses_and_names_layer_types_qualified(string file)
	{
		string code = Generate(file);
		CancellationToken ct = TestContext.Current.CancellationToken;
		SyntaxTree tree = CSharpSyntaxTree.ParseText(code, cancellationToken: ct);
		var syntax = tree.GetDiagnostics(ct).Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		Assert.True(syntax.Count == 0, file + ": " + string.Join("; ", syntax.Take(3)));

		SyntaxTree stub = CSharpSyntaxTree.ParseText(CodeBehind(file, code), cancellationToken: ct);
		CSharpCompilation compilation = CSharpCompilation.Create("g", new[] { tree, stub }, References(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var errors = compilation.GetDiagnostics(ct).Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
		_output.WriteLine($"{file}: {errors.Count} semantic error(s)");
		foreach (Diagnostic d in errors.Take(8)) _output.WriteLine("  " + d);

		if (Clean.Contains(file)) Assert.True(errors.Count == 0, file + ": " + string.Join("; ", errors));

		if (Residual.TryGetValue(file, out string[] expected))
		{
			var actual = errors.Select(d => d.Id + ":" + System.Text.RegularExpressions.Regex
				.Match(d.GetMessage(), @"definition for '(\w+)'").Groups[1].Value).OrderBy(s => s, StringComparer.Ordinal);
			Assert.Equal(expected, actual);
		}

		var contentGaps = errors.Where(d => d.Id == "CS1061" && d.GetMessage().Contains("'Content'")).ToList();
		Assert.True(contentGaps.Count == 0, file + ": " + string.Join("; ", contentGaps));

		var unresolved = errors.Where(d => (d.Id == "CS0246" || d.Id == "CS0103" || d.Id == "CS0234")
			&& Unqualified.Any(n => d.GetMessage().Contains("'" + n + "'"))).ToList();
		Assert.True(unresolved.Count == 0, file + ": " + string.Join("; ", unresolved));
	}
}
