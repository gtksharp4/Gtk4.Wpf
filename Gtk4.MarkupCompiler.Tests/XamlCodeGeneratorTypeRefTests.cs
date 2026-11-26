using System.Linq;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>
/// {x:Type} and plain forms in TargetType/DataType, and fully-qualified layer type names
/// in generated code (TargetType, attached-property owners, x:Type values).
/// </summary>
public class XamlCodeGeneratorTypeRefTests
{
	const string Pres = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	static string Window(string body, string extraNs = "") =>
		$@"<Window x:Class=""MyApp.MainWindow"" xmlns=""{Pres}""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml"" {extraNs}>
	{body}
</Window>";

	static string Gen(string xaml, XamlCodeGenerator gen = null)
	{
		string code = (gen ?? new XamlCodeGenerator()).Generate(xaml, out _);
		Assert.NotNull(code);
		return code;
	}

	static string StyleXaml(string targetType, string extraNs = "") => Window(
		$@"<Window.Resources><Style x:Key=""s"" TargetType=""{targetType}"" /></Window.Resources>", extraNs);

	[Theory]
	[InlineData("Button")]
	[InlineData("{x:Type Button}")]
	[InlineData("{x:Type TypeName=Button}")]
	[InlineData("{x:Type  Button }")]
	public void Style_TargetType_forms_emit_the_same_typeof(string form)
		=> Assert.Contains("new System.Windows.Style(typeof(global::System.Windows.Controls.Button))",
			Gen(StyleXaml(form)));

	[Fact]
	public void Style_TargetType_x_Type_never_emits_the_markup_text()
	{
		string code = Gen(StyleXaml("{x:Type Rectangle}"));
		Assert.DoesNotContain("Type Rectangle", code);
		Assert.Contains("typeof(global::System.Windows.Shapes.Rectangle)", code);
	}

	[Theory]
	[InlineData("{x:Type local:Thing}")]
	[InlineData("{x:Type TypeName=local:Thing}")]
	[InlineData("local:Thing")]
	public void Style_TargetType_prefixed_user_type_is_global_qualified(string form)
		=> Assert.Contains("new System.Windows.Style(typeof(global::My.Ns.Thing))",
			Gen(StyleXaml(form, @"xmlns:local=""clr-namespace:My.Ns""")));

	[Fact]
	public void Style_TargetType_probes_xmlns_definitions_for_unmapped_types()
	{
		var gen = new XamlCodeGenerator();
		gen.SetXmlnsDefinitionSource(new FakeXmlnsDefinitionSource((Pres, "Some.Controls")));
		Assert.Contains("typeof(global::Some.Controls.Gizmo)", Gen(StyleXaml("{x:Type Gizmo}"), gen));
	}

	[Theory]
	[InlineData("ListBoxItem", "System.Windows.Controls.ListBoxItem")]
	[InlineData("Ellipse", "System.Windows.Shapes.Ellipse")]
	[InlineData("Rectangle", "System.Windows.Shapes.Rectangle")]
	[InlineData("Polygon", "System.Windows.Shapes.Polygon")]
	[InlineData("Shape", "System.Windows.Shapes.Shape")]
	[InlineData("ScrollViewer", "System.Windows.Controls.ScrollViewer")]
	public void Style_TargetType_layer_types_are_qualified(string name, string fq)
	{
		Assert.Contains($"typeof(global::{fq})", Gen(StyleXaml(name)));
		Assert.Contains($"typeof(global::{fq})", Gen(StyleXaml("{x:Type " + name + "}")));
	}

	[Fact]
	public void ControlTemplate_TargetType_x_Type()
	{
		string code = Gen(Window(@"<Button><Button.Template>
<ControlTemplate TargetType=""{x:Type Button}""><Border /></ControlTemplate></Button.Template></Button>"));
		Assert.Contains("new System.Windows.Controls.ControlTemplate(typeof(global::System.Windows.Controls.Button))", code);
	}

	[Theory]
	[InlineData("{x:Type local:Place}")]
	[InlineData("{x:Type TypeName=local:Place}")]
	public void DataTemplate_DataType_x_Type_forms(string form)
	{
		string code = Gen(Window(
			$@"<Window.Resources><DataTemplate x:Key=""d"" DataType=""{form}"">" +
			"<Border /></DataTemplate></Window.Resources>",
			@"xmlns:local=""clr-namespace:My.Ns"""));
		Assert.Contains(".DataType = typeof(global::My.Ns.Place);", code);
	}

	[Fact]
	public void DataTemplate_without_DataType_emits_no_DataType()
		=> Assert.DoesNotContain("DataType", Gen(Window(
			@"<Window.Resources><DataTemplate x:Key=""d""><Border /></DataTemplate></Window.Resources>")));

	[Fact]
	public void EventSetter_owner_resolves_through_type_ref()
	{
		string code = Gen(Window(@"<Window.Resources><Style x:Key=""s"" TargetType=""{x:Type ListBoxItem}"">
<EventSetter Event=""MouseDoubleClick"" Handler=""OnDbl"" /></Style></Window.Resources>"));
		Assert.Contains("typeof(global::System.Windows.Controls.ListBoxItem)", code);
	}

	[Theory]
	[InlineData("ScrollViewer.HorizontalScrollBarVisibility", "global::System.Windows.Controls.ScrollViewer.Set")]
	[InlineData("SpellCheck.IsEnabled", "global::System.Windows.Controls.SpellCheck.Set")]
	[InlineData("Canvas.Left", "global::System.Windows.Controls.Canvas.Set")]
	[InlineData("Grid.Row", "global::System.Windows.Controls.Grid.Set")]
	public void Attached_property_owners_are_qualified(string attr, string expected)
	{
		string code = Gen(Window($@"<Button {attr}=""1"" />"));
		Assert.Contains(expected, code);
		Assert.DoesNotContain(" " + attr.Split('.')[0] + ".Set", code);
		Assert.DoesNotContain("(" + attr.Split('.')[0] + ".Set", code);
	}

	[Fact]
	public void Attached_property_owner_is_qualified_in_a_property_element_context()
	{
		string code = Gen(Window(@"<TextBox SpellCheck.IsEnabled=""True"" ScrollViewer.CanContentScroll=""True"" />"));
		Assert.Contains("typeof(global::System.Windows.Controls.SpellCheck)", code);
		Assert.Contains("typeof(global::System.Windows.Controls.ScrollViewer)", code);
	}

	[Fact]
	public void XType_attribute_value_of_unmapped_layer_type_is_resolved_via_xmlns_definitions()
	{
		var gen = new XamlCodeGenerator();
		gen.SetXmlnsDefinitionSource(new FakeXmlnsDefinitionSource((Pres, "Some.Controls")));
		string code = Gen(Window(@"<Button Tag=""{x:Type Gizmo}"" />"), gen);
		Assert.Contains("typeof(global::Some.Controls.Gizmo)", code);
	}

	[Theory]
	[InlineData("{x:Type Button}", "", "Button")]
	[InlineData("{x:Type p:Button}", "p", "Button")]
	[InlineData("{x:Type TypeName=p:Button}", "p", "Button")]
	[InlineData("p:Button", "p", "Button")]
	[InlineData("Button", "", "Button")]
	public void TryParseTypeRef_accepts_all_forms(string value, string prefix, string name)
	{
		Assert.True(XamlCodeGenerator.TryParseTypeRef(value, out string p, out string n));
		Assert.Equal(prefix, p);
		Assert.Equal(name, n);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("{Binding X}")]
	[InlineData("{x:Type }")]
	[InlineData("{x:Type Button")]
	[InlineData("{x:Type TypeName=}")]
	public void TryParseTypeRef_rejects_non_type_values(string value)
		=> Assert.False(XamlCodeGenerator.TryParseTypeRef(value, out _, out _));

	[Fact]
	public void Existing_plain_forms_are_unchanged()
		=> Assert.Equal(1, Gen(StyleXaml("Button")).Split('\n')
			.Count(l => l.Contains("new System.Windows.Style(typeof(global::System.Windows.Controls.Button))")));
}
