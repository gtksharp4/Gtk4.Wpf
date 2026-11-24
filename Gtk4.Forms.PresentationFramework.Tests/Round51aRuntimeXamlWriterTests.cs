using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests;

/// <summary>XamlWriter fidelity on what the target apps serialize (MergedResources, GeometryDesignerDemo).</summary>
public class Round51aRuntimeXamlWriterTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	static T RoundTrip<T>(T value) => (T)XamlReader.Parse(XamlWriter.Save(value!));

	[Fact]
	public void Default_Paragraph_writes_no_Margin() => Run(() =>
	{
		string xaml = XamlWriter.Save(new Paragraph(new Run("x")));

		Assert.DoesNotContain("Margin", xaml);
		Assert.DoesNotContain("NaN", xaml);
	});

	[Fact]
	public void ResourceDictionary_with_double_and_string_entries_round_trips() => Run(() =>
	{
		var dictionary = new ResourceDictionary { ["MeaningOfLife"] = 42.0, ["HelloWorld"] = "Hello, world" };

		var loaded = RoundTrip(dictionary);

		Assert.Equal(42.0, loaded["MeaningOfLife"]);
		Assert.Equal("Hello, world", loaded["HelloWorld"]);
	});

	[Fact]
	public void MergedDictionaries_are_written_as_a_property_element_and_round_trip() => Run(() =>
	{
		var root = new ResourceDictionary();
		root.MergedDictionaries.Add(new ResourceDictionary { ["MeaningOfLife"] = 42.0 });
		root.MergedDictionaries.Add(new ResourceDictionary { ["HelloWorld"] = "Hello, world" });

		string xaml = XamlWriter.Save(root);
		var loaded = (ResourceDictionary)XamlReader.Parse(xaml);

		Assert.Contains("ResourceDictionary.MergedDictionaries", xaml);
		Assert.Equal(2, loaded.MergedDictionaries.Count);
		Assert.Equal(42.0, loaded["MeaningOfLife"]);
		Assert.Equal("Hello, world", loaded["HelloWorld"]);
	});

	[Fact]
	public void Element_resources_round_trip_next_to_a_dynamic_resource_attribute() => Run(() =>
	{
		var button = new Button();
		button.Resources["BodyBrush"] = new SolidColorBrush(Colors.Green);
		XamlValueConverter.SetLocalResourceReference(button, Control.BackgroundProperty, "BodyBrush");

		string xaml = XamlWriter.Save(button);
		var loaded = (Button)XamlReader.Parse(xaml);

		Assert.Contains("Background=\"{DynamicResource BodyBrush}\"", xaml);
		Assert.Equal(Colors.Green, ((SolidColorBrush)loaded.Background!).Color);
		loaded.Resources["BodyBrush"] = new SolidColorBrush(Colors.Pink);
		Assert.Equal(Colors.Pink, ((SolidColorBrush)loaded.Background!).Color);
	});

	[Fact]
	public void Dynamic_resource_loaded_from_XAML_is_written_back_in_attribute_form() => Run(() =>
	{
		var button = (Button)XamlReader.Parse("<Button " + XamlReaderTests.Ns + " Background=\"{DynamicResource B}\">" +
			"<Button.Resources><SolidColorBrush x:Key=\"B\" Color=\"Blue\"/></Button.Resources></Button>");

		string xaml = XamlWriter.Save(button);

		Assert.Contains("Background=\"{DynamicResource B}\"", xaml);
		Assert.DoesNotContain("Background=\"#", xaml);
	});

	[Fact]
	public void Binding_is_written_in_attribute_form_and_round_trips() => Run(() =>
	{
		var block = new TextBlock { Width = 5 };
		block.SetBinding(TextBlock.TextProperty, new Binding("Label") { Mode = BindingMode.OneWay });

		string xaml = XamlWriter.Save(block);
		var loaded = RoundTrip(block);

		Assert.Contains("Text=\"{Binding Path=Label, Mode=OneWay}\"", xaml);
		var binding = BindingOperations.GetBinding(loaded, TextBlock.TextProperty)!;
		Assert.Equal("Label", binding.Path!.Path);
		Assert.Equal(BindingMode.OneWay, binding.Mode);
		Assert.Equal(5, loaded.Width);
	});

	[Fact]
	public void Binding_with_ElementName_is_written_with_ElementName() => Run(() =>
	{
		var box = new TextBox();
		box.SetBinding(TextBox.TextProperty, new Binding("Width") { ElementName = "other" });

		Assert.Contains("Text=\"{Binding Path=Width, ElementName=other}\"", XamlWriter.Save(box));
	});

	[Fact]
	public void Local_values_equal_to_the_default_are_omitted() => Run(() =>
	{
		var button = new Button { Margin = new Thickness(0), IsEnabled = true, Visibility = Visibility.Visible, Width = 9 };

		string xaml = XamlWriter.Save(button);

		Assert.Contains("Width=\"9\"", xaml);
		Assert.DoesNotContain("Margin", xaml);
		Assert.DoesNotContain("IsEnabled", xaml);
		Assert.DoesNotContain("Visibility", xaml);
	});

	[Fact]
	public void Children_are_written_as_content_not_as_a_property_element() => Run(() =>
	{
		var panel = new StackPanel { Orientation = Orientation.Horizontal };
		panel.Children.Add(new Button { Content = "One" });
		panel.Children.Add(new Button { Content = "Two" });

		string xaml = XamlWriter.Save(panel);
		var loaded = RoundTrip(panel);

		Assert.DoesNotContain("StackPanel.Children", xaml);
		Assert.Equal(new object?[] { "One", "Two" }, loaded.Children.Cast<Button>().Select(b => b.Content));
		Assert.Equal(Orientation.Horizontal, loaded.Orientation);
	});

	[Fact]
	public void Attached_properties_round_trip_on_Canvas_children() => Run(() =>
	{
		var canvas = new Canvas { Width = 100, Height = 50 };
		var label = new Label { Content = "Step 1:" };
		Canvas.SetLeft(label, 10);
		Canvas.SetTop(label, 5);
		canvas.Children.Add(label);

		string xaml = XamlWriter.Save(canvas);
		var loaded = RoundTrip(canvas);

		Assert.Contains("Canvas.Left=\"10\"", xaml);
		var child = (Label)loaded.Children[0];
		Assert.Equal(10, Canvas.GetLeft(child));
		Assert.Equal(5, Canvas.GetTop(child));
		Assert.Equal("Step 1:", child.Content);
	});

	[Fact]
	public void Shapes_in_a_drawing_pane_keep_their_stroke_and_thickness() => Run(() =>
	{
		var pane = new Canvas { Width = 200, Height = 200 };
		pane.Children.Add(new Rectangle { Width = 20, Height = 10, Stroke = Brushes.Red, StrokeThickness = 2 });

		var loaded = RoundTrip(pane);

		var rect = Assert.IsType<Rectangle>(Assert.Single(loaded.Children));
		Assert.Equal(20, rect.Width);
		Assert.Equal(2, rect.StrokeThickness);
		Assert.Equal(Colors.Red, ((SolidColorBrush)rect.Stroke!).Color);
	});

	[Fact]
	public void Local_default_value_on_an_inheriting_property_is_still_written() => Run(() =>
	{
		var panel = new StackPanel();
		TextElement.SetFontWeight(panel, FontWeights.Bold);
		var text = new TextBlock { FontWeight = FontWeights.Normal };
		panel.Children.Add(text);

		string xaml = XamlWriter.Save(panel);
		var loaded = (StackPanel)XamlReader.Parse(xaml);

		Assert.Matches("<TextBlock[^>]*FontWeight=", xaml);
		Assert.Equal(FontWeights.Normal, ((TextBlock)loaded.Children[0]).FontWeight);
	});
}
