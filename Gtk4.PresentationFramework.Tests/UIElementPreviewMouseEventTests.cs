using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests;

public class UIElementPreviewMouseEventTests : WpfGtkTestBase
{
	[Fact]
	public void PreviewMouseDown_and_Up_clr_events_receive_the_tunnel_events() => Run(() =>
	{
		var panel = new StackPanel();
		int down = 0, up = 0;
		panel.PreviewMouseDown += (s, e) => down++;
		panel.PreviewMouseUp += (s, e) => up++;

		RoutedEventHost.RaiseEvent(panel, new MouseButtonEventArgs(UIElement.PreviewMouseDownEvent, new Point(0, 0), MouseButton.Left));
		RoutedEventHost.RaiseEvent(panel, new MouseButtonEventArgs(UIElement.PreviewMouseUpEvent, new Point(0, 0), MouseButton.Left));

		Assert.Equal(1, down);
		Assert.Equal(1, up);
	});

	[Fact]
	public void Removed_PreviewMouseDown_handler_is_not_called() => Run(() =>
	{
		var panel = new StackPanel();
		int down = 0;
		MouseButtonEventHandler h = (s, e) => down++;
		panel.PreviewMouseDown += h;
		panel.PreviewMouseDown -= h;

		RoutedEventHost.RaiseEvent(panel, new MouseButtonEventArgs(UIElement.PreviewMouseDownEvent, new Point(0, 0), MouseButton.Left));

		Assert.Equal(0, down);
	});
}
