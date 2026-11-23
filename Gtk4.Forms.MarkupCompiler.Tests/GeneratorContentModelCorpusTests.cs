using System.Collections.Generic;
using Xunit;

namespace Gtk4.Forms.MarkupCompiler.Tests;

/// <summary>
/// Compile-checked content-model corpus (ContentProperty children, text content, generic and gated property
/// elements, inline CLR properties, typed CLR attribute values): each snippet is the child of a Window and the
/// generated code must compile with Roslyn.
/// </summary>
public class GeneratorContentModelCorpusTests : Sweep3Base
{
	const string ExtraAttrs = "xmlns:sys=\"clr-namespace:System;assembly=System.Runtime\" "
		+ "xmlns:local=\"clr-namespace:Missing\"";

	static readonly Dictionary<string, string> Snippets = new()
	{
		["InputBindings"] =
			"<Window.InputBindings><KeyBinding Key=\"F5\" Command=\"{Binding X}\"/></Window.InputBindings>",
		["CommandBindings"] =
			"<Window.CommandBindings><CommandBinding Command=\"ApplicationCommands.Copy\"/></Window.CommandBindings>",
		["MenuItemIcon"] = "<Menu><MenuItem Header=\"a\"><MenuItem.Icon><Image/></MenuItem.Icon></MenuItem></Menu>",
		["DataGridColumns"] =
			"<DataGrid><DataGrid.Columns><DataGridTextColumn Binding=\"{Binding A}\"/></DataGrid.Columns></DataGrid>",
		["ButtonCommandStatic"] =
			"<Button><Button.Command><x:Static Member=\"ApplicationCommands.Copy\"/></Button.Command></Button>",
		["RunAttributes"] = "<TextBlock><Run FontWeight=\"Bold\" Foreground=\"Red\">x</Run></TextBlock>",
		["StatusBar"] = "<StatusBar><TextBlock Text=\"s\"/></StatusBar>",
		["UniformGrid"] = "<UniformGrid Columns=\"2\"><Button/><Button/></UniformGrid>",
		["HyperlinkUri"] = "<TextBlock><Hyperlink NavigateUri=\"http://example.com\">go</Hyperlink></TextBlock>",
		["BitmapImageUri"] = "<Image><Image.Source><BitmapImage UriSource=\"a.png\"/></Image.Source></Image>",
		["ComboItemsSourceArray"] =
			"<ComboBox><ComboBox.ItemsSource><x:Array Type=\"x:String\"><x:String>a</x:String>" +
			"</x:Array></ComboBox.ItemsSource></ComboBox>",
		["XmlSpacePreserve"] = "<TextBlock xml:space=\"preserve\">a  b</TextBlock>",
		["UnresolvedAttachedElement"] = "<Button><local:Foo.Value>5</local:Foo.Value></Button>",
		["ListBoxItems"] = "<ListBox><ListBoxItem Content=\"a\"/><ListBoxItem Content=\"b\"/></ListBox>",
		["MenuItems"] = "<Menu><MenuItem Header=\"File\"><MenuItem Header=\"Open\"/></MenuItem></Menu>",
		["TabControl"] = "<TabControl><TabItem Header=\"t\"><Button/></TabItem></TabControl>",
		["BorderChild"] = "<Border><Button/></Border>",
		["TextBlockMixed"] = "<TextBlock>a <Bold>b</Bold> c</TextBlock>",
		["GridDefinitions"] =
			"<Grid><Grid.RowDefinitions><RowDefinition Height=\"Auto\"/><RowDefinition Height=\"2*\"/>" +
			"</Grid.RowDefinitions><Grid.ColumnDefinitions><ColumnDefinition Width=\"100\"/>" +
			"</Grid.ColumnDefinitions></Grid>",
		["ButtonText"] = "<Button>Click me</Button>",
		["TextBoxText"] = "<TextBox>hello</TextBox>",
		["GradientBackground"] =
			"<Grid><Grid.Background><LinearGradientBrush><GradientStop Color=\"Red\" Offset=\"0\"/>" +
			"<GradientStop Color=\"Blue\" Offset=\"1\"/></LinearGradientBrush></Grid.Background></Grid>",
		["ComboBoxItems"] = "<ComboBox><ComboBoxItem Content=\"a\"/><ComboBoxItem Content=\"b\"/></ComboBox>",
		["StaticCommandAttribute"] = "<Button Content=\"x\" Command=\"{x:Static ApplicationCommands.Copy}\"/>",
		["RunResource"] = "<TextBlock><Run Foreground=\"{StaticResource K}\" Text=\"{Binding A}\"/></TextBlock>",
		["ExpanderScroll"] =
			"<Expander Header=\"h\"><ScrollViewer><StackPanel><CheckBox>c</CheckBox><Label>l</Label></StackPanel>" +
			"</ScrollViewer></Expander>",
		["ListViewGridView"] =
			"<ListView><ListView.View><GridView><GridViewColumn Header=\"a\"/></GridView></ListView.View></ListView>",
		["ToolTipElement"] = "<Button><Button.ToolTip><ToolTip Content=\"t\"/></Button.ToolTip></Button>",
		["ContextMenuElement"] =
			"<Button><Button.ContextMenu><ContextMenu><MenuItem Header=\"a\"/><Separator/></ContextMenu>" +
			"</Button.ContextMenu></Button>",
		["TextBlockHyperlinkInline"] =
			"<TextBlock>See <Hyperlink NavigateUri=\"http://a.b\">here</Hyperlink> now<LineBreak/></TextBlock>",
		["ImageSourceAttribute"] = "<Image Source=\"a.png\" Stretch=\"Uniform\"/>",
		["TransformElement"] =
			"<Border><Border.RenderTransform><ScaleTransform ScaleX=\"2\"/></Border.RenderTransform></Border>",
		["BindingElementProp"] = "<TextBox><TextBox.Text><Binding Path=\"A\"/></TextBox.Text></TextBox>",
	};

	public static IEnumerable<object[]> Names()
	{
		foreach (string k in Snippets.Keys) yield return new object[] { k };
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Generated_code_compiles(string name)
	{
		string xaml = "<Window " + Head + " x:Class=\"App.C\" " + ExtraAttrs + ">" + Snippets[name] + "</Window>";

		string code = Gen(xaml, sources: new[] { Stub });

		AssertCompiles(code, Stub);
	}
}
