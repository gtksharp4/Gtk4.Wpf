using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>XamlReader content model: types without [ContentProperty] follow the compiler's table.</summary>
public class XamlReaderContentTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	const string Ns = XamlReaderTests.Ns;

	static T Parse<T>(string xaml) => (T)XamlReader.Parse(xaml);

	[Fact]
	public void Style_children_are_Setters_resolved_against_the_TargetType() => Run(() =>
	{
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"Button\">" +
			"<Setter Property=\"Width\" Value=\"5\"/><Setter Property=\"Background\" Value=\"Red\"/></Style>");
		Assert.Equal(2, s.Setters.Count);
		var first = (Setter)s.Setters[0];
		Assert.Same(FrameworkElement.WidthProperty, first.Property);
		Assert.Equal("5", first.Value);
		Assert.Same(Control.BackgroundProperty, ((Setter)s.Setters[1]).Property);
	});

	[Fact]
	public void Trigger_children_are_Setters() => Run(() =>
	{
		var s = Parse<Style>("<Style " + Ns + " TargetType=\"Button\"><Style.Triggers>" +
			"<Trigger Property=\"IsDefault\" Value=\"True\"><Setter Property=\"Width\" Value=\"7\"/></Trigger>" +
			"</Style.Triggers></Style>");
		var t = Assert.IsType<Trigger>(s.Triggers[0]);
		Assert.Same(Button.IsDefaultProperty, t.Property);
		Assert.Single(t.Setters);
	});

	[Fact]
	public void Paragraph_and_Hyperlink_take_inlines() => Run(() =>
	{
		var p = Parse<Paragraph>("<Paragraph " + Ns + ">One<Bold>Two</Bold><Hyperlink>Three</Hyperlink></Paragraph>");
		Assert.Equal(3, p.Inlines.Count);
		Assert.IsType<Bold>(p.Inlines.ElementAt(1));
		var link = Assert.IsType<Hyperlink>(p.Inlines.ElementAt(2));
		Assert.Single(link.Inlines);
	});

	[Fact]
	public void Span_takes_inlines() => Run(() =>
	{
		var s = Parse<Span>("<Span " + Ns + ">a<Run>b</Run></Span>");
		Assert.Equal(2, s.Inlines.Count);
	});

	[Fact]
	public void TextBox_text_content_sets_Text() => Run(() =>
	{
		Assert.Equal("abc", Parse<TextBox>("<TextBox " + Ns + ">abc</TextBox>").Text);
	});

	[Fact]
	public void Menu_and_MenuItem_take_items() => Run(() =>
	{
		var m = Parse<Menu>("<Menu " + Ns + "><MenuItem Header=\"File\"><MenuItem Header=\"Open\"/></MenuItem></Menu>");
		var file = Assert.IsType<MenuItem>(Assert.Single(m.Items));
		Assert.Single(file.Items);
	});

	[Fact]
	public void LinearGradientBrush_children_are_stops() => Run(() =>
	{
		var b = Parse<LinearGradientBrush>("<LinearGradientBrush " + Ns +
			"><GradientStop Color=\"Red\" Offset=\"0\"/>" +
			"<GradientStop Color=\"Blue\" Offset=\"1\"/></LinearGradientBrush>");
		Assert.Equal(2, b.GradientStops.Count);
		Assert.Equal(Colors.Blue, b.GradientStops[1].Color);
	});

	[Fact]
	public void PathGeometry_and_TransformGroup_take_children() => Run(() =>
	{
		var g = Parse<PathGeometry>("<PathGeometry " + Ns + "><PathFigure StartPoint=\"0,0\"/></PathGeometry>");
		Assert.Single(g.Figures);
		var t = Parse<TransformGroup>("<TransformGroup " + Ns + "><RotateTransform Angle=\"3\"/></TransformGroup>");
		Assert.Single(t.Children);
	});

	[Fact]
	public void DoubleCollection_converts_from_a_string() => Run(() =>
	{
		var r = Parse<System.Windows.Shapes.Rectangle>("<Rectangle " + Ns + " StrokeDashArray=\"3\"/>");
		Assert.Equal(new[] { 3.0 }, r.StrokeDashArray!.ToArray());
	});
}
