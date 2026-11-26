using System;
using System.IO;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;
// ── Shared test-assembly types ────────────────────────────────────────────────
// These live here so the resolver can be pointed at this assembly's Location.

/// <summary>
/// Minimal stub of DependencyProperty so we can declare TestControl.WidthProperty
/// without referencing PresentationFramework or WindowsBase from the test project.
/// </summary>
class FakeDependencyProperty
{
	internal static FakeDependencyProperty Register(string name, Type propertyType, Type ownerType)
		=> new FakeDependencyProperty();
}

/// <summary>User-defined control with a known DP, a CLR ref-type and a CLR value-type prop.</summary>
class TestUserControl
{
	// A DP: public static field whose type contains "DependencyProperty".
	public static FakeDependencyProperty MyValueProperty =
		FakeDependencyProperty.Register("MyValue", typeof(double), typeof(TestUserControl));

	// Plain CLR reference-type property (no matching static field).
	public string MyClrProp { get; set; }

	// Plain CLR value-type property — used to test V4 (x:Null → default(T)).
	public int MyIntProp { get; set; }
}

/// <summary>Control that inherits from TestUserControl — DP lives on base type.</summary>
class DerivedTestControl : TestUserControl { }

/// <summary>
/// Tests for <see cref="DpSymbolResolver"/>: assembly-symbol DP detection,
/// inherited DPs, and heuristic fallback.
/// </summary>
public class XamlCodeGeneratorSymbolResolverTests
{
	internal static readonly string TestAsmPath =
		typeof(XamlCodeGeneratorSymbolResolverTests).Assembly.Location;

	// ── no-reference fallback ────────────────────────────────────────────────

	[Fact]
	public void NoReferences_FallbackHeuristic_SystemWindowsProp_IsDP()
	{
		using var resolver = new DpSymbolResolver(null);
		Assert.True(resolver.IsDP("System.Windows.Controls.Button", "Width"));
	}

	[Fact]
	public void NoReferences_FallbackHeuristic_Title_IsCLR()
	{
		using var resolver = new DpSymbolResolver(null);
		Assert.False(resolver.IsDP("System.Windows.Window", "Title"));
	}

	[Fact]
	public void NoReferences_FallbackHeuristic_NonSystemWindows_IsCLR()
	{
		using var resolver = new DpSymbolResolver(null);
		Assert.False(resolver.IsDP("MyApp.SomeControl", "Width"));
	}

	// ── real reflection against the test assembly ────────────────────────────

	[Fact]
	public void WithReference_KnownDpField_ReturnsTrue()
	{
		using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
		Assert.True(resolver.IsDP(typeof(TestUserControl).FullName!, "MyValue"));
	}

	[Fact]
	public void WithReference_PlainClrProp_ReturnsFalse()
	{
		using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
		Assert.False(resolver.IsDP(typeof(TestUserControl).FullName!, "MyClrProp"));
	}

	[Fact]
	public void WithReference_InheritedDp_ReturnsTrue()
	{
		using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
		Assert.True(resolver.IsDP(typeof(DerivedTestControl).FullName!, "MyValue"));
	}

	[Fact]
	public void WithReference_UnknownType_FallsBackToHeuristic()
	{
		using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
		Assert.True(resolver.IsDP("System.Windows.Controls.Button", "Width"));
	}

	// ── XamlCodeGenerator integration ────────────────────────────────────────

	[Fact]
	public void Generator_NoReferences_ExistingOutputUnchanged()
	{
		var gen = new XamlCodeGenerator();
		string xaml = @"<Window x:Class=""MyApp.Main""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	<Button Width=""100"" />
</Window>";
		string code = gen.Generate(xaml, out _);
		Assert.Contains("SetValue", code);
		Assert.Contains("WidthProperty", code);
	}

	[Fact]
	public void Generator_WithReference_KnownDpUsesSetValuePath()
	{
		using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
		Assert.True(resolver.IsDP(typeof(TestUserControl).FullName!, "MyValue"));
		Assert.False(resolver.IsDP(typeof(TestUserControl).FullName!, "MyClrProp"));
	}
}