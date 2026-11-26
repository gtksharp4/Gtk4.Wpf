using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Tests that the compiler emits <c>XamlValueConverter.Convert(...)</c> for DP attributes
/// and keeps the existing path for markup extensions and CLR-only properties.
/// Each test fails without the Round-5 compiler changes.
/// </summary>
public class XamlCodeGeneratorConverterTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body = "", string attrs = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	{attrs}>
	{body}
</Window>";

	// ── Attribute on a child DP → XamlValueConverter.Convert ─────────────────

	[Fact]
	public void Border_Padding_EmitsConverterCall()
	{
		string xaml = Window("<Border Padding=\"5,10\" />");
		string code = _gen.Generate(xaml, out _);
		// Must contain the converter call wrapping the raw string.
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"5,10\"", code);
	}

	[Fact]
	public void Border_Width_EmitsConverterCall()
	{
		string xaml = Window("<Border Width=\"100\" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"100\"", code);
	}

	[Fact]
	public void TextBlock_Background_EmitsConverterCall()
	{
		string xaml = Window("<TextBlock Background=\"Red\" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("XamlValueConverter.Convert(", code);
		Assert.Contains("\"Red\"", code);
	}

	[Fact]
	public void Padding_Uses_SetValue_Not_Direct_Assign()
	{
		string xaml = Window("<Border Padding=\"5\" />");
		string code = _gen.Generate(xaml, out _);
		// DP path uses SetValue, not direct assignment.
		Assert.Contains("SetValue(", code);
		Assert.Contains("PaddingProperty", code);
	}

	// ── Markup extensions still take their own path ───────────────────────────

	[Fact]
	public void StaticResource_Does_Not_Wrap_In_Converter()
	{
		string xaml = Window("<Border Background=\"{StaticResource myBrush}\" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("StaticResourceScope.Find(", code);
		Assert.DoesNotContain("XamlValueConverter.Convert(", code);
	}

	[Fact]
	public void Binding_Does_Not_Wrap_In_Converter()
	{
		string xaml = Window("<TextBlock Text=\"{Binding Name}\" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("SetBinding(", code);
		Assert.DoesNotContain("XamlValueConverter.Convert(", code);
	}

	// ── CLR-only properties still use the old path (direct assign) ────────────

	[Fact]
	public void Window_Title_Direct_Assign_Not_Converter()
	{
		// Title is in _clrOnlyProperties; root-level Title attr.
		string xaml = Window(attrs: "Title=\"MyApp\"");
		string code = _gen.Generate(xaml, out _);
		// Direct assignment, no converter.
		Assert.Contains("this.Title = ", code);
		Assert.DoesNotContain("XamlValueConverter.Convert(", code);
	}

	// ── Compiler output compiles (string-level sanity) ────────────────────────

	[Fact]
	public void Generated_Code_Contains_XamlValueConverter_Namespace()
	{
		string xaml = Window("<Border Padding=\"5\" />");
		string code = _gen.Generate(xaml, out _);
		// The emitted call uses the full namespace.
		Assert.Contains("System.Windows.Markup.XamlValueConverter", code);
	}
}