using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

// ── Fake source used by all tests in this file ────────────────────────────────

/// <summary>
/// Fake <see cref="IXmlnsDefinitionSource"/> that returns a caller-supplied list of entries.
/// Injected into <see cref="XamlCodeGenerator"/> instead of the production assembly-scanning
/// implementation, keeping tests file-system-free and self-contained.
/// </summary>
sealed class FakeXmlnsDefinitionSource : IXmlnsDefinitionSource
{
	readonly List<XmlnsDefinitionEntry> _entries;

	internal FakeXmlnsDefinitionSource(params (string xmlNs, string clrNs)[] entries)
	{
		_entries = new List<XmlnsDefinitionEntry>(entries.Length);
		foreach ((string xml, string clr) in entries)
			_entries.Add(new XmlnsDefinitionEntry(xml, clr));
	}

	IReadOnlyList<XmlnsDefinitionEntry> IXmlnsDefinitionSource.GetEntries() => _entries;
}

// ── Tests ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Tests for xmlns-definition-driven type resolution in <see cref="XamlCodeGenerator"/>.
/// </summary>
public class XamlCodeGeneratorXmlnsDefTests
{
	const string TestNs = "http://example.com/test/controls";
	const string TestClr = "MyApp.Controls";

	// ── Helper ───────────────────────────────────────────────────────────────

	static XamlCodeGenerator MakeGenWithFakeSource(IXmlnsDefinitionSource source)
	{
		var gen = new XamlCodeGenerator();
		gen.SetXmlnsDefinitionSource(source);
		return gen;
	}

	static string WindowWithCustomNs(string body)
	{
		return $@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:test=""{TestNs}"">
	{body}
</Window>";
	}

	// ── Test 1: type from injected xmlns-def source resolves correctly ────────

	[Fact]
	public void XmlnsDef_FakeSource_CustomElement_ResolvesToFqType()
	{
		// No x:Name → child is emitted via EmitChildren → needs ResolveType to return a type.
		// Use x:Name so the field is emitted and we can assert the FQ type name.
		string xaml = WindowWithCustomNs(@"<test:MyWidget x:Name=""w"" />");
		var source = new FakeXmlnsDefinitionSource((TestNs, TestClr));
		var gen = MakeGenWithFakeSource(source);

		string code = gen.Generate(xaml, out _);

		// The field declaration should use the FQ type from the fake source.
		Assert.Contains("MyApp.Controls.MyWidget", code);
		// The constructor call should also be present.
		Assert.Contains("new MyApp.Controls.MyWidget()", code);
	}

	// ── Test 2: element in built-in map still resolves (fallback intact) ──────

	[Fact]
	public void XmlnsDef_FakeSource_BuiltInElement_StillResolves()
	{
		// Grid is in WpfXmlnsMap; even with a fake source injected, it must still resolve.
		string xaml = @"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	<Grid />
</Window>";
		// Inject a source that maps a completely different URI — must not affect built-in map.
		var source = new FakeXmlnsDefinitionSource(("http://example.com/unrelated", "Unrelated.Ns"));
		var gen = MakeGenWithFakeSource(source);

		string code = gen.Generate(xaml, out _);

		Assert.Contains("new System.Windows.Controls.Grid()", code);
	}

	// ── Test 3: element in neither source nor built-in map is skipped ─────────

	[Fact]
	public void XmlnsDef_UnknownElement_IsSkipped()
	{
		// UnknownWidget is not in WpfXmlnsMap and not in the source (source maps a different NS).
		string xaml = WindowWithCustomNs(@"<test:UnknownWidget />");
		var source = new FakeXmlnsDefinitionSource(("http://example.com/other", "Other.Ns"));
		var gen = MakeGenWithFakeSource(source);

		string code = gen.Generate(xaml, out _);

		// Element should simply be skipped (no constructor emitted for it).
		Assert.DoesNotContain("UnknownWidget", code);
	}

	// ── Test 4: no source injected → defaults to built-in map only ───────────

	[Fact]
	public void XmlnsDef_NoSourceSet_BuiltInMapStillWorks()
	{
		// Default generator (no SetXmlnsDefinitionSource call) must resolve built-in types.
		var gen = new XamlCodeGenerator();
		string xaml = @"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	<Button />
</Window>";
		string code = gen.Generate(xaml, out _);
		Assert.Contains("new System.Windows.Controls.Button()", code);
	}

	// ── Test 5: source maps presentation xmlns to extra CLR namespace ─────────

	[Fact]
	public void XmlnsDef_PresentationNs_ExtraClrNs_UnknownType_ResolvesViaSource()
	{
		// MyCustomShape is NOT in WpfXmlnsMap but the fake source maps the presentation xmlns
		// to an extra CLR namespace where it lives.
		const string presentationNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
		string xaml = $@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""{presentationNs}""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	<MyCustomShape x:Name=""s"" />
</Window>";
		var source = new FakeXmlnsDefinitionSource((presentationNs, "My.Shapes"));
		var gen = MakeGenWithFakeSource(source);

		string code = gen.Generate(xaml, out _);

		Assert.Contains("My.Shapes.MyCustomShape", code);
	}

	// ── Test 6: AssemblyXmlnsDefinitionSource reads PF attributes from DLL ────

	[Fact]
	public void AssemblyXmlnsDefSource_ReadsPresentationFrameworkAttributes()
	{
		// The PresentationFramework DLL is copied to the test output directory.
		string testDir = Path.GetDirectoryName(typeof(XamlCodeGeneratorXmlnsDefTests).Assembly.Location)!;
		string pfDll = Path.Combine(testDir, "Gtk4.PresentationFramework.dll");
		if (!File.Exists(pfDll))
		{
			// Not available in this test run (e.g. CI step without build); skip gracefully.
			return;
		}

		// As in MarkupCompileTask, the resolver is created first so WindowsBase resolves while attributes load.
		string wbDll = Path.Combine(testDir, "Gtk4.WindowsBase.dll");
		using var resolver = new DpSymbolResolver(new[] { pfDll, wbDll });
		var source = new AssemblyXmlnsDefinitionSource(new[] { pfDll });
		IReadOnlyList<XmlnsDefinitionEntry> entries = ((IXmlnsDefinitionSource)source).GetEntries();

		const string presentationNs = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
		bool hasSystemWindows = false;
		bool hasControls = false;
		foreach (XmlnsDefinitionEntry entry in entries)
		{
			if (entry.XmlNamespace != presentationNs) continue;
			if (entry.ClrNamespace == "System.Windows") hasSystemWindows = true;
			if (entry.ClrNamespace == "System.Windows.Controls") hasControls = true;
		}

		Assert.True(hasSystemWindows, "Expected System.Windows entry in PresentationFramework XmlnsDefinition.");
		Assert.True(hasControls, "Expected System.Windows.Controls entry in PresentationFramework XmlnsDefinition.");
	}
}
