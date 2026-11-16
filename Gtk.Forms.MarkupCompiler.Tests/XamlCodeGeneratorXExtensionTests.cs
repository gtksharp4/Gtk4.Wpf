using Gtk.Forms.MarkupCompiler;
using Xunit;

namespace Gtk.Forms.MarkupCompiler.Tests
{
	/// <summary>
	/// Tests for <c>x:</c> markup extensions:
	/// <c>{x:Null}</c>, <c>{x:Static …}</c>, <c>{x:Type …}</c>, <c>&lt;x:Array&gt;</c>.
	/// Each test fails without the feature and passes with it.
	/// </summary>
	public class XamlCodeGeneratorXExtensionTests
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
		public void XStatic_ParsesPrefixedMember()
		{
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
			Assert.Contains("FindResource", code);  // StaticResource still dispatched
			Assert.Contains("typeof", code);         // x:Type also dispatched
		}
	}
}
