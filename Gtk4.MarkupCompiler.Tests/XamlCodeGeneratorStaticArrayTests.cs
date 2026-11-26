using System.Collections.Generic;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// Tests for <c>{x:Static …}</c> and <c>&lt;x:Array&gt;</c> markup extensions,
/// dispatch order, and V2 (xmlns clr-namespace for x:Static).
/// </summary>
public class XamlCodeGeneratorStaticArrayTests
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

	// ── x:Static ─────────────────────────────────────────────────────────────

	[Fact]
	public void XStatic_ParsesTypeMember()
	{
		Assert.True(XamlCodeGenerator.IsXStatic("{x:Static SystemColors.ControlBrush}", out string expr));
		Assert.Contains("SystemColors", expr);
		Assert.Contains("ControlBrush", expr);
		Assert.Contains("global::", expr);
	}

	[Fact]
	public void XStatic_ParsesPrefixedMember_NoPrefixMap()
	{
		// Without a prefix map, prefix is stripped; name used as-is (backward compat).
		Assert.True(XamlCodeGenerator.IsXStatic("{x:Static local:Foo.Bar}", out string expr));
		Assert.Contains("Foo", expr);
		Assert.Contains("Bar", expr);
		Assert.Contains("global::", expr);
	}

	[Fact]
	public void XStatic_NotRecognised_ForBinding()
	{
		Assert.False(XamlCodeGenerator.IsXStatic("{Binding}", out _));
		Assert.False(XamlCodeGenerator.IsXStatic(null, out _));
	}

	[Fact]
	public void XStatic_OnDpProperty_EmitsSetValueWithStaticRef()
	{
		string xaml = Window(@"<Button Background=""{x:Static SystemColors.ControlBrush}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("SetValue", code);
		Assert.Contains("BackgroundProperty", code);
		Assert.Contains("SystemColors.ControlBrush", code);
	}

	[Fact]
	public void XStatic_OnClrProperty_EmitsDirectAssignment()
	{
		// Title is a CLR-only property
		string xaml = Window(attrs: @"Title=""{x:Static MyApp.Constants.AppTitle}""");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("this.Title =", code);
		Assert.Contains("Constants.AppTitle", code);
	}

	// ── V2: xmlns clr-namespace for x:Static ─────────────────────────────────

	[Fact]
	public void XStatic_LocalPrefix_ResolvesClrNamespace()
	{
		// {x:Static local:Foo.Bar} with xmlns:local="clr-namespace:MyApp"
		// must emit global::MyApp.Foo.Bar, not global::Foo.Bar.
		var prefixMap = new Dictionary<string, string> { { "local", "MyApp" } };
		Assert.True(XamlCodeGenerator.IsXStatic("{x:Static local:Foo.Bar}", out string expr, prefixMap));
		Assert.Equal("global::MyApp.Foo.Bar", expr);
	}

	[Fact]
	public void XStatic_LocalPrefix_InGeneratedCode_EmitsFqn()
	{
		// Full integration: xmlns:local="clr-namespace:MyApp" → Foo.Bar emits MyApp.Foo.Bar.
		string xaml = @"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:local=""clr-namespace:MyApp"">
	<Button Background=""{x:Static local:Brushes.Red}"" />
</Window>";
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("global::MyApp.Brushes.Red", code);
	}

	// ── x:Array ───────────────────────────────────────────────────────────────

	[Fact]
	public void XArray_StringArray_EmitsArrayConstruction()
	{
		string xaml = @"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:sys=""clr-namespace:System;assembly=mscorlib"">
	<ContentControl>
		<x:Array Type=""sys:String"">
			<sys:String>Alpha</sys:String>
			<sys:String>Beta</sys:String>
		</x:Array>
	</ContentControl>
</Window>";
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.String[]", code);
		Assert.Contains("\"Alpha\"", code);
		Assert.Contains("\"Beta\"", code);
	}

	[Fact]
	public void XArray_Int32Array_EmitsTypedArray()
	{
		string xaml = @"<Window x:Class=""MyApp.MainWindow""
	xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	xmlns:sys=""clr-namespace:System;assembly=mscorlib"">
	<ContentControl>
		<x:Array Type=""sys:Int32"">
			<sys:Int32>1</sys:Int32>
			<sys:Int32>2</sys:Int32>
		</x:Array>
	</ContentControl>
</Window>";
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("System.Int32[]", code);
		Assert.Contains("1,", code);
		Assert.Contains("2,", code);
	}

	// ── dispatch order: x: extensions before Binding/StaticResource ──────────

	[Fact]
	public void XNull_DoesNotCollideWithBinding()
	{
		// A Binding on a different prop must still be emitted normally.
		string xaml = Window(@"<Button Background=""{x:Null}"" Tag=""{Binding Name}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("SetBinding", code);    // Binding still dispatched
		Assert.Contains("BackgroundProperty", code);
		Assert.Contains("null", code);
	}

	[Fact]
	public void XType_DoesNotCollideWithStaticResource()
	{
		string xaml = Window(
			@"<Button Style=""{StaticResource myStyle}"" ContentTemplate=""{x:Type Button}"" />");
		string code = _gen.Generate(xaml, out _);
		Assert.Contains("StaticResourceScope.Find", code);  // StaticResource still dispatched
		Assert.Contains("typeof", code);         // x:Type also dispatched
	}
}