using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class SetResourceValueScopeTests : WpfGtkTestBase
{
	[Fact]
	public void Scoped_resource_is_resolved_from_the_enclosing_element() => Run(() =>
	{
		var root = new Window();
		var grid = new Grid();
		grid.Resources["c"] = Colors.Red;
		var brush = new SolidColorBrush();
		XamlValueConverter.SetResourceValue(root, brush, SolidColorBrush.ColorProperty, "c", grid);
		Assert.Equal(Colors.Red, brush.Color);
	});

	[Fact]
	public void Four_argument_form_still_resolves_against_the_root() => Run(() =>
	{
		var root = new Window();
		root.Resources["c"] = Colors.Blue;
		var brush = new SolidColorBrush();
		XamlValueConverter.SetResourceValue(root, brush, SolidColorBrush.ColorProperty, "c");
		Assert.Equal(Colors.Blue, brush.Color);
	});
}
