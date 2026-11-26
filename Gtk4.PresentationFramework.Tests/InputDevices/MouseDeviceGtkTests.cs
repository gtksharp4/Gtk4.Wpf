using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;

namespace Gtk4.PresentationFramework.Tests.InputDevices;

public class MouseDeviceGtkTests : WpfGtkTestBase
{
	static void Pump()
	{
		for (int i = 0; i < 200 && Gtk.Application.EventsPending(); i++)
			Gtk.Application.RunIteration(false);
	}

	[Fact]
	public void Device_position_matches_event_and_Mouse_position_on_GTK_raised_events() => Run(() =>
	{
		var canvas = new Canvas();
		var child = new Canvas { Width = 20, Height = 20 };
		Canvas.SetLeft(child, 30);
		Canvas.SetTop(child, 40);
		canvas.Children.Add(child);
		var win = new Gtk.Window();
		win.SetDefaultSize(200, 200);
		win.Child = canvas.Widget;
		win.Present();
		try
		{
			Pump();
			MouseEventArgs? move = null;
			MouseButtonEventArgs? down = null;
			MouseWheelEventArgs? wheel = null;
			child.MouseMove += (_, e) => move = e;
			child.MouseDown += (_, e) => down = e;
			child.MouseWheel += (_, e) => wheel = e;
			child.DispatchGtkMotion(new MouseEventKey(3, 1000, 1, 2), new Point(5, 6));
			child.DispatchGtkButton(new MouseEventKey(4, 1001, 1, 2), 1, new Point(5, 6), 1, pressed: true);
			child.RaiseMouseWheel(120);

			Assert.NotNull(move);
			Assert.Same(Mouse.PrimaryDevice, move!.MouseDevice);
			Assert.Same(move.Device, move.MouseDevice);
			Assert.NotEqual(0, move.Timestamp);
			Assert.Equal(move.GetPosition(canvas), move.MouseDevice.GetPosition(canvas));
			Assert.Equal(new Point(35, 46), move.MouseDevice.GetPosition(canvas));
			Assert.Equal(Mouse.GetPosition(canvas), Mouse.PrimaryDevice.GetPosition(canvas));
			Assert.Equal(Mouse.GetPosition(null), Mouse.PrimaryDevice.GetPosition(null));
			Assert.NotNull(down);
			Assert.Same(Mouse.PrimaryDevice, down!.MouseDevice);
			Assert.NotNull(wheel);
			Assert.Same(Mouse.PrimaryDevice, wheel!.MouseDevice);
			Assert.Same(wheel.Device, wheel.MouseDevice);
		}
		finally
		{
			Mouse.SetButtonState(MouseButton.Left, MouseButtonState.Released);
			Mouse.DirectlyOver = null;
			win.Destroy();
		}
	});
}
