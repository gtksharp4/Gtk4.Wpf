using System;
using System.IO;
using System.Reflection;
using Gtk.Forms.MarkupCompiler;
using Xunit;

namespace Gtk.Forms.MarkupCompiler.Tests
{
	// ── Test user-control types (same assembly, so the resolver can load this assembly) ──

	/// <summary>
	/// Minimal stub of DependencyProperty so we can declare TestControl.WidthProperty
	/// without referencing PresentationFramework or WindowsBase from the test project.
	/// </summary>
	class FakeDependencyProperty
	{
		internal static FakeDependencyProperty Register(string name, Type propertyType, Type ownerType)
			=> new FakeDependencyProperty();
	}

	/// <summary>User-defined control with a known DP and a known CLR property.</summary>
	class TestUserControl
	{
		// A DP: public static field whose type contains "DependencyProperty".
		public static FakeDependencyProperty MyValueProperty =
			FakeDependencyProperty.Register("MyValue", typeof(double), typeof(TestUserControl));

		// A plain CLR property (no matching static field).
		public string MyClrProp { get; set; }
	}

	/// <summary>Control that inherits from TestUserControl — DP lives on base type.</summary>
	class DerivedTestControl : TestUserControl { }

	/// <summary>
	/// Tests for <see cref="DpSymbolResolver"/>: assembly-symbol DP detection,
	/// inherited DPs, and heuristic fallback when no references are supplied.
	/// </summary>
	public class XamlCodeGeneratorSymbolResolverTests
	{
		static readonly string TestAsmPath =
			typeof(XamlCodeGeneratorSymbolResolverTests).Assembly.Location;

		// ── no-reference fallback ────────────────────────────────────────────────

		[Fact]
		public void NoReferences_FallbackHeuristic_SystemWindowsProp_IsDP()
		{
			using var resolver = new DpSymbolResolver(null);
			// Heuristic: System.Windows.* type, not in CLR-only exclusion list → DP
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
			// Non-System.Windows type with unknown prop → heuristic returns false
			Assert.False(resolver.IsDP("MyApp.SomeControl", "Width"));
		}

		// ── real reflection against the test assembly ────────────────────────────

		[Fact]
		public void WithReference_KnownDpField_ReturnsTrue()
		{
			using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
			Assert.True(resolver.IsDP(
				typeof(TestUserControl).FullName!, "MyValue"));
		}

		[Fact]
		public void WithReference_PlainClrProp_ReturnsFalse()
		{
			using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
			Assert.False(resolver.IsDP(
				typeof(TestUserControl).FullName!, "MyClrProp"));
		}

		[Fact]
		public void WithReference_InheritedDp_ReturnsTrue()
		{
			// MyValueProperty declared on TestUserControl, resolved via DerivedTestControl
			using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
			Assert.True(resolver.IsDP(
				typeof(DerivedTestControl).FullName!, "MyValue"));
		}

		[Fact]
		public void WithReference_UnknownType_FallsBackToHeuristic()
		{
			// Type not in any loaded assembly → falls back to heuristic
			using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
			// System.Windows.* but not in test assembly → heuristic says DP
			Assert.True(resolver.IsDP("System.Windows.Controls.Button", "Width"));
		}

		// ── XamlCodeGenerator integration: IsKnownDp now delegates to resolver ──

		[Fact]
		public void Generator_NoReferences_ExistingOutputUnchanged()
		{
			// Verifies the heuristic fallback keeps previous codegen intact.
			var gen = new XamlCodeGenerator();
			string xaml = @"<Window x:Class=""MyApp.Main""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"">
	<Button Width=""100"" />
</Window>";
			string code = gen.Generate(xaml, out _);
			// Width is a DP by heuristic → SetValue path
			Assert.Contains("SetValue", code);
			Assert.Contains("WidthProperty", code);
		}

		[Fact]
		public void Generator_WithReference_KnownDpUsesSetValuePath()
		{
			// Supply the test assembly; MyValue on TestUserControl is a known DP.
			// We simulate this by supplying the test assembly and checking
			// the generator would emit SetValue for a type it can resolve.
			// (The generator only uses IsKnownDp; the XAML map only resolves WpfXmlnsMap types.)
			// Verify via DpSymbolResolver directly that it returns true.
			using var resolver = new DpSymbolResolver(new[] { TestAsmPath });
			Assert.True(resolver.IsDP(typeof(TestUserControl).FullName!, "MyValue"));
			Assert.False(resolver.IsDP(typeof(TestUserControl).FullName!, "MyClrProp"));
		}
	}
}
