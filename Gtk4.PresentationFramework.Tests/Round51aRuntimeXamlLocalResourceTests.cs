using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

/// <summary>XamlReader {DynamicResource} keeps Local precedence, like the compiled path.</summary>
public class Round51aRuntimeXamlLocalResourceTests : WpfGtkTestBase, IDisposable
{
	public void Dispose() => Application.ResetCurrentForTests();

	const string Ns = XamlReaderTests.Ns;

	const string ButtonWithTrigger = "<Button " + Ns + " Background=\"{DynamicResource B}\">" +
		"<Button.Resources><SolidColorBrush x:Key=\"B\" Color=\"Blue\"/></Button.Resources>" +
		"<Button.Style><Style TargetType=\"Button\"><Style.Triggers>" +
		"<Trigger Property=\"IsDefault\" Value=\"True\"><Setter Property=\"Background\" Value=\"Red\"/></Trigger>" +
		"</Style.Triggers></Style></Button.Style></Button>";

	static Color ColorOf(object? brush) => ((SolidColorBrush)brush!).Color;

	[Fact]
	public void Dynamic_resource_attribute_is_reported_by_ReadLocalValue() => Run(() =>
	{
		var button = (Button)XamlReader.Parse(ButtonWithTrigger);

		Assert.Equal(Colors.Blue, ColorOf(button.Background));
		Assert.Equal(Colors.Blue, ColorOf(button.ReadLocalValue(Control.BackgroundProperty)));
	});

	[Fact]
	public void Dynamic_resource_attribute_beats_a_style_trigger_on_the_same_property() => Run(() =>
	{
		var button = (Button)XamlReader.Parse(ButtonWithTrigger);

		button.IsDefault = true;

		Assert.Equal(Colors.Blue, ColorOf(button.Background));
	});

	[Fact]
	public void Dynamic_resource_attribute_stays_live() => Run(() =>
	{
		var button = (Button)XamlReader.Parse(ButtonWithTrigger);

		button.Resources["B"] = new SolidColorBrush(Colors.Yellow);

		Assert.Equal(Colors.Yellow, ColorOf(button.Background));
	});

	[Fact]
	public void Explicit_SetValue_detaches_the_dynamic_resource_attribute() => Run(() =>
	{
		var button = (Button)XamlReader.Parse(ButtonWithTrigger);
		var green = new SolidColorBrush(Colors.Green);

		button.Background = green;
		button.Resources["B"] = new SolidColorBrush(Colors.Black);

		Assert.Same(green, button.Background);
	});

	[Fact]
	public void ClearValue_detaches_the_dynamic_resource_attribute() => Run(() =>
	{
		var button = (Button)XamlReader.Parse(ButtonWithTrigger);

		button.ClearValue(Control.BackgroundProperty);
		button.Resources["B"] = new SolidColorBrush(Colors.Black);

		Assert.Equal(DependencyProperty.UnsetValue, button.ReadLocalValue(Control.BackgroundProperty));
	});

	[Fact]
	public void Dynamic_resource_resolves_from_an_ancestor_resource_dictionary() => Run(() =>
	{
		var panel = (StackPanel)XamlReader.Parse("<StackPanel " + Ns + ">" +
			"<StackPanel.Resources><SolidColorBrush x:Key=\"B\" Color=\"Blue\"/></StackPanel.Resources>" +
			"<Button Background=\"{DynamicResource B}\"/></StackPanel>");
		var button = (Button)panel.Children[0];

		Assert.Equal(Colors.Blue, ColorOf(button.ReadLocalValue(Control.BackgroundProperty)));
		panel.Resources["B"] = new SolidColorBrush(Colors.Pink);
		Assert.Equal(Colors.Pink, ColorOf(button.Background));
	});

	[Fact]
	public void Dynamic_resource_in_a_Setter_inside_a_Style_stays_in_the_style_layer() => Run(() =>
	{
		var button = (Button)XamlReader.Parse("<Button " + Ns + ">" +
			"<Button.Resources><SolidColorBrush x:Key=\"B\" Color=\"Blue\"/></Button.Resources>" +
			"<Button.Style><Style TargetType=\"Button\">" +
			"<Setter Property=\"Background\" Value=\"{DynamicResource B}\"/></Style></Button.Style></Button>");

		Assert.Equal(Colors.Blue, ColorOf(button.Background));
		Assert.Equal(DependencyProperty.UnsetValue, button.ReadLocalValue(Control.BackgroundProperty));
	});

	[Fact]
	public void Dynamic_resource_on_a_FrameworkContentElement_is_a_local_value() => Run(() =>
	{
		var panel = (StackPanel)XamlReader.Parse("<StackPanel " + Ns + ">" +
			"<StackPanel.Resources><SolidColorBrush x:Key=\"B\" Color=\"Blue\"/></StackPanel.Resources>" +
			"<TextBlock><Run Foreground=\"{DynamicResource B}\" Text=\"a\"/></TextBlock></StackPanel>");
		var run = (Run)((TextBlock)panel.Children[0]).Inlines.First();

		Assert.Equal(Colors.Blue, ColorOf(run.ReadLocalValue(TextElement.ForegroundProperty)));
		var green = new SolidColorBrush(Colors.Green);
		run.Foreground = green;
		Assert.Same(green, run.Foreground);
	});
}
