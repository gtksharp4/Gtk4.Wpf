using System.IO;
using System.Linq;
using Gtk4.MarkupCompiler;
using Xunit;

namespace Gtk4.MarkupCompiler.Tests;

/// <summary>[ContentProperty]-driven emission of element children and text content.</summary>
public class XamlCodeGeneratorContentModelTests
{
	const string Pres = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

	static readonly string Dir = Path.GetDirectoryName(typeof(XamlCodeGeneratorContentModelTests).Assembly.Location)!;

	static string Gen(string body, bool resolve = true)
	{
		using var gen = new XamlCodeGenerator();
		if (resolve)
		{
			string pf = Path.Combine(Dir, "Gtk4.PresentationFramework.dll");
			string wb = Path.Combine(Dir, "Gtk4.WindowsBase.dll");
			gen.SetReferencePaths(new[] { pf, wb }.Concat(Directory.GetFiles(Dir, "*Sharp.dll")).ToArray());
			gen.SetXmlnsDefinitionSource(new AssemblyXmlnsDefinitionSource(new[] { pf }));
		}
		string code = gen.Generate($@"<Window x:Class=""MyApp.MainWindow"" xmlns=""{Pres}""
	xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:xml=""http://www.w3.org/XML/1998/namespace"">
	{body}
</Window>", out _);
		Assert.NotNull(code);
		return code;
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Single_content_property_is_assigned(bool resolve)
	{
		string code = Gen("<Border><Button/></Border>", resolve);
		Assert.Contains(".Child = __e1;", code);
		Assert.DoesNotContain("__e0.Content", code);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Collection_content_property_is_added_to(bool resolve)
	{
		string code = Gen("<ListBox><ListBoxItem/></ListBox>", resolve);
		Assert.Contains(".Items.Add(__e1);", code);
		Assert.DoesNotContain(".Content = __e1", code);
	}

	[Fact]
	public void Panel_still_uses_Children_and_Window_uses_Content()
	{
		string code = Gen("<StackPanel><Button/></StackPanel>");
		Assert.Contains("__e0.Children.Add(__e1);", code);
		Assert.Contains("this.Content = __e0;", code);
	}

	[Fact]
	public void Transform_and_geometry_collections_use_their_content_property()
	{
		// Odd placement, but it exercises the content property of the nested collection types.
		string group = Gen("<Grid><TransformGroup><ScaleTransform/></TransformGroup></Grid>");
		string geometry = Gen("<Grid><PathGeometry><PathFigure/></PathGeometry></Grid>");
		Assert.Contains("__e1.Children.Add(__e2);", group);
		Assert.Contains("__e1.Figures.Add(__e2);", geometry);
		Assert.DoesNotContain(".Content = ", (group + geometry).Replace("this.Content", ""));
	}

	[Fact]
	public void Document_blocks_are_added()
	{
		string code = Gen("<FlowDocumentScrollViewer><FlowDocument><Paragraph/></FlowDocument>"
			+ "</FlowDocumentScrollViewer>");
		Assert.Contains(".Blocks.Add(", code);
		Assert.Contains(".Document = ", code);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void TextBlock_text_becomes_a_Run_in_Inlines(bool resolve)
	{
		string code = Gen("<TextBlock>Hello   world</TextBlock>", resolve);
		Assert.Contains(".Inlines.Add(new global::System.Windows.Documents.Run { Text = \"Hello world\" });", code);
	}

	[Fact]
	public void Mixed_inline_text_and_elements_keep_document_order()
	{
		string code = Gen("<TextBlock>a <Bold>b</Bold> c</TextBlock>");
		int a = code.IndexOf("Text = \"a \"");
		int bold = code.IndexOf(".Inlines.Add(__e");
		int c = code.IndexOf("Text = \" c\"");
		Assert.True(a >= 0 && bold > a && c > bold, code);
		Assert.Contains("Text = \"b\"", code);
	}

	[Fact]
	public void Button_text_is_assigned_to_Content_and_trimmed()
	{
		string code = Gen("<Button>\n\t\t  OK \n\t</Button>");
		Assert.Contains(".Content = \"OK\";", code);
	}

	[Fact]
	public void Run_text_is_assigned_to_Text()
	{
		string code = Gen("<TextBlock><Run>  one\n two </Run></TextBlock>");
		Assert.Contains(".Text = \"one two\";", code);
	}

	[Fact]
	public void Xml_space_preserve_keeps_whitespace_and_is_not_a_property()
	{
		string code = Gen("<TextBlock xml:space=\"preserve\">  a   b </TextBlock>");
		Assert.Contains("Text = \"  a   b \"", code);
		Assert.DoesNotContain(".space", code);
	}

	[Fact]
	public void Text_in_a_panel_is_ignored()
	{
		string code = Gen("<StackPanel>stray<Button/></StackPanel>");
		Assert.DoesNotContain("stray", code);
	}

	[Fact]
	public void Items_text_is_added_as_a_string()
	{
		Assert.Contains(".Items.Add(\"one\");", Gen("<ListBox>one</ListBox>"));
	}

	[Fact]
	public void Unknown_content_type_keeps_legacy_emission()
	{
		string code = Gen("<Foo/>", resolve: false);
		Assert.DoesNotContain("Foo", code.Replace("\"Foo\"", ""));
		string code2 = Gen("<ContentControl><Button/></ContentControl>", resolve: false);
		Assert.Contains("__e0.Content = __e1;", code2);
	}

	[Fact]
	public void Bindings_still_emit_SetBinding()
	{
		string code = Gen("<Border><TextBlock Text=\"{Binding Name}\"/></Border>");
		Assert.Contains("SetBinding(", code);
		Assert.Contains(".Child = ", code);
	}

	[Fact]
	public void Property_elements_keep_their_position_among_children()
	{
		string code = Gen("<Grid><Button/><Grid.Resources/><Button/></Grid>");
		Assert.Equal(2, code.Split(new[] { ".Children.Add(" }, System.StringSplitOptions.None).Length - 1);
	}
}
