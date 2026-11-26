using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.XamlBacked;

public class NamedBrushWindowTests : WpfGtkTestBase
{
	[Fact]
	public void Freezable_name_is_found_and_animated_by_TargetName() => Run(() =>
	{
		var window = new NamedBrushWindow();

		Assert.NotNull(window.LogoBrush.Visual);
		Assert.Same(window.LogoBrush, window.FindName("LogoBrush"));
		window.Pane.RaiseEvent(new RoutedEventArgs(FrameworkElement.LoadedEvent));
		var bs = (BeginStoryboard)((EventTrigger)window.Pane.Triggers[0]).Actions[0];
		Assert.Single(bs.Storyboard!.ActiveClocks);
		bs.Storyboard!.ActiveClocks[0].Tick(TimeSpan.FromSeconds(0.5));
		Assert.Same(window.LogoBrush, ((Rectangle)window.Pane.Children[0]).Fill);
		Assert.Equal(0.5, window.LogoBrush.Opacity, precision: 6);
	});
}
