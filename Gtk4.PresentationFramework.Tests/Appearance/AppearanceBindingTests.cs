using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.Appearance;

/// <summary>Bindings convert the source value to the DP type, so the CSS builders never see a wrong type.</summary>
public class AppearanceBindingTests : WpfGtkTestBase
{
	sealed class Src
	{
		public int Zoom { get; set; } = 20;
		public string Weight { get; set; } = "Bold";
	}

	[Fact]
	public void Int_source_binds_to_the_double_FontSize() => Run(() =>
	{
		var tb = new TextBlock { DataContext = new Src() };
		tb.SetBinding(TextBlock.FontSizeProperty, new Binding("Zoom"));
		Assert.Equal(20.0, tb.FontSize);
		Assert.IsType<double>(tb.GetValue(TextBlock.FontSizeProperty));
	});

	[Fact]
	public void String_source_binds_to_FontWeight() => Run(() =>
	{
		var c = new Button { DataContext = new Src() };
		c.SetBinding(Control.FontWeightProperty, new Binding("Weight"));
		Assert.Equal(FontWeights.Bold, c.FontWeight);
	});

	[Fact]
	public void Int_binding_with_a_created_widget_renders_the_size() => Run(() =>
	{
		var tb = new TextBlock { DataContext = new Src() };
		_ = tb.Widget;
		tb.SetBinding(TextBlock.FontSizeProperty, new Binding("Zoom"));
		Assert.Contains("font-size: 20px", tb.AppearanceCss);
	});
}
