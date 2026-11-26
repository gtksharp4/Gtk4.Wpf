using System.Linq;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>Sweep III review fixes: project command owners, keyed resource fallbacks, unwrapped collections.</summary>
public class GeneratorSweep3ReviewTests : Sweep3Base
{
	const string Local = "xmlns:local=\"clr-namespace:App\"";
	const string Sys = "xmlns:sys=\"clr-namespace:System;assembly=mscorlib\"";

	static string Owner(string member) => "namespace App { public partial class C { } public partial class MainWindow { "
		+ member + " } }";

	[Fact]
	public void Prefixed_command_on_a_project_type_resolves_to_its_static_member()
	{
		string stub = Owner("public static System.Windows.Input.RoutedUICommand ExitCommand = "
			+ "new System.Windows.Input.RoutedUICommand();");

		string code = Compile(Page("<MenuItem Command=\"local:MainWindow.ExitCommand\"/>", "", Local), stub);

		Has(code, "global::App.MainWindow.ExitCommand");
		Lacks(code, "\"local:MainWindow.ExitCommand\"");
	}

	[Fact]
	public void Prefixed_command_on_a_project_type_tries_the_Command_suffix()
	{
		string stub = Owner("public static System.Windows.Input.RoutedUICommand ExitCommand { get; } = "
			+ "new System.Windows.Input.RoutedUICommand();");

		string code = Compile(Page("<MenuItem Command=\"local:MainWindow.Exit\"/>", "", Local), stub);

		Has(code, "global::App.MainWindow.ExitCommand");
	}

	[Fact]
	public void Prefixed_command_without_a_static_member_keeps_the_runtime_converter()
	{
		string code = Gen(Page("<MenuItem Command=\"local:MainWindow.Exit\"/>", "", Local),
			sources: new[] { Owner("public int Other;") });

		Lacks(code, "global::App.MainWindow.Exit");
		Has(code, "local:MainWindow.Exit");
	}

	[Fact]
	public void Keyed_Color_struct_is_built_not_stringified()
	{
		string code = Compile(Page("<Button/>", "<Color x:Key=\"c\" R=\"255\" G=\"255\" B=\"255\" A=\"0\"/>"));

		Lacks(code, "Resources[\"c\"] = \"\"");
		Has(code, "Color.FromArgb(0, 255, 255, 255)");
	}

	[Fact]
	public void Keyed_attribute_less_converter_is_instantiated()
	{
		string code = Compile(Page("<Button/>", "<BooleanToVisibilityConverter x:Key=\"b\"/>"));

		Has(code, "new global::System.Windows.Controls.BooleanToVisibilityConverter()");
	}

	[Fact]
	public void Keyed_primitive_element_stays_a_string()
	{
		string code = Compile(Page("<Button/>", "<sys:String x:Key=\"s\">hi</sys:String>", Sys));

		Has(code, "Resources[\"s\"] = \"hi\"");
	}

	[Fact]
	public void Unbuildable_keyed_element_gets_no_entry_and_a_message()
	{
		string xaml = Page("<Button/>", "<BooleanToVisibilityConverter x:Key=\"b\" Foo=\"1\"/>");

		string code = Compile(xaml);

		Lacks(code, "Resources[\"b\"]");
		Assert.Contains(Messages(xaml), m => m.Contains("Skipped resource BooleanToVisibilityConverter (line"));
	}

	[Fact]
	public void Unbuildable_keyed_element_used_by_a_StaticResource_keeps_a_placeholder_entry()
	{
		string xaml = Page("<Button Tag=\"{StaticResource b}\"/>", "<BooleanToVisibilityConverter x:Key=\"b\" Foo=\"1\"/>");

		string code = Gen(xaml, sources: new[] { Stub });

		Has(code, "Resources[\"b\"] = ");
	}

	[Fact]
	public void Unwrapped_collection_items_keep_their_line_info()
	{
		string xaml = "<Viewport3D " + Head + "><ModelVisual3D><ModelVisual3D.Transform><Transform3DGroup>"
			+ "<Transform3DGroup.Children><Transform3DCollection>\n<RotateTransform3D/>\n</Transform3DCollection>"
			+ "</Transform3DGroup.Children></Transform3DGroup></ModelVisual3D.Transform></ModelVisual3D></Viewport3D>";
		string dir = System.IO.Path.GetDirectoryName(typeof(GeneratorV2TestBase).Assembly.Location)!;
		string pf = System.IO.Path.Combine(dir, "Gtk4.PresentationFramework.dll");
		using var gen = new XamlCodeGenerator { HasCodeBehind = true };
		gen.SetReferencePaths(new[] { pf, System.IO.Path.Combine(dir, "Gtk4.WindowsBase.dll") });
		gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		var doc = System.Xml.Linq.XDocument.Parse(xaml, System.Xml.Linq.LoadOptions.SetLineInfo);

		gen.UnwrapExplicitCollections(doc.Root);

		var child = doc.Descendants().Single(e => e.Name.LocalName == "RotateTransform3D");
		Assert.Equal("Transform3DGroup.Children", child.Parent.Name.LocalName);
		Assert.True(((System.Xml.IXmlLineInfo)child).HasLineInfo());
		Assert.Equal(2, ((System.Xml.IXmlLineInfo)child).LineNumber);
	}
}
