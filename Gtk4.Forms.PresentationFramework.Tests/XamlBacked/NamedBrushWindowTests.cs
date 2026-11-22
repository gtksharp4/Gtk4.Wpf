using System.Windows;
using Xunit;

namespace Gtk4.Forms.PresentationFramework.Tests.XamlBacked;

public class NamedBrushWindowTests : WpfGtkTestBase
{
	[Fact]
	public void Freezable_name_is_found_and_animated_by_TargetName() => Run(() =>
	{
		var window = new NamedBrushWindow();

		Assert.NotNull(window.LogoBrush.Visual);
		Assert.Same(window.LogoBrush, window.FindName("LogoBrush"));
		window.Pane.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
		Assert.Equal(1, window.LogoBrush.Opacity, precision: 6);
	});
}
