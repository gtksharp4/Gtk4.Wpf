using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Tests for <c>{x:Null}</c> and <c>{x:Type …}</c> markup extensions,
/// including V2 (xmlns clr-namespace resolution) and V3 (unresolvable type error).
/// </summary>
public class XamlCodeGeneratorNullTypeTests
{
	readonly XamlCodeGenerator _gen = new XamlCodeGenerator();

	static string Window(string body = "", string attrs = "") =>
		$@"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:local=""clr-namespace:MyApp""
	{attrs}>
	{body}
</Window>";

	// ── x:Null ───────────────────────────────────────────────────────────────

	[Fact]
	public void XNull_IsRecognised()
	{
		Assert.True(XamlCodeGenerator.IsXNull("{x:Null}"));
		Assert.True(XamlCodeGenerator.IsXNull("  {x:Null}  "));
		Assert.False(XamlCodeGenerator.IsXNull("{x:Type Button}"));
		Assert.False(XamlCodeGenerator.IsXNull(null));
	}

	[Fact]
	public void XNull_OnDpProperty_EmitsSetValueNull()
	{
		// Background is a DP on the heuristic path (System.Windows.*).
		string xaml = Window(@"<Button Background=""{x:Null}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("SetValue", code);
		Assert.Contains("BackgroundProperty", code);
		Assert.Contains("null", code);
	}

	[Fact]
	public void XNull_OnClrProperty_EmitsDirectAssignmentNull()
	{
		// Title is a CLR-only property.
		string xaml = Window(attrs: @"Title=""{x:Null}""");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("this.Title = null;", code);
	}

	// ── x:Type ───────────────────────────────────────────────────────────────

	[Fact]
	public void XType_ParsesLocalName()
	{
		Assert.True(XamlCodeGenerator.IsXType("{x:Type Button}", out string expr));
		Assert.Contains("typeof", expr);
		Assert.Contains("Button", expr);
	}

	[Fact]
	public void XType_ParsesFullyQualifiedName()
	{
		Assert.True(XamlCodeGenerator.IsXType("{x:Type ctrl:Grid}", out string expr));
		Assert.Contains("typeof", expr);
		Assert.Contains("Grid", expr);
	}

	[Fact]
	public void XType_NotRecognised_ForBinding()
	{
		Assert.False(XamlCodeGenerator.IsXType("{Binding Path=X}", out _));
		Assert.False(XamlCodeGenerator.IsXType(null, out _));
	}

	[Fact]
	public void XType_OnDpProperty_EmitsSetValueTypeof()
	{
		string xaml = Window(@"<ContentControl ContentTemplate=""{x:Type Button}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("typeof", code);
		Assert.Contains("SetValue", code);
	}

	[Fact]
	public void XType_ButtonResolvesToFqn()
	{
		Assert.True(XamlCodeGenerator.IsXType("{x:Type Button}", out string expr));
		// WpfXmlnsMap resolves "Button" → System.Windows.Controls.Button
		Assert.Contains("System.Windows.Controls.Button", expr);
	}

	// ── V2: xmlns clr-namespace resolution ───────────────────────────────────

	[Fact]
	public void XType_LocalPrefix_ResolvesClrNamespace()
	{
		// {x:Type local:MyCtrl} with xmlns:local="clr-namespace:MyApp"
		// must emit typeof(global::MyApp.MyCtrl), not typeof(global::MyCtrl).
		var prefixMap = new System.Collections.Generic.Dictionary<string, string>
		{
			{ "local", "MyApp" }
		};
		Assert.True(XamlCodeGenerator.IsXType("{x:Type local:MyCtrl}", out string expr, prefixMap));
		Assert.Equal("typeof(global::MyApp.MyCtrl)", expr);
	}

	[Fact]
	public void XType_LocalPrefix_InGeneratedCode_EmitsFqn()
	{
		// Full generator integration: xmlns:local="clr-namespace:MyApp" in XAML
		// means {x:Type local:MyCtrl} emits global::MyApp.MyCtrl.
		string xaml = @"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:local=""clr-namespace:MyApp"">
	<ContentControl ContentTemplate=""{x:Type local:MyCtrl}"" />
</Window>";
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("global::MyApp.MyCtrl", code);
	}

	// ── V3: unresolvable type produces error ──────────────────────────────────

	[Fact]
	public void XType_Unresolvable_WithPrefixMap_ThrowsException()
	{
		// {x:Type Bogus} when namespace context is available but Bogus is not in any map.
		var prefixMap = new System.Collections.Generic.Dictionary<string, string>
		{
			{ "local", "MyApp" }
		};
		Assert.Throws<XamlCodeGeneratorException>(
			() => XamlCodeGenerator.IsXType("{x:Type Bogus}", out _, prefixMap));
	}

	[Fact]
	public void XType_UnknownPrefix_WithPrefixMap_ThrowsException()
	{
		// {x:Type unknown:Foo} where prefix "unknown" is not declared.
		var prefixMap = new System.Collections.Generic.Dictionary<string, string>
		{
			{ "local", "MyApp" }
		};
		Assert.Throws<XamlCodeGeneratorException>(
			() => XamlCodeGenerator.IsXType("{x:Type unknown:Foo}", out _, prefixMap));
	}
}