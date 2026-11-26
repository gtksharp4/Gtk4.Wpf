using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class XamlReaderTestWidget : DependencyObject
{
	public static readonly DependencyProperty LabelProperty =
		DependencyProperty.Register("Label", typeof(string), typeof(XamlReaderTestWidget));
	public string? Label { get => (string?)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
	public int Count { get; set; }
	public static string Greeting = "hello";
}

public class XamlReaderTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	internal const string Ns = "xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
		"xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +
		"xmlns:t=\"clr-namespace:Gtk4.PresentationFramework.Tests;" +
		"assembly=Gtk4.PresentationFramework.Tests\"";

	static T Parse<T>(string xaml) => (T)XamlReader.Parse(xaml);

	[Fact]
	public void Parse_builds_an_element_and_converts_attributes() => Run(() =>
	{
		var b = Parse<Button>(
			"<Button " + Ns + " Width=\"42\" Margin=\"1,2,3,4\" Content=\"Hi\" IsEnabled=\"False\"/>");
		Assert.Equal(42, b.Width);
		Assert.Equal(new Thickness(1, 2, 3, 4), b.Margin);
		Assert.Equal("Hi", b.Content);
		Assert.False(b.IsEnabled);
	});

	[Fact]
	public void Content_property_collects_children() => Run(() =>
	{
		var p = Parse<StackPanel>("<StackPanel " + Ns + "><Button/><TextBlock Text=\"a\"/></StackPanel>");
		Assert.Equal(2, p.Children.Count);
		Assert.IsType<TextBlock>(p.Children[1]);
	});

	[Fact]
	public void Text_content_and_property_elements_set_the_content() => Run(() =>
	{
		Assert.Equal("Hello world", Parse<Button>("<Button " + Ns + ">  Hello   world </Button>").Content);
		var b = Parse<Button>("<Button " + Ns + "><Button.Content><TextBlock Text=\"x\"/></Button.Content></Button>");
		Assert.IsType<TextBlock>(b.Content);
	});

	[Fact]
	public void Bare_text_in_a_TextBlock_becomes_a_Run() => Run(() =>
	{
		var tb = Parse<TextBlock>("<TextBlock " + Ns + ">abc</TextBlock>");
		Assert.Single(tb.Inlines);
	});

	[Fact]
	public void ResourceDictionary_entries_use_x_Key_and_StaticResource_sees_earlier_siblings() => Run(() =>
	{
		var rd = Parse<ResourceDictionary>("<ResourceDictionary " + Ns + ">" +
			"<SolidColorBrush x:Key=\"A\" Color=\"#FF0000\"/>" +
			"<Border x:Key=\"B\" Background=\"{StaticResource A}\"/></ResourceDictionary>");
		Assert.Equal(2, rd.Count);
		Assert.Equal(Colors.Red, ((SolidColorBrush)rd["A"]!).Color);
		Assert.Equal(Colors.Red, ((SolidColorBrush)((Border)rd["B"]!).Background!).Color);
	});

	[Fact]
	public void ResourceDictionary_Source_and_MergedDictionaries_property_element() => Run(() =>
	{
		GtkFormsXamlRegistry.Register("Round49/Reader.xaml", () => new ResourceDictionary { ["Z"] = 1 });
		var rd = Parse<ResourceDictionary>("<ResourceDictionary " + Ns + "><ResourceDictionary.MergedDictionaries>" +
			"<ResourceDictionary Source=\"Round49/Reader.xaml\"/>" +
			"</ResourceDictionary.MergedDictionaries></ResourceDictionary>");
		Assert.Single(rd.MergedDictionaries);
		Assert.Equal(1, rd["Z"]);
	});

	[Fact]
	public void Style_without_a_key_is_keyed_by_its_TargetType() => Run(() =>
	{
		var rd = Parse<ResourceDictionary>(
			"<ResourceDictionary " + Ns + "><Style TargetType=\"Button\"/></ResourceDictionary>");
		Assert.True(rd.ContainsKey(typeof(Button)));
	});

	[Fact]
	public void x_Name_registers_on_the_root_name_scope() => Run(() =>
	{
		var p = Parse<StackPanel>("<StackPanel " + Ns + "><Button x:Name=\"go\"/><TextBlock Name=\"t\"/></StackPanel>");
		Assert.Same(p.Children[0], p.FindName("go"));
		Assert.Same(p.Children[1], p.FindName("t"));
		Assert.Equal("go", ((Button)p.Children[0]).Name);
	});

	[Fact]
	public void StaticResource_resolves_from_an_ancestor_Resources_property_element() => Run(() =>
	{
		var g = Parse<StackPanel>("<StackPanel " + Ns + "><StackPanel.Resources>" +
			"<SolidColorBrush x:Key=\"R\" Color=\"Blue\"/>" +
			"</StackPanel.Resources><Border Background=\"{StaticResource R}\"/></StackPanel>");
		Assert.Equal(Colors.Blue, ((SolidColorBrush)((Border)g.Children[0]).Background!).Color);
	});

	[Fact]
	public void DynamicResource_creates_a_live_link() => Run(() =>
	{
		var app = new Application();
		app.Resources["F"] = Brushes.Red;
		var border = Parse<Border>("<Border " + Ns + " Background=\"{DynamicResource F}\"/>");
		Assert.Equal(Colors.Red, ((SolidColorBrush)border.Background!).Color);
		app.Resources["F"] = Brushes.Blue;
		Assert.Equal(Colors.Blue, ((SolidColorBrush)border.Background!).Color);
	});

	[Fact]
	public void Binding_extension_binds_to_the_DataContext() => Run(() =>
	{
		var tb = Parse<TextBlock>("<TextBlock " + Ns + " Text=\"{Binding Label}\"/>");
		tb.DataContext = new XamlReaderTestWidget { Label = "bound" };
		Assert.Equal("bound", tb.Text);
		var named = Parse<TextBlock>("<TextBlock " + Ns + " Text=\"{Binding Path=Label, Mode=OneWay}\"/>");
		named.DataContext = new XamlReaderTestWidget { Label = "named" };
		Assert.Equal("named", named.Text);
	});

	[Fact]
	public void Static_Null_and_Type_extensions() => Run(() =>
	{
		var w = Parse<XamlReaderTestWidget>(
			"<t:XamlReaderTestWidget " + Ns + " Label=\"{x:Static t:XamlReaderTestWidget.Greeting}\"/>");
		Assert.Equal("hello", w.Label);
		var n = Parse<XamlReaderTestWidget>("<t:XamlReaderTestWidget " + Ns + " Label=\"{x:Null}\"/>");
		Assert.Null(n.Label);
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"{x:Type Button}\"/>");
		Assert.Equal(typeof(Button), s.TargetType);
	});

	[Fact]
	public void Clr_namespace_types_and_attached_properties_work() => Run(() =>
	{
		var w = Parse<XamlReaderTestWidget>("<t:XamlReaderTestWidget " + Ns + " Count=\"3\" Label=\"l\"/>");
		Assert.Equal(3, w.Count);
		var grid = Parse<Grid>("<Grid " + Ns + "><Button Grid.Row=\"2\"/></Grid>");
		Assert.Equal(2, Grid.GetRow(grid.Children[0]));
	});

	[Fact]
	public void Load_accepts_a_Stream_and_an_XmlReader() => Run(() =>
	{
		string xaml = "<Button " + Ns + " Content=\"s\"/>";
		using var ms = new MemoryStream(Encoding.UTF8.GetBytes(xaml));
		Assert.Equal("s", ((Button)XamlReader.Load(ms)).Content);
		using var xr = XmlReader.Create(new StringReader(xaml));
		Assert.Equal("s", ((Button)XamlReader.Load(xr)).Content);
	});

	[Fact]
	public void mc_Ignorable_attributes_are_skipped() => Run(() =>
	{
		var b = Parse<Button>("<Button " + Ns +
			" xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\" " +
			"xmlns:d=\"http://schemas.microsoft.com/expression/blend/2008\" " +
			"mc:Ignorable=\"d\" d:DesignWidth=\"10\" Width=\"5\"/>");
		Assert.Equal(5, b.Width);
	});
}
