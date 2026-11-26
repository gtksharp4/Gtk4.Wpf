using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class XamlWriterTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	[Fact]
	public void Save_writes_the_element_in_the_presentation_namespace_with_local_values_only() => Run(() =>
	{
		var xaml = XamlWriter.Save(new Button { Width = 10, Content = "x" });
		Assert.Contains("<Button", xaml);
		Assert.Contains("xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"", xaml);
		Assert.Contains("Width=\"10\"", xaml);
		Assert.Contains(">x</Button>", xaml);
		Assert.DoesNotContain("Height", xaml);
		Assert.DoesNotContain("Margin", xaml);
	});

	[Fact]
	public void Save_uses_the_invariant_culture() => Run(() =>
	{
		CultureInfo saved = CultureInfo.CurrentCulture;
		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("de-DE");
			var xaml = XamlWriter.Save(new Border { Width = 1.5, Margin = new Thickness(0.5, 1, 2, 3) });
			Assert.Contains("Width=\"1.5\"", xaml);
			Assert.Contains("Margin=\"0.5,1,2,3\"", xaml);
		}
		finally { CultureInfo.CurrentCulture = saved; }
	});

	[Fact]
	public void Save_writes_a_bound_value_as_a_binding_not_its_resolved_value() => Run(() =>
	{
		var tb = new TextBlock { Width = 5 };
		tb.SetBinding(TextBlock.TextProperty, new Binding("Label"));
		tb.DataContext = new XamlReaderTestWidget { Label = "bound" };
		var xaml = XamlWriter.Save(tb);
		Assert.DoesNotContain("Text=\"bound\"", xaml);
		Assert.Contains("Text=\"{Binding Path=Label}\"", xaml);
		Assert.Contains("Width=\"5\"", xaml);
	});

	[Fact]
	public void Save_writes_collection_content_and_round_trips() => Run(() =>
	{
		var panel = new StackPanel { Width = 7 };
		panel.Children.Add(new Button { Content = "a", Margin = new Thickness(1, 2, 3, 4) });
		panel.Children.Add(new TextBlock { Text = "b" });

		var copy = (StackPanel)XamlReader.Parse(XamlWriter.Save(panel));

		Assert.Equal(7, copy.Width);
		Assert.Equal(2, copy.Children.Count);
		Assert.Equal("a", ((Button)copy.Children[0]).Content);
		Assert.Equal(new Thickness(1, 2, 3, 4), ((Button)copy.Children[0]).Margin);
		Assert.Equal("b", ((TextBlock)copy.Children[1]).Text);
	});

	[Fact]
	public void Save_round_trips_brushes_attached_properties_and_names() => Run(() =>
	{
		var grid = new Grid();
		var border = new Border { Background = new SolidColorBrush(Color.FromRgb(1, 2, 3)), Name = "frame" };
		Grid.SetRow(border, 2);
		grid.Children.Add(border);

		var copy = (Grid)XamlReader.Parse(XamlWriter.Save(grid));

		var b = (Border)copy.Children[0];
		Assert.Equal(Color.FromRgb(1, 2, 3), ((SolidColorBrush)b.Background!).Color);
		Assert.Equal(2, Grid.GetRow(b));
		Assert.Equal("frame", b.Name);
	});

	[Fact]
	public void Save_writes_ResourceDictionary_entries_with_x_Key_and_round_trips() => Run(() =>
	{
		var merged = new ResourceDictionary { ["M"] = new SolidColorBrush(Colors.Green) };
		var rd = new ResourceDictionary { ["Red"] = new SolidColorBrush(Colors.Red), ["Size"] = 12.5, ["Name"] = "text" };
		rd.MergedDictionaries.Add(merged);

		string xaml = XamlWriter.Save(rd);
		var copy = (ResourceDictionary)XamlReader.Parse(xaml);

		Assert.Contains("x:Key=\"Red\"", xaml);
		Assert.Equal(Colors.Red, ((SolidColorBrush)copy["Red"]!).Color);
		Assert.Equal(12.5, copy["Size"]);
		Assert.Equal("text", copy["Name"]);
		Assert.Equal(Colors.Green, ((SolidColorBrush)copy.MergedDictionaries[0]["M"]!).Color);
	});

	[Fact]
	public void Save_gives_CLR_types_a_clr_namespace_prefix() => Run(() =>
	{
		var xaml = XamlWriter.Save(new XamlReaderTestWidget { Label = "l", Count = 4 });
		Assert.Contains("xmlns:clr=\"clr-namespace:Gtk4.PresentationFramework.Tests;assembly=" +
			"Gtk4.PresentationFramework.Tests\"", xaml);
		Assert.Contains("<clr:XamlReaderTestWidget", xaml);
		var copy = (XamlReaderTestWidget)XamlReader.Parse(xaml);
		Assert.Equal("l", copy.Label);
		Assert.Equal(4, copy.Count);
	});

	[Fact]
	public void Save_writes_FrameworkElement_Resources_as_a_property_element() => Run(() =>
	{
		var panel = new StackPanel();
		panel.Resources["B"] = new SolidColorBrush(Colors.Blue);
		string xaml = XamlWriter.Save(panel);
		Assert.Contains("StackPanel.Resources", xaml);
		var copy = (StackPanel)XamlReader.Parse(xaml);
		Assert.Equal(Colors.Blue, ((SolidColorBrush)copy.Resources["B"]!).Color);
	});

	[Fact]
	public void Save_overloads_write_the_same_document() => Run(() =>
	{
		var button = new Button { Content = "x", Width = 3 };
		string text = XamlWriter.Save(button);

		var sw = new StringWriter();
		XamlWriter.Save(button, sw);
		Assert.Equal(text, sw.ToString());

		var sb = new StringBuilder();
		using (var xw = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
			XamlWriter.Save(button, xw);
		Assert.Equal(((Button)XamlReader.Parse(text)).Width, ((Button)XamlReader.Parse(sb.ToString())).Width);

		using var ms = new MemoryStream();
		XamlWriter.Save(button, ms);
		ms.Position = 0;
		Assert.Equal("x", ((Button)XamlReader.Load(ms)).Content);
	});

	[Fact]
	public void Save_rejects_null_arguments() => Run(() =>
	{
		Assert.Throws<ArgumentNullException>(() => XamlWriter.Save(null!));
		Assert.Throws<ArgumentNullException>(() => XamlWriter.Save(new Button(), (Stream)null!));
		Assert.Throws<ArgumentNullException>(() => XamlWriter.Save(new Button(), (TextWriter)null!));
		Assert.Throws<ArgumentNullException>(() => XamlWriter.Save(new Button(), (XmlWriter)null!));
	});

	[Fact]
	public void Style_round_trips_its_TargetType_through_an_x_Type_attribute() => Run(() =>
	{
		var xaml = XamlWriter.Save(new Style(typeof(Button)));
		Assert.Contains("xmlns:x=", xaml);
		Assert.Equal(typeof(Button), ((Style)XamlReader.Parse(xaml)).TargetType);
	});
}
