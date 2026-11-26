using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xunit;
using WpfButton = System.Windows.Controls.Button;

namespace Gtk4.PresentationFramework.Tests.Input;

/// <summary>Real-widget smoke tests for the GTK 4 mouse controllers and coordinate mapping.</summary>
public class MouseInputGtkSmokeTests : WpfGtkTestBase
{
	static List<Gtk.EventController> Controllers(Gtk.Widget widget)
	{
		var list = new List<Gtk.EventController>();
		var model = widget.ObserveControllers();
		for (uint i = 0; i < model.NItems; i++)
			if (model.GetObject(i) is Gtk.EventController c) list.Add(c);
		return list;
	}

	static void Pump()
	{
		for (int i = 0; i < 200 && Gtk.Application.EventsPending(); i++)
			Gtk.Application.RunIteration(false);
	}

	[Fact]
	public void Realized_button_has_scroll_controller_attached() => Run(() =>
	{
		var btn = new WpfButton();
		var controllers = Controllers(btn.Widget);
		Assert.Contains(controllers, c => c is Gtk.EventControllerScroll);
	});

	[Fact]
	public void Click_controller_listens_to_every_button() => Run(() =>
	{
		var btn = new WpfButton();
		var click = Assert.Single(Controllers(btn.Widget), c => c is Gtk.GestureClick
			&& ((Gtk.GestureClick)c).Button == 0);
		Assert.NotNull(click);
	});

	[Fact]
	public void Motion_controller_contains_pointer_maps_to_IsMouseOver() => Run(() =>
	{
		var btn = new WpfButton();
		var motion = (Gtk.EventControllerMotion)Assert.Single(Controllers(btn.Widget),
			c => c is Gtk.EventControllerMotion);
		// No pointer in a headless test: the mapped state equals GTK's (false/false).
		btn.UpdateMouseOver(true, true);
		btn.UpdateMouseOver(motion.ContainsPointer(), motion.IsPointer);
		Assert.False(btn.IsMouseOver);
		Assert.False(btn.IsMouseDirectlyOver);
	});

	[Fact]
	public void GetPosition_relativeTo_parent_translates_via_ComputePoint() => Run(() =>
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
			var e = new MouseEventArgs(UIElement.MouseMoveEvent, new Point(5, 6)) { PositionSource = child };
			Assert.Equal(new Point(35, 46), e.GetPosition(canvas));
			Assert.Equal(new Point(5, 6), e.GetPosition(child));
			Assert.Equal(new Point(5, 6), e.GetPosition(null));
		}
		finally
		{
			win.Destroy();
		}
	});

	[Fact]
	public void GetPosition_without_PositionSource_returns_raw_position() => Run(() =>
	{
		var btn = new WpfButton();
		_ = btn.Widget;
		var e = new MouseEventArgs(UIElement.MouseMoveEvent, new Point(7, 8));
		Assert.Equal(new Point(7, 8), e.GetPosition(btn));
	});

	[Fact]
	public void CaptureMouse_returns_false_for_disabled_or_collapsed_element() => Run(() =>
	{
		var btn = new WpfButton();
		btn.IsEnabled = false;
		Assert.False(btn.CaptureMouse());
		Assert.Null(Mouse.Captured);

		btn.IsEnabled = true;
		btn.Visibility = Visibility.Collapsed;
		Assert.False(btn.CaptureMouse());

		btn.Visibility = Visibility.Visible;
		Assert.True(btn.CaptureMouse());
		btn.ReleaseMouseCapture();
		Assert.Null(Mouse.Captured);
	});
}
